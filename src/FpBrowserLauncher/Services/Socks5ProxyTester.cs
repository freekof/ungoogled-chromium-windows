using System.Net.Sockets;
using System.Text;
using FpBrowserLauncher.Models;

namespace FpBrowserLauncher.Services;

public sealed class Socks5ProxyTester : ISocks5ProxyTester
{
    public async Task<ProxyTestResult> TestAsync(ProxyConfig proxy, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(proxy.Type, "socks5", StringComparison.OrdinalIgnoreCase))
        {
            return ProxyTestResult.Failed("仅支持 SOCKS5 代理。");
        }

        if (string.IsNullOrWhiteSpace(proxy.Host) || proxy.Port <= 0 || proxy.Port > 65535)
        {
            return ProxyTestResult.Failed("代理 host 或 port 无效。");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeoutMs);

        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(proxy.Host, proxy.Port, timeoutCts.Token);
            await using var stream = tcpClient.GetStream();

            var methods = proxy.HasCredentials ? new byte[] { 0x00, 0x02 } : [0x00];
            byte[] greeting = [0x05, (byte)methods.Length, .. methods];
            await stream.WriteAsync(greeting, timeoutCts.Token);

            var response = new byte[2];
            await ReadExactAsync(stream, response, timeoutCts.Token);
            if (response[0] != 0x05)
            {
                return ProxyTestResult.Failed("代理没有返回 SOCKS5 协议版本。");
            }

            return response[1] switch
            {
                0x00 => ProxyTestResult.Alive(),
                0x02 => await AuthenticateAsync(stream, proxy, timeoutCts.Token),
                0xFF => ProxyTestResult.Failed("SOCKS5 代理拒绝可用认证方式。"),
                _ => ProxyTestResult.Failed($"SOCKS5 代理返回未知认证方式 0x{response[1]:X2}。")
            };
        }
        catch (OperationCanceledException)
        {
            return ProxyTestResult.Failed("SOCKS5 代理测活超时。");
        }
        catch (SocketException ex)
        {
            return ProxyTestResult.Failed($"SOCKS5 代理连接失败：{ex.Message}");
        }
        catch (IOException ex)
        {
            return ProxyTestResult.Failed($"SOCKS5 代理握手失败：{ex.Message}");
        }
    }

    private static async Task<ProxyTestResult> AuthenticateAsync(NetworkStream stream, ProxyConfig proxy, CancellationToken cancellationToken)
    {
        var username = Encoding.UTF8.GetBytes(proxy.Username ?? string.Empty);
        var password = Encoding.UTF8.GetBytes(proxy.Password ?? string.Empty);
        if (username.Length > byte.MaxValue || password.Length > byte.MaxValue)
        {
            return ProxyTestResult.Failed("SOCKS5 用户名或密码过长。");
        }

        var request = new byte[3 + username.Length + password.Length];
        request[0] = 0x01;
        request[1] = (byte)username.Length;
        username.CopyTo(request, 2);
        request[2 + username.Length] = (byte)password.Length;
        password.CopyTo(request, 3 + username.Length);

        await stream.WriteAsync(request, cancellationToken);
        var response = new byte[2];
        await ReadExactAsync(stream, response, cancellationToken);

        return response is [0x01, 0x00]
            ? ProxyTestResult.Alive()
            : ProxyTestResult.Failed("SOCKS5 用户名/密码认证失败。");
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
            if (read == 0)
            {
                throw new IOException("代理在握手期间关闭连接。");
            }

            offset += read;
        }
    }
}
