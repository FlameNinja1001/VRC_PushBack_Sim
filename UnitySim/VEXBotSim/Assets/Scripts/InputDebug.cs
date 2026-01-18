using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebug : MonoBehaviour
{
    DriverControls controls;

    public Vector2 left;
    public Vector2 right;

    public bool a_button;
    public bool l1_button;
    public bool l2_button;

    public bool r1_button;
    public bool r2_button;

    public bool down_button;

    void Awake()
    {
        controls = new DriverControls();
    }

    void OnEnable()
    {
        controls.Driver.Enable();
    }

    void OnDisable()
    {
        controls.Driver.Disable();
    }

    void Update()
    {
        left  = controls.Driver.LeftStick.ReadValue<Vector2>();
        right = controls.Driver.RightStick.ReadValue<Vector2>();

        a_button = controls.Driver.A_Button.triggered;
        l1_button = controls.Driver.L1_Button.triggered;
        r1_button = controls.Driver.R1_Button.triggered;
        l2_button = controls.Driver.L2_Button.triggered;
        r2_button = controls.Driver.R2_Button.triggered;
        down_button = controls.Driver.Down_Dpad.triggered;


        Debug.Log($"a button: {a_button}, l1 button: {l1_button}, l2 button: {l2_button} r1 button: {r1_button}, r2 button: {r2_button}, down dpad button: {down_button}");
        
    }
}

