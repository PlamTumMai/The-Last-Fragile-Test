using UnityEngine;

public class Chest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // ถาม GameManager ว่ากุญแจครบ 3 ดอกไหม
            if (GameManager.instance.HasAllKeys())
            {
                Debug.Log("มีกุญแจครบ 3 ดอกแล้ว! หีบสมบัติเปิดออก!");
                gameObject.SetActive(false); // ทดสอบซ่อนหีบไปก่อนเวลาเปิดสำเร็จ
            }
            else
            {
                Debug.Log("กุญแจยังไม่ครบนะ ขาดอีกนิด!");
            }
        }
    }
}