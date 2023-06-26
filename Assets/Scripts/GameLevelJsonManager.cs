using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Responsible for Generating, Handling JSON
/// </summary>
public class GameLevelJsonManager : MonoBehaviour
{

    [SerializeField] LevelData _levelData;

    /// <summary>
    /// Readonly Min Value Threshold
    /// </summary>
    public int _minGridValue=>4;

    /// <summary>
    /// Readonly Max Value Threshold
    /// </summary>
    public int _maxGridValue =>64;


    private void OnValidate()
    {
        if(_levelData!=null)
        {
            
            //Validation on Values while generating levels
            
            _levelData._levelWidth = Mathf.Clamp(_levelData._levelWidth, _minGridValue, _maxGridValue);
            _levelData._levelHeight = Mathf.Clamp(_levelData._levelHeight, _minGridValue, _maxGridValue);
            
            _levelData._totalMinesCount = Mathf.Clamp(_levelData._totalMinesCount, 0, _levelData._levelWidth * _levelData._levelHeight);



        }
    }


    /// <summary>
    /// Export JSON Function. Handles Json Creation, Modification.
    /// </summary>
    /// <returns></returns>
    public bool ExportLevelJSON()
    {
        try
        {


            if (_levelData == null)
                return false;


            if (string.IsNullOrEmpty(_levelData._levelName))
            {
                Debug.Log("LEVEL NAME CAN'T BE NULL");
            }

            if (_levelData._levelWidth < 1 || _levelData._levelHeight < 1)
                Debug.Log("Height and Width should be greater than 1");



            string jsonData = JsonUtility.ToJson(_levelData);

            string directoryPath = string.Format(Path.Combine(Application.dataPath, "JSON_LEVELS"));
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, $"{_levelData._levelName}.json");

            StreamWriter streamWriter = File.CreateText(filePath);
            streamWriter.Write(jsonData);
            streamWriter.Close();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
            return false;
        }

        return true;

    }

}

/// <summary>
/// Level Data Serialized Class, Used for Data Serialization in Json
/// </summary>
[Serializable]
public class LevelData
{

    public string _levelName=string.Empty;
    public int _levelWidth=4;
    public int _levelHeight=4;
    public int _totalMinesCount=0;

}

