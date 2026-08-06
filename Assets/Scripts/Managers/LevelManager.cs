using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // New Input System kütüphanesi

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // R tuşuna basıldığında bölümü yeniden başlat
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }
    }

    public void RestartLevel()
    {
        // Sahneyi yeniden yüklemek pozisyonları, klonları ve kapıları 
        // sıfırlayarak oyunu başlangıç bekleme durumuna getirir.
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}