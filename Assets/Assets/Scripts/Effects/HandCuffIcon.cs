using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCuffIcon : MonoBehaviour
{
    Unit unit;
    public SpriteRenderer spriteRenderer;
    void Start()
    {
        unit = GetComponentInParent<Unit>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
