using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBar : MonoBehaviour
{
    Vector3 localScale;
    Color color;
    Loot loot;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        localScale = transform.localScale;
        loot = GetComponentInParent<Loot>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color;
        spriteRenderer.color = Color.clear;
    }

    public void showLootBar()
    {
        spriteRenderer.color = color;
    }

    void Update()
    {
        localScale.y = (float)loot.remainingTime / loot.totalTime;
        transform.localScale = localScale;
    }
}
