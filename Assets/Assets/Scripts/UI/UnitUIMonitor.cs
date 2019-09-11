using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUIMonitor : MonoBehaviour
{
    public RectTransform rectTransform;
    bool raised = false;
    
    public void raiseMonitor()
    {
        if (!raised)
        {
            Debug.Log("Raising Unit monitor");
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y + 65, rectTransform.localPosition.z);
            raised = true;
        }
    }

    public void lowerMonitor()
    {
        if (raised)
        {
            Debug.Log("Lowering Unit monitor");
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y-65, rectTransform.localPosition.z);
            raised = false;
        }
    }
}
