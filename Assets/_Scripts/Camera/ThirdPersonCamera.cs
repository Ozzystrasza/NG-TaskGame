using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.6f, -4f);

    [Header("Orbit")]
    public float sensitivity = 200f;
    public float smoothTime = 0.08f;
    public float pitchMin = -30f;
    public float pitchMax = 60f;

    [Header("Input")]
    public InputActionReference lookAction;

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
            var cc = FindAnyObjectByType<CharacterController>();
            if (cc != null) target = cc.transform;
            if (target == null) return;
        }

        Vector2 look = Vector2.zero;
        if (lookAction != null && lookAction.action != null && lookAction.action.enabled)
            look = lookAction.action.ReadValue<Vector2>();
        else
        {
            if (Mouse.current != null) look = Mouse.current.delta.ReadValue();
            else if (Gamepad.current != null) look = Gamepad.current.rightStick.ReadValue();
        }

        float dt = Application.isPlaying ? Time.deltaTime : (1f / 60f);
        yaw += look.x * sensitivity * dt * 0.01f;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rot * offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 1f - Mathf.Exp(-10f * dt));
    }
}
