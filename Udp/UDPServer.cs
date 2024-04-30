using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Hwdtech;

public class UDPServer
{
    private Thread? listenThread;
    private Socket? _socket;

    public UDPServer()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    private void StartListener()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        var bytes = new byte[1024];

        listenThread = new Thread(() =>
        {
            try
            {
                while (!bytes.SequenceEqual(Encoding.ASCII.GetBytes("STOP")))
                {
                    _socket.Receive(bytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _socket.Shutdown(SocketShutdown.Receive);
            }
        });

        listenThread.Start();
    }

    public void Main()
    {
        StartListener();
    }
    public static void TableOfThreadsAndQueues()
    {
        var gameToThread = new ConcurrentDictionary<string, string>();
        var threadToQueue = new ConcurrentDictionary<string, BlockingCollection<_ICommand.ICommand>>();
        IoC.Resolve<ICommand>("IoC.Register", "Get GameToThreadDict", (object[] args) => gameToThread).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Get ThreadToQueueDict", (object[] args) => threadToQueue).Execute();

        gameToThread.TryAdd("asdfg", "thefirst");
        threadToQueue.TryAdd("thefirst", new BlockingCollection<_ICommand.ICommand>());
    }
}
