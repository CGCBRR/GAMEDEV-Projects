using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;   // drag Main Camera here
    public float maxLookAngle = 80f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock and hide the cursor for FPS-style control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            Debug.LogWarning("PlayerController: cameraTransform was not assigned. Using Camera.main automatically.");
        }
    }

    void Update()
    {
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Yaw: rotate the whole player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Pitch: rotate the camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Gravity so the player sticks to the ground
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}