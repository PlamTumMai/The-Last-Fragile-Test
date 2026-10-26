using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelKey : MonoBehaviour
{
    public int keyNumber = 1; // กำหนดใน Inspector ว่านี่คือกุญแจดอกที่ 1, 2 หรือ 3

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // บันทึกกุญแจลง GameManager
            if (keyNumber == 1) GameManager.instance.hasKey1 = true;
            if (keyNumber == 2) GameManager.instance.hasKey2 = true;
            if (keyNumber == 3) GameManager.instance.hasKey3 = true;

            Debug.Log("เก็บกุญแจด่าน " + keyNumber + " สำเร็จ! วาร์ปกลับ Lobby");
            
            // เก็บเสร็จวาร์ปกลับ Lobby ทันที (พร้อมกับรีเซ็ตเลือดในด่าน Lobby ให้เต็มอัตโนมัติ)
            SceneManager.LoadScene("Lobby");
        }
    }
}