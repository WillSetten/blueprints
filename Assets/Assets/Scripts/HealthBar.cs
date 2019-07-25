using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    Vector3 localScale;
    Color color;
    float unitMaxHealth;
    Unit unit;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        localScale = transform.localScale;
        unit = GetComponentInParent<Unit>();
        unitMaxHealth = unit.hp;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateHealth()
    {
        localScale.x = (float)unit.hp / unitMaxHealth;
        transform.localScale = localScale;
    }

    public void toggleHealthBar(bool on)
    {
        if (on)
        {
            spriteRenderer.color = color;
        }
        else
        {
            spriteRenderer.color = Color.clear;
        }
    }
}
