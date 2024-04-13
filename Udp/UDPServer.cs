using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Hwdtech;

public class UDPServer
{
    private const int listenPort = 11000;

    public static void StartListener(byte[] sendbuf, IPEndPoint ep)
    {
        var listener = new UdpClient(listenPort);

        while (true)
        {
            listener.Send(sendbuf, sendbuf.Length, ep);

            break;
        }

        listener.Close();
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
