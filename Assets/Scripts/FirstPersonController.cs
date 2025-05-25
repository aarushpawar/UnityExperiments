using System.Security.Permissions;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {
    [Header("Movement")]
    public float speed = 6f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 1000f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;
    private bool isGrounded;

    void Start() {
        playerCamera = GameObject.Find("Main Camera").transform;
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {

        // settings
        MenuScript options = GameObject.Find("Menu").GetComponent<MenuScript>();

        if (options.menuEnabled) {
            Cursor.lockState = CursorLockMode.None;
            mouseSensitivity = 0f;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            mouseSensitivity = options.sensitivity * 10000f;
        }

        // camera stuff
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.Rotate(Vector3.up * mouseX);
        playerCamera.localRotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
        playerCamera.position = transform.position + Vector3.up * 0.5f;

        // movement
        Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        controller.Move(move * speed * Time.deltaTime);

        // gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // jumping
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        
        // time travel
        if (Input.GetKeyDown(KeyCode.F)) Rewind.rewindInput = true;
    }
}


