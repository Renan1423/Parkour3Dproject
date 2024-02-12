using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform orientation;
    private Rigidbody rig;
    [SerializeField]
    private LayerMask whatIsWall;
    private PlayerPhysics playerPhysics;
    private PlayerMovement playerMovement;

    [Header("Climbing")]
    [SerializeField]
    private float climbSpeed;
    [SerializeField]
    private float maxClimbTime;
    private float climbTimer;

    private bool isClimbing;

    [Header("Climb Jumping")]
    [SerializeField]
    private float climbJumpUpForce;
    [SerializeField]
    private float climbJumpBackForce;

    [SerializeField]
    private int climbJumpsAmount;
    private int climbJumpsLeft;

    [Header("Detection")]
    [SerializeField]
    private float detectionLength;
    [SerializeField]
    private float sphereCastRadius;
    [SerializeField]
    private float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    [SerializeField]
    private float minWallNormalAngleChange;

    [Header("Exiting")]
    [SerializeField]
    private float exitWallTime;
    [HideInInspector]
    public bool exitingWall;
    private float exitWallTimer;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        playerPhysics = GetComponent<PlayerPhysics>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        WallCheck();
        ClimbStateMachine();

        if (isClimbing && !exitingWall) ClimbingMovement();
    }

    private void WallCheck() 
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius,
            orientation.forward, out frontWallHit, detectionLength, whatIsWall);

        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall ||
           Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || playerPhysics.GetIsGrounded()) 
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumpsAmount;
        }

    }

    private void ClimbStateMachine() 
    {
        if (wallFront && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!isClimbing && climbTimer > 0) StartClimbing();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer <= 0) StopClimbing();
        }
        else if (exitingWall) 
        {
            if (isClimbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }
        else 
        {
            if (isClimbing) StopClimbing();
        }
    }

    private void StartClimbing() 
    {
        isClimbing = true;
        playerMovement.isClimbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;

        // camera fov change
    }

    private void ClimbingMovement() 
    {
        rig.velocity = new Vector3(rig.velocity.x, climbSpeed, rig.velocity.z);
    }

    private void StopClimbing() 
    {
        isClimbing = false;
        playerMovement.isClimbing = false;
    }

    public void ClimbJump()
    {
        if (wallFront && climbJumpsLeft > 0)
        {
            exitingWall = true;
            exitWallTimer = exitWallTime;

            Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

            rig.velocity = new Vector3(rig.velocity.x, 0f, rig.velocity.z);
            rig.AddForce(forceToApply, ForceMode.Impulse);

            climbJumpsLeft--;
        }
    }
}
