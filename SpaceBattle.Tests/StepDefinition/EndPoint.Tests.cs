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

        var dictOfCommands = new ConcurrentDictionary<string, _ICommand.ICommand>();
        var command = new ActionCommand(() => { });
        dictOfCommands.TryAdd("fire", command);
        dictOfCommands.TryAdd("start", command);
        dictOfCommands.TryAdd("stop", command);
        dictOfCommands.TryAdd("spin", command);
        IoC.Resolve<ICommand>("IoC.Register", "Get CommandsDict", (object[] args) => dictOfCommands).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Send Message",
        (object[] args) =>
        {
            var dictthread = IoC.Resolve<ConcurrentDictionary<string, string>>("Get GameToThreadDict");
            var threadId = dictthread[(string)args[0]];
            var dictqu = IoC.Resolve<ConcurrentDictionary<string, BlockingCollection<_ICommand.ICommand>>>("Get ThreadToQueueDict");
            var commanddd = (CommandData)args[1];
            var command = commanddd.CommandType;
            dictqu[(string)threadId].Add(IoC.Resolve<ConcurrentDictionary<string, _ICommand.ICommand>>("Get CommandsDict")[command!]);
            return dictqu[(string)threadId];
        }).Execute();

    }

    [Fact]
    public void MessageWasRecivedAndAddedToNessesaryQueue()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var client = new UdpClient();

        UDPServer.TableOfThreadsAndQueues();
        var server = new UDPServer();
        server.Main();

        var message = new CommandData
        {
            CommandType = "fire",
            gameId = "asdfg",
            gameItemId = "548",
        };
        var s = JsonConvert.SerializeObject(message, Formatting.Indented);
        var sendbuf = Encoding.ASCII.GetBytes(s);

        var ep = new IPEndPoint(IPAddress.Parse("192.168.1.33"), 11000);

        client.Send(sendbuf, sendbuf.Length, ep);
        var message2 = Encoding.ASCII.GetBytes("STOP");
        client.Send(message2, message2.Length, ep);

        Udp.EndPoint.GetMessage(sendbuf);
        client.Close();
        server.Stop();

        var qu = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get Queue");
        Assert.Single(qu);
    }
}
