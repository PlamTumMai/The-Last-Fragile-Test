using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private bool isStarting = false;

    void Update()
    {
        // ตรวจจับการกดปุ่มคีย์บอร์ดหรือคลิกเมาส์
        if (!isStarting &&
            (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            isStarting = true;
            SceneManager.LoadScene("Lobby");
        }
    }
}
