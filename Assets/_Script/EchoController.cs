using UnityEngine;

public class EchoController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

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
    public float fallSpeedThreshold = 18f; // ความเร็ว/แรงกระแทกขั้นต่ำที่จะทำให้เลือดลดเมื่อตกถึงพื้น

    [Header("Effects")]
    public GameObject shatterEffectPrefab;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float jumpStaminaCost = 30f;
    public float baseStaminaRegen = 25f;
    private float currentStaminaRegen;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        rb.gravityScale = 3f; 
        
        currentStamina = maxStamina;
        currentStaminaRegen = baseStaminaRegen;
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.gravityScale = 3f; 
            }
            return; 
        }

        if (currentStamina < maxStamina)
        {
            currentStamina += currentStaminaRegen * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }

        moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded && currentStamina >= jumpStaminaCost)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            
            currentStamina -= jumpStaminaCost;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartDash();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        rb.gravityScale = 0f; 
        float dashDirection = moveInput != 0 ? Mathf.Sign(moveInput) : 1f; 
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
    }

    public void TakeDamage()
    {
        if (invincibilityTimer > 0) return; 

        invincibilityTimer = invincibilityDuration;

        if (health > 1)
        {
            health--;
            transform.localScale = originalScale * (health / 4f);
            
            moveSpeed += speedBoostPerHit;
            jumpForce += jumpBoostPerHit; 

            currentStaminaRegen = baseStaminaRegen * (health / 4f);

            if (shatterEffectPrefab != null)
            {
                GameObject effect = Instantiate(shatterEffectPrefab, transform.position, Quaternion.identity);
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
        // 1. เช็คเมื่อร่างกายสัมผัสพื้นหรืออุปสรรคเพื่อรีเซ็ตการกระโดด
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            isGrounded = true;

            // เช็คแรงกระแทกตอนตกถึงพื้น หากตกแรงกว่าค่า Threshold จะได้รับดาเมจ
            if (collision.relativeVelocity.magnitude >= fallSpeedThreshold)
            {
                Debug.Log("ร่วงกระแทกพื้นแรงเกินไป! แรงกระแทก: " + collision.relativeVelocity.magnitude);
                TakeDamage();
            }
        }

        // 2. เช็คเมื่อพุ่งชนหรือชนอุปสรรคแรงๆ (เหมือนเดิม)
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (collision.relativeVelocity.magnitude > 10f || isDashing)
            {
                TakeDamage();
            }
        }
    }
}