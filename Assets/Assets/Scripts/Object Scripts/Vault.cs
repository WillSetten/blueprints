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
        if (drill.unitInDrillArea()) {
            drill.beginDrilling();
            GetComponentInParent<TileMap>().GeneratePathTo((int)drill.transform.position.x+1,(int)drill.transform.position.y, drill.unitInDrillArea());
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