using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeArea : MonoBehaviour
{
    public List<GameObject> escapeSquares;
    Color squareColor;

    private void Start()
    {
        squareColor = escapeSquares[0].GetComponent<SpriteRenderer>().color;
    }

    //Returns true if there are units present in the escape area
    public bool unitsInEscapeArea()
    {
        foreach (GameObject r in escapeSquares) {
            if (Physics2D.OverlapCircleAll(r.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits")).Length != 0)
            {
                return true;
            }
        }
        return false;
    }

    //Shows or hides the escape squares
    public void toggleEscapeSquares(bool show)
    {
        if (show)
        {
            foreach (GameObject e in escapeSquares)
            {
                e.GetComponent<SpriteRenderer>().color = squareColor;
            }
        }
        else
        {
            foreach (GameObject e in escapeSquares)
            {
                e.GetComponent<SpriteRenderer>().color = Color.clear;
            }
        }
    }
}
