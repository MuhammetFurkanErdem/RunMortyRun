using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Start, Playing, LevelComplete, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Start;

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

    private void Start()
    {
        CurrentState = GameState.Start;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Start) return;

        CurrentState = GameState.Playing;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInGameUI();
        }
    }

    public void LevelCompleted()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.LevelComplete;
        Debug.Log("LEVEL COMPLETED! TEBRİKLER!");

        if (PlayerManager.Instance != null)
        {
            PlayerMovement movement = PlayerManager.Instance.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelCompleteUI();
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelWin();
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        CurrentState = GameState.GameOver;
        Debug.Log("GAME OVER: Kaybettiniz!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverUI();
        }
        else
        {
            Debug.LogError("GameManager: Sahnede UIManager.Instance bulunamadı! Hierarchy'de UI Manager var mı?");
        }

        // Oyun dünyasını dondur (Düşmanların koşmaya devam etmesini engeller)
        Time.timeScale = 0f;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}