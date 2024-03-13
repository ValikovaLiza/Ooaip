namespace SpaceBattle;
public class HardStop : _ICommand.ICommand
{
    private readonly ServerThread thread;
    public HardStop(ServerThread thread)
    {
        this.thread = thread;
    }
    public void Execute()
    {
        thread.Stop();
    }
}
