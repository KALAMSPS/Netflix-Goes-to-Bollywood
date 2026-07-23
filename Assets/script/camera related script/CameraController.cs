using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Enable to move the camera by holding the right mouse button. Does not work with joysticks.")]
    public bool clickToMoveCamera = false;
    [Tooltip("Enable zoom in/out when scrolling the mouse wheel. Does not work with joysticks.")]
    public bool canZoom = false;
    [Tooltip("Camera movement sensitivity.")]
    public float sensitivity = 10f;
    [Tooltip("Camera Y rotation limits (X: up limit, Y: down limit).")]
    public Vector2 cameraLimit;

    [Header("Alternate Camera Position")]
    [Tooltip("Alternative camera position when switching view.")]
    public Transform alternateCameraPosition;

    private float mouseX;
    private float mouseY;
    private float offsetDistanceY;
    private Transform player;
    private bool isAlternatePosition = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        canZoom = false;
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Ensure the player has the 'Player' tag.");
            return;
        }
        offsetDistanceY = transform.position.y;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchCameraPosition();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            clickToMoveCamera = !clickToMoveCamera;
        }

        if (!isAlternatePosition)
        {
            FollowPlayer();
        }

       // HandleZoom();
        HandleCameraRotation();
    }

    private void FollowPlayer()
    {
        if (player != null)
        {
            transform.position = player.position + new Vector3(0, offsetDistanceY, 0);
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }
    }

    private void HandleZoom()
    {
        if (canZoom && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * 2;
        }
    }

    private void HandleCameraRotation()
    {
        if (clickToMoveCamera && !Input.GetMouseButton(1))
        {
            return;
        }

        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY = Mathf.Clamp(mouseY + Input.GetAxis("Mouse Y") * sensitivity, cameraLimit.x, cameraLimit.y);
        transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
    }

    private void SwitchCameraPosition()
    {
        if (alternateCameraPosition == null)
        {
            Debug.LogWarning("Alternate camera position is not set!");
            return;
        }

        if (isAlternatePosition)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
        else
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            transform.position = alternateCameraPosition.position;
            transform.rotation = alternateCameraPosition.rotation;
        }
        isAlternatePosition = !isAlternatePosition;
    }
}
