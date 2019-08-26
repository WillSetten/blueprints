using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drill : MonoBehaviour
{
    public GameObject drill;
    public GameObject drillArea;
    TileMap map;
    public Vault vault;
    private float drillTime=15;
    private float setupTime = 2;
    private float timer = 0;
    public Image loadCircle;

    // Start is called before the first frame update
    void Start()
    {
        drill.GetComponent<SpriteRenderer>().color = Color.clear;
        map = GetComponentInParent<TileMap>();
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
        StartCoroutine(drillTimer());
    }

    public IEnumerator setupDrill()
    {
        while (timer < setupTime)
        {
            timer = timer + Time.deltaTime;
            loadCircle.fillAmount = timer / setupTime;
            yield return null;
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
        yield return new WaitForSeconds(drillTime);
        endDrilling();
    }
}
