using UnityEngine;
using UnityEngine.UI;

public class HeatDamageZone : MonoBehaviour
{
    [Header("Heat Settings")]
    public float maxHeat = 100f;
    public float heatIncreaseRate = 20f;

    [Header("Damage Settings")]
    public int damageAmount = 1;

    [Header("Heat UI")]
    public Slider heatSlider;

    private float currentHeat;
    private EchoController player;
    private bool playerInside;

    private void Update()
    {
        if (!playerInside || player == null)
            return;

        currentHeat += heatIncreaseRate * Time.deltaTime;

        if (currentHeat >= maxHeat)
        {
            currentHeat = 0f;

            player.TakeDamage(damageAmount);
        }

        UpdateHeatUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EchoController foundPlayer =
            other.GetComponentInParent<EchoController>();

        if (foundPlayer == null)
            return;

        player = foundPlayer;
        playerInside = true;
        UpdateHeatUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EchoController foundPlayer =
            other.GetComponentInParent<EchoController>();

        if (foundPlayer == null || foundPlayer != player)
            return;

        playerInside = false;
        player = null;
        currentHeat = 0f;

        UpdateHeatUI();
    }

    private void UpdateHeatUI()
    {
        if (heatSlider == null)
            return;

        heatSlider.maxValue = maxHeat;
        heatSlider.value = currentHeat;
    }
}
