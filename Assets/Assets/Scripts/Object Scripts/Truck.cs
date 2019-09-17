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
    Vector3 heistStartPos;

    private void Start()
    {
        map = FindObjectOfType<TileMap>();
        audioSource = GetComponent<AudioSource>();
        foreach (GameObject d in doors)
        {
            d.GetComponent<TruckDoor>().truck = this;
            d.SetActive(false);
        }
        escapeArea.toggleEscapeSquares(false);
        heistStartPos = new Vector3(transform.position.x, transform.position.y-7);
        foreach (GameObject u in map.units)
        {
            u.SetActive(false);
        }
        StartCoroutine(moveTruckIn());
    }
    
    IEnumerator moveTruckIn()
    {
        while (transform.position.y > heistStartPos.y)
        {
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            GetComponent<Rigidbody2D>().velocity = new Vector3(0, -5, 0);
            map.viewingCamera.transform.parent.transform.position = new Vector3(transform.position.x, transform.position.y,-10);
            yield return null;
        }
        transform.position = heistStartPos;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        open = true;
        foreach (GameObject d in doors)
        {
            d.SetActive(true);
            d.GetComponent<TruckDoor>().toggleOpen(open);
        }
        yield return new WaitForSeconds(0.5f);
        escapeArea.toggleEscapeSquares(true);
        foreach (GameObject g in map.units)
        {
            g.SetActive(true);
            Unit u = g.GetComponent<Unit>();
            u.tileX = (int)g.transform.position.x;
            u.tileY = (int)g.transform.position.y;
            map.GeneratePathTo((int)escapeArea.escapeSquares[0].transform.position.x, (int)escapeArea.escapeSquares[0].transform.position.y, u);
        }
    }

    private void OnMouseOver()
    {
        //When the van is clicked on, check if there are units in the escape area.
        if (Input.GetMouseButtonDown(0) && Time.timeScale != 0)
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
                                map.UIhandler.incrementLootCount();
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
        else if (Input.GetMouseButtonDown(1) && Time.timeScale != 0)
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
                            map.UIhandler.incrementLootCount();
                        }
                        //Code for when a unit escapes
                        unitsInVan.Add(u);
                        u.transform.position = transform.position;
                        u.healthBar.toggleHealthBar(false);
                        map.removeUnit(u.gameObject);
                    }
                }
                //If all surviving units have got in the van, close the doors and drive off
                if (map.heisterCount() == 0)
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
