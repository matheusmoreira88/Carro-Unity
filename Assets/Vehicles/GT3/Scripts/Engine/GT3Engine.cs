
using UnityEngine;

public class GT3Engine : MonoBehaviour
{
    #region Settings

    [Header("Engine")]
    [SerializeField] private float idleRPM = 1000f;
    [SerializeField] private float maxRPM = 8000f;
    [SerializeField] private float maxTorque = 350f;

    [Header("RPM Response")]
    [SerializeField] private float rpmResponse = 14000f;

    [Header("Torque Curve")]
    [SerializeField] private AnimationCurve torqueCurve =
        new AnimationCurve(
            new Keyframe(0f, 0.50f),
            new Keyframe(0.25f, 0.75f),
            new Keyframe(0.55f, 1f),
            new Keyframe(0.85f, 0.90f),
            new Keyframe(1f, 0.60f)
        );

    #endregion

    #region Runtime

    [Header("Debug")]
    [SerializeField] private float currentRPM;

    public float CurrentRPM => currentRPM;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeEngine();
    }

    #endregion

    #region Initialization

    private void InitializeEngine()
    {
        currentRPM = idleRPM;
    }

    #endregion

    #region RPM

    public void UpdateRPM(float wheelRPM, float totalRatio)
    {
        float targetRPM = Mathf.Abs(wheelRPM) * totalRatio;

        targetRPM = Mathf.Clamp(
            targetRPM,
            idleRPM,
            maxRPM
        );

        currentRPM = Mathf.MoveTowards(
            currentRPM,
            targetRPM,
            rpmResponse * Time.fixedDeltaTime
        );
    }

    #endregion

    #region Torque

    public float GetEngineTorque(float throttle)
    {
        // Corta o torque no limitador.
        if (currentRPM >= maxRPM - 30f)
            return 0f;

        float normalizedRPM = Mathf.InverseLerp(
            idleRPM,
            maxRPM,
            currentRPM
        );

        float torqueMultiplier =
            Mathf.Max(0f, torqueCurve.Evaluate(normalizedRPM));

        return maxTorque
            * torqueMultiplier
            * Mathf.Clamp01(throttle);
    }

    #endregion
}
