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
        offsetDistanceY = transform.position.y - player.position.y;

        defaultLocalPos = cam.localPosition;
        currentLocalPos = defaultLocalPos;
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
        if (Physics.SphereCast(origin,
                               sphereRadius,
                               dir.normalized,
                               out RaycastHit hit,
                               dist,
                               wallLayers,
                               QueryTriggerInteraction.Ignore))
        {
            Vector3 safe = hit.point - dir.normalized * wallOffset;
            target = transform.InverseTransformPoint(safe);
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
