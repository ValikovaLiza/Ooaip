using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hwdtech;

namespace Udp;

public class UDPServer
{
    private const int PORT = 8080;

    private Socket? _socket;
    private IPEndPoint? _ep;

    private byte[]? _buffer_recv;
    private ArraySegment<byte> _buffer_recv_segment;

    public void Initialize()
    {
        _buffer_recv = new byte[4096];
        _buffer_recv_segment = new(_buffer_recv);

        _ep = new IPEndPoint(IPAddress.Any, PORT);

        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        _socket.Bind(_ep);
    }
    public void StartMessageLoop()
    {
        _ = Task.Run(async () =>
        {
            SocketReceiveMessageFromResult res;
            while (true)
            {
                res = await _socket!.ReceiveMessageFromAsync(_buffer_recv_segment, SocketFlags.None, _ep!);
                await SendTo((EndPoint)res.RemoteEndPoint, Encoding.UTF8.GetBytes("Hello back!"));
            }
        });
    }
    public async Task SendTo(EndPoint recipient, byte[] data)
    {
        var s = new ArraySegment<byte>(data);
        await _socket!.SendToAsync(s, SocketFlags.None, recipient);
    }
    public static void TableOfThreadsAndQueues()
    {
        var gameToThread = new Dictionary<string, string>();
        var threadToQueue = new Dictionary<string, BlockingCollection<_ICommand.ICommand>>();
        IoC.Resolve<ICommand>("IoC.Register", "Get GameToThreadDict", (object[] args) => gameToThread).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Get ThreadToQueueDict", (object[] args) => threadToQueue).Execute();

        gameToThread.Add("asdfg", "thefirst");
        threadToQueue.Add("thefirst", new BlockingCollection<_ICommand.ICommand>());
    }
}
