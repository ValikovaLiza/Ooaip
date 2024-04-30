using System.Collections.Concurrent;
using System.Text;
using Hwdtech;
using Newtonsoft.Json;

namespace Udp;
public class EndPoint
{
    public static void GetMessage(byte[] sendbuf)
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var message = JsonConvert.DeserializeObject<CommandData>(Encoding.ASCII.GetString(sendbuf, 0, sendbuf.Length));

        var q = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Send Message", message!.gameId!, message);

        IoC.Resolve<ICommand>("IoC.Register", "Get Queue", (object[] args) => q).Execute();
    }
}
