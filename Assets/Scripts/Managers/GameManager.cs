using UnityEngine;

public enum GameState { NotStarted, Playing, GameOver, LevelCompleted }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.NotStarted;

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

    public void StartGame()
    {
        if (CurrentState == GameState.NotStarted)
        {
            CurrentState = GameState.Playing;
            Debug.Log("Oyun Başladı!");
        }
    }

    public void SetGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        CurrentState = GameState.GameOver;
        Debug.Log("GAME OVER!");

        // GameManager sahnede silinmediği için 1 saniye sonra restart garantili çalışır
        Invoke(nameof(AutoRestart), 1.0f);
    }

    private void AutoRestart()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
    }

    public void SetLevelCompleted()
    {
        CurrentState = GameState.LevelCompleted;
        Debug.Log("LEVEL COMPLETED!");
    }
}