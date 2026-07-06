using System.Net;
using System.Net.Sockets;
using FpBrowserLauncher.Models;
using FpBrowserLauncher.Services;
using Xunit;

namespace FpBrowserLauncher.Tests;

public sealed class Socks5ProxyTesterTests
{
    [Fact]
    public async Task TestAsync_WithNoAuthHandshake_ReturnsAlive()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var serverTask = AcceptNoAuthHandshakeAsync(listener);

        var tester = new Socks5ProxyTester();
        var result = await tester.TestAsync(new ProxyConfig { Host = "127.0.0.1", Port = port }, timeoutMs: 3000);

        Assert.True(result.IsAlive, result.Message);
        await serverTask;
    }

    [Fact]
    public async Task TestAsync_WithInvalidPort_ReturnsFailed()
    {
        var tester = new Socks5ProxyTester();

        var result = await tester.TestAsync(new ProxyConfig { Host = "127.0.0.1", Port = 0 }, timeoutMs: 100);

        Assert.False(result.IsAlive);
    }

    private static async Task AcceptNoAuthHandshakeAsync(TcpListener listener)
    {
        using var client = await listener.AcceptTcpClientAsync();
        await using var stream = client.GetStream();
        var buffer = new byte[3];
        var read = 0;
        while (read < buffer.Length)
        {
            read += await stream.ReadAsync(buffer.AsMemory(read, buffer.Length - read));
        }

        Assert.Equal(0x05, buffer[0]);
        Assert.Equal(0x01, buffer[1]);
        Assert.Equal(0x00, buffer[2]);
        byte[] response = [0x05, 0x00];
        await stream.WriteAsync(response);
    }
}
