using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIHandler : MonoBehaviour
{
    public Text hostageText;
    public int hostageCount;
    public Text lootText;
    public int lootCount;
    public Text lootTotalText;
    public int lootTotal = 0;
    public Text pausedText;
    public Image pausedOverlay;
    public Text DoneText;
    public Text DoneLootText;
    public Canvas MenuCanvas;
    public Canvas GUICanvas;
    public Canvas DoneCanvas;

    private void Start()
    {
        MenuCanvas.enabled = false;
        GUICanvas.enabled = true;
        DoneCanvas.enabled = false;
    }

    public void togglePause(bool paused)
    {
        togglePauseOverlay(paused);
        if (paused)
        {
            pausedText.color = Color.white;
        }
        else
        {
            pausedText.color = Color.clear;
        }
    }

    public void toggleMenu(bool paused)
    {
        togglePauseOverlay(paused);
        if (paused)
        {
            Time.timeScale = 0;
            MenuCanvas.enabled = true;
        }
        else
        {
            Time.timeScale = 1;
            MenuCanvas.enabled = false;
        }
    }

    public void gameOver(bool win)
    {
        MenuCanvas.enabled = false;
        GUICanvas.enabled = false;
        DoneCanvas.enabled = true;
        Time.timeScale = 0;
        if (win)
        {
            if (lootCount>0) {
                DoneText.text = "Heist Completed";
                DoneLootText.text = "Loot Secured: " + lootCount;
            }
            else
            {
                DoneText.text = "Heist Failed";
                DoneLootText.text = "No Loot Secured";
            }
        }
        else
        {
            DoneText.text = "Game Over";
            DoneLootText.enabled = false;
        }
    }

    public void togglePauseOverlay(bool paused)
    {
        if (paused)
        {
            pausedOverlay.color = new Color(255, 255, 255, 255);
        }
        else
        {
            pausedOverlay.color = new Color(255, 255, 255, 0);
        }
    }

    //Increase the number of hostages captured
    public void incrementHostageCount()
    {
        hostageCount = hostageCount + 1;
        hostageText.text = hostageCount.ToString();
    }

    //Decrease the number of hostages captured. Call this when a hostage is freed.
    public void decrementHostageCount()
    {
        hostageCount = hostageCount - 1;
        hostageText.text = hostageCount.ToString();
    }

    //Increase the amount of loot secured
    public void incrementLootCount()
    {
        lootCount = lootCount + 1;
        lootText.text = lootCount.ToString();
    }

    public void exitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
