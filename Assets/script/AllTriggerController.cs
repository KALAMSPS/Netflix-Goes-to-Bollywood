using System.Collections;
using UnityEngine;

public class AllTriggerController : MonoBehaviour
{
    [Header("Trigger")]
    public string TagName;
    private string PlayerTagName = "Player";

    [Header("Door")]
    public Transform door;

    [Header("Rotation Axis")]
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;

    [Header("Rotation Angles")]
    public float initialAngle = 0f;
    public float finalAngle = -150f;

    [Header("Speed")]
    public float rotationSpeed = 1f;

    private bool isOpen = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.tag);
        Debug.Log("other.gameObject.name: " + other.gameObject.name );
        if (other.tag == "Player" )
        {
            StartCoroutine(RotateDoor());
        }
    }

    private IEnumerator RotateDoor()
    {
        if (door == null)
        {
            Debug.LogWarning("Door reference not assigned!");
            yield break;
        }

        float startAngle = isOpen ? finalAngle : initialAngle;
        float targetAngle = isOpen ? initialAngle : finalAngle;

        Vector3 startRotation = door.localEulerAngles;
        Vector3 endRotation = startRotation;

        if (rotateX)
        {
            startRotation.x = startAngle;
            endRotation.x = targetAngle;
        }

        if (rotateY)
        {
            startRotation.y = startAngle;
            endRotation.y = targetAngle;
        }

        if (rotateZ)
        {
            startRotation.z = startAngle;
            endRotation.z = targetAngle;
        }

        float elapsed = 0f;

        while (elapsed < rotationSpeed)
        {
            elapsed += Time.deltaTime;

            Vector3 current = Vector3.Lerp(startRotation, endRotation, elapsed / rotationSpeed);
            door.localRotation = Quaternion.Euler(current);

            yield return null;
        }

        door.localRotation = Quaternion.Euler(endRotation);

        isOpen = !isOpen;
    }
}