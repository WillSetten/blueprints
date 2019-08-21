using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    HingeJoint2D hinge;
    AudioSource audioSource;
    public AudioClip vaultOpen;

    private void Start()
    {
        hinge = GetComponent<HingeJoint2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnMouseUp()
    {
        if (!hinge.useMotor) {
            Debug.Log("Opening Vault door");
            hinge.useMotor = true;
            audioSource.PlayOneShot(vaultOpen);
        }
    }
}