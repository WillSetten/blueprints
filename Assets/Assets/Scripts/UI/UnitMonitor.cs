using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitMonitor : MonoBehaviour
{
    public Text characterName;
    public Text className;
    public Text hp;
    public Image sprite;
    public Unit unit;
    
    public void selectUnit()
    {
        unit.map.setSelectedUnit(unit.gameObject);
    }
    public void newHp(int newHp)
    {
        hp.text = newHp.ToString();
    }
}
