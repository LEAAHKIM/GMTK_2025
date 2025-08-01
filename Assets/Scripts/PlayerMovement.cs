using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    const int GROUNDLAYER = 1 << 6;

    private Rigidbody2D _rb;
    private float _movementInput = 0;
    private bool _jumpPressInput;
    private bool _jumpHoldInput;

    //for jump buffering
    private float _lastJumpPressTime = -1;
    private bool _jumpBufferingActive { get { return (Time.time - _lastJumpPressTime) <= jumpBufferingTime; } }

    private float _lastOnGroundTime = -1;
    private bool _coyoteTimeActive { get { return (Time.time - _lastOnGroundTime) <= coyoteTime; } }

    //i use _jumping and _jumpapexy
    private bool _jumping;

    [Header("Walking")]
    public float maxSpeed;
    // time it takes to reach full speed
    public float accelerationTime;
    public float friction;

    [Header("onground check")]
    public Vector2 ongroundBoxOffset;
    public Vector2 ongroundBoxSize;

    [Header("Jumping")]
    public float jumpHeight = 2;
    public float jumpApexTime = 1;

    // if not holding space, multiply gravity with this;
    //public float stopJumpGravityMultiplier = 2;
    
    
    // to make sure gravity doesnt make player lose control. if y velocity is < -this, we set it equal to this.
    public float maxFallVelocity = 30;
    public float jumpBufferingTime = 0.1f;
    public float coyoteTime = 0.2f;
    public float jumpApexGravityMult = 0.75f;
    public float jumpApexWhenAbsVelYIsSmallerThan = 0.4f;
    public float jumpBreakVelYMult = 0.5f;

    private bool _isJumpCut;
    private bool _holdingJump;
    private float _gravity;
    private float _initialJumpVelocity;
    private StatesManager _statesManager;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _statesManager = GetComponent<StatesManager>();
    }
    private void Start()
    {
        if (jumpApexTime <= 0) { Debug.LogError("jumpapextime can't be negative or 0"); }
        _gravity = (2 * jumpHeight) / (jumpApexTime * jumpApexTime);
        _initialJumpVelocity = (2 * jumpHeight) / jumpApexTime;
        InputSystem.current.actions.Player.Move.performed += ctx => { _movementInput = ctx.ReadValue<float>(); };
        InputSystem.current.actions.Player.Move.canceled += ctx => { _movementInput = 0; };
        InputSystem.current.actions.Player.Jump.performed += ctx => { _jumpPressInput = true; _jumpHoldInput = true; };
        InputSystem.current.actions.Player.Jump.canceled += ctx => { _jumpHoldInput = false; };
    }
    private void FixedUpdate()
    {
        Vector2 velocity = _rb.velocity;
        // handles walking, friction and stuff
        ApplyXMovement();
        // handles jumping, gravity and stuff
        ApplyYMovement();
        _rb.velocity = velocity;

        //since it is a press input, once we process it we dont need it
        _jumpPressInput = false;

        void ApplyXMovement()
        {
            float speedModifier = _statesManager.GetSpeedModifier();

            //we want friction to not affect movement.
            if (_movementInput == 0)
            {
                float velSign = velocity.x > 0 ? 1 : -1;
                velocity.x = Mathf.Max(Mathf.Abs(velocity.x) - friction * Time.fixedDeltaTime, 0) * velSign;
            }

            //for smoothness
            if ((velocity.x > 0 && _movementInput < 0) || (velocity.x < 0 && _movementInput > 0)) { velocity.x = 0; }
            //movement
            velocity.x += (1 / accelerationTime) * maxSpeed * _movementInput * speedModifier * Time.fixedDeltaTime;
            if (velocity.x > maxSpeed) { velocity.x = maxSpeed; }
            else if (velocity.x < -maxSpeed) { velocity.x = -maxSpeed; }
        }

        void ApplyYMovement()
        {            
            bool onground = Physics2D.BoxCast((Vector2)transform.position + ongroundBoxOffset, ongroundBoxSize, 0, Vector2.zero, 0, GROUNDLAYER);

            //game feel stuff

            if (!_jumpHoldInput) { _holdingJump = false; if (!_isJumpCut && _jumping) { velocity.y *= jumpBreakVelYMult; _isJumpCut = true; } }
            if (_jumpPressInput) { _lastJumpPressTime = Time.time; }
            if (onground) { _lastOnGroundTime = Time.time; _jumping = false; }

            //gravity
            float currentGravity = _statesManager.GetGravityModifier() * _gravity;

            if (_holdingJump && _jumping && (Mathf.Abs(velocity.y) < jumpApexWhenAbsVelYIsSmallerThan)) { currentGravity *= jumpApexGravityMult; }
            //if (!_holdingJump && _jumping) { currentGravity *= stopJumpGravityMultiplier; }
            velocity.y -= currentGravity * Time.fixedDeltaTime;

            //fall vel limit
            if (velocity.y < -maxFallVelocity) { velocity.y = -maxFallVelocity; }

            //jump
            if (_coyoteTimeActive && _jumpBufferingActive)
            {
                _lastOnGroundTime = -1;
                _lastJumpPressTime = -1;
                velocity.y = _initialJumpVelocity;
                _holdingJump = true;
                _jumping = true;
                _isJumpCut = false;
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((Vector2)transform.position + ongroundBoxOffset, ongroundBoxSize);
    }
}
