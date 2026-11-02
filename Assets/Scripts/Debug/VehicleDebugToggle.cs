using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class VehicleDebugToggle : MonoBehaviour
{
    [Header("Toggle")]
    [SerializeField] private bool startVisible = false;

    private WheelDebugVisual[] wheelDebugs;
    private bool currentState;

    private void Awake()
    {
        wheelDebugs = GetComponentsInChildren<WheelDebugVisual>(true);
        SetDebugState(startVisible);
    }

    private void Update()
    {
        if (WasTogglePressed())
        {
            SetDebugState(!currentState);
        }
    }

    private bool WasTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.f3Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.F3);
#else
        return false;
#endif
    }

    private void SetDebugState(bool visible)
    {
        currentState = visible;

        if (wheelDebugs == null)
            return;

        for (int i = 0; i < wheelDebugs.Length; i++)
        {
            if (wheelDebugs[i] != null)
                wheelDebugs[i].SetVisible(visible);
        }
    }
}