using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private GameState _mGameState;
    
    void Start()
    {
        StartGame();
    }
    public void StartGame()
    {
        _mGameState = GameState.Start;
        Time.timeScale = 1;
        EventManager.OnStartGame?.Invoke();
        SoundManager.Instance.PlayBackgroundMusic();
        UIManager.Instance.ShowGameplayUI();
    }

    public void WinStage()
    {
        if(_mGameState == GameState.GameOver) return;

        _mGameState = GameState.Win;
        EventManager.OnWinGame?.Invoke();
        SoundManager.Instance.PlaySound(SoundManager.Instance.win);
    }

    public void GameOver()
    {
        if(_mGameState == GameState.GameOver) return;
        _mGameState = GameState.GameOver;
        // Time.timeScale = 0;
        EventManager.OnLoseGame?.Invoke();
        SoundManager.Instance.PlaySound(SoundManager.Instance.lose);
        SoundManager.Instance.StopPlayMusic();
    }
    
    public void ContinueGame()
    {
        if(_mGameState != GameState.GameOver) return;
        _mGameState = GameState.Continue;
        EventManager.OnContinueGame?.Invoke();
        SoundManager.Instance.PlayBackgroundMusic();
    }
}

public enum GameState
{
    Start,
    Win,
    GameOver,
    Continue,
}
