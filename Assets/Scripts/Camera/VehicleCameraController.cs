
using UnityEngine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class VehicleCameraController : MonoBehaviour
{
    #region References

    [Header("Vehicle")]
    [SerializeField] private Rigidbody targetVehicle;

    #endregion

    #region Camera Settings

    [Header("Camera Position")]
    [SerializeField] private Vector3 followOffset =
        new Vector3(0f, 3f, -7f);

    [SerializeField] private float lookAtHeight = 0.5f;

    [Header("Camera Smoothing")]
    [Min(0.01f)]
    [SerializeField] private float positionSmoothTime = 0.2f;

    [Min(0f)]
    [SerializeField] private float rotationSpeed = 8f;

    #endregion

    #region Runtime

    private Vector3 followVelocity;

    #endregion

    #region Unity Methods

    private void Start()
    {
        InitializeCamera();
    }

    private void LateUpdate()
    {
        if (targetVehicle == null)
            return;

        FollowVehicle();
        RotateCamera();
    }

    #endregion

    #region Initialization

    private void InitializeCamera()
    {
        if (targetVehicle == null)
        {
            Debug.LogWarning(
                "VehicleCameraController: Nenhum veiculo configurado!",
                this
            );

            return;
        }

        SnapToVehicle();
    }

    #endregion

    #region Vehicle Management

    public void SetTarget(Rigidbody vehicle)
    {
        targetVehicle = vehicle;
        followVelocity = Vector3.zero;

        if (targetVehicle != null)
            SnapToVehicle();
    }

    #endregion

    #region Camera Position

    private Quaternion GetVehicleHeading()
    {
        // Ignora inclinacao lateral e longitudinal do carro.
        Vector3 forward = Vector3.ProjectOnPlane(
            targetVehicle.transform.forward,
            Vector3.up
        );

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        return Quaternion.LookRotation(
            forward.normalized,
            Vector3.up
        );
    }

    private Vector3 GetDesiredPosition()
    {
        Quaternion heading = GetVehicleHeading();

        return targetVehicle.position
            + heading * followOffset;
    }

    private void FollowVehicle()
    {
        Vector3 desiredPosition = GetDesiredPosition();

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            positionSmoothTime
        );
    }

    #endregion

    #region Camera Rotation

    private Vector3 GetLookAtPosition()
    {
        return targetVehicle.position
            + Vector3.up * lookAtHeight;
    }

    private void RotateCamera()
    {
        Vector3 direction =
            GetLookAtPosition() - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        float t = 1f - Mathf.Exp(
            -rotationSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            t
        );
    }

    #endregion

    #region Camera Snap

    private void SnapToVehicle()
    {
        transform.position = GetDesiredPosition();
        followVelocity = Vector3.zero;

        Vector3 direction =
            GetLookAtPosition() - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(
                direction,
                Vector3.up
            );
        }
    }

    #endregion
}
