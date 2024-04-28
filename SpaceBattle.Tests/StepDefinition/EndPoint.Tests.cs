using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hwdtech;
using Hwdtech.Ioc;
using Newtonsoft.Json;
using Udp;

namespace SpaceBattle.Test;

public class EndPointTests
{
    public EndPointTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var dictOfCommands = new Dictionary<string, _ICommand.ICommand>();
        var command = new ActionCommand(() => { });
        dictOfCommands.Add("fire", command);
        dictOfCommands.Add("start", command);
        dictOfCommands.Add("stop", command);
        dictOfCommands.Add("spin", command);
        IoC.Resolve<ICommand>("IoC.Register", "Get CommandsDict", (object[] args) => dictOfCommands).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Send Message",
        (object[] args) =>
        {
            var dictthread = IoC.Resolve<Dictionary<string, string>>("Get GameToThreadDict");
            var threadId = dictthread[(string)args[0]];
            var dictqu = IoC.Resolve<Dictionary<string, BlockingCollection<_ICommand.ICommand>>>("Get ThreadToQueueDict");
            var commanddd = (CommandData)args[1];
            var command = commanddd.CommandType;
            dictqu[(string)threadId].Add(IoC.Resolve<Dictionary<string, _ICommand.ICommand>>("Get CommandsDict")[command!]);
            return dictqu[(string)threadId];
        }).Execute();

    }

    [Fact]
    public void MessageWasRecivedAndAddedToNessesaryQueue()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        var so = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        UDPServer.TableOfThreadsAndQueues();

        UDPServer.Main();

        var broadcast = IPAddress.Parse("192.168.1.33");

        var message = new CommandData
        {
            CommandType = "fire",
            gameId = "asdfg",
            gameItemId = "548",
        };
        var s = JsonConvert.SerializeObject(message, Formatting.Indented);
        var sendbuf = Encoding.ASCII.GetBytes(s);

        var ep = new IPEndPoint(broadcast, 11000);

        so.SendTo(sendbuf, ep);

        Udp.EndPoint.GetMessage(sendbuf);

        var qu = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get Queue");
        Assert.Single(qu);
    }
}
