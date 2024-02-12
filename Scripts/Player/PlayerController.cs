using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement playerMovement;
    [SerializeField]
    private PlayerJump playerJump;
    [SerializeField]
    private PlayerCrouching playerCrouch;
    [SerializeField]
    private PlayerSliding playerSliding;
    [SerializeField]
    private PlayerWallRunning playerWallRunning;
    [SerializeField]
    private PlayerClimbing playerClimbing;
    [SerializeField]
    private PlayerGrappling playerGrappling;
    [SerializeField]
    private PlayerSwinging playerSwinging;

    public void MovePlayer(InputAction.CallbackContext value) 
    {
        Vector2 dir = value.ReadValue<Vector2>();

        if (playerSwinging.GetJoint() != null) 
        {
            playerSwinging.ExtendSwingCable(dir.x, dir.y);
        }

        playerMovement.MoveInput(dir.x, dir.y);

        if(!playerWallRunning.exitingWall)
            playerWallRunning.WallRunStateMachine(dir.x, dir.y);
    }

    public void Jump(InputAction.CallbackContext value) 
    {
        if (value.canceled)
            return;

        if (playerSwinging.GetJoint() != null) 
        {
            playerSwinging.ShortenSwingCable();
            return;
        }

        if (playerMovement.isWallRunning)
            playerWallRunning.WallJump();
        else if (playerMovement.isClimbing)
            playerClimbing.ClimbJump();
        else
            playerJump.Jump();
    }

    public void Crouch(InputAction.CallbackContext value)
    {
        if (value.canceled) 
        {
            playerCrouch.StopCrouch();
            return;
        }

        playerCrouch.Crouch();
    }

    public void Slide(InputAction.CallbackContext value) 
    {
        //if (value.canceled)
        //{
        //    playerSliding.StopSlide();
        //    return;
        //}

        playerSliding.Slide();
    }

    public void ThrowGrapple(InputAction.CallbackContext value) 
    {
        if (value.canceled)
        {
            playerSliding.StopSlide();
            return;
        }

        playerGrappling.StartGrappling();
    }

    public void Swing(InputAction.CallbackContext value)
    {
        if (value.canceled)
        {
            playerSwinging.StopSwing();
            return;
        }

        playerSwinging.StartSwing();
    }
}
