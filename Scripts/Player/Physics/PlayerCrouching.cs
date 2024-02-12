using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouching : MonoBehaviour
{
    private Rigidbody rig;

    [SerializeField]
    private float crouchYScale;
    private float startYScale;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();

        startYScale = transform.localScale.y;
    }

    public void Crouch() 
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rig.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    public void StopCrouch() 
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
}
