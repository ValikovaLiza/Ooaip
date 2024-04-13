using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Hwdtech;
using Newtonsoft.Json;

namespace Udp;
public class EndPoint : IPEndPoint
{
    private int port;
    private readonly IPAddress address;
    public EndPoint(IPAddress address, int port) : base(address, port)
    {
        this.address = address;
        this.port = port;
    }
    public void GetMessage(byte[] sendbuf)
    {
        var message = JsonConvert.DeserializeObject<CommandData>(Encoding.ASCII.GetString(sendbuf, 0, sendbuf.Length));

        var q = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Send Message", message!.gameId!, message);

        IoC.Resolve<ICommand>("IoC.Register", "Get Queue", (object[] args) => q).Execute();
    }
}
