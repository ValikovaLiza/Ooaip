namespace SpaceBattle;

public class DeleteObject : _ICommand.ICommand
{
    Dictionary<string, object> objects;
    string gameItemId;
    public DeleteObject(Dictionary<string, object> objects, string gameItemId)
    {
        this.objects = objects;
        this.gameItemId = gameItemId;
    }
    public void Execute()
    {
        objects.Remove(gameItemId);
    }
}