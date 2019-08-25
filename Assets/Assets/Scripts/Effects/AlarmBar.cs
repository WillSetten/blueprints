using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmBar : MonoBehaviour
{
    public Image mainFill;
    public Image stageFill;
    public Text alarmText;

    public void updateFill(float fillAmount)
    {
        mainFill.fillAmount = fillAmount;
    }

    public void nextStage(int stageNo)
    {
        if (stageNo<4)
        {
            mainFill.fillAmount = 0;
            switch (stageNo)
            {
                case 0:
                    stageFill.fillAmount = 0.25f;
                    alarmText.text = "Police Assault";
                    break;
                case 1:
                    stageFill.fillAmount = 0.5f;
                    alarmText.text = "SWAT Assault";
                    break;
                case 2:
                    stageFill.fillAmount = 0.75f;
                    alarmText.text = "Military Assault";
                    break;
                case 3:
                    stageFill.fillAmount = 1;
                    alarmText.text = "Deathwish";
                    break;
            }
        }
    }
}
