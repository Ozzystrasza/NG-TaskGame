using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.6f, -4f);

    [Header("Orbit")]
    // distance/zoom removed; use offset.z for behind distance
    public float sensitivity = 200f;
    public float smoothTime = 0.08f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    [Header("Input (optional)")]
    public InputActionReference lookAction;   // Vector2

    Vector3 currentVelocity;
    float yaw = 0f;
    float pitch = 10f;

    void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
    }

    void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            // try to auto-find a CharacterController in scene
            var cc = FindAnyObjectByType<CharacterController>();
            if (cc != null) target = cc.transform;
            if (target == null) return;
        }

        // Read look input via Input System if provided; otherwise try common devices
        Vector2 look = Vector2.zero;
        if (lookAction != null && lookAction.action != null && lookAction.action.enabled)
            look = lookAction.action.ReadValue<Vector2>();
        else
        {
            if (Mouse.current != null) look = Mouse.current.delta.ReadValue();
            else if (Gamepad.current != null) look = Gamepad.current.rightStick.ReadValue();
        }

        // apply rotation (yaw only). pitch remains fixed in inspector.
        float dt = Application.isPlaying ? Time.deltaTime : (1f / 60f);
        yaw += look.x * sensitivity * dt * 0.01f;

        // calculate desired position & rotation (offset.z holds the behind distance)
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rot * offset;

        // smooth move
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 1f - Mathf.Exp(-10f * dt));
    }
}
