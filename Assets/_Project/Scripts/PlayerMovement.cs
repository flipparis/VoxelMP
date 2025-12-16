using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = -18f;
    public float jumpHeight = 1.2f;

    CharacterController cc;
    Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    public override void OnStartLocalPlayer()
    {
        // Make the local player easy to spot
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null) renderer.material.color = Color.white;

        // Optional: simple camera follow
        if (Camera.main != null)
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 1.6f, -3.5f);
            Camera.main.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        cc.Move(move.normalized * moveSpeed * Time.deltaTime);

        bool grounded = cc.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        if (grounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);

        // Mouse look (simple yaw only)
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * mouseX * 3.0f);
    }
}
