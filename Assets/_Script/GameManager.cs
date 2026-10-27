using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Key Inventory")]
    public bool hasKey1;
    public bool hasKey2;
    public bool hasKey3;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public bool HasKey(int keyNumber)
    {
        switch (keyNumber)
        {
            case 1: return hasKey1;
            case 2: return hasKey2;
            case 3: return hasKey3;
            default: return false;
        }
    }

    public bool HasAllKeys()
    {
        return hasKey1 && hasKey2 && hasKey3;
    }
}
