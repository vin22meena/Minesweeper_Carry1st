
/// <summary>
/// SetFlag Command, Manage Autoplay Set Flag Functionality
/// </summary>
public class SetFlagCommand : Command
{

    public int xFlagPosition;
    public int yFlagPosition;


    public SetFlagCommand(int xFlagPos,int yFlagPos)
    {
        xFlagPosition = xFlagPos;
        yFlagPosition = yFlagPos;
    }

    public override void PreExecute()
    {
        IsExecuting = true;
        CommandName = "Set Flag Command";
    }
    public override void Execute(GameController gameController)
    {
        if (IsExecuting)
        {
            gameController.SetFlagOnRandomBlockAutoPlay(xFlagPosition,yFlagPosition);
            IsExecuting = false;
        }
    }

}
