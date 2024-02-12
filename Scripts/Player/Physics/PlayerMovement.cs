using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementState
{
    WALKING,
    SLOPE_SLIDING,
    SLIDING,
    WALLRUNNING,
    CLIMBING,
    FREEZE,
    SWINGING,
    AIR
}

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rig;
    private PlayerPhysics playerPhysics;
    private PlayerClimbing playerClimbing;

    [Header("Movement")]
    [HideInInspector]
    public float moveSpeed;
    public float walkSpeed = 6f;
    public float slideSpeed = 50f;
    public float slopeSlideSpeed = 75f;
    public float wallRunSpeed;
    public float climbSpeed;
    public float swingSpeed;
    public float grapplingHookJumpSpeed = 2f;

    //Momentum
    [HideInInspector]
    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Slope Handling")]
    [SerializeField]
    private float maxSlopeAngle;
    private RaycastHit slopeRaycast;

    private MovementState movementState;
    [SerializeField]
    private Transform playerOrientation;

    private float xInput, yInput;

    private Vector3 moveDir;

    [HideInInspector]
    public bool isSliding;
    [HideInInspector]
    public bool isWallRunning;
    [HideInInspector]
    public bool isClimbing;
    [HideInInspector]
    public bool isFreezing;
    [HideInInspector]
    public bool isSwinging;
    [HideInInspector]
    public bool isGrappleActive;
    [SerializeField] 
    private float speedIncreaseMultiplier;
    [SerializeField]
    private float slopeIncreaseMultiplier;

    private Dictionary<MovementState, IMovementState> movementStatesDict;

    private void Awake()
    {
        BuildStates();
    }

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        playerPhysics = GetComponent<PlayerPhysics>();
        playerClimbing = GetComponent<PlayerClimbing>();
    }

    private void BuildStates() 
    {
        movementStatesDict = new Dictionary<MovementState, IMovementState>();

        movementStatesDict.Add(MovementState.WALKING, new WalkingState());
        movementStatesDict.Add(MovementState.SLOPE_SLIDING, new SlopeSlidingState());
        movementStatesDict.Add(MovementState.SLIDING, new SlidingState());
        movementStatesDict.Add(MovementState.WALLRUNNING, new WallRunningState());
        movementStatesDict.Add(MovementState.CLIMBING, new ClimbingState());
        movementStatesDict.Add(MovementState.FREEZE, new FreezingState());
        movementStatesDict.Add(MovementState.SWINGING, new SwingingState());
        movementStatesDict.Add(MovementState.AIR, new AirState());
    }

    private void Update()
    {
        ControlSpeed();
        HandleMovementState();
        playerPhysics.isOnSlope = OnSlope();
        playerPhysics.isGrappleActive = isGrappleActive;
        Debug.Log(moveSpeed + ", " + desiredMoveSpeed);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public void MoveInput(float inputX, float inputY) 
    {
        xInput = inputX;
        yInput = inputY;
    }

    private void MovePlayer() 
    {
        if (playerClimbing.exitingWall || isGrappleActive)
            return;

        if (movementState == MovementState.SLOPE_SLIDING)
            yInput = 0f;

        moveDir = (playerOrientation.forward * yInput) + 
            (playerOrientation.right * xInput);

        if (OnSlope() && !playerPhysics.exitingSlope) 
        {
            rig.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);

            if (rig.velocity.y > 0)
                rig.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        bool isGrounded = playerPhysics.GetIsGrounded();
        float airMult = playerPhysics.GetAirMult();
        float speed = (isGrounded) ? moveSpeed * 10f : moveSpeed * 10f * airMult;

        rig.AddForce(moveDir.normalized * speed, ForceMode.Force);

        if(!isWallRunning)
            rig.useGravity = !OnSlope();
    }

    private void HandleMovementState() 
    {
        bool isGrounded = playerPhysics.GetIsGrounded();
        SelectMovementState(isGrounded);

        movementStatesDict[movementState].Execute(this);

        if (moveSpeed != desiredMoveSpeed && desiredMoveSpeed != lastDesiredMoveSpeed) 
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private void SelectMovementState(bool isGrounded) 
    {
        if (isFreezing)
        {
            movementState = MovementState.FREEZE;
        }
        else if (isSwinging)
        {
            movementState = MovementState.SWINGING;
        }
        else if (isClimbing)
        {
            movementState = MovementState.CLIMBING;
        }
        else if (isWallRunning)
        {
            movementState = MovementState.WALLRUNNING;
        }
        else if (OnSlope()) 
        {
            movementState = MovementState.SLOPE_SLIDING;
        }
        else if (isSliding)
        {
            movementState = MovementState.SLIDING;
        }
        else if (isGrounded)
        {
            movementState = MovementState.WALKING;
        }
        else
        {
            movementState = MovementState.AIR;
        }
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float startValue = moveSpeed;

        while (time < 1)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeRaycast.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return new WaitForSeconds(0.01f);
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void ControlSpeed() 
    {
        if (isGrappleActive)
            return;

        //Limiting speed on slope
        if (OnSlope() && !playerPhysics.exitingSlope)
        {
            if (rig.velocity.magnitude > moveSpeed)
            {
                rig.velocity = rig.velocity.normalized * moveSpeed;
            }
        }
        else 
        {
            Vector3 flatSpd = new Vector3(rig.velocity.x, 0f, rig.velocity.z);

            if (flatSpd.magnitude > moveSpeed)
            {
                Vector3 limitedSpd = flatSpd.normalized * moveSpeed;
                rig.velocity = new Vector3(limitedSpd.x, rig.velocity.y, limitedSpd.z);
            }
        }
    }

    public bool OnSlope() 
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeRaycast, playerPhysics.GetPlayerHeight() * 0.5f + 0.3f)) 
        {
            float angle = Vector3.Angle(Vector3.up, slopeRaycast.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) 
    {
        return Vector3.ProjectOnPlane(direction, slopeRaycast.normal).normalized;
    }

    public float GetXInput() 
    {
        return xInput;
    }

    public float GetYInput() 
    {
        return yInput;
    }

    public Rigidbody GetRig()
    {
        return rig;
    }

    public MovementState GetPlayerMovementState() 
    {
        return movementState;
    }

    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight) 
    {
        isGrappleActive = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity() 
    {
        enableMovementOnNextTouch = true;
        rig.velocity = velocityToSet;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight) 
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(Mathf.Abs(-2f * gravity * trajectoryHeight));
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(Mathf.Abs(-2f * trajectoryHeight / gravity)) 
            + Mathf.Sqrt(Mathf.Abs(2f * (displacementY - trajectoryHeight) / gravity)));

        return (grapplingHookJumpSpeed * velocityXZ) + velocityY;
    }

    public void ResetRestrictions() 
    {
        isGrappleActive = false;
        moveSpeed = desiredMoveSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch) 
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<PlayerGrappling>().StopGrapple();
        }
    }
}
