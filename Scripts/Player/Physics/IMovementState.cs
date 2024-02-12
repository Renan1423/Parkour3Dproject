using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementState
{
    public void Execute(PlayerMovement pm);
}

public class WalkingState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.desiredMoveSpeed = pm.walkSpeed;
        if (Mathf.Abs(pm.moveSpeed) <= 0.1f && pm.desiredMoveSpeed == pm.walkSpeed)
            pm.moveSpeed = pm.walkSpeed;
    }
}

public class SlopeSlidingState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.desiredMoveSpeed = pm.slopeSlideSpeed;
    }
}

public class SlidingState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.desiredMoveSpeed = pm.slideSpeed;
        pm.moveSpeed = pm.desiredMoveSpeed;
    }
}

public class WallRunningState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.desiredMoveSpeed = pm.wallRunSpeed;
    }
}

public class ClimbingState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.desiredMoveSpeed = pm.climbSpeed;
    }
}

public class FreezingState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.moveSpeed = 0f;
        pm.GetRig().velocity = Vector3.zero;
    }
}

public class SwingingState : IMovementState
{
    public void Execute(PlayerMovement pm)
    {
        pm.desiredMoveSpeed = pm.swingSpeed;
    }
}

public class AirState : IMovementState
{
    public void Execute(PlayerMovement playerMovement)
    {

    }
}
