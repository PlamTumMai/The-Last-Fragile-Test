using UnityEngine;

public class WindZone2D : MonoBehaviour
{
    [Header("Wind Direction")]
    public Vector2 windDirection = Vector2.right;
    public float windForce = 15f;

    [Header("Wind Timing")]
    public float windDuration = 2f;
    public float calmDuration = 3f;

    [Header("Wind Effect")]
    public ParticleSystem windParticles;

    private Rigidbody2D playerRb;
    private float timer;
    private bool isWindActive;

    private void Start()
    {
        // เริ่มต้นด้วยช่วงลมหยุด
        isWindActive = false;
        timer = calmDuration;

        if (windParticles != null)
        {
            windParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isWindActive = !isWindActive;

            timer = isWindActive
                ? windDuration
                : calmDuration;

            UpdateWindEffect();
        }
    }

    private void FixedUpdate()
    {
        if (!isWindActive || playerRb == null)
            return;

        Vector2 direction = windDirection.normalized;

        playerRb.AddForce(
            direction * windForce,
            ForceMode2D.Force
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EchoController player =
            other.GetComponentInParent<EchoController>();

        if (player == null)
            return;

        playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb == null)
        {
            playerRb = player.GetComponentInParent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EchoController player =
            other.GetComponentInParent<EchoController>();

        if (player == null)
            return;

        Rigidbody2D exitingRb =
            player.GetComponent<Rigidbody2D>();

        if (exitingRb == null)
        {
            exitingRb = player.GetComponentInParent<Rigidbody2D>();
        }

        if (exitingRb == playerRb)
        {
            playerRb = null;
        }
    }

    private void UpdateWindEffect()
    {
        if (windParticles == null)
            return;

        if (isWindActive)
        {
            windParticles.Play();
        }
        else
        {
            windParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }
    }
}
