
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

    [Header("Fragility System")]
    public int health = 4;
    private Vector3 originalScale;

    [Header("Fragility Stat Boosts")]
    public float speedBoostPerHit = 2.5f;
    public float jumpBoostPerHit = 3.0f;

    [Header("Damage Settings (I-Frames)")]
    public float invincibilityDuration = 0.5f;
    private float invincibilityTimer;

    [Header("Fall Damage Settings")]
    public float fallSpeedThreshold = 18f;

    [Header("Effects")]
    public GameObject shatterEffectPrefab;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float jumpStaminaCost = 30f;
    public float baseStaminaRegen = 25f;

    private float currentStaminaRegen;

    [Header("Stamina UI")]
    public Slider staminaSlider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        originalScale = transform.localScale;
        rb.gravityScale = 3f;

        currentStamina = maxStamina;
        currentStaminaRegen = baseStaminaRegen;

        // ตั้งค่าหลอด Stamina
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.gravityScale = 3f;
            }

            UpdateAnimation();
            UpdateStaminaUI();
            return;
        }

        // ฟื้นฟู Stamina
        if (currentStamina < maxStamina)
        {
            currentStamina += currentStaminaRegen * Time.deltaTime;

            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }

        // รับค่าการเดิน
        moveInput = Input.GetAxisRaw("Horizontal");

        // หันหน้าตามทิศทางการเดิน
        if (spriteRenderer != null)
        {
            if (moveInput > 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveInput < 0)
            {
                spriteRenderer.flipX = false;
            }
        }

        // เคลื่อนที่ซ้าย-ขวา
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        // กระโดดจากพื้นหรือกำแพง
        if (Input.GetButtonDown("Jump") &&
            currentStamina >= jumpStaminaCost)
        {
            if (isGrounded)
            {
                // กระโดดจากพื้น
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                isGrounded = false;
                currentStamina -= jumpStaminaCost;
            }
            else if (isWallClinging && enableWallCling)
            {
                // กระโดดออกจากกำแพง
                rb.linearVelocity = new Vector2(
                    wallDirection * wallJumpForceX,
                    wallJumpForceY
                );

                isWallClinging = false;
                isTouchingWall = false;
                isGrounded = false;

                wallJumpTimer = wallJumpLockTime;
                currentStamina -= jumpStaminaCost;
            }
        }

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartDash();
        }

        // ทดสอบรับดาเมจด้วยปุ่ม H
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }

        // อัปเดต Animation และหลอด Stamina
        UpdateAnimation();
        UpdateStaminaUI();
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider == null)
        {
            return;
        }

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;
    }

    void UpdateAnimation()
    {
        if (animator == null)
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
            moveInput != 0 && isGrounded && !isDashing
        );

        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
        animator.SetBool("IsWallClinging", isWallClinging);
        animator.SetBool("IsDashing", isDashing);
    }

    void FixedUpdate()
    {
        if (!enableWallCling || isDashing)
        {
            isWallClinging = false;
            isTouchingWall = false;
            return;
        }

        // เกาะกำแพงเมื่อสัมผัสด้านข้าง
        // ไม่ได้อยู่บนพื้น และพ้นช่วงล็อกหลัง Wall Jump
        isWallClinging =
            isTouchingWall &&
            !isGrounded &&
            wallJumpTimer <= 0;

        if (isWallClinging && rb.linearVelocity.y < 0f)
        {
            // หยุดการตกลงด้านล่าง
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );
        }

        // รีเซ็ตเพื่อรอการตรวจจับรอบถัดไป
        isTouchingWall = false;
    }

    void StartDash()
    {
        isDashing = true;
        isWallClinging = false;
        dashTimeLeft = dashDuration;

        rb.gravityScale = 0f;

        float dashDirection = moveInput != 0
            ? Mathf.Sign(moveInput)
            : 1f;

        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );
    }

    public void TakeDamage()
    {
        if (invincibilityTimer > 0)
        {
            return;
        }

        invincibilityTimer = invincibilityDuration;

        if (health > 1)
        {
            health--;

            transform.localScale =
                originalScale * (health / 4f);

            moveSpeed += speedBoostPerHit;
            jumpForce += jumpBoostPerHit;

            currentStaminaRegen =
                baseStaminaRegen * (health / 4f);

            if (shatterEffectPrefab != null)
            {
                GameObject effect = Instantiate(
                    shatterEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

                Destroy(effect, 0.5f);
            }

            Debug.Log("ร่างร้าว! เลือด: " + health);
        }
        else
        {
            Debug.Log("ตัวละครแตกสลาย! Game Over");
            gameObject.SetActive(false);
        }
    }

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
        bool isValidSurface =
            collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Obstacle");

        if (!isValidSurface)
        {
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // ตรวจจับพื้นจาก Normal ที่ชี้ขึ้น
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }

            // ตรวจจับกำแพงจาก Normal ด้านข้าง
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isTouchingWall = true;

                // กำหนดทิศทางกระโดดออกจากกำแพง
                wallDirection =
                    contact.normal.x > 0 ? 1 : -1;
            }
        }

        // ตรวจสอบแรงกระแทก
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
            if (collision.relativeVelocity.magnitude > 10f ||
                isDashing)
            {
                TakeDamage();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground") &&
            !collision.gameObject.CompareTag("Obstacle"))
        {
            return;
        }

        isGrounded = false;
        isWallClinging = false;
    }
}
