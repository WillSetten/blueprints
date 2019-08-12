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
    public List<Unit> unitsInEscapeArea()
    {
        List<Unit> units = new List<Unit>();
        foreach (GameObject r in escapeSquares) {
            if (Physics2D.OverlapCircleAll(r.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits")).Length != 0)
            {
                units.Add(Physics2D.OverlapCircleAll(r.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits"))[0].GetComponent<Unit>());
            } 
        }
        return units;
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
