using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 2f, 0f);
    public float followSmoothTime = 0.08f;

    [Header("Rotation")]
    public bool holdRightMouseToRotate = true;
    public float sensitivity = 200f;
    public Vector2 pitchLimits = new Vector2(-35f, 70f);

    [Header("Zoom")]
    public bool allowZoom = true;
    public float minDistance = 2f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;

    [Header("Alternate Camera")]
    public Transform alternateCameraPosition;

    private float yaw;
    private float pitch;
    private float distance = 4f;

    private Vector3 currentVelocity;

    private bool alternateView;

    private Vector3 storedPos;
    private Quaternion storedRot;

    void Start()
    {
        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        if (Input.GetKeyDown(KeyCode.V))
            ToggleView();

        if (alternateView)
            return;

        RotateCamera();
        ZoomCamera();
        FollowCamera();
    }

    void RotateCamera()
    {
        if (holdRightMouseToRotate && !Input.GetMouseButton(1))
            return;

        yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        pitch -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
    }

    void ZoomCamera()
    {
        if (!allowZoom)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void FollowCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 targetPosition =
            player.position +
            offset -
            rotation * Vector3.forward * distance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            followSmoothTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            15f * Time.deltaTime);
    }

    void ToggleView()
    {
        if (alternateCameraPosition == null)
            return;

        if (!alternateView)
        {
            storedPos = transform.position;
            storedRot = transform.rotation;

            transform.position = alternateCameraPosition.position;
            transform.rotation = alternateCameraPosition.rotation;
        }
        else
        {
            transform.position = storedPos;
            transform.rotation = storedRot;
        }

        alternateView = !alternateView;
    }
}