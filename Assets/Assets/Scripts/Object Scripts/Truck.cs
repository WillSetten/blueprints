using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
    public List<GameObject> doors;
    public EscapeArea escapeArea;
    AudioSource audioSource;
    public AudioClip doorOpen;
    public AudioClip doorClose;
    public int lootCount;
    bool open = false;
    public TileMap map;
    public List<Unit> unitsInVan;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        foreach (GameObject door in doors)
        {
            door.GetComponent<TruckDoor>().truck = this;
        }
    }
    
    private void OnMouseOver()
    {
        //When the van is clicked on, check if there are units in the escape area.
        if (Input.GetMouseButtonDown(0))
        {
            List<Unit> units = escapeArea.unitsInEscapeArea();
            if (units.Count > 0)
            {
                bool hasLoot = false;
                //Iterate through all the units in the escape area to see if any of them have any loot.
                foreach (Unit u in units)
                {
                    if (u.hasLoot)
                    {
                        hasLoot = true;
                    }
                }

                if (open)
                {
                    //If there are units in the escape area, the doors are open and one or more of the units has loot on them, unload the loot into the van
                    if (hasLoot)
                    {
                        Debug.Log("Unloading loot into van!");
                        foreach (Unit u in units)
                        {
                            if (u.hasLoot)
                            {
                                u.hasLoot = false;
                                map.incrementLootCount();
                            }
                        }
                    }
                    //If there are units in the escape area and the doors are open, close the doors
                    else
                    {
                        open = false;
                        foreach (GameObject d in doors)
                        {
                            d.GetComponent<TruckDoor>().toggleOpen(open);
                        }
                    }
                }
                //If there are units in the escape area and the doors are closed, open the doors
                else
                {
                    open = true;
                    playSound(doorOpen);
                    foreach (GameObject d in doors)
                    {
                        d.GetComponent<TruckDoor>().toggleOpen(open);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (open) {
                List<Unit> units = escapeArea.unitsInEscapeArea();
                foreach (Unit u in units)
                {
                    if (u.selected&&u.combatant)
                    {
                        if (u.hasLoot)
                        {
                            u.hasLoot = false;
                            map.incrementLootCount();
                        }
                        //Code for when a unit escapes
                        unitsInVan.Add(u);
                        u.transform.position = transform.position;
                        u.healthBar.toggleHealthBar(false);
                        map.removeUnit(u.gameObject);
                    }
                }
                //If all surviving units have got in the van, close the doors and drive off
                if (map.units.Count == 0)
                {
                    open = false;
                    foreach (GameObject d in doors)
                    {
                        d.GetComponent<TruckDoor>().toggleOpen(open);
                    }
                    escapeArea.toggleEscapeSquares(false);
                }
            }
        }
    }

    public void playSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
