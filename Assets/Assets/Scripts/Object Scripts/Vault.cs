using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vault : MonoBehaviour
{
    public HingeJoint2D hinge;

    private void OnMouseUp()
    {
        Debug.Log("Opening Vault door");
        hinge.useMotor=true;
    }
}