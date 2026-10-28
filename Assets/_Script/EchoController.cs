using UnityEngine;
using UnityEngine.UI;

public class EchoController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    [Header("Sprite Direction")]
    private SpriteRenderer spriteRenderer;

    [Header("Animation")]
    private Animator animator;

    [Header("Wall Cling")]
    public bool enableWallCling = true;
    public float wallJumpForceX = 8f;
    public float wallJumpForceY = 12f;
    public float wallJumpLockTime = 0.2f;

    private bool isTouchingWall;
    private bool isWallClinging;
    private int wallDirection;
    private float wallJumpTimer;

    [Header("Dash Mechanism")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;

    private bool isDashing;
    private float dashTimeLeft;

    [Header("Ice Floor Settings")]
    public float iceAcceleration = 8f;
    public float iceDeceleration = 2f;

    private bool isOnIce;

    [Header("Fragility System")]
    public int health = 4;
    public int maxHealth = 4;

    private Vector3 originalScale;

    [Header("Fragility Stat Boosts")]
    public float speedBoostPerHit = 2.5f;
    public float jumpBoostPerHit = 3f;

    [Header("Damage Settings (I-Frames)")]
    public float invincibilityDuration = 0.5f;
    private float invincibilityTimer;

    [Header("Damage Sound")]
    public AudioClip damageSound;

    [Range(0f, 1f)]
    public float damageVolume = 1f;

    private AudioSource audioSource;

    [Header("Fall Damage Settings")]
    public float fallSpeedThreshold = 18f;

    [Header("Effects")]
    public GameObject shatterEffectPrefab;

    private float currentStaminaRegen;

    [Header("Game Over UI")]
    public GameObject gameOverText;

    private void Start()
    {
        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }

        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        originalScale = transform.localScale;

        rb.gravityScale = 3f;

        maxHealth = Mathf.Max(1, maxHealth);
        health = Mathf.Clamp(health, 1, maxHealth);


        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;


        UpdateCharacterScale();
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;

            if (dashTimeLeft <= 0f)
            {
                isDashing = false;
                rb.gravityScale = 3f;
            }

            UpdateAnimation();
            return;
        }

        // รับค่าการเดิน
        moveInput = Input.GetAxisRaw("Horizontal");

        // หันหน้าตามทิศทางการเดิน
        if (spriteRenderer != null)
        {
            if (moveInput > 0f)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveInput < 0f)
            {
                spriteRenderer.flipX = false;
            }
        }

        // ระบบเคลื่อนที่: พื้นปกติกับพื้นน้ำแข็ง
        if (isOnIce)
        {
            float targetSpeed = moveInput * moveSpeed;

            float acceleration = moveInput != 0f
                ? iceAcceleration
                : iceDeceleration;

            float newSpeed = Mathf.MoveTowards(
                rb.linearVelocity.x,
                targetSpeed,
                acceleration * Time.deltaTime
            );

            rb.linearVelocity = new Vector2(
                newSpeed,
                rb.linearVelocity.y
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(
                moveInput * moveSpeed,
                rb.linearVelocity.y
            );
        }

        // กระโดดจากพื้นหรือกำแพง
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                isGrounded = false;
            }
            else if (isWallClinging && enableWallCling)
            {
                rb.linearVelocity = new Vector2(
                    wallDirection * wallJumpForceX,
                    wallJumpForceY
                );

                isWallClinging = false;
                isTouchingWall = false;
                isGrounded = false;

                wallJumpTimer = wallJumpLockTime;
            }
        }

        // Dash ด้วย Left Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartDash();
        }

        // ทดสอบรับดาเมจด้วยปุ่ม H
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (!enableWallCling || isDashing)
        {
            isWallClinging = false;
            isTouchingWall = false;
            return;
        }

        isWallClinging =
            isTouchingWall &&
            !isGrounded &&
            wallJumpTimer <= 0f;

        if (isWallClinging && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );
        }

        isTouchingWall = false;
    }

    private void StartDash()
    {
        isDashing = true;
        isWallClinging = false;
        dashTimeLeft = dashDuration;

        rb.gravityScale = 0f;

        float dashDirection = moveInput != 0f
            ? Mathf.Sign(moveInput)
            : (spriteRenderer != null && spriteRenderer.flipX ? 1f : -1f);

        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );
    }

    private void UpdateAnimation()
    {
        if (animator == null || rb == null)
        {
            return;
        }

        bool isJumping =
            !isGrounded &&
            rb.linearVelocity.y > 0.1f &&
            !isWallClinging;

        bool isFalling =
            !isGrounded &&
            rb.linearVelocity.y < -0.1f &&
            !isWallClinging;

        animator.SetBool(
            "IsRunning",
            moveInput != 0f && isGrounded && !isDashing
        );

        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsWallClinging", isWallClinging);
        animator.SetBool("IsDashing", isDashing);
    }


    // =========================
    // DAMAGE SYSTEM
    // =========================


    public void TakeDamage(int damageAmount = 1)
    {
        if (invincibilityTimer > 0f)
            return;

        invincibilityTimer = invincibilityDuration;

        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound, damageVolume);
        }

        if (health > damageAmount)
        {
            health -= damageAmount;

            UpdateCharacterScale();

            moveSpeed += speedBoostPerHit;
            jumpForce += jumpBoostPerHit;

            if (shatterEffectPrefab != null)
            {
                GameObject effect = Instantiate(
                    shatterEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

                Destroy(effect, 0.5f);
            }

            Debug.Log("ได้รับดาเมจ! HP: " + health);
        }
        else
        {
            health = 0;

            Debug.Log("ตัวละครแตกสลาย! Game Over");

            // แสดงข้อความ GAME OVER
            if (gameOverText != null)
            {
                gameOverText.SetActive(true);
            }

            // ปิดการควบคุมตัวละคร
            gameObject.SetActive(false);
        }
    }


    // =========================
    // HEAL SYSTEM
    // =========================

    public void HealHealth(int healAmount)
    {
        if (health >= maxHealth || healAmount <= 0)
        {
            return;
        }

        int previousHealth = health;

        health = Mathf.Clamp(
            health + healAmount,
            0,
            maxHealth
        );

        if (health > previousHealth)
        {
            UpdateCharacterScale();

            Debug.Log("ฟื้นฟู HP สำเร็จ! HP: " + health);
        }
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }

    private void UpdateCharacterScale()
    {
        if (maxHealth <= 0)
        {
            return;
        }

        float scaleMultiplier = (float)health / maxHealth;
        transform.localScale = originalScale * scaleMultiplier;
    }

    // =========================
    // COLLISION SYSTEM
    // =========================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckCollision(collision);
    }

    private void CheckCollision(Collision2D collision)
    {
        bool isIce = collision.gameObject.CompareTag("Ice");

        bool isValidSurface =
            collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Obstacle") ||
            isIce;

        if (!isValidSurface)
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // ตรวจจับพื้นจากด้านล่างของตัวละคร
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                isOnIce = isIce;
            }

            // ตรวจจับกำแพง
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isTouchingWall = true;
                wallDirection = contact.normal.x > 0f ? 1 : -1;
            }
        }

        // ตรวจสอบแรงกระแทกจากการตก
        if (collision.relativeVelocity.magnitude >= fallSpeedThreshold)
        {
            Debug.Log(
                "ร่วงกระแทกพื้นแรงเกินไป! แรงกระแทก: " +
                collision.relativeVelocity.magnitude
            );

            TakeDamage();
        }

        // ตรวจสอบการชนสิ่งกีดขวางระหว่าง Dash
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (collision.relativeVelocity.magnitude > 10f || isDashing)
            {
                TakeDamage();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ice"))
        {
            isOnIce = false;
        }

        if (!collision.gameObject.CompareTag("Ground") &&
            !collision.gameObject.CompareTag("Obstacle") &&
            !collision.gameObject.CompareTag("Ice"))
        {
            return;
        }

        isGrounded = false;
        isWallClinging = false;
    }
}
