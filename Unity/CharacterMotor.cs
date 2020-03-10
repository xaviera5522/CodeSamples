//This class is a motor for an action Platformer. 
//The movement handled in this class and BaseMotor are designed for a 2D character 
//that can be either the player or an enemy that uses Dashes as a primary form of movement

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotor : BaseMotor
{
    public float dashSpeed;
    //public float dashDist = 5;//moved to the Dash function

    public float diveKickDist = 5;
    public float diveKickSpeed = 10;

    public float upperCutSpeed = 10; //up Dash attack
    //public float upperCutDist = 5;//moved to the Dash function

    PublicEnums.Direction wallDirection; //relative direction to the wall the player is sliding on
    public bool sliding { get; set; }
    public float slideFallMultiplier = 0.33f;	//wall slide gravity reduction
    public float slideJumpMultiplier = 0.5f;	//wall jump reduction

    // Use this for initialization
    void Start ()
    {
        direction = PublicEnums.Direction.Right;	//Player character starts facing right (for now)

        initJumpForce = jumpForce;
        height = transform.lossyScale.y;
        stun = false;

        rigid = GetComponent<Rigidbody2D>();	//Get the Rigidbody
        sliding = false;

        ResetDiveEnd();	//clear dashEnded delegate
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
		if(!stun && currentMovement == PublicEnums.CurrentMovement.None)
        {
            rigid.MovePosition(Vector2.Lerp(rigid.position, 
                rigid.position + currentSpeed * Time.fixedDeltaTime, maxSpeed));
        }

        if (!onGround)
        {
            if (currentSpeed.x > 0)
            {
                currentSpeed.x -= inAirAccel;
                if (currentSpeed.x <= 0.25f)
                    currentSpeed.x = 0;
            }
            else if (currentSpeed.x < 0)
            {
                currentSpeed.x += inAirAccel;
                if (currentSpeed.x >= -0.25f)
                    currentSpeed.x = 0;
            }
            if (!sliding)
            {
                currentSpeed.y -= gravity * Time.fixedDeltaTime;
            }
            else
            {
                currentSpeed.y -= gravity * slideFallMultiplier * Time.fixedDeltaTime;
            }
        }
    }

    public override void Move(float multiplier)//Multiplier is for movement with a thumbstick
    {
        multiplier = Mathf.Clamp(multiplier, -1, 1);//just in case the multiplier goes out of bounds
        if(multiplier < 0)
        {
            direction = PublicEnums.Direction.Left;
        }
        else if(multiplier > 0)
        {
            direction = PublicEnums.Direction.Right;
        }
        if (!sliding || (sliding && direction != wallDirection))//sliding is checked by a collider
            currentSpeed.x = maxSpeed * multiplier;//move normally if the movement direction is oppisite of the sliding dirction
    }
    public override void MoveLeft(float multiplier)
    {
        direction = PublicEnums.Direction.Left;
        multiplier = Mathf.Clamp(multiplier, 0, 1);
        if (!sliding || (sliding && direction != wallDirection))
            currentSpeed.x = -maxSpeed * multiplier;
    }
    public override void MoveLeftInAir(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, 0, 1);
        currentSpeed.x -= multiplier * inAirAccel;
        direction = PublicEnums.Direction.Left;
        if (!sliding || (sliding && direction != wallDirection))
            currentSpeed.x = Mathf.Clamp(currentSpeed.x, -inAirSpeed, inAirSpeed);
    }
    public override void MoveRight(float multiplier)
    {
        direction = PublicEnums.Direction.Right;
        multiplier = Mathf.Clamp(multiplier, 0, 1);
        if (!sliding || (sliding && direction != wallDirection))
            currentSpeed.x = maxSpeed * multiplier;
    }
    public override void MoveRightInAir(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, 0, 1);
        currentSpeed.x = multiplier * inAirAccel;
        direction = PublicEnums.Direction.Right;
        if (!sliding || (sliding && direction != wallDirection))
            currentSpeed.x = Mathf.Clamp(currentSpeed.x, -inAirSpeed, inAirSpeed);
    }
    public override void MoveInAir(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, -1, 1);
        if (multiplier < 0)
        {
            direction = PublicEnums.Direction.Left;
        }
        else
        {
            direction = PublicEnums.Direction.Right;
        }
        if (!sliding || (sliding && direction != wallDirection))
            currentSpeed.x += inAirAccel * multiplier;

        currentSpeed.x = Mathf.Clamp(currentSpeed.x, -inAirSpeed, inAirSpeed);
    }
    public override void Jump()
    {
        if (jumpCount < maxJumps)
        {
            if(sliding)
            {
                currentSpeed.y = jumpForce * slideJumpMultiplier;
                float xForce = jumpForce / slideJumpMultiplier;
                currentSpeed.x = direction == PublicEnums.Direction.Right ? -xForce : xForce;
            }
            else
            {
                currentSpeed.y = jumpForce;
                currentSpeed.x *= 0.75f; //TODO: Also lose a lot of x-speed when the player jumps
            }

            if (jumpCount != 0)
                jumpForce = initJumpForce / ((float)jumpCount + 0.25f);

            jumpCount++;
        }
    }
    public IEnumerator DashLeft(float distance)
    {
        currentMovement = (PublicEnums.CurrentMovement.DashLeft);

        float dist = 0;
        float delta = 0;

        direction = PublicEnums.Direction.Left;

        while (dist < distance)
        {
            delta = dashSpeed * Time.fixedDeltaTime;
            dist += delta;

            rigid.position += Vector2.left * delta;
            yield return new WaitForFixedUpdate();
        }
        if (dashEnded != null)
        {
            dashEnded();
            dashEnded = null;
        }
        SetCurrentMovement(PublicEnums.CurrentMovement.None);
    }
    public IEnumerator DashRight(float distance)
    {
        SetCurrentMovement(PublicEnums.CurrentMovement.DashRight);

        float dist = 0;
        float delta = 0;

        direction = PublicEnums.Direction.Right;

        while (dist < distance)
        {
            delta = dashSpeed * Time.fixedDeltaTime;
            dist += delta;
            rigid.position += Vector2.right * delta;
            yield return new WaitForFixedUpdate();
        }
        if (dashEnded != null)
        {
            dashEnded();
            dashEnded = null;
        }
        SetCurrentMovement(PublicEnums.CurrentMovement.None);
    }
    public IEnumerator DashUp(float distance)
    {
        SetCurrentMovement(PublicEnums.CurrentMovement.DashUp);

        float dist = 0;
        float delta = 0; ;

        while (dist < distance)
        {
            delta = dashSpeed * Time.fixedDeltaTime;
            dist += delta;
            rigid.position += Vector2.up * delta;
            yield return new WaitForFixedUpdate();
        }
        if (dashEnded != null)
        {
            dashEnded();
            dashEnded = null;
        }
        SetCurrentMovement(PublicEnums.CurrentMovement.None);
    }
    public IEnumerator DashDown(float distance)
    {
        SetCurrentMovement(PublicEnums.CurrentMovement.DashDown);

        float dist = 0;
        float delta = 0;

        while (dist < distance)
        {
            delta = dashSpeed * Time.fixedDeltaTime;
            dist += delta;
            rigid.position += Vector2.down * delta;
            yield return new WaitForFixedUpdate();
        }
        if (dashEnded != null)
        {
            dashEnded();
            dashEnded = null;
        }
        SetCurrentMovement(PublicEnums.CurrentMovement.None);
    }
    public override void Dash(PublicEnums.Direction dir, float distance)
    {
        gravity *= 0.25f;
        dashEnded += ResetGravity;
        switch (dir)
        {
            case PublicEnums.Direction.Left:
                StartCoroutine(DashLeft(distance));
                break;
            case PublicEnums.Direction.Right:
                StartCoroutine(DashRight(distance));
                break;
            case PublicEnums.Direction.Up:
                StartCoroutine(DashUp(distance));
                break;
            case PublicEnums.Direction.Down:
                StartCoroutine(DashDown(distance));
                break;
            default:
                break;
        }
    }
    public void WallSlide() //CUT
    {
        //currentSpeed = Vector2.zero;//Could be a quick decceleration
        //wallDirection = direction;
        //sliding = true;
    }
    public void WallSlide(PublicEnums.Direction dir) //CUT
    {
        //currentSpeed = Vector2.zero;//Could be a quick decceleration
        //wallDirection = dir;
        //sliding = true;
    }
    public override void DiveKick()
    {
        Vector2 dir;

        switch (direction)
        {
            case PublicEnums.Direction.Left:
                dir = new Vector2(-1f, -1f);
                break;
            case PublicEnums.Direction.Right:
                dir = new Vector2(1f, -1f);
                break;
            default:
                dir = new Vector2(0, -1f);
                break;
        }
        StartCoroutine(DiveDash(dir.normalized));
    }
    public IEnumerator DiveDash(Vector2 dir)
    {
        SetCurrentMovement(PublicEnums.CurrentMovement.DiveKick);

        float dist = 0;
        float delta = 0;

        while(dist < diveKickDist)
        {
            delta = diveKickSpeed * Time.fixedDeltaTime;
            dist += delta;
            rigid.position += dir * delta;
            yield return new WaitForFixedUpdate();
        }
        if (dashEnded != null)
        {
            dashEnded();
            dashEnded = null;
        }
        SetCurrentMovement(PublicEnums.CurrentMovement.None);
    }

    public void EndKick()
    {
        StopCoroutine("DiveDash");

        if (dashEnded != null)
        {
            dashEnded();
            dashEnded = null;
        }
        SetCurrentMovement(PublicEnums.CurrentMovement.None);
    }
    public void ResetDiveEnd()
    {
        dashEnded = null;
    }
}
