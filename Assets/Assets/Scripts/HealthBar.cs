using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Vector3 localScale;
    float unitMaxHealth;

    // Start is called before the first frame update
    void Start()
    {
        localScale = transform.localScale;
        unitMaxHealth = GetComponentInParent<Unit>().hp;
    }

    public void UpdateHealth()
    {
        localScale.x = (float)GetComponentInParent<Unit>().hp / unitMaxHealth;
        transform.localScale = localScale;
    }
}
