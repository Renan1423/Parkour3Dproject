using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunning : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform orientation;
    private PlayerMovement playerMovement;
    private PlayerPhysics playerPhysics;
    private Rigidbody rig;
    private CameraRotator cameraRotator;

    [Header("Wall Running")]
    [SerializeField]
    private LayerMask whatIsWall;
    [SerializeField]
    private LayerMask whatIsGround;
    [SerializeField]
    private float wallRunForce;
    [SerializeField]
    private float maxWallRunTime;
    private float wallRunTimer;

    [Header("Wall Jump")]
    [SerializeField]
    private float wallJumpUpForce;
    [SerializeField]
    private float wallJumpSideForce;

    [Header("Vertical Wall Running")]
    [SerializeField]
    private float verticalWallRunSpeed;
    [SerializeField]
    private float minCamRotationToVerticalMovement = 10f;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    [SerializeField]
    private float wallCheckDistance;
    [SerializeField]
    private float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    [SerializeField]
    private float exitWallTime;
    private float exitWallTimer;
    [HideInInspector]
    public bool exitingWall;

    private GameObject lastWall;

    [Header("Gravity")]
    [SerializeField]
    private bool useGravity;
    [SerializeField]
    private float gravityCounterForce;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        playerPhysics = GetComponent<PlayerPhysics>();
        cameraRotator = CameraRotator.instance;
    }

    private void Update()
    {
        CheckForWall();
        //Debug.Log(exitingWall + "," + exitWallTimer);

        if (!playerMovement.isWallRunning || exitingWall)
            WallRunStateMachine(horizontalInput, verticalInput);

        HandleWallRunTime();

        if (playerPhysics.GetIsGrounded())
            lastWall = null;
    }

    private void CheckForWall() 
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, 
            out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right,
            out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround() 
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    public void WallRunStateMachine(float dirX, float dirY) 
    {
        horizontalInput = dirX;
        verticalInput = dirY;

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            //Start wall run
            if (!playerMovement.isWallRunning)
                StartWallRun();
        }
        else if (exitingWall) 
        {
            if (playerMovement.isWallRunning) 
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        else
        {
            if (playerMovement.isWallRunning)
                StopWallRun();
        }
    }

    private void FixedUpdate()
    {
        if (playerMovement.isWallRunning)
            WallRunMovement();
    }

    private void HandleWallRunTime() 
    {
        if (!playerMovement.isWallRunning)
            return;

        wallRunTimer -= Time.deltaTime;

        if (wallRunTimer <= 0)
        {
            exitingWall = true;
            exitWallTimer = exitWallTime;
        }
    }

    private void StartWallRun()
    {
        if ((wallRight && lastWall == rightWallHit.transform.gameObject) 
            || (wallLeft && lastWall == leftWallHit.transform.gameObject)) 
            return;

        if (wallRight) lastWall = rightWallHit.transform.gameObject;
        else if (wallLeft) lastWall = leftWallHit.transform.gameObject;

        playerMovement.isWallRunning = true;

        wallRunTimer = maxWallRunTime;

        rig.velocity = new Vector3(rig.velocity.x, 0f, rig.velocity.z);

        cameraRotator.DoFov(65);
        if (wallLeft) cameraRotator.DoTilt(-10f);
        else if (wallRight) cameraRotator.DoTilt(10f);
    }

    private void WallRunMovement() 
    {
        rig.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) 
        {
            wallForward = -wallForward;
        }
        //Forward Force
        rig.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //Up/Downwards Force
        //float xCamRotation = cameraRotator.gameObject.transform.rotation.x;

        //if (Mathf.Abs(xCamRotation) > minCamRotationToVerticalMovement)
        //    rig.velocity = new Vector3(rig.velocity.x, verticalWallRunSpeed * (xCamRotation / Mathf.Abs(xCamRotation)), rig.velocity.z);

        //Pushing to wall
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rig.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
            rig.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        playerMovement.isWallRunning = false;

        cameraRotator.DoFov(75);
        cameraRotator.DoTilt(0);
    }

    public void WallJump() 
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rig.velocity = new Vector3(rig.velocity.x, 0f, rig.velocity.z);
        rig.AddForce(forceToApply, ForceMode.Impulse);
    }
}
