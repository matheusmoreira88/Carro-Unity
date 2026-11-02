using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class GT3Controller : MonoBehaviour
{
    #region Components

    private Rigidbody rb;
    private GT3Engine engine;
    private GT3Transmission transmission;

    [Header("Wheels")]
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider rearLeft;
    [SerializeField] private WheelCollider rearRight;

    #endregion

    #region Settings

    [Header("Brakes")]
    [SerializeField] private float brakeForce = 2500f;

    [Header("Steering")]
    [SerializeField] private float maxSteerAngle = 25f;
    [SerializeField] private bool useAckermannSteering = true;
    [SerializeField] private float wheelBase = 2.8f;
    [SerializeField] private float frontTrack = 2.0f;

    [Header("Center of Mass")]
    [SerializeField] private Vector3 centerOfMass =
        new Vector3(0f, -0.25f, 0f);

    #endregion

    #region Input Variables

    private float throttle;
    private float steering;
    private bool braking;

    public bool IsCoasting => throttle <= 0.01f && !braking;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        transmission.UpdateShifting(Time.fixedDeltaTime);

        UpdateEngine();
        UpdateTransmission();

        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        engine = GetComponent<GT3Engine>();
        transmission = GetComponent<GT3Transmission>();

        rb.centerOfMass = centerOfMass;
    }

    private void ValidateComponents()
    {
        if (engine != null &&
            transmission != null &&
            transmission.enabled &&
            frontLeft != null &&
            frontRight != null &&
            rearLeft != null &&
            rearRight != null)
        {
            return;
        }

        Debug.LogError(
            "GT3Controller: Componentes nao configurados!",
            this
        );

        enabled = false;
    }

    #endregion

    #region Input Handling

    private void ReadInput()
    {
#if ENABLE_INPUT_SYSTEM

        Keyboard kb = Keyboard.current;

        if (kb == null)
        {
            ResetInput();
            return;
        }

        throttle = kb.wKey.isPressed ? 1f : 0f;

        steering =
            (kb.dKey.isPressed ? 1f : 0f) -
            (kb.aKey.isPressed ? 1f : 0f);

        braking = kb.sKey.isPressed ||
                  kb.spaceKey.isPressed;

        if (!transmission.IsAutomatic)
        {
            if (kb.eKey.wasPressedThisFrame)
                transmission.ShiftUp();

            if (kb.qKey.wasPressedThisFrame)
                transmission.ShiftDown();
        }

#elif ENABLE_LEGACY_INPUT_MANAGER

        throttle = Input.GetKey(KeyCode.W) ? 1f : 0f;

        steering =
            (Input.GetKey(KeyCode.D) ? 1f : 0f) -
            (Input.GetKey(KeyCode.A) ? 1f : 0f);

        braking =
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.Space);

        if (!transmission.IsAutomatic)
        {
            if (Input.GetKeyDown(KeyCode.E))
                transmission.ShiftUp();

            if (Input.GetKeyDown(KeyCode.Q))
                transmission.ShiftDown();
        }

#else

        ResetInput();

#endif
    }

    private void ResetInput()
    {
        throttle = 0f;
        steering = 0f;
        braking = false;
    }

    #endregion

    #region Engine

    private void UpdateEngine()
    {
        float wheelRPM =
            (rearLeft.rpm + rearRight.rpm) * 0.5f;

        engine.UpdateRPM(
            wheelRPM,
            transmission.TotalRatio
        );
    }

    private void ApplyMotor()
    {
        float wheelTorque = 0f;

        if (!braking && !transmission.IsShifting)
        {
            float engineTorque =
                engine.GetEngineTorque(throttle);

            wheelTorque =
                transmission.GetWheelTorque(engineTorque);
        }

        rearLeft.motorTorque = wheelTorque;
        rearRight.motorTorque = wheelTorque;
    }

    #endregion

    #region Transmission

    private void UpdateTransmission()
    {
        transmission.UpdateAutomaticShifting(
            engine.CurrentRPM,
            throttle
        );
    }

    #endregion

    #region Steering

    private void ApplySteering()
    {
        if (!useAckermannSteering)
        {
            float angle = steering * maxSteerAngle;

            frontLeft.steerAngle = angle;
            frontRight.steerAngle = angle;
            return;
        }

        ApplyAckermannSteering();
    }

    private void ApplyAckermannSteering()
    {
        float steerInput = Mathf.Clamp(steering, -1f, 1f);

        if (Mathf.Abs(steerInput) < 0.001f)
        {
            frontLeft.steerAngle = 0f;
            frontRight.steerAngle = 0f;
            return;
        }

        float baseAngleDeg = Mathf.Abs(steerInput) * maxSteerAngle;
        float baseAngleRad = baseAngleDeg * Mathf.Deg2Rad;

        float turnRadius = wheelBase / Mathf.Tan(baseAngleRad);

        float innerDenominator = Mathf.Max(0.01f, turnRadius - frontTrack * 0.5f);
        float outerDenominator = Mathf.Max(0.01f, turnRadius + frontTrack * 0.5f);

        float innerAngle = Mathf.Atan(wheelBase / innerDenominator) * Mathf.Rad2Deg;
        float outerAngle = Mathf.Atan(wheelBase / outerDenominator) * Mathf.Rad2Deg;

        if (steerInput > 0f)
        {
            // Curva para a direita
            frontRight.steerAngle = innerAngle;
            frontLeft.steerAngle = outerAngle;
        }
        else
        {
            // Curva para a esquerda
            frontLeft.steerAngle = -innerAngle;
            frontRight.steerAngle = -outerAngle;
        }
    }

    #endregion

    #region Braking

    private void ApplyBrakes()
    {
        float torque = braking ? brakeForce : 0f;

        frontLeft.brakeTorque = torque;
        frontRight.brakeTorque = torque;

        rearLeft.brakeTorque = torque;
        rearRight.brakeTorque = torque;
    }

    #endregion
}