using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simpel rondlopen op desktop: WASD/pijltjes lopen, Shift rennen, muis kijken.
/// Esc maakt de muis vrij, klik pakt hem weer. Zet dit op een object met een CharacterController;
/// de camera is een kind-object (ooghoogte).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleWalker : MonoBehaviour
{
    public Transform cameraTransform;
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float mouseSensitivity = 0.12f;

    CharacterController _cc;
    float _yaw, _pitch, _fall;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        _yaw = transform.eulerAngles.y;
        if (cameraTransform != null) _pitch = cameraTransform.localEulerAngles.x > 180f ? cameraTransform.localEulerAngles.x - 360f : cameraTransform.localEulerAngles.x;
        LockCursor(true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame) LockCursor(false);
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) LockCursor(true);

        // Kijken (alleen als de muis gevangen is)
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            var d = mouse.delta.ReadValue() * mouseSensitivity;
            _yaw += d.x;
            _pitch = Mathf.Clamp(_pitch - d.y, -85f, 85f);
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (cameraTransform != null) cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        // Lopen
        Vector2 input = Vector2.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
        input = Vector2.ClampMagnitude(input, 1f);

        float speed = kb.leftShiftKey.isPressed ? runSpeed : walkSpeed;
        Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;

        if (_cc.isGrounded && _fall < 0f) _fall = -1f;
        _fall += Physics.gravity.y * Time.deltaTime;
        move.y = _fall;
        _cc.Move(move * Time.deltaTime);
    }

    static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
