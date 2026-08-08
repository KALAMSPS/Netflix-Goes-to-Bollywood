using UnityEngine;

public class NeedleRotation : MonoBehaviour
{
    public Transform needle;
    public bool rotateClockwise = true;

    private const float rotationSpeed = 12f;

    void Update()
    {
        if (needle == null) return;

        float direction = rotateClockwise ? -1f : 1f;

        needle.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime); 
    }
}