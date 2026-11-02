
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelVisualSync : MonoBehaviour
{
    private WheelCollider wheelCollider;
    private Transform wheelMesh;

    void Awake()
    {
        wheelCollider = GetComponent<WheelCollider>();

        if (transform.childCount == 0)
        {
            Debug.LogError("Roda visual nao encontrada!", this);
            enabled = false;
            return;
        }

        wheelMesh = transform.GetChild(0);
    }

    void LateUpdate()
    {
        wheelCollider.GetWorldPose(
            out Vector3 position,
            out Quaternion rotation
        );

        // Corrige a orientacao do Cylinder da Unity.
        Quaternion correction = Quaternion.Euler(0f, 0f, 90f);

        wheelMesh.SetPositionAndRotation(
            position,
            rotation * correction
        );
    }
}
