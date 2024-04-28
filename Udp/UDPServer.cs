using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hwdtech;

public class UDPServer
{
    private const int listenPort = 11000;
    private Thread? listenThread;
    private bool running = true;

    private void StartListener()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        var listener = new UdpClient(listenPort);
        var groupEP = new IPEndPoint(IPAddress.Any, 0);

        listenThread = new Thread(() =>
        {
            try
            {
                var bytes = new byte[1024];
                while (!bytes.SequenceEqual(Encoding.ASCII.GetBytes("STOP")) && running)
                {
                    bytes = listener.Receive(ref groupEP);
                }
            }
            catch (Exception e)
            {
                IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", e).Execute();
            }
            finally
            {
                listener.Close();
            }
        });

        listenThread.Start();
    }

    public void Main()
    {
        StartListener();
    }
    public void Stop()
    {
        running = false;
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
