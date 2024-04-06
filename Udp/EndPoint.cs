using System.Net;
using System.Text;
using Hwdtech;
using Newtonsoft.Json;

namespace Udp;
public class EndPoint : IPEndPoint
{
    public EndPoint(IPAddress address, int port) : base(address, port)
    {

    }
    public static void GetMessage(byte[] sendbuf)
    {
        var message = JsonConvert.DeserializeObject<CommandData>(Encoding.ASCII.GetString(sendbuf, 0, sendbuf.Length));

        IoC.Resolve<_ICommand.ICommand>("Send Message", message!.gameId!, message).Execute();
    }
}
