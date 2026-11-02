
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class GT3Controller : MonoBehaviour
{
    [Header("Rodas")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    [Header("Motor e freios")]
    public float motorTorque = 900f;
    public float brakeForce = 2500f;

    [Header("Direcao")]
    public float maxSteerAngle = 25f;

    private float throttle;
    private float steering;
    private bool braking;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // Centro de massa um pouco mais baixo.
        rb.centerOfMass = new Vector3(0f, -0.25f, 0f);
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        throttle = kb.wKey.isPressed ? 1f : 0f;

        steering =
            (kb.dKey.isPressed ? 1f : 0f) -
            (kb.aKey.isPressed ? 1f : 0f);

        braking = kb.sKey.isPressed ||
                  kb.spaceKey.isPressed;

#elif ENABLE_LEGACY_INPUT_MANAGER
        throttle = Input.GetKey(KeyCode.W) ? 1f : 0f;

        steering =
            (Input.GetKey(KeyCode.D) ? 1f : 0f) -
            (Input.GetKey(KeyCode.A) ? 1f : 0f);

        braking = Input.GetKey(KeyCode.S) ||
                  Input.GetKey(KeyCode.Space);
#endif
    }

    void FixedUpdate()
    {
        // Direcao nas rodas dianteiras.
        frontLeft.steerAngle = steering * maxSteerAngle;
        frontRight.steerAngle = steering * maxSteerAngle;

        // Tracao traseira.
        float torque = braking ? 0f : throttle * motorTorque;

        rearLeft.motorTorque = torque;
        rearRight.motorTorque = torque;

        // Freios nas quatro rodas.
        float brake = braking ? brakeForce : 0f;

        frontLeft.brakeTorque = brake;
        frontRight.brakeTorque = brake;
        rearLeft.brakeTorque = brake;
        rearRight.brakeTorque = brake;
    }
}
