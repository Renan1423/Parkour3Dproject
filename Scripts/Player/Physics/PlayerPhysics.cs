using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    private Rigidbody rig;

    [Header("Ground Check")]
    [SerializeField]
    private float playerHeight = 2f;
    [SerializeField]
    private LayerMask whatIsGround;
    private bool isGrounded;
    [SerializeField]
    private float groundDrag = 5f;
    [SerializeField]
    private float airMult = 0.5f;
    [HideInInspector]
    public bool exitingSlope = false;
    [HideInInspector]
    public bool isOnSlope = false;
    [HideInInspector]
    public bool isGrappleActive;
    [SerializeField]
    private float slopeDrag = 7.5f;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        rig.freezeRotation = true;
    }

    private void Update()
    {
        //Checking ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        HandleDrag();
    }

    private void HandleDrag()
    {
        if (isGrounded && !isOnSlope && !isGrappleActive)
            rig.drag = groundDrag;
        else if (isOnSlope && !isGrappleActive)
            rig.drag = slopeDrag;
        else
            rig.drag = 0;
    }

    public bool GetIsGrounded() 
    {
        return isGrounded;
    }

    public float GetAirMult()
    {
        return airMult;
    }

    public float GetPlayerHeight() 
    {
        return playerHeight;
    }
}
