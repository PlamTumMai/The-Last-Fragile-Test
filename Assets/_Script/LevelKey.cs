using UnityEngine;

public class LevelKey : MonoBehaviour
{
    public int keyNumber = 1;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || isCollected)
            return;

        if (GameManager.instance == null)
        {
            Debug.LogWarning("ไม่พบ GameManager!");
            return;
        }

        switch (keyNumber)
        {
            case 1:
                GameManager.instance.hasKey1 = true;
                break;

            case 2:
                GameManager.instance.hasKey2 = true;
                break;

            case 3:
                GameManager.instance.hasKey3 = true;
                break;

            default:
                Debug.LogWarning("หมายเลขกุญแจต้องเป็น 1, 2 หรือ 3");
                return;
        }

        isCollected = true;
        Debug.Log("เก็บกุญแจด่าน " + keyNumber + " สำเร็จ!");

        Destroy(gameObject);
    }
}
