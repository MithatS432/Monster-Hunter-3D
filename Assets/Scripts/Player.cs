using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private CharacterController controller;
    private Animator anim;
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
    public AudioClip swordSound;
    private bool inWater = false;
    public AudioClip fireBallSound;


    public float stepInterval = 0.5f;
    private float stepTimer = 0f;

    public Button exitGameButton;
    public bool isGamePaused = false;

    public GameObject speacialPower;
    private int superPowerLeft = 5;
    public TextMeshProUGUI superPowerText;
    public bool isUseSuperPower = true;

    public Image healthBar;
    public float maxHealt = 400f;
    public float currentHealth;
    public TextMeshProUGUI enemyCountText;
    public int enemyCount = 0;
    public bool enemyDeadSuperPower;
    int attackIndex = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
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
        currentHealth = maxHealt;
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
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Attack" + attackIndex);

            if (swordSound != null && footstepSource != null)
            {
                footstepSource.PlayOneShot(swordSound);
            }

            attackIndex = (attackIndex + 1) % 2;
        }


        if (Input.GetKeyDown(KeyCode.F) && isUseSuperPower && superPowerLeft > 0)
        {
            SuperPower();
            Vector3 spawnPos = playerCamera.position + playerCamera.forward * 1f;
            Quaternion rot = Quaternion.LookRotation(playerCamera.forward);

            GameObject superPower = Instantiate(speacialPower, spawnPos, rot);
            AudioSource.PlayClipAtPoint(fireBallSound, transform.position, 1f);
            Destroy(superPower, 2f);
        }
        if (enemyCount == 5 && !enemyDeadSuperPower)
        {
            enemyDeadSuperPower = true;
            superPowerLeft += 5;
            superPowerText.text = "Left:" + superPowerLeft.ToString();
        }
    }
    void SuperPower()
    {
        superPowerLeft--;
        superPowerText.text = "Left:" + superPowerLeft.ToString();
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
    public void GetDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealt);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        float normalized = currentHealth / maxHealt;
        healthBar.fillAmount = normalized;
    }
    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
