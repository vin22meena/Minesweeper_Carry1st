using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLevelJsonManager))]
public class Editor_LevelCreator : Editor
{
    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        GameLevelJsonManager gameleveljson = target as GameLevelJsonManager;

        GUILayout.Space(10f);

        
        GUILayout.Label($"By Current Settings Min {gameleveljson._minGridValue} and Max {gameleveljson._maxGridValue} is the Grid blocks Thershold,\nThat is customizable from the code.");

        GUILayout.Space(10f);

        if (GUILayout.Button("EXPORT"))
        {
            if(gameleveljson.ExportLevelJSON())
            {
                AssetDatabase.Refresh();
            }

        }
    }
}
