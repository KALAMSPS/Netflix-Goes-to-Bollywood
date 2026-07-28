/* using UnityEngine;

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
    public Vector2 cameraLimit = new Vector2(-45, 40);

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
 */
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool clickToMoveCamera = false;
    public bool canZoom = false;
    public float sensitivity = 4f;
    public Vector2 cameraLimit = new Vector2(-45, 40);

    [Header("Collision")]
    public LayerMask wallLayers;
    public float sphereRadius = 0.25f;
    public float wallOffset = 0.15f;
    public float smooth = 10f;

    public Transform alternateCameraPosition;

    float mouseX, mouseY;
    Transform player, cam;
    float offsetDistanceY;
    bool isAlternatePosition;
    Vector3 defaultLocalPos, currentLocalPos;
    Vector3 originalPosition;
    Quaternion originalRotation;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main.transform;

        Debug.Log("Player : " + player.name);
        Debug.Log("Camera : " + cam.name);

        offsetDistanceY = transform.position.y - player.position.y;

        defaultLocalPos = cam.localPosition;
        currentLocalPos = defaultLocalPos;

        Debug.Log("Default Local Position : " + defaultLocalPos);

        // Ye dekho Wall layer exist karti hai ya nahi
        Debug.Log("Wall Layer Index : " + LayerMask.NameToLayer("Wall"));
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.V)) SwitchCameraPosition();
        if (Input.GetKeyDown(KeyCode.C)) clickToMoveCamera = !clickToMoveCamera;

        if (!isAlternatePosition) transform.position = player.position + Vector3.up * offsetDistanceY;

        if (!(clickToMoveCamera && !Input.GetMouseButton(1)))
        {
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY = Mathf.Clamp(mouseY + Input.GetAxis("Mouse Y") * sensitivity, cameraLimit.x, cameraLimit.y);
            transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        }

        if (!isAlternatePosition) HandleCollision();
    }

    void HandleCollision()
    {
        Vector3 origin = transform.position;
        Vector3 desired = transform.TransformPoint(defaultLocalPos);

        Debug.DrawLine(origin, desired, Color.red);

        Vector3 dir = desired - origin;
        float dist = dir.magnitude;

        Vector3 target = defaultLocalPos;

        Debug.Log("--------------------------------");
        Debug.Log("Origin : " + origin);
        Debug.Log("Desired : " + desired);
        Debug.Log("Distance : " + dist);
        Debug.Log("Wall LayerMask Value : " + wallLayers.value);

        if (Physics.SphereCast(origin,
                               sphereRadius,
                               dir.normalized,
                               out RaycastHit hit,
                               dist,
                               wallLayers,
                               QueryTriggerInteraction.Ignore))
        {
            Debug.Log("HIT WALL : " + hit.collider.name);
            Debug.Log("Hit Layer : " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            Debug.Log("Hit Distance : " + hit.distance);

            Debug.DrawRay(hit.point, hit.normal, Color.green);

            Vector3 safe = hit.point - dir.normalized * wallOffset;
            target = transform.InverseTransformPoint(safe);
        }
        else
        {
            Debug.Log("NO HIT");
        }

        currentLocalPos = Vector3.Lerp(currentLocalPos,
                                       target,
                                       Time.deltaTime * smooth);

        cam.localPosition = currentLocalPos;
    }

    void SwitchCameraPosition()
    {
        if (alternateCameraPosition == null) return;
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
