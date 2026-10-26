
using UnityEngine;

public class StartScreenController : MonoBehaviour
{
    public GameObject startScreen;

    private bool gameStarted = false;

    void Start()
    {
        Time.timeScale = 0f;
        startScreen.SetActive(true);
    }

    void Update()
    {
        if (!gameStarted &&
            (Input.GetMouseButtonDown(0) ||
             Input.anyKeyDown))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        startScreen.SetActive(false);
    }
}
