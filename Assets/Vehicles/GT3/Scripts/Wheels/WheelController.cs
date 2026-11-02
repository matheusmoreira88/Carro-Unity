
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WheelCollider))]
[DisallowMultipleComponent]
public class WheelController : MonoBehaviour
{
    #region Components

    private WheelCollider wheel;
    private WheelHit groundHit;

    #endregion

    #region Debug Settings

    [Header("Developer Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool showLabels = true;
    [SerializeField] private bool showSlipVectors = true;

    [Header("Visualization Size")]
    [Min(0.01f)]
    [SerializeField] private float visualWidth = 0.26f;

    [Min(0.01f)]
    [SerializeField] private float slipVectorScale = 0.7f;

    #endregion

    #region Telemetry

    public bool IsGrounded { get; private set; }
    public float WheelRPM { get; private set; }
    public float ForwardSlip { get; private set; }
    public float SidewaysSlip { get; private set; }
    public float ContactForce { get; private set; }
    public float SuspensionCompression { get; private set; }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
    }

    private void FixedUpdate()
    {
        UpdateTelemetry();
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!showGizmos)
            return;

        DrawWheelDiagnostics();
#endif
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        wheel = GetComponent<WheelCollider>();
    }

    #endregion

    #region Telemetry Updates

    private void UpdateTelemetry()
    {
        if (wheel == null)
            return;

        WheelRPM = wheel.rpm;

        UpdateGroundContact();
        UpdateSuspension();
    }

    private void UpdateGroundContact()
    {
        IsGrounded = wheel.GetGroundHit(out groundHit);

        if (!IsGrounded)
        {
            ForwardSlip = 0f;
            SidewaysSlip = 0f;
            ContactForce = 0f;
            return;
        }

        ForwardSlip = groundHit.forwardSlip;
        SidewaysSlip = groundHit.sidewaysSlip;
        ContactForce = groundHit.force;
    }

    private void UpdateSuspension()
    {
        if (!IsGrounded)
        {
            SuspensionCompression = 0f;
            return;
        }

        wheel.GetWorldPose(
            out Vector3 center,
            out _
        );

        float distance = Vector3.Dot(
            transform.position - center,
            transform.up
        );

        float travel = Mathf.Max(
            wheel.suspensionDistance,
            0.001f
        );

        // Estimativa da compressao entre 0 e 1.
        SuspensionCompression = Mathf.Clamp01(
            1f - distance / travel
        );
    }

    #endregion

    #region Wheel Visualization

    private void DrawWheelDiagnostics()
    {
        WheelCollider wc = GetComponent<WheelCollider>();

        if (wc == null)
            return;

        wc.GetWorldPose(
            out Vector3 center,
            out _
        );

        Quaternion steeringRotation =
            transform.rotation *
            Quaternion.Euler(0f, wc.steerAngle, 0f);

        DrawWheelOutline(wc, center, steeringRotation);
        DrawSuspension(center);

        if (!Application.isPlaying || !IsGrounded)
            return;

        DrawGroundContact();

        if (showSlipVectors)
            DrawSlipVectors();

        if (showLabels)
            DrawTelemetryLabel(wc, center);
    }

    private void DrawWheelOutline(
        WheelCollider wc,
        Vector3 center,
        Quaternion rotation)
    {
        float slip = Mathf.Max(
            Mathf.Abs(ForwardSlip),
            Mathf.Abs(SidewaysSlip)
        );

        Gizmos.color = Application.isPlaying &&
                       IsGrounded && slip > 0.45f
            ? Color.red
            : new Color(1f, 0.55f, 0f);

        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        Vector3 forward = rotation * Vector3.forward;

        float halfWidth = visualWidth * 0.5f;

        DrawCircle(
            center - right * halfWidth,
            up,
            forward,
            wc.radius
        );

        DrawCircle(
            center + right * halfWidth,
            up,
            forward,
            wc.radius
        );
    }

    private void DrawCircle(
        Vector3 center,
        Vector3 up,
        Vector3 forward,
        float radius)
    {
        const int segments = 32;

        for (int i = 0; i < segments; i++)
        {
            float angleA =
                i * Mathf.PI * 2f / segments;

            float angleB =
                (i + 1) * Mathf.PI * 2f / segments;

            Vector3 pointA = center + radius *
                (up * Mathf.Cos(angleA) +
                 forward * Mathf.Sin(angleA));

            Vector3 pointB = center + radius *
                (up * Mathf.Cos(angleB) +
                 forward * Mathf.Sin(angleB));

            Gizmos.DrawLine(pointA, pointB);
        }
    }

    #endregion

    #region Suspension Visualization

    private void DrawSuspension(Vector3 center)
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            transform.position,
            center
        );
    }

    #endregion

    #region Contact Visualization

    private void DrawGroundContact()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(
            groundHit.point,
            0.045f
        );

        Gizmos.DrawLine(
            groundHit.point,
            groundHit.point + groundHit.normal * 0.3f
        );
    }

    #endregion

    #region Slip Visualization

    private void DrawSlipVectors()
    {
        DrawSlipVector(
            groundHit.point,
            groundHit.forwardDir,
            ForwardSlip,
            Color.yellow
        );

        DrawSlipVector(
            groundHit.point,
            groundHit.sidewaysDir,
            SidewaysSlip,
            Color.red
        );
    }

    private void DrawSlipVector(
        Vector3 origin,
        Vector3 direction,
        float slip,
        Color color)
    {
        Gizmos.color = color;

        float length = Mathf.Clamp(
            slip * slipVectorScale,
            -1.5f,
            1.5f
        );

        Vector3 end = origin + direction * length;

        Gizmos.DrawLine(origin, end);
        Gizmos.DrawSphere(end, 0.025f);
    }

    #endregion

    #region Debug Labels

    private void DrawTelemetryLabel(
        WheelCollider wc,
        Vector3 center)
    {
#if UNITY_EDITOR
        Vector3 labelPosition =
            center + Vector3.up * (wc.radius + 0.22f);

        string label =
            $"{name}\n" +
            $"RPM: {WheelRPM:0}\n" +
            $"Load: {ContactForce:0} N\n" +
            $"Slip F: {ForwardSlip:F2}\n" +
            $"Slip S: {SidewaysSlip:F2}\n" +
            $"Susp: {SuspensionCompression:P0}";

        Handles.Label(labelPosition, label);
#endif
    }

    #endregion
}
