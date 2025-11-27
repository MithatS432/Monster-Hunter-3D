using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private Animator anim;
    private AudioSource sound;
    public AudioClip deathClip;
    public AudioClip[] shoutSounds;

    public Image enemyHealthbar;
    public float maxHealth = 50f;
    public float health;
    public float damage = 10f;
    public GameObject bloodEffect;

    public Transform healthbarTarget;
    public Vector2 healthbarOffset = new Vector2(0f, 30f);

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolSpeed = 5f;
    public float chaseSpeed = 10f;
    public float chaseDistance = 10f;

    private NavMeshAgent agent;
    private float attackCooldown = 1f;
    private float lastAttackTime;

    private Transform player;
    private float shoutCooldown = 3f; // Kaç saniyede bir bağırabilir
    private float lastShoutTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sound = GetComponent<AudioSource>();
        health = maxHealth;
        UpdateUI();

        // NavMesh üzerinde değilse Warp ile konumlandır
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // Agent ayarları
        agent.speed = patrolSpeed;
        agent.acceleration = 20f;
        agent.angularSpeed = 360f;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (patrolPoints.Length > 0)
        {
            agent.destination = patrolPoints[currentPatrolIndex].position;
        }
    }

    void LateUpdate()
    {
        UpdateHealthbarPosition();
        HandleMovement();
        UpdateAnimatorSpeed();
    }

    void UpdateHealthbarPosition()
    {
        if (enemyHealthbar == null || healthbarTarget == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(healthbarTarget.position);
        enemyHealthbar.gameObject.SetActive(screenPos.z >= 0f);

        if (!enemyHealthbar.gameObject.activeSelf) return;

        Canvas parentCanvas = enemyHealthbar.canvas;
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            parentCanvas.worldCamera,
            out anchoredPos
        );

        anchoredPos += healthbarOffset;
        enemyHealthbar.rectTransform.anchoredPosition = anchoredPos;
    }

    void HandleMovement()
    {
        if (agent == null || patrolPoints.Length == 0 || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseDistance)
        {
            // Player'ı görünce chase
            anim.SetTrigger("See");

            // Bağırma sesi
            if (sound != null && shoutSounds.Length > 0 && Time.time - lastShoutTime >= shoutCooldown)
            {
                AudioClip clip = shoutSounds[Random.Range(0, shoutSounds.Length)];
                sound.PlayOneShot(clip);
                lastShoutTime = Time.time;
            }

            agent.speed = chaseSpeed;
            agent.stoppingDistance = 0.5f;

            Vector3 chaseTarget = player.position;
            chaseTarget.y = transform.position.y; // yatay düzlem
            agent.destination = chaseTarget;

            // Player'a dön
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            // Devriye
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0f;

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.destination = patrolPoints[currentPatrolIndex].position;
            }
        }
    }


    void UpdateAnimatorSpeed()
    {
        if (anim != null && agent != null)
        {
            // Agent hızı ile animasyon uyumu
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    public void GetDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (bloodEffect)
        {
            GameObject blood = Instantiate(bloodEffect, transform.position, Quaternion.identity);
            Destroy(blood, 1f);
        }
        UpdateUI();
        if (health <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (enemyHealthbar != null)
        {
            enemyHealthbar.fillAmount = health / maxHealth;
        }
    }

    void Die()
    {
        anim.SetTrigger("Die");
        if (sound && deathClip) sound.PlayOneShot(deathClip);
        Destroy(gameObject, 0.8f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time - lastAttackTime >= attackCooldown)
        {
            Player playerComp = other.GetComponent<Player>();
            if (playerComp != null)
            {
                playerComp.GetDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }
}
