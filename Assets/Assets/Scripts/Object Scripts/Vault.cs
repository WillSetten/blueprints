using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    HingeJoint2D hinge;
    AudioSource audioSource;
    public AudioClip vaultOpen;
    public Drill drill;

    private void Start()
    {
        hinge = GetComponent<HingeJoint2D>();
        audioSource = GetComponent<AudioSource>();
        drill.vault = this;
    }

    private void OnMouseUp()
    {
        drill.beginDrilling();
    }
}