
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelVisualSync : MonoBehaviour
{
    #region Components

    private WheelCollider wheelCollider;

    [Header("Visual Wheel")]
    [SerializeField] private Transform wheelMesh;

    #endregion

    #region Settings

    // Ajuste para os cilindros padrao da Unity.
    private static readonly Quaternion rotationOffset =
        Quaternion.Euler(0f, 0f, 90f);

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
    }

    private void LateUpdate()
    {
        UpdateWheelVisual();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        wheelCollider = GetComponent<WheelCollider>();

        // Usa o primeiro filho se nada foi configurado.
        if (wheelMesh == null && transform.childCount > 0)
        {
            wheelMesh = transform.GetChild(0);
        }

        if (wheelMesh == null)
        {
            Debug.LogError(
                "WheelVisualSync: Roda visual nao encontrada!",
                this
            );

            enabled = false;
        }
    }

    #endregion

    #region Visual Updates

    private void UpdateWheelVisual()
    {
        if (wheelMesh == null)
            return;

        wheelCollider.GetWorldPose(
            out Vector3 position,
            out Quaternion rotation
        );

        wheelMesh.SetPositionAndRotation(
            position,
            rotation * rotationOffset
        );
    }

    #endregion
}
