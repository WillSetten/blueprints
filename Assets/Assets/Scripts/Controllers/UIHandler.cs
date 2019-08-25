using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void togglePause(bool paused)
    {
        if (paused)
        {
            pausedText.color = Color.white;
            pausedOverlay.color = new Color(255, 255, 255, 255);
        }
        else
        {
            pausedText.color = Color.clear;
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

}
