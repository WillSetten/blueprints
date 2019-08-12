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
    //If there are units in the escape area, open the doors.
    private void OnMouseUp()
    {
        if (escapeArea.unitsInEscapeArea()) {
            if (open)
            {
                open = false;
            }
            else
            {
                open = true;
                playSound(doorOpen);
            }
            foreach (GameObject d in doors)
            {
                d.GetComponent<TruckDoor>().toggleOpen(open);
            }
        }
    }

    public void playSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
