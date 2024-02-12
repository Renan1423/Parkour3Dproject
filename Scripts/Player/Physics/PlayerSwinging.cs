using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwinging : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private LineRenderer lr;
    [SerializeField]
    private Transform gunTip, cam, player;
    [SerializeField]
    private LayerMask whatIsGrappleable;
    private PlayerMovement playerMovement;
    private PlayerGrappling playerGrappling;

    [Header("Swinging")]
    [SerializeField]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;

    [Header("Swing Movement")]
    [SerializeField]
    private Transform orientation;
    private Rigidbody rig;
    [SerializeField]
    private float horizontalThrustForce;
    [SerializeField]
    private float forwardThrustForce;
    [SerializeField]
    private float shortenCableForce;
    [SerializeField]
    private float extendCableSpeed;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    [SerializeField]
    private float predictionSphereCastRadius;
    [SerializeField]
    private Transform predictionPoint;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerGrappling = GetComponent<PlayerGrappling>();
        rig = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CheckForSwingPoints();
    }

    public void StartSwing() 
    {
        if (predictionHit.point == Vector3.zero)
            return;

        playerMovement.isSwinging = true;

        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }

    public void ExtendSwingCable(float dirX, float dirY)
    {
        //float xMult = 0;
        //if (dirX != 0) 
        //{
        //    xMult = (dirX > 0) ? 1 : -1;
        //    rig.AddForce(xMult * orientation.right * horizontalThrustForce * Time.deltaTime);
        //}
        //if (dirY > 0)
        //    rig.AddForce(xMult * orientation.forward * forwardThrustForce * Time.deltaTime);

        if (dirY < 0 && predictionHit.point.y > player.position.y) 
        {
            //extend the cable
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) * extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    public void ShortenSwingCable() 
    {
        Vector3 directionToPoint = swingPoint - transform.position;
        rig.AddForce(directionToPoint.normalized * shortenCableForce * Time.deltaTime);

        float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;
    }

    private void CheckForSwingPoints()
    {
        if (joint != null || playerMovement.isGrappleActive)
            return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
            out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
            out raycastHit, maxSwingDistance, whatIsGrappleable);

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

        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else 
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = (raycastHit.point == Vector3.zero) ? sphereCastHit : raycastHit;
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope() 
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    public void StopSwing() 
    {
        playerMovement.isSwinging = false;

        lr.positionCount = 0;
        Destroy(player.gameObject.GetComponent<SpringJoint>());
        Destroy(joint);
    }

    public SpringJoint GetJoint() 
    {
        return joint;
    }
}
