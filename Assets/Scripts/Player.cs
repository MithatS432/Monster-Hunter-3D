using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 6f;
    public float jumpForce = 8f;
    public float gravity = -9.81f;
    public bool isGrounded;
    private Vector3 velocity;
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    private AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public AudioClip[] waterstepClips;
    private bool inWater = false;


    public float stepInterval = 0.5f;
    private float stepTimer = 0f;

    public Button exitGameButton;
    public bool isGamePaused = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        footstepSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        exitGameButton.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        });
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        speed = Input.GetKey(KeyCode.LeftShift) ? 25f : 15f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        HandleMouseLook();

        if (isGrounded && move.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isGamePaused = !isGamePaused;

            exitGameButton.gameObject.SetActive(isGamePaused);

            if (isGamePaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
            }
        }

    }
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    void PlayFootstep()
    {
        AudioClip clip;
        if (inWater && waterstepClips.Length > 0)
        {
            clip = waterstepClips[Random.Range(0, waterstepClips.Length)];
        }
        else if (footstepClips.Length > 0)
        {
            clip = footstepClips[Random.Range(0, footstepClips.Length)];
        }
        else return;

        footstepSource.clip = clip;
        footstepSource.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            inWater = true;
            if (waterstepClips.Length == 0) return;
            int index = Random.Range(0, waterstepClips.Length);
            footstepSource.clip = waterstepClips[index];
            footstepSource.Play();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
            inWater = false;
    }
}
