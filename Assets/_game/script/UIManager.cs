using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class UIManager : Singleton<UIManager>
{
    protected override bool PersistAcrossScenes => false;

    public List<string> tagValueTypes;
    public TextMeshProUGUI straightSlotText;

    public GameObject straightSlot;
    public UnityEngine.EventSystems.EventSystem eventSystem;

    public void OnDownloadButtonClicked()
    {
        //Luna.Unity.Playable.InstallFullGame();
    }

    public void UpdateStraightSlot(float count, float maxSlot)
    {
        if (straightSlotText == null)
        {
            return;
        }
        straightSlotText.text = count + "/" + maxSlot;
    }

    public void UpdateScore(int score)
    {
        // scoreText.text = score.ToString();
    }

    public void RestartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void OnBackToEditorClicked()
    {
        SceneManager.LoadScene("5.level_editor");
        // GameManagerForTesting.Instance.ClearPlayTestConfig();
    }
}
