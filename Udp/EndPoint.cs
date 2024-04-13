using System.Collections.Concurrent;
using System.Text;
using Hwdtech;
using Newtonsoft.Json;

namespace Udp;
public class EndPoint
{
    public static void GetMessage(byte[] sendbuf)
    {
        var message = JsonConvert.DeserializeObject<CommandData>(Encoding.ASCII.GetString(sendbuf, 0, sendbuf.Length));

        var q = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Send Message", message!.gameId!, message);

        IoC.Resolve<ICommand>("IoC.Register", "Get Queue", (object[] args) => q).Execute();
    }
}
