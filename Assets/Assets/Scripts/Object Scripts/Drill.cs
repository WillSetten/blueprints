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
    private float drillTime = 15;
    private float setupTime = 8;
    private float timer = 0;
    public Image loadCircle;

    // Start is called before the first frame update
    void Start()
    {
        drill.SetActive(false);
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
        Debug.Log("Drill starting! Moving units out the way");
        map.GeneratePathTo((int)transform.position.x + 1, (int)transform.position.y, unitInDrillArea());
        drill.SetActive(true);
        drillArea.SetActive(false);
        timer = 0;
        StartCoroutine(drillTimer());
    }

    public IEnumerator setupDrill()
    {
        while (timer < setupTime)
        {
            //If there is a unit still present to set up the drill, increase the timer
            if (!map.paused && unitInDrillArea() && unitInDrillArea().currentState == Unit.state.Interacting) {
                timer = timer + Time.deltaTime;
            }
            //If there is no unit
            else if (!map.paused && (unitInDrillArea()==null || unitInDrillArea().currentState != Unit.state.Interacting) && timer > 0)
            {
                timer = timer - Time.deltaTime;
            }
            if (timer>0) {
                loadCircle.fillAmount = timer / setupTime;
            }
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
        loadCircle.enabled = false;
        drill.SetActive(false);
        vault.openVault();
    }

    IEnumerator drillTimer()
    {
        while (timer < drillTime) {
            if (!map.paused)
            {
                timer = timer + Time.deltaTime;
            }
            loadCircle.fillAmount = timer / drillTime;
            yield return null;
        }
        endDrilling();
    }
}
