
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class SpeedometerUI : MonoBehaviour
{
    #region Components

    [Header("Vehicle")]
    [SerializeField] private Rigidbody activeVehicle;

    private TMP_Text speedText;

    #endregion

    #region Settings

    [Header("Display")]
    [SerializeField] private bool showUnit = true;

    #endregion

    #region Variables

    private const float MpsToKmh = 3.6f;

    private int lastSpeed = -1;
    private bool lastShowUnit;
    private bool lastHadVehicle;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        UpdateSpeedDisplay();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        speedText = GetComponent<TMP_Text>();

        ResetDisplayCache();
        UpdateSpeedDisplay();
    }

    #endregion

    #region Vehicle Management

    public void SetVehicle(Rigidbody vehicle)
    {
        activeVehicle = vehicle;

        ResetDisplayCache();
        UpdateSpeedDisplay();
    }

    private void ResetDisplayCache()
    {
        lastSpeed = -1;
        lastShowUnit = !showUnit;
        lastHadVehicle = false;
    }

    #endregion

    #region Speed Calculation

    private int CalculateSpeedKmh()
    {
        if (activeVehicle == null)
            return 0;

        float speedMs = activeVehicle.linearVelocity.magnitude;
        float speedKmh = speedMs * MpsToKmh;

        return Mathf.RoundToInt(speedKmh);
    }

    #endregion

    #region Display

    private void UpdateSpeedDisplay()
    {
        bool hasVehicle = activeVehicle != null;

        int currentSpeed = hasVehicle
            ? CalculateSpeedKmh()
            : 0;

        if (currentSpeed == lastSpeed &&
            showUnit == lastShowUnit &&
            hasVehicle == lastHadVehicle)
        {
            return;
        }

        UpdateSpeedText(currentSpeed, hasVehicle);

        lastSpeed = currentSpeed;
        lastShowUnit = showUnit;
        lastHadVehicle = hasVehicle;
    }

    private void UpdateSpeedText(int speed, bool hasVehicle)
    {
        string value = hasVehicle
            ? speed.ToString()
            : "--";

        speedText.text = showUnit
            ? $"{value} km/h"
            : value;
    }

    #endregion
}
