using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    HingeJoint2D hinge;
    AudioSource audioSource;
    public AudioClip vaultOpen;
    public Drill drill;
    bool open = false;
    IEnumerator drillSetup;

    private void Start()
    {
        hinge = GetComponent<HingeJoint2D>();
        audioSource = GetComponent<AudioSource>();
        drill.vault = this;
        drillSetup = null;
    }

    private void OnMouseUp()
    {
        if (drill.unitInDrillArea().currentState == Unit.state.Idle && Time.timeScale != 0) {
            drill.unitInDrillArea().currentState = Unit.state.Interacting;
            if (drillSetup == null && !drill.enabled) {
                StartCoroutine(drill.setupDrill());
            }
        }
    }

    public void openVault()
    {
        if (!open)
        {
            open = true;
            hinge.useMotor = true;
            audioSource.PlayOneShot(vaultOpen);
        }
    }
}