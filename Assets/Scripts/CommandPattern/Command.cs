
/// <summary>
/// Base Command Class, Used for Command Pattern
/// </summary>
public abstract class Command
{
    public bool IsExecuting { get; set; }
    public string CommandName { get; protected set; }

    public abstract void PreExecute();
    public abstract void Execute(GameController gameController);

}
