
public abstract class Command
{
    public bool IsExecuting { get; set; }
    public string CommandName { get; protected set; }

    public abstract void PreExecute();
    public abstract void Execute(GameController gameController);

}
