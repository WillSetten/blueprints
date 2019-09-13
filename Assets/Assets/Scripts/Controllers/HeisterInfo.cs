using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is designed to hold less dynamic data relevent to the player character
public class HeisterInfo : MonoBehaviour
{
    public Unit unit;
    public bool isLarge=false;
    public int interactionRadius;
    public float moveRate; //Speed at which the unit moves
    public float lootMoveRate; //Speed at which the unit moves whilst carrying loot
    public string name = "Dave";
    public string className;
    private void Start()
    {
        unit = GetComponent<Unit>();
    }
}
