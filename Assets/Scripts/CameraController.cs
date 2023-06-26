using System;
using UnityEngine;

/// <summary>
/// Responsible for Camera Move, Zoom and Auto Set Dimension based on Grid Size.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("CAMERA SETTINGS")]
    [SerializeField] GameInput _gameInput;
    [SerializeField] [Range(20f,100f)]float _maxZoom;
    [SerializeField][Range(5f,20f)] float _minZoom;
    [SerializeField][Range(0.01f,1f)] float _moveSpeed;

    Camera m_camera;

    Vector3 m_lastPosition;
    float m_zoomAmount=0f;

    private void Start()
    {
        m_camera = Camera.main;
    }


    /// <summary>
    /// Get Current Cached Camera
    /// </summary>
    /// <returns></returns>
    public Camera GetCurrentCamera()
    {
        return m_camera;
    }


    private void Update()
    {
        if (_gameInput == null)
            return;


        if (Input.GetKeyDown(_gameInput._cameraMoveKey))
        {
            m_lastPosition = Input.mousePosition;

        }

        if (Input.GetKey(_gameInput._cameraMoveKey))
        {
            MoveCamera(Input.mousePosition);
        }


        if(Input.GetKey(_gameInput._cameraZoomKey))
        {
            ZoomCamera(Input.mouseScrollDelta);
        }

    }


    /// <summary>
    /// Moving the Camera Throughout the Game. Currently Pan Area Code is not implemented.
    /// </summary>
    /// <param name="panPosition"></param>
    void MoveCamera(Vector3 panPosition)
    {
        Vector3 movePos = panPosition-m_lastPosition;

        m_camera.transform.Translate(-movePos.x * _moveSpeed, -movePos.y * _moveSpeed, 0f);

        m_lastPosition = panPosition;
    }

    /// <summary>
    /// Zooming In and Out the Camera my Mouse Scroll
    /// </summary>
    /// <param name="scrollDelta"></param>
    void ZoomCamera(Vector3 scrollDelta)
    {
        m_zoomAmount += scrollDelta.y;

        m_zoomAmount = Mathf.Clamp(m_zoomAmount, _minZoom, _maxZoom);

        m_camera.orthographicSize = m_zoomAmount;

    }


    /// <summary>
    /// Detecting mid of Grid and updating the Camera Position By Default at Start.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool SetCameraDimension(int width, int height)
    {
        try
        {

            float x = width / 2f;
            float y = height/ 2f;

            var camPos = m_camera.transform.position;
            camPos.x = x;
            camPos.y = y;
            m_camera.transform.position = camPos;

        }
        catch(Exception e)
        {
            Debug.LogException(e);
            return false;
        }

        return true;
    }




}
