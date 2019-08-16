using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    VaultDoor vaultDoor;
    HingeJoint2D hinge;
    // Start is called before the first frame update
    void Start()
    {
        vaultDoor = GetComponentInChildren<VaultDoor>();
        hinge = GetComponentInChildren<HingeJoint2D>();
    }

    private void OnMouseUp()
    {
        hinge.useMotor=true;
    }
}