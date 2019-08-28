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

    private void Start()
    {
        hinge = GetComponent<HingeJoint2D>();
        audioSource = GetComponent<AudioSource>();
        drill.vault = this;
    }

    private void OnMouseUp()
    {
        if (drill.unitInDrillArea().currentState == Unit.state.Idle) {
            StartCoroutine(drill.setupDrill());
        }
    }

    public void openVault()
    {
        if (!open)
        {
            open = true;
            hinge.useMotor = true;
            GetComponentInChildren<Rigidbody2D>().freezeRotation = false;
            audioSource.PlayOneShot(vaultOpen);
        }
    }
}