using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private Rigidbody rig;
    private PlayerPhysics playerPhysics;
    private PlayerSliding playerSliding;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce = 10f;
    [SerializeField]
    private float jumpCooldown = 0.25f;
    private bool canJump = true;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
        playerPhysics = GetComponent<PlayerPhysics>();
        playerSliding = GetComponent<PlayerSliding>();
    }

    public void Jump()
    {
        bool isGrounded = playerPhysics.GetIsGrounded();

        if (canJump && isGrounded)
        {
            playerSliding.StopSlide();
            playerPhysics.exitingSlope = true;

            canJump = false;
            Invoke(nameof(ResetJump), jumpCooldown);

            //Reseting the vertical velocity
            rig.velocity = new Vector3(rig.velocity.x, 0f, rig.velocity.z);

            rig.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ResetJump()
    {
        canJump = true;
        playerPhysics.exitingSlope = false;
    }
}
