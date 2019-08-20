using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetainBar : MonoBehaviour
{
    Color color;
    Vector3 localScale;
    public SpriteRenderer spriteRenderer;
    Unit unit;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color;
        localScale = transform.localScale;
        unit = GetComponentInParent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        localScale.x = unit.detainTimer / unit.detainTimerMax;
        transform.localScale = localScale;
    }
}
