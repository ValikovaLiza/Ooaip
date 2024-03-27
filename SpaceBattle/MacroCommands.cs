using _ICommand;

namespace SpaceBattle;

public class MacroCommand : ICommand
{
    private readonly IList<ICommand> list;

    public MacroCommand(IList<ICommand> list)
    {
        this.list = list;
    }

    public void Execute()
    {
        foreach (var c in list)
        {
            c.Execute();
        }
    }
}
