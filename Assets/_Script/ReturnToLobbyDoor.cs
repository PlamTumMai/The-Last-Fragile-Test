using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobbyDoor : MonoBehaviour
{
    public string lobbySceneName = "Lobby";
    [Range(1, 3)]
    public int requiredKeyNumber = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (GameManager.instance == null)
        {
            Debug.LogWarning("ไม่พบ GameManager!");
            return;
        }

        if (GameManager.instance.HasKey(requiredKeyNumber))
        {
            SceneManager.LoadScene(lobbySceneName);
        }
        else
        {
            Debug.Log("คุณยังไม่ได้เก็บกุญแจประจำด่านนี้!");
        }
    }
}
