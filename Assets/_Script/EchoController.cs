using UnityEngine;

public class EchoController : MonoBehaviour
{
    [Header("Movement")] public float moveSpeed = 5f;
    public float jumpForce = 30f;
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    [Header("Dash Mechanism")] public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    private bool isDashing;
    private float dashTimeLeft;

    [Header("Fragility System")]
    public int health = 4; // ระดับความคงทน 4 ขั้น (ปกติ - ร้าวเล็กน้อย - ร้าวหนัก - แตกสลาย)

    private Vector3 originalScale;
    [Header("Effects")] public GameObject shatterEffectPrefab;

    [Header("Fragility Stat Boosts")] public float speedBoostPerHit = 2.5f; // ทุกครั้งที่ร้าว จะเดินไวขึ้นเท่าไหร่
    public float jumpBoostPerHit = 3.0f; // ทุกครั้งที่ร้าว จะโดดสูงขึ้นเท่าไหร่

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        rb.gravityScale = 3f; // ทำให้ตกพื้นไวขึ้นแบบเกม Platformer
    }

    void Update()
    {
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.gravityScale = 3f; // คืนค่าแรงโน้มถ่วง
            }

            return; // ระหว่างพุ่งตัว จะไม่รับคำสั่งเดิน
        }

        // 1. การเดิน ซ้าย-ขวา
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // 2. การกระโดด
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }

        // 3. การพุ่งตัว (Dash กลางอากาศหรือพื้น)
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        rb.gravityScale = 0f; // ไม่ตกพื้นระหว่างพุ่งตัว
        float dashDirection = moveInput != 0 ? Mathf.Sign(moveInput) : 1f; // พุ่งไปตามทิศที่กด หรือขวาถ้าอยู่นิ่ง
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
    }

    public void TakeDamage()
    {
        if (health > 1)
        {
            health--;
            // ขนาดตัวเล็กลงตามขีดความคงทน
            transform.localScale = originalScale * (health / 3f);

            // ยิ่งแตก ยิ่งไว ยิ่งกระโดดสูง
            moveSpeed += speedBoostPerHit;
            jumpForce += jumpBoostPerHit;

            // เสกเอฟเฟกต์กระจกแตกที่ตำแหน่งเดียวกับตัวละคร
            if (shatterEffectPrefab != null)
            {
                GameObject effect = Instantiate(shatterEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f);
            }

            Debug.Log("ร่างร้าว! เลือด: " + health + " | ความเร็ว: " + moveSpeed + " | แรงกระโดด: " + jumpForce);
        }
        else
        {
            Debug.Log("ตัวละครแตกสลาย! Game Over");
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // เพิ่ม CompareTag("Obstacle") เข้าไป เพื่อให้ระบบนับว่าเหยียบอุปสรรคก็ถือว่าแตะพื้น (isGrounded = true)
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
        {
            isGrounded = true;
        }

        // ส่วนนี้คือระบบรับดาเมจเมื่อชนอุปสรรคแรงๆ หรือ Dash ชน (เหมือนเดิม)
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (collision.relativeVelocity.magnitude > 10f || isDashing)
            {
                TakeDamage();
            }
        }
    }
}