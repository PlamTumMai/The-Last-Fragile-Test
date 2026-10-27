using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public TMP_Text targetText;
    public float blinkSpeed = 1f;

    void Update()
    {
        if (targetText == null) return;

        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        targetText.alpha = alpha;
    }
}
