using UnityEngine;

public enum GameState { NotStarted, Playing, GameOver, LevelCompleted }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.NotStarted;

    private void Awake()
    {
        // Singleton Yapısı
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
        CurrentState = GameState.GameOver;
        Debug.Log("GAME OVER!");
    }

    public void SetLevelCompleted()
    {
        CurrentState = GameState.LevelCompleted;
        Debug.Log("LEVEL COMPLETED!");
    }
}