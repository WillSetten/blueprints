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

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        foreach (GameObject door in doors)
        {
            door.GetComponent<TruckDoor>().truck = this;
        }
    }

    //When the van is clicked on, check if there are units in the escape area.
    private void OnMouseUp()
    {
        List<Unit> units = escapeArea.unitsInEscapeArea();
        if (units.Count>0) {
            bool hasLoot=false;
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
                if (hasLoot) {
                    Debug.Log("Unloading loot into van!");
                    foreach (Unit u in units)
                    {
                        if (u.hasLoot)
                        {
                            u.hasLoot = false;
                            lootCount = lootCount + 1;
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

    public void playSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
