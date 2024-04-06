using System.Collections.Concurrent;
using System.Net;
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

        IoC.Resolve<ICommand>("IoC.Register", "Send Message",
        (object[] args) =>
        {
            var dictthread = IoC.Resolve<Dictionary<string, string>>("Get GameToThreadDict"); // ???????? проблема с int должно быть string
            var threadId = dictthread[(string)args[0]];
            var dictqu = IoC.Resolve<Dictionary<string, BlockingCollection<_ICommand.ICommand>>>("Get ThreadToQueueDict");
            dictqu[(string)threadId].Add((_ICommand.ICommand)args[1]);
            return dictqu[(string)threadId];
        }).Execute();

    }

    [Fact]
    public void Test()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        var PORT = 8080;

        var server = new UDPServer();
        server.Initialize();
        server.StartMessageLoop();
        Console.WriteLine("Server listening!");

        UDPServer.TableOfThreadsAndQueues();

        var client = new UDPClient();
        client.Initialize(IPAddress.Loopback, PORT);
        client.StartMessageLoop();

        Console.WriteLine("Client sending!");

        var message = new CommandData
        {
            CommandType = "fire",
            gameId = "asdfg",
            gameItemId = "548",
        };
        var s = JsonConvert.SerializeObject(message, Formatting.Indented);
        _ = client.Send(Encoding.UTF8.GetBytes(s));
    }
}

