using _ICommand;

namespace SpaceBattle;

public interface IQueue
{
    void Add(ICommand cmd);
    ICommand Take();
}