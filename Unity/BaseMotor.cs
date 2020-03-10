//This class is a motor for an action Platformer.
//This class is not expected to be used on its own. This is a parent class for all movent motors

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMotor : MonoBehaviour
{
    public Del dashEnded;	//A delegate that fires at the end of a dash. this is mostly used for combos
    public PublicEnums.Direction direction;	//The Direction this character if facing

    public float maxSpeed;
    public float jumpForce;
    protected int jumpCount;
    public int maxJumps;

    public float inAirSpeed;
    public float inAirAccel;

    protected float initJumpForce;

    public bool onGround;

    public delegate void Del();

    protected Vector2 currentSpeed;

    [HideInInspector]
    public float height;

    public bool stun;

    public float gravity = 10f;
    float initGravity;

    protected Rigidbody2D rigid;

    public Character character;

    protected PublicEnums.CurrentMovement currentMovement;

    public BaseController controller;

    public float dashDist, upperCutDist;

    // Use this for initialization
    private void Awake()
    {
        initGravity = gravity;
        direction = PublicEnums.Direction.Right;

        initJumpForce = jumpForce;
        height = transform.lossyScale.y;
        stun = false;

        rigid = GetComponent<Rigidbody2D>();
        currentMovement = PublicEnums.CurrentMovement.None;
        if (!controller)
            controller = GetComponent<BaseController>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!stun && currentMovement == PublicEnums.CurrentMovement.None)
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
        }
    }

    public virtual void Move(float multiplier)
    {
        \\Do Nothing
    }
    public virtual void MoveLeft(float multiplier)
    {
        \\Do Nothing
    }
    public virtual void Move()
    {
        currentSpeed = maxSpeed * PublicEnums.DirectionToVector2(direction);
    }
    public void MoveLeft()
    {
        MoveLeft(1);
    }
    public virtual void MoveLeftInAir(float multiplier)
    {
        \\Do Nothing
    }
    public void MoveLeftInAir()
    {
        MoveLeftInAir(1);
    }
    public virtual void MoveRight(float multiplier)
    {
        \\Do Nothing
    }
    public void MoveRight()
    {
        MoveRight(1);
    }
    public virtual void MoveRightInAir(float multiplier)
    {
        \\Do Nothing
    }
    public void MoveRightInAir()
    {
        MoveRightInAir(1);
    }
    public virtual void MoveInAir(float multiplier)
    {
        \\Do Nothing
    }
    public virtual void Jump()
    {
        \\Do Nothing
    }

    public virtual void Dash(PublicEnums.Direction dir, float distance)
    {
        gravity *= 0f;
        dashEnded += ResetGravity;
    }
    public virtual void Dash(float distance)
    {
        Dash(direction, distance);
        gravity *= 0f;
        dashEnded += ResetGravity;
    }
    public virtual void DiveKick()
    {
        
    }
    public virtual void Step()
    {
        StartCoroutine("StepAction");
    }
    public virtual void Charge()
    {

    }

    public IEnumerator StepAction()
    {
        SetCurrentMovement(PublicEnums.CurrentMovement.Step);
        Vector2 startPos = rigid.position;

        Vector2 dir = direction == PublicEnums.Direction.Right ? Vector2.right : Vector2.left; 

        while(Vector2.Distance(rigid.position, startPos) < 0.5)
        {
            rigid.position += dir * maxSpeed * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        startPos = rigid.position;
        dir = -dir;
        while (Vector2.Distance(rigid.position, startPos) < 0.25)
        {
            rigid.position += dir * maxSpeed * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        currentMovement = (PublicEnums.CurrentMovement.None);
        controller.ReturnControl();
    }

    public void Stun(float duration)
    {
        if (stun)
        {
            CancelInvoke("EndStun");
        }
        stun = true;
        StopMomentum();
        Invoke("EndStun", duration);
    }

    public bool Higher(GameObject o) //returns true when this character is higer than o
    {
        return o.transform.position.y < (transform.position.y - (height * 0.5f));
    }
    public bool Lower(GameObject o) //returns true wheh this character is lower than o
    {
        return o.transform.position.y > (transform.position.y + (height * 0.5f));
    }

    public void EndStun()
    {
        currentSpeed.x = 0;
        stun = false;
    }

    public void StopMomentum()	//Stop Horizontal Movement
    {
        currentSpeed.x = 0;
    }
    public void FullStopMomentum()	//stop all movement
    {
        currentSpeed = Vector2.zero;
    }
    public void SetOnGround(bool grounded)//Changes state from in air to on ground and vice versa
    {
        onGround = grounded;
        if (onGround)
        {
            currentSpeed.y = 0;//Stops the gravity vector

            if(currentMovement == PublicEnums.CurrentMovement.DiveKick)
            {
                StopCurrentMovement();	//End DiveKick to avoid weird sliding
            }

            ResetJumps();//Resets ability to double and tripple jump
        }
    }
    public void ResetJumps()
    {
        jumpCount = 0;//Now they can jump again
        jumpForce = initJumpForce;//sets jumpForce to the correct value 
    }
    protected void SetCurrentMovement(PublicEnums.CurrentMovement movement)
    {
        if(currentMovement != PublicEnums.CurrentMovement.None || 
            movement == PublicEnums.CurrentMovement.None)
        {
            StopCurrentMovement();//If the current movement is None then stop whatever was happening
        }

        currentMovement = movement;
        FullStopMomentum();//Full stop so there aren't any surprises with the vectors adding together.
    }
    public void StopCurrentMovement()
    {
        switch (currentMovement)
        {
            case PublicEnums.CurrentMovement.DiveKick:
                StopCoroutine("DiveDash");
                break;
            case PublicEnums.CurrentMovement.DashLeft:
                StopCoroutine("DashLeft");
                break;
            case PublicEnums.CurrentMovement.DashRight:
                StopCoroutine("DashRight");
                break;
            case PublicEnums.CurrentMovement.DashUp:
                StopCoroutine("DashUp");
                break;
            case PublicEnums.CurrentMovement.DashDown:
                StopCoroutine("DashDown");
                break;
            case PublicEnums.CurrentMovement.Step:
                StopCoroutine("StepAction");
                break;
            case PublicEnums.CurrentMovement.Charge:
                StopCoroutine("ChargeMove");
                break;
            default:
                break;
        }
        currentMovement = PublicEnums.CurrentMovement.None;	//reset currentMovement to None
        controller.ReturnControl();	//Some actions may remove control. This line is a failsafe.
    }
    protected void ResetGravity()	//Sets gravity to the value it was when this object was initiated
    {
        gravity = initGravity;
    }
}
