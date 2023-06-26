using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Autoplay Manager Uses Command Pattern to perform each operation for Auto Gameplay.
/// </summary>
public class AutoPlayCommandManager : MonoBehaviour
{
    [Header("SETTINGS")]
    [SerializeField] GameController _gameController;
    [SerializeField][Tooltip("Change this value to manage speed of running steps for Autoplay")] float _timeDelayInBetweenCommands = 0.5f;


    Command m_currentCommand;
    Queue<Command> m_commandsInQueue = new Queue<Command>();

    float m_currentDelayTimer = 0f;

    private void Start()
    {
        m_commandsInQueue.Enqueue(new RevealCommand());
    }

    private void Update()
    {
        AutoplayCommands();
    }


    /// <summary>
    /// Auto Play Command Management.
    /// Checking whether any command is pending in the queue or not and execute those accordingly.
    /// </summary>
    void AutoplayCommands()
    {

        if (_gameController == null)
            return;

        if (!_gameController.GetStatusGameStartedAutoPlay())
        {
            return;
        }
        


        if (!_gameController.GetAutoPlayStatus())
            return;




        if (m_currentCommand != null)
        {
            //Time delay in between Execution of Commands

            m_currentDelayTimer += Time.deltaTime;

            if (m_currentDelayTimer < _timeDelayInBetweenCommands)
                return;


            if (m_currentCommand.IsExecuting)
            {
                m_currentCommand.Execute(_gameController);

                UpdateCommands(_gameController);

                return;
            }

        }


        



        if (m_commandsInQueue.Count == 0)
        {
            m_currentCommand = null;
            return;
        }

        m_currentCommand = m_commandsInQueue.Dequeue();

        m_currentCommand.PreExecute();

        m_currentDelayTimer = 0f;

    }

    /// <summary>
    /// Autoplay Command Selectio Logic, Chosing Random Command.
    /// Checknig if it's set flag command than get the adjancy of number and choose random block to set flag.
    /// </summary>
    /// <param name="gameController"></param>
    void UpdateCommands(GameController gameController)
    {
        if (gameController == null)
            return;


        if(gameController.GetStatusGameFinishedAutoPlay() || !gameController.GetStatusValidBlocksAvailableAutoPlay())
        {
            m_currentCommand = null;
            m_currentDelayTimer = 0f;
            m_commandsInQueue.Clear();
            m_commandsInQueue.Enqueue(new RevealCommand());
            return;
        }

        //Random Choosing Commands
        int randomCommandChooseNumber = Random.Range(0, 2);

        if (randomCommandChooseNumber == 0)
            m_commandsInQueue.Enqueue(new RevealCommand());
        else if (randomCommandChooseNumber == 1)
        {

            int xPos = -1;
            int yPos = -1;

            Block[,] currentGameState = gameController.GetCurrentStateAutoPlay();


            //Checkind Adjancy and Setting Flag
            for(int i=0;i<currentGameState.GetLength(0);i++)
            {
                for(int j=0;j<currentGameState.GetLength(1);j++)
                {
                    if (currentGameState[i,j]._blockType==BLOCK_TYPE.NUMBER && currentGameState[i, j].isBlockRevealed)
                    {
                        List<Block> adjancy = gameController.GetAdjancyBlocksAutoPlay(currentGameState[i, j]);


                        foreach(var adjancyBlock in adjancy)
                        {
                            if(!adjancyBlock.isBlockRevealed && !adjancyBlock.isBlockFlagged)
                            {
                                xPos = adjancyBlock._blockPositionInGrid.x;
                                yPos = adjancyBlock._blockPositionInGrid.y;

                                break;
                            }    
                        }
                        


                    }
                }
            }

            m_commandsInQueue.Enqueue(new SetFlagCommand(xPos,yPos));

        }
    }


}
