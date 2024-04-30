using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using Hwdtech;

namespace Udp;
public class EndPoint
{
    private readonly Thread _listenThread;
    private Action _HookAfter = () => { };
    private Action _HookBefore = () => { };
    private readonly int _listenPort;
    private bool running = true;

    public EndPoint(int port)
    {
        _listenPort = port;
        var listener = new UdpClient(_listenPort);
        var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, _listenPort);
        _listenThread = new Thread(() =>
        {
            try
            {
                IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

                _HookBefore();

                var bytes = new byte[1024];

                while (!bytes.SequenceEqual(Encoding.ASCII.GetBytes("STOP")) && running)
                {
                    bytes = listener.Receive(ref RemoteIpEndPoint);

                    if (bytes.SequenceEqual(Encoding.ASCII.GetBytes("STOP")))
                    {
                        break;
                    }

                    var jsonString = Encoding.UTF8.GetString(bytes);

                    var serializer = new DataContractJsonSerializer(typeof(CommandData));
                    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                    var message = serializer.ReadObject(memoryStream) as CommandData;

                    var cmd = IoC.Resolve<_ICommand.ICommand>("InterpretCommand", message!);
                    var threadID = IoC.Resolve<object>("Get Thread ID by Game ID", message!.gameId!);
                    IoC.Resolve<_ICommand.ICommand>("Send Message", threadID, cmd).Execute();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                listener.Close();
                _HookAfter();
            }
        });
    }

    public void Start()
    {
        _listenThread.Start();
    }

    public void UpdateHookAfter(Action NewHookAfter)
    {
        _HookAfter = NewHookAfter;
    }

    public void UpdateHookBefore(Action NewHookBefore)
    {
        _HookBefore = NewHookBefore;
    }
    public void Stop()
    {
        running = false;
    }
}
