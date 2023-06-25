using UnityEngine;

public class GameInput : MonoBehaviour
{


    public KeyCode _cameraMoveKey = KeyCode.LeftShift;
    public KeyCode _cameraZoomKey;

    public bool IsBlockOpenKeyPressed { get; private set; }

    public bool IsBlockMarkFlagKeyPressed { get; private set; }



    private void Update()
    {
        IsBlockOpenKeyPressed = Input.GetMouseButtonDown(0);
        IsBlockMarkFlagKeyPressed = Input.GetMouseButtonDown(1);
    }


}
