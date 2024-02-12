using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform orientation;
    private Rigidbody rig;
    private PlayerMovement playerMovement;
    private PlayerPhysics playerPhysics;
    private CameraRotator cameraRotator;

    [Header("Sliding")]
    [SerializeField]
    private float maxSlideTime;
    [SerializeField]
    private float slideForce;
    private float slideTimer;

    [SerializeField]
    private float slideYScale;
    private float startYScale;

    private float slideXDir;
    private float slideYDir;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        playerPhysics = GetComponent<PlayerPhysics>();
        cameraRotator = CameraRotator.instance;

        startYScale = transform.localScale.y;
    }

    public void Slide() 
    {
        if (playerMovement.GetXInput() != 0 || playerMovement.GetYInput() != 0 
            && playerPhysics.GetIsGrounded() && !playerMovement.isSliding) 
        {
            StartSlide(playerMovement.GetXInput(), playerMovement.GetYInput());
        }
    }

    private void FixedUpdate()
    {
        if (playerMovement.GetPlayerMovementState() == MovementState.SLOPE_SLIDING
            || playerMovement.isSliding) 
        {
            SlidingMovement();
        }

        if (playerMovement.OnSlope())
            StartSlide(0f, 1f);
    }

    private void StartSlide(float xInput, float yInput) 
    {
        playerMovement.isSliding = !playerMovement.OnSlope();
        slideXDir = xInput;
        slideYDir = yInput;

        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rig.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

        //VFX
        cameraRotator.DoFov(60f);
    }

    private void SlidingMovement() 
    {
        Vector3 inputDirection = orientation.forward * slideYDir + orientation.right * slideXDir;

        if (!playerMovement.OnSlope() /*|| rig.velocity.y > -0.1f*/)
        {
            rig.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else 
        {
            rig.AddForce(playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce * 2, ForceMode.Force);
            rig.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        }

        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    public void StopSlide() 
    {
        playerMovement.isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        //VFX
        cameraRotator.DoFov(75f);
    }
}
