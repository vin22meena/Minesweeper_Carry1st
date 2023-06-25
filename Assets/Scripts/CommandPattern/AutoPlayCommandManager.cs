using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AutoPlayCommandManager : MonoBehaviour
{

    [SerializeField] GameController _gameController;

    Command m_currentCommand;
    
    Queue<Command> m_commandsInQueue = new Queue<Command>();

    [SerializeField] bool isAutoPlayEnabled = false;
    [SerializeField] float _timeDelayInBetweenCommands = 0.5f;


    float m_currentDelayTimer = 0f;

    private void Start()
    {
        m_commandsInQueue.Enqueue(new RevealCommand());
    }

    private void Update()
    {
        AutoplayCommands();
    }


    void AutoplayCommands()
    {

        if (!isAutoPlayEnabled)
            return;



        if (m_currentCommand != null)
        {
            if (m_currentCommand.IsExecuting)
            {

                Debug.Log($"Command {m_currentCommand.CommandName} Still Executing!!");
                m_currentCommand.Execute(_gameController);

                UpdateCommands(_gameController);

                return;
            }


            m_currentDelayTimer += Time.deltaTime;

            if (m_currentDelayTimer < _timeDelayInBetweenCommands)
                return;

        }


        



        if (m_commandsInQueue.Count == 0)
        {
            Debug.Log("NO COMMAND AVAILABLE::");
            m_currentCommand = null;
            return;
        }

        m_currentCommand = m_commandsInQueue.Dequeue();

        m_currentCommand.PreExecute();

        m_currentDelayTimer = 0f;

    }


    void UpdateCommands(GameController gameController)
    {
        if (gameController == null)
            return;


        if(gameController.GetStatusGameFinishedAutoPlay() || !gameController.GetStatusValidBlocksAvailableAutoPlay())
        {
            isAutoPlayEnabled = false;
            m_currentCommand = null;
            m_commandsInQueue.Clear();
            return;
        }

        int randomCommandChooseNumber = Random.Range(0, 2);

        if (randomCommandChooseNumber == 0)
            m_commandsInQueue.Enqueue(new RevealCommand());
        else if (randomCommandChooseNumber == 1)
        {

            int xPos = -1;
            int yPos = -1;

            Block[,] currentGameState = gameController.GetCurrentStateAutoPlay();



            for(int i=0;i<currentGameState.GetLength(0);i++)
            {
                for(int j=0;j<currentGameState.GetLength(1);j++)
                {
                    if (currentGameState[i,j]._blockType==BLOCK_TYPE.NUMBER)
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


        Debug.Log("Added Command"+ randomCommandChooseNumber);
    }


}
