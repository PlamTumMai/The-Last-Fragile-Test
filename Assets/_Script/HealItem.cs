using UnityEngine;

public class HealItem : MonoBehaviour
{
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        EchoController player =
            other.GetComponentInParent<EchoController>();

        if (player == null)
            return;

        // เก็บ Heal เฉพาะตอนที่เลือดไม่เต็ม
        if (player.health < 4)
        {
            player.health = Mathf.Min(
                player.health + healAmount,
                4
            );

            // ปรับขนาดตัวละครกลับตามเลือด
            player.transform.localScale =
                player.GetOriginalScale() * (player.health / 4f);

            Debug.Log("เก็บ Heal สำเร็จ! HP: " + player.health);

            Destroy(gameObject);
        }
    }
}
