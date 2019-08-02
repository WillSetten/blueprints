using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmBar : MonoBehaviour
{
    public GameObject fill;

    public void updateFill(Vector3 newScale)
    {
        fill.transform.localScale = newScale;
    }
}
