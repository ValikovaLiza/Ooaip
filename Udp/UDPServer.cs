using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hwdtech;

public class UDPServer
{
    private const int listenPort = 11000;

    public void StartListener()
    {
        var listener = new UdpClient(listenPort);
        var groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for broadcast");
                var bytes = listener.Receive(ref groupEP);

                Console.WriteLine($"Received broadcast from {groupEP} :");
                Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            listener.Close();
        }
    }

    public void TableOfThreadsAndQueues()
    {
        var gameToThread = new Dictionary<string, string>();
        var threadToQueue = new Dictionary<string, BlockingCollection<_ICommand.ICommand>>();
        IoC.Resolve<ICommand>("IoC.Register", "Get GameToThreadDict", (object[] args) => gameToThread).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Get ThreadToQueueDict", (object[] args) => threadToQueue).Execute();

        gameToThread.Add("asdfg", "thefirst");
        threadToQueue.Add("thefirst", new BlockingCollection<_ICommand.ICommand>());
    }
}
