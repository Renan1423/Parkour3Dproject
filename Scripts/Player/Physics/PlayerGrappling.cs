using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement playerMovement;
    [SerializeField]
    private Transform cam;
    [SerializeField]
    private Transform gunTip;
    [SerializeField]
    private LayerMask whatIsGrappleable;
    [SerializeField]
    private LineRenderer lineRenderer;
    private PlayerSwinging playerSwinging;

    [Header("Grappling")]
    [SerializeField]
    private float maxGrappleDistance;
    [SerializeField]
    private float grappleDelayTime;
    [SerializeField]
    private float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField]
    private float grapplingCd;
    private float grapplingCdTimer;

    private bool isGrappling;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    [SerializeField]
    private float predictionSphereCastRadius;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerSwinging = GetComponent<PlayerSwinging>();
    }

    private void Update()
    {
        if (grapplingCdTimer > 0) 
            grapplingCdTimer -= Time.deltaTime;

        CheckForGrapplePoints();
    }

    private void LateUpdate()
    {
        if (isGrappling) 
        {
            lineRenderer.SetPosition(0, gunTip.position);
        }
    }

    public void StartGrappling() 
    {
        if (grapplingCdTimer > 0) return;

        isGrappling = true;

        playerSwinging.StopSwing();

        playerMovement.isFreezing = true;

        if (predictionHit.point != Vector3.zero)
        {
            grapplePoint = predictionHit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else 
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(1, grapplePoint);

        CameraRotator.instance.DoFov(60f);
    }

    private void ExecuteGrapple() 
    {
        playerMovement.isFreezing = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) 
            highestPointOnArc = overshootYAxis;

        playerMovement.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    private void CheckForGrapplePoints()
    {
        if (playerMovement.isGrappleActive || playerMovement.isSwinging)
            return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
            out sphereCastHit, maxGrappleDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
            out raycastHit, maxGrappleDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        //Real Hit Point
        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        //Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        //Not hitting
        else
            realHitPoint = Vector3.zero;

        predictionHit = (raycastHit.point == Vector3.zero) ? sphereCastHit : raycastHit;
    }

    public void StopGrapple() 
    {
        playerMovement.isFreezing = false;
        playerMovement.moveSpeed = 30f;

        isGrappling = false;

        grapplingCdTimer = grapplingCd;

        lineRenderer.enabled = false;

        CameraRotator.instance.DoFov();
    }
}
