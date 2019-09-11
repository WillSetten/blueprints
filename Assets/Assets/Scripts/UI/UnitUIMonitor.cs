using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUIMonitor : MonoBehaviour
{
    public RectTransform rectTransform;
    bool mouseOver = false;
    float loweredHeight;
    float raisedHeight;
    float speed = 250;

    private void Start()
    {
        loweredHeight = rectTransform.localPosition.y;
        raisedHeight = rectTransform.localPosition.y+65;
    }

    //Called when the mouse moves over the unit monitors
    public void raiseMonitor()
    {
        if (!mouseOver) {
            mouseOver = true;
            Debug.Log("Raising Unit monitor");
            StartCoroutine(changeHeight());
        }
    }

    //Called when the mouse moves off the unit monitors
    public void lowerMonitor()
    {
        if (mouseOver) {
            mouseOver = false;
            Debug.Log("Lowering Unit monitor");
            StartCoroutine(changeHeight());
        }
    }

    //Coroutine to manage the raising and lowering of the unit monitors
    IEnumerator changeHeight()
    {
        //If the mouse has moved off the unit monitors
        if (!mouseOver)
        {
            //While the monitors are above their required height, decrease that height
            while (rectTransform.localPosition.y > loweredHeight) {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x,
                    rectTransform.localPosition.y - Time.deltaTime * speed,
                    rectTransform.localPosition.z);
                //If the mouse has gone over a monitor, break as the monitor will now need to be raised
                if (mouseOver)
                {
                    yield break;
                }
                yield return null;
            }
        }
        //If the mouse has moved over the unit monitors
        else
        {
            //While the monitors are below their required height, increase that height
            while (rectTransform.localPosition.y < raisedHeight)
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x,
                    rectTransform.localPosition.y + Time.deltaTime*speed,
                    rectTransform.localPosition.z);
                //If the mouse has moved off the monitor, break as the monitor will now need to be lowered
                if (!mouseOver)
                {
                    yield break;
                }
                yield return null;
            }
        }
    }
}
