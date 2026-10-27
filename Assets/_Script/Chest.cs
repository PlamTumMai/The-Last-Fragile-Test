using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Chest : MonoBehaviour
{
    [Header("Chest Sound")]
    public AudioClip openChestSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("BGM Settings")]
    public AudioSource bgmAudioSource;

    [Header("Player Floating")]
    public float initialFloatSpeed = 2f;
    public float initialFloatDuration = 0.8f;
    public float launchSpeed = 25f;

    [Header("Chest Settings")]
    public float hideDelay = 0.5f;

    [Header("Thank You Text")]
    public TextMeshProUGUI thankYouText;

    [Header("Return To Main Menu")]
    public string mainMenuSceneName = "MainMenu";
    public float returnDelay = 5f;

    private bool isOpened = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        if (thankYouText != null)
        {
            thankYouText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || isOpened)
            return;

        if (GameManager.instance != null &&
            GameManager.instance.HasAllKeys())
        {
            isOpened = true;

            Debug.Log("มีกุญแจครบ 3 ดอกแล้ว! หีบสมบัติเปิดออก!");

            // หยุดเพลง BGM
            if (bgmAudioSource != null)
            {
                bgmAudioSource.Stop();
            }

            // เล่นเสียงเปิดหีบ
            if (openChestSound != null)
            {
                audioSource.PlayOneShot(
                    openChestSound,
                    soundVolume
                );
            }

            // แสดงข้อความ THANK YOU
            if (thankYouText != null)
            {
                thankYouText.gameObject.SetActive(true);
            }

            // ให้ตัวละครลอยขึ้นแล้วพุ่งขึ้นเร็ว
            Rigidbody2D playerRb =
                collision.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                StartCoroutine(FloatPlayer(playerRb));
            }

            // ซ่อนหีบหลังจากเวลาที่กำหนด
            StartCoroutine(HideChest());

            // กลับหน้า Main Menu หลังจากเวลาที่กำหนด
            StartCoroutine(ReturnToMainMenu());
        }
        else
        {
            Debug.Log("กุญแจยังไม่ครบนะ ขาดอีกนิด!");
        }
    }

    private IEnumerator FloatPlayer(Rigidbody2D playerRb)
    {
        float elapsed = 0f;

        // ช่วงที่ 1: ลอยขึ้นช้า ๆ
        playerRb.linearVelocity = Vector2.zero;

        while (elapsed < initialFloatDuration &&
               playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                initialFloatSpeed
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // ช่วงที่ 2: พุ่งขึ้นอย่างรวดเร็ว
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                launchSpeed
            );
        }
    }

    private IEnumerator HideChest()
    {
        yield return new WaitForSeconds(hideDelay);
        gameObject.SetActive(false);
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(returnDelay);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
