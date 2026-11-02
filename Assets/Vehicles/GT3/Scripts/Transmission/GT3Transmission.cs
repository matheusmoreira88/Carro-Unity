
using UnityEngine;

public class GT3Transmission : MonoBehaviour
{
    #region Gear Settings

    [Header("Gear Ratios")]
    [SerializeField] private float[] gearRatios =
    {
        2.80f,
        2.10f,
        1.65f,
        1.32f,
        1.08f,
        0.90f
    };

    [Header("Drivetrain")]
    [SerializeField] private float finalDrive = 3.20f;

    [Range(0f, 1f)]
    [SerializeField] private float efficiency = 0.85f;

    #endregion

    #region Transmission Settings

    [Header("Transmission Mode")]
    [SerializeField] private bool automatic = true;

    [Header("Automatic Shifting")]
    [SerializeField] private float upshiftRPM = 6800f;
    [SerializeField] private float downshiftRPM = 2800f;

    [Range(0f, 1f)]
    [SerializeField] private float minUpshiftThrottle = 0.1f;

    [Header("Shift Timing")]
    [SerializeField] private float shiftDuration = 0.15f;
    [SerializeField] private float shiftCooldown = 0.40f;

    #endregion

    #region Runtime

    [Header("Debug")]
    [SerializeField] private int currentGear = 1;

    private float shiftTimer;
    private float cooldownTimer;

    public int CurrentGear => currentGear;
    public bool IsAutomatic => automatic;
    public bool IsShifting => shiftTimer > 0f;

    public float TotalRatio =>
        gearRatios[currentGear - 1] * finalDrive;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeTransmission();
    }

    #endregion

    #region Initialization

    private void InitializeTransmission()
    {
        if (gearRatios == null || gearRatios.Length == 0)
        {
            Debug.LogError(
                "GT3Transmission: Nenhuma marcha configurada!",
                this
            );

            enabled = false;
            return;
        }

        currentGear = Mathf.Clamp(
            currentGear,
            1,
            gearRatios.Length
        );

        shiftTimer = 0f;
        cooldownTimer = 0f;
    }

    #endregion

    #region Automatic Transmission

    public void UpdateAutomaticShifting(
        float engineRPM,
        float throttle
    )
    {
        if (!automatic || IsShifting || cooldownTimer > 0f)
            return;

        if (ShouldShiftUp(engineRPM, throttle))
        {
            ShiftUp();
        }
        else if (ShouldShiftDown(engineRPM))
        {
            ShiftDown();
        }
    }

    private bool ShouldShiftUp(float rpm, float throttle)
    {
        return currentGear < gearRatios.Length
            && rpm >= upshiftRPM
            && throttle >= minUpshiftThrottle;
    }

    private bool ShouldShiftDown(float rpm)
    {
        return currentGear > 1
            && rpm <= downshiftRPM;
    }

    #endregion

    #region Gear Shifting

    public void ShiftUp()
    {
        if (!CanShift() || currentGear >= gearRatios.Length)
            return;

        currentGear++;
        StartShift();
    }

    public void ShiftDown()
    {
        if (!CanShift() || currentGear <= 1)
            return;

        currentGear--;
        StartShift();
    }

    private bool CanShift()
    {
        return !IsShifting && cooldownTimer <= 0f;
    }

    private void StartShift()
    {
        shiftTimer = Mathf.Max(0f, shiftDuration);

        cooldownTimer = Mathf.Max(
            shiftTimer,
            shiftCooldown
        );
    }

    public void UpdateShifting(float deltaTime)
    {
        shiftTimer = Mathf.Max(
            0f,
            shiftTimer - deltaTime
        );

        cooldownTimer = Mathf.Max(
            0f,
            cooldownTimer - deltaTime
        );
    }

    #endregion

    #region Torque

    public float GetWheelTorque(float engineTorque)
    {
        return engineTorque
            * TotalRatio
            * efficiency
            * 0.5f;
    }

    #endregion
}
