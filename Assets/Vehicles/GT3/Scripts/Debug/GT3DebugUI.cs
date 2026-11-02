
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class GT3DebugUI : MonoBehaviour
{
    #region Components

    [Header("Vehicle References")]
    [SerializeField] private GT3Engine engine;
    [SerializeField] private GT3Transmission transmission;

    private TextMeshProUGUI debugText;

    #endregion

    #region Settings

    [Header("Display Settings")]
    [SerializeField] private bool showRPM = true;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
    }

    private void Update()
    {
        UpdateTelemetry();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        debugText = GetComponent<TextMeshProUGUI>();
    }

    private void ValidateComponents()
    {
        if (engine != null && transmission != null)
            return;

        Debug.LogError(
            "GT3DebugUI: Engine ou Transmission nao conectado!",
            this
        );

        enabled = false;
    }

    #endregion

    #region Telemetry

    private void UpdateTelemetry()
    {
        int gear = transmission.CurrentGear;
        int rpm = Mathf.RoundToInt(engine.CurrentRPM);

        if (showRPM)
        {
            debugText.text = $"G{gear} | {rpm:N0} RPM";
        }
        else
        {
            debugText.text = $"G{gear}";
        }
    }

    #endregion
}
