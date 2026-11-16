using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace TWIN.Player
{
    /// <summary>
    /// Desktop stand-in for the VR player. Active only when no XR device is running:
    /// WASD / arrows move, mouse looks, left click or E interacts with the Interactable under the crosshair,
    /// Esc frees the cursor, click re-locks it. In VR this component disables itself and XR tracking drives the camera.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class DesktopRig : MonoBehaviour
    {
        public static DesktopRig Current { get; private set; }

        [Header("References")]
        public Camera playerCamera;
        public Transform cameraPivot;
        [Tooltip("Behaviours that only make sense with a headset (TrackedPoseDriver etc). Disabled on desktop.")]
        public Behaviour[] xrOnly;

        [Header("Movement")]
        public float moveSpeed = 2.5f;
        public float lookSensitivity = 0.12f;
        public float eyeHeight = 1.6f;
        public float interactDistance = 3f;
        public LayerMask interactMask = ~0;

        [Header("Optional UI")]
        public TMP_Text promptText;

        static bool s_cursorFree;
        static int s_freeRequests;

        CharacterController _cc;
        float _yaw, _pitch, _verticalVelocity;
        Interactable _lookingAt;
        bool _desktopMode;

        public bool DesktopMode => _desktopMode;
        public Interactable LookingAt => _lookingAt;

        /// <summary>Called by UI (fork vote, end screen) so the mouse can click buttons.</summary>
        public static void SetCursorFree(bool free)
        {
            s_freeRequests = Mathf.Max(0, s_freeRequests + (free ? 1 : -1));
            s_cursorFree = s_freeRequests > 0;
            ApplyCursor();
        }

        static void ApplyCursor()
        {
            if (Current == null || !Current._desktopMode) return;
            Cursor.lockState = s_cursorFree ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = s_cursorFree;
        }

        void Awake()
        {
            Current = this;
            _cc = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (cameraPivot == null && playerCamera != null) cameraPivot = playerCamera.transform;
        }

        void Start()
        {
            _desktopMode = !XRSettings.isDeviceActive;
            if (!_desktopMode)
            {
                Debug.Log("[TWIN] XR device active: DesktopRig off, headset tracking drives the camera.");
                enabled = false;
                return;
            }
            Debug.Log("[TWIN] No XR device: DesktopRig on (WASD + mouse, click/E to interact, Esc for cursor).");
            foreach (var b in xrOnly) if (b != null) b.enabled = false;
            if (cameraPivot != null) cameraPivot.localPosition = new Vector3(0f, eyeHeight, 0f);
            _yaw = transform.eulerAngles.y;
            s_freeRequests = 0;
            s_cursorFree = false;
            ApplyCursor();
        }

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            // Cursor handling
            if (kb != null && kb.escapeKey.wasPressedThisFrame && !s_cursorFree)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            bool cursorLocked = Cursor.lockState == CursorLockMode.Locked;
            if (!cursorLocked && !s_cursorFree && mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                ApplyCursor();
                return;
            }

            // Look
            if (cursorLocked && mouse != null)
            {
                var d = mouse.delta.ReadValue() * lookSensitivity;
                _yaw += d.x;
                _pitch = Mathf.Clamp(_pitch - d.y, -85f, 85f);
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }

            // Move
            Vector2 input = Vector2.zero;
            if (kb != null && !s_cursorFree)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
            }
            input = Vector2.ClampMagnitude(input, 1f);
            var move = transform.right * input.x + transform.forward * input.y;
            float speed = kb != null && kb.leftShiftKey.isPressed ? moveSpeed * 2f : moveSpeed;

            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            _cc.Move((move * speed + Vector3.up * _verticalVelocity) * Time.deltaTime);

            // Interact
            UpdateLookTarget();
            bool interactPressed = (mouse != null && mouse.leftButton.wasPressedThisFrame && cursorLocked)
                                   || (kb != null && kb.eKey.wasPressedThisFrame);
            if (interactPressed && !s_cursorFree && _lookingAt != null) _lookingAt.Interact();
        }

        void UpdateLookTarget()
        {
            Interactable hitInteractable = null;
            if (playerCamera != null)
            {
                var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                if (Physics.Raycast(ray, out var hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
                    hitInteractable = hit.collider.GetComponentInParent<Interactable>();
                if (hitInteractable != null && !hitInteractable.IsAvailable) hitInteractable = null;
            }
            _lookingAt = hitInteractable;
            if (promptText != null)
                promptText.text = _lookingAt != null && !s_cursorFree ? _lookingAt.prompt : "";
        }
    }
}
