using UnityEngine;

/// <summary>
/// Responsible for GameInput Management
/// </summary>
public class GameInput : MonoBehaviour
{

    [Header("GAME INPUT SETTINGS")]
    public KeyCode _cameraMoveKey;
    public KeyCode _cameraZoomKey;

    /// <summary>
    /// Block Open Key
    /// </summary>
    public bool IsBlockOpenKeyPressed { get; private set; }

    /// <summary>
    /// Block Flag Key
    /// </summary>
    public bool IsBlockMarkFlagKeyPressed { get; private set; }



    private void Update()
    {
        IsBlockOpenKeyPressed = Input.GetMouseButtonDown(0);
        IsBlockMarkFlagKeyPressed = Input.GetMouseButtonDown(1);
    }


}
