using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drill : MonoBehaviour
{
    public GameObject drill;
    public GameObject drillArea;
    AudioSource audioSource;
    public AudioClip drillSound;
    TileMap map;
    public Vault vault;
    private float drillTime=15;
    private float setupTime = 8;
    private float timer = 0;
    public Image loadCircle;

    // Start is called before the first frame update
    void Start()
    {
        drill.GetComponent<SpriteRenderer>().color = Color.clear;
        map = GetComponentInParent<TileMap>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    //Returns true if there are units present in the drill area
    public Unit unitInDrillArea()
    {
        if (Physics2D.OverlapCircleAll(drillArea.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits")).Length != 0)
        {
            return Physics2D.OverlapCircleAll(drillArea.transform.position, 0.1f, LayerMask.GetMask("PlayerUnits"))[0].GetComponent<Unit>();
        }
        return null;
    }

    //Begins the drill timer
    public void beginDrilling()
    {
        map.GeneratePathTo((int)transform.position.x + 1, (int)transform.position.y, unitInDrillArea());
        drill.GetComponent<SpriteRenderer>().color = Color.white;
        drill.GetComponent<Rigidbody2D>().simulated = true;
        drillArea.GetComponent<SpriteRenderer>().color = Color.clear;
        loadCircle.color = Color.clear;
        timer = 0;
        StartCoroutine(drillTimer());
    }

    public IEnumerator setupDrill()
    {
        while (timer < setupTime)
        {
            if (!map.paused) {
                timer = timer + Time.deltaTime;
            }
            loadCircle.fillAmount = timer / setupTime;
            yield return null;
        }
        //If the alarm has not already been set off, set it off
        if (!map.enemyController.alarm)
        {
            map.enemyController.triggerAlarm();
        }
        beginDrilling();
    }

    //Ends the drilling and opens the vault, also removing the drill sprite and stopping its physics simulation
    void endDrilling()
    {
        drill.GetComponent<Rigidbody2D>().simulated = false;
        drill.GetComponent<SpriteRenderer>().color = Color.clear;
        vault.openVault();
    }

    IEnumerator drillTimer()
    {
        while (timer < drillTime) {
            if (!map.paused)
            {
                timer = timer + Time.deltaTime;
            }
            yield return null;
        }
        endDrilling();
    }
}
