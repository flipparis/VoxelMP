using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotorServerAuth : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -18f;
    public float jumpHeight = 1.2f;

    [Header("Look")]
    public float mouseSensitivity = 3.0f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Networking")]
    public float inputSendRate = 60f;

    CharacterController cc;
    Transform camPivot;

    // Client-side view state (smooth)
    float localYaw;
    float localPitch;

    // Server-side input state (authoritative)
    Vector2 serverMoveInput;
    float serverYawAbsolute;
    bool serverJumpQueued;
    float verticalVelocity;

    float nextSendTime;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        camPivot = transform.Find("CameraPivot");
    }

    public override void OnStartLocalPlayer()
    {
        // First-person camera (recommended for interaction)
        if (Camera.main != null && camPivot != null)
        {
            Camera.main.transform.SetParent(camPivot);
            Camera.main.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            Camera.main.transform.localRotation = Quaternion.identity;
        }

        // Initialize local yaw from current body yaw
        localYaw = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // --- Read input locally (instant feel) ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Jump: works even if Input Manager mappings are weird
        bool jumpPressed = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);

        // Update local view (smooth, every frame)
        localYaw += mouseX * mouseSensitivity;
        localPitch -= mouseY * mouseSensitivity;
        localPitch = Mathf.Clamp(localPitch, minPitch, maxPitch);

        ApplyLocalViewToCameraPivot();

        // Send inputs to server at a rate, but ALWAYS send immediately on jump
        bool shouldSend = (Time.time >= nextSendTime) || jumpPressed;

        if (shouldSend)
        {
            float rate = Mathf.Max(1f, inputSendRate);
            nextSendTime = Time.time + (1f / rate);

            CmdSendInput(new Vector2(x, z), localYaw, jumpPressed);
        }
    }

    void LateUpdate()
    {
        // Re-apply after NetworkTransform updates so camera stays smooth
        if (isLocalPlayer)
            ApplyLocalViewToCameraPivot();
    }

    void ApplyLocalViewToCameraPivot()
    {
        if (camPivot == null) return;

        // Body yaw is controlled by server and replicated; camera yaw should follow localYaw smoothly.
        float bodyYaw = transform.eulerAngles.y;

        // Calculate the yaw offset needed so camera faces localYaw even if bodyYaw lags
        float yawOffset = Mathf.DeltaAngle(bodyYaw, localYaw);

        camPivot.localRotation = Quaternion.Euler(localPitch, yawOffset, 0f);
    }

    // ---------------- SERVER ----------------

    [Command]
    void CmdSendInput(Vector2 moveInput, float yawAbsolute, bool jumpPressed)
    {
        serverMoveInput = Vector2.ClampMagnitude(moveInput, 1f);
        serverYawAbsolute = yawAbsolute;

        if (jumpPressed)
            serverJumpQueued = true;
    }

    void FixedUpdate()
    {
        if (!isServer) return;

        // Server sets body yaw directly from the absolute yaw the client is requesting
        transform.rotation = Quaternion.Euler(0f, serverYawAbsolute, 0f);

        // Move (server-authoritative)
        Vector3 move = (transform.right * serverMoveInput.x) + (transform.forward * serverMoveInput.y);
        cc.Move(move.normalized * moveSpeed * Time.fixedDeltaTime);

        // Grounding (stable jump)
        bool grounded = cc.isGrounded;
        if (!grounded)
        {
            Vector3 origin = transform.position + cc.center;
            float castDist = (cc.height * 0.5f) + 0.05f;
            grounded = Physics.SphereCast(origin, cc.radius * 0.95f, Vector3.down, out _, castDist, ~0, QueryTriggerInteraction.Ignore);
        }

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (grounded && serverJumpQueued)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            serverJumpQueued = false;
        }

        verticalVelocity += gravity * Time.fixedDeltaTime;
        cc.Move(new Vector3(0f, verticalVelocity, 0f) * Time.fixedDeltaTime);
    }
}
