using _ICommand;

namespace SpaceBattle;

public interface IQueue
{
    void Add(Hwdtech.ICommand cmd);
    ICommand Take();
}
