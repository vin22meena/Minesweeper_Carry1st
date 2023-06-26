
/// <summary>
/// Reveal Command, Manage Autoplay Reveal Functionality
/// </summary>
public class RevealCommand : Command
{

    public override void PreExecute()
    {
        IsExecuting = true;
        CommandName = "Reveal Command";
    }

    public override void Execute(GameController gameController)
    {
        if(IsExecuting)
        { 
            gameController.RevealRandomBlockAutoPlay();
            IsExecuting = false;
        }
    }

}
