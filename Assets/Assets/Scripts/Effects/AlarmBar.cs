using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlarmBar : MonoBehaviour
{
    public GameObject fill;
    public Text alarmText;

    public void updateFill(Vector3 newScale)
    {
        fill.transform.localScale = newScale;
    }

    public void nextStage(int stageNo)
    {
        fill.transform.localScale = new Vector3(0,200,64);
        switch (stageNo)
        {
            case 0:
                alarmText.text = "Police Assault";
                break;
            case 1:
                alarmText.text = "SWAT Assault";
                break;
            case 2:
                alarmText.text = "Agency Assault";
                break;
            case 3:
                alarmText.text = "Military Assault";
                break;
        }
    }
}
