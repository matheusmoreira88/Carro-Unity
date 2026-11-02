
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class GT3Controller : MonoBehaviour
{
    #region Components

    private Rigidbody rb;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider rearLeft;
    [SerializeField] private WheelCollider rearRight;

    #endregion

    #region Vehicle Settings

    [Header("Engine")]
    [SerializeField] private float motorTorque = 900f;

    [Header("Brakes")]
    [SerializeField] private float brakeForce = 2500f;

    [Header("Steering")]
    [SerializeField] private float maxSteerAngle = 25f;

    [Header("Center of Mass")]
    [SerializeField]
    private Vector3 centerOfMass = new Vector3(0f, -0.25f, 0f);

    #endregion

    #region Input Variables

    private float throttle;
    private float steering;
    private bool braking;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        ConfigureRigidbody();
        ValidateWheels();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        if (!enabled)
            return;

        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
    }

    #endregion

    #region Initialization

    private void ConfigureRigidbody()
    {
        rb.centerOfMass = centerOfMass;
    }

    private void ValidateWheels()
    {
        if (frontLeft != null &&
            frontRight != null &&
            rearLeft != null &&
            rearRight != null)
        {
            return;
        }

        Debug.LogError(
            "GT3Controller: WheelColliders nao configurados!",
            this
        );

        enabled = false;
    }

    #endregion

    #region Input

    private void ReadInput()
    {
#if ENABLE_INPUT_SYSTEM

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            ResetInput();
            return;
        }

        throttle = keyboard.wKey.isPressed ? 1f : 0f;

        steering =
            (keyboard.dKey.isPressed ? 1f : 0f) -
            (keyboard.aKey.isPressed ? 1f : 0f);

        braking =
            keyboard.sKey.isPressed ||
            keyboard.spaceKey.isPressed;

#elif ENABLE_LEGACY_INPUT_MANAGER

        throttle = Input.GetKey(KeyCode.W) ? 1f : 0f;

        steering =
            (Input.GetKey(KeyCode.D) ? 1f : 0f) -
            (Input.GetKey(KeyCode.A) ? 1f : 0f);

        braking =
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.Space);

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

    #region Steering

    private void ApplySteering()
    {
        float steerAngle = steering * maxSteerAngle;

        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;
    }

    #endregion

    #region Engine

    private void ApplyMotor()
    {
        float torque = braking ? 0f : throttle * motorTorque;

        // Tracao traseira (RWD).
        rearLeft.motorTorque = torque;
        rearRight.motorTorque = torque;
    }

    #endregion

    #region Braking

    private void ApplyBrakes()
    {
        float appliedBrake = braking ? brakeForce : 0f;

        frontLeft.brakeTorque = appliedBrake;
        frontRight.brakeTorque = appliedBrake;

        rearLeft.brakeTorque = appliedBrake;
        rearRight.brakeTorque = appliedBrake;
    }

    #endregion
}
