using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private GameState _mGameState;

    private int _lastStartedSceneHandle = -1;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        TryStartScene(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryStartScene(scene);
    }

    private void TryStartScene(Scene scene)
    {
        if (!scene.isLoaded)
        {
            return;
        }

        if (_lastStartedSceneHandle == scene.handle)
        {
            return;
        }

        _lastStartedSceneHandle = scene.handle;
        StartGame();
    }

    public void StartGame()
    {
        _mGameState = GameState.Start;
        Time.timeScale = 1;
        EventManager.OnStartGame?.Invoke();
    }

    public void WinStage()
    {
        if(_mGameState == GameState.GameOver) return;

        _mGameState = GameState.Win;
        EventManager.OnWinGame?.Invoke();
        SoundManager.Instance.PlaySound(SoundManager.Instance.win);
        DataManager.Instance.SaveData();
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
