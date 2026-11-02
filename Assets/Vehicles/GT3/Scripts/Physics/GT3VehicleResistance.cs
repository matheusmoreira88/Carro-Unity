
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GT3VehicleResistance : MonoBehaviour
{
    #region Components

    private Rigidbody rb;
    private GT3Controller controller;
    private GT3Engine engine;
    private GT3Transmission transmission;
    private WheelCollider[] wheels;

    #endregion

    #region Settings

    [Header("Aerodynamics")]
    [SerializeField] private float airDensity = 1.225f;

    // Coeficiente de arrasto multiplicado pela area frontal.
    [SerializeField] private float dragArea = 1.2f;

    [Header("Rolling Resistance")]
    [SerializeField] private float rollingCoefficient = 0.015f;

    [Header("Engine Braking")]
    [SerializeField] private float engineBrakingTorque = 40f;
    [SerializeField] private float wheelRadius = 0.34f;
    [SerializeField] private float rpmReference = 6000f;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
    }

    private void FixedUpdate()
    {
        ApplyResistance();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<GT3Controller>();
        engine = GetComponent<GT3Engine>();
        transmission = GetComponent<GT3Transmission>();

        wheels = GetComponentsInChildren<WheelCollider>();
    }

    private void ValidateComponents()
    {
        if (controller != null &&
            engine != null &&
            transmission != null &&
            wheels.Length > 0)
        {
            return;
        }

        Debug.LogError(
            "GT3VehicleResistance: Componentes ausentes!",
            this
        );

        enabled = false;
    }

    #endregion

    #region Ground Detection

    private bool IsGrounded()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.isGrounded)
                return true;
        }

        return false;
    }

    #endregion

    #region Aerodynamics

    private float CalculateAirResistance(float speed)
    {
        return 0.5f
            * airDensity
            * dragArea
            * speed
            * speed;
    }

    #endregion

    #region Rolling Resistance

    private float CalculateRollingResistance()
    {
        return rollingCoefficient
            * rb.mass
            * Physics.gravity.magnitude;
    }

    #endregion

    #region Engine Braking

    private float CalculateEngineBraking()
    {
        if (!controller.IsCoasting ||
            transmission.IsShifting)
        {
            return 0f;
        }

        float rpmFactor = Mathf.Clamp(
            engine.CurrentRPM / rpmReference,
            0.5f,
            1.2f
        );

        float brakingTorque =
            engineBrakingTorque * rpmFactor;

        return brakingTorque
            * transmission.TotalRatio
            / Mathf.Max(wheelRadius, 0.01f);
    }

    #endregion

    #region Physics

    private void ApplyResistance()
    {
        Vector3 velocity = Vector3.ProjectOnPlane(
            rb.linearVelocity,
            Vector3.up
        );

        float speed = velocity.magnitude;

        if (speed < 0.001f)
            return;

        float resistance = CalculateAirResistance(speed);

        if (IsGrounded())
        {
            resistance += CalculateRollingResistance();
            resistance += CalculateEngineBraking();
        }

        // Evita que a resistencia inverta o movimento.
        float maxForce = rb.mass * speed
            / Time.fixedDeltaTime;

        resistance = Mathf.Min(resistance, maxForce);

        Vector3 force = -velocity.normalized * resistance;

        rb.AddForce(force, ForceMode.Force);
    }

    #endregion
}
