using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that manages the list of monitors for different heister units
public class UnitList : MonoBehaviour
{
    public RectTransform rectTransform;
    bool mouseOver = false;
    float loweredHeight;
    float raisedHeight;
    float speed = 250;
    public List<HeisterInfo> heisterList;
    public GameObject UnitMonitor;

    private void Start()
    {
        loweredHeight = rectTransform.localPosition.y;
        raisedHeight = rectTransform.localPosition.y+55;
        heisterList.AddRange(FindObjectsOfType<HeisterInfo>());
        //Creates a Monitor object for each heister and fills in the correct information
        for(int i=0; i<heisterList.Count; i++)
        {
            HeisterInfo h = heisterList[i];

            UnitMonitor newMonitor = Instantiate(UnitMonitor,transform).GetComponent<UnitMonitor>();
            h.unit.unitMonitor = newMonitor;
            newMonitor.unit = h.unit;
            newMonitor.className.text = h.className;
            newMonitor.characterName.text = h.characterName;
            newMonitor.hp.text = h.unit.hp.ToString();
            newMonitor.sprite.sprite = h.unit.GetComponent<SpriteRenderer>().sprite;
            //If there are an odd amount of heisters
            if ((float)heisterList.Count % 2 != 0)
            {
                newMonitor.GetComponent<RectTransform>().localPosition = new Vector3(newMonitor.GetComponent<RectTransform>().rect.width * (i+1 - Mathf.Ceil((float)heisterList.Count / 2)),
                    newMonitor.GetComponent<RectTransform>().localPosition.y,
                    newMonitor.GetComponent<RectTransform>().localPosition.z);
            }
            //If there are an even amount of heisters
            else
            {
                newMonitor.GetComponent<RectTransform>().localPosition = new Vector3((newMonitor.GetComponent<RectTransform>().rect.width * i) -
                    ((heisterList.Count-1) * newMonitor.GetComponent<RectTransform>().rect.width/2),
                    newMonitor.GetComponent<RectTransform>().localPosition.y,
                    newMonitor.GetComponent<RectTransform>().localPosition.z);
            }
        }
    }

    //Called when the mouse moves over the unit monitors
    public void raiseMonitor()
    {
        if (!mouseOver) {
            mouseOver = true;
            StartCoroutine(changeHeight());
        }
    }

    //Called when the mouse moves off the unit monitors
    public void lowerMonitor()
    {
        if (mouseOver) {
            mouseOver = false;
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
