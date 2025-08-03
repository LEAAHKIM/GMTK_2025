using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //includes movingplatform layer too since it moves as well
    public const int GROUNDLAYER = (1 << 6);
    public const int MOVINGPLATFORMLAYER = (1 << 7);


    [HideInInspector] public Rigidbody2D _rb;
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
    private SpriteRenderer[] _ghosts;

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

    public Animator _animator;
    private SpriteRenderer _spriteRend;
    private bool _lookingRight;
    public static int walkAnim = Animator.StringToHash("Walk");
    public static int turnIntoCloudAnim = Animator.StringToHash("TurnIntoCloud");
    public static int turnIntoPlayerAnim = Animator.StringToHash("TurnIntoPlayer");
    public static int turnAnim = Animator.StringToHash("Turn");
    public static int jumpAnim = Animator.StringToHash("Jump");
    public static int jumpEndAnim = Animator.StringToHash("JumpEnd");
    public static int chibiPoseAnim = Animator.StringToHash("ChibiPose");

    public bool freezeMovement = false;
    private Vector2 freezeAddedMovement = Vector2.zero;

    [HideInInspector] public Vector2 _prevPosition;
    public Transform spriteRendererTransform;
    private float lastFixedUpdateTime;

    private InputAction moveAction;
    private InputAction jumpAction;


    //CHARACTER TALK STUFF
    public List<AudioClip> jumpAudios;  
    public void FreezePlayerMovement()
    {
        freezeAddedMovement = _rb.velocity;
        _rb.bodyType = RigidbodyType2D.Static;
        freezeMovement = true;
    }
    public void UnFreezePlayerMovement()
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        freezeMovement = false;
    }
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = spriteRendererTransform.GetComponent<Animator>();
        _spriteRend = spriteRendererTransform.GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        var actionAsset = InputSystem.current?.actions;
        if (actionAsset == null) return;

        moveAction = actionAsset.FindAction("Player/Move");
        jumpAction = actionAsset.FindAction("Player/Jump");

        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpPerformed;
            jumpAction.canceled += OnJumpCanceled;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _movementInput = ctx.ReadValue<float>();
    }

private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _movementInput = 0;
    }

private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        _jumpPressInput = true;
        _jumpHoldInput = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        _jumpHoldInput = false;
    }

    private void Start()
    {
        _prevPosition = transform.position;
        if (jumpApexTime <= 0) { Debug.LogError("jumpapextime can't be negative or 0"); }
        _gravity = (2 * jumpHeight) / (jumpApexTime * jumpApexTime);
        _initialJumpVelocity = (2 * jumpHeight) / jumpApexTime;
        _ghosts = new SpriteRenderer[8];
        for (int i = 0; i < 8; i++)
        {
            GameObject b = new GameObject();
            _ghosts[i] = b.AddComponent<SpriteRenderer>();
            _ghosts[i].sprite = _spriteRend.sprite;
        }
    }
    private void Update()
    {
        Vector3 pos = Vector3.Lerp(_prevPosition, transform.position, (Time.time - lastFixedUpdateTime) / 0.02f);
        spriteRendererTransform.position = pos;
        int idx = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) { continue; }
                Vector3 offset = new Vector3(LevelManager.current.currentLevelExtents.x *2* i, LevelManager.current.currentLevelExtents.y *2* j, 0);
                _ghosts[idx].transform.position = pos + offset;
                _ghosts[idx].sprite = _spriteRend.sprite;
                _ghosts[idx].flipX = _spriteRend.flipX;
                idx++;
            }
        }

    }

    private void FixedUpdate()
    {
        Vector2 velocity = Vector2.zero;
        _prevPosition = transform.position;
        Vector2 prevVel = _rb.velocity;
        if (!freezeMovement)
        {
            velocity = _rb.velocity + freezeAddedMovement;
            freezeAddedMovement = Vector2.zero;
            // handles walking, friction and stuff
            ApplyXMovement();
            // handles jumping, gravity and stuff
            ApplyYMovement();
            _rb.velocity = velocity;
        }
        //since it is a press input, once we process it we dont need it
        _jumpPressInput = false;
        lastFixedUpdateTime = Time.time;
        ApplyAnimations();

        void ApplyAnimations()
        {
            int currentAnim = turnAnim;

            if ( (_movementInput > 0 || _movementInput < 0) &&!freezeMovement) { _lookingRight = _movementInput > 0; currentAnim = walkAnim; }
            if (_jumping)
            { 
                currentAnim = jumpAnim; 
                if (Physics2D.BoxCast(transform.position, new Vector2(.8f, .1f), 0, Vector2.down, 1.5f, GROUNDLAYER | MOVINGPLATFORMLAYER)) { currentAnim = jumpEndAnim; } 
            }
            else if ((!Physics2D.BoxCast(transform.position, new Vector2(.8f, .1f), 0, Vector2.down, 1, GROUNDLAYER | MOVINGPLATFORMLAYER) && prevVel.y != 0))
            {
                currentAnim = jumpEndAnim;
            }
            if (freezeMovement) { currentAnim = turnAnim; }
            _animator.Play(currentAnim);
            _spriteRend.flipX = !_lookingRight;
        }

        void ApplyXMovement()
        {
            //we want friction to not affect movement.
            if (_movementInput == 0)
            {
                float velSign = velocity.x > 0 ? 1 : -1;
                velocity.x = Mathf.Max(Mathf.Abs(velocity.x) - friction * Time.fixedDeltaTime, 0) * velSign;
            }

            //for smoothness
            if ((velocity.x > 0 && _movementInput < 0) || (velocity.x < 0 && _movementInput > 0)) { velocity.x = 0; }
            //movement
            velocity.x += (1 / accelerationTime) * maxSpeed * _movementInput * Time.fixedDeltaTime;
            if (velocity.x > maxSpeed) { velocity.x = maxSpeed; }
            else if (velocity.x < -maxSpeed) { velocity.x = -maxSpeed; }
        }
        void ApplyYMovement()
        {
            bool onground = Physics2D.BoxCast((Vector2)transform.position + ongroundBoxOffset, ongroundBoxSize, 0, Vector2.zero, 0, GROUNDLAYER);
            RaycastHit2D movingPlatformHit = Physics2D.BoxCast((Vector2)transform.position + ongroundBoxOffset, ongroundBoxSize, 0, Vector2.zero, 0, MOVINGPLATFORMLAYER);
            //game feel stuff
            if (!_jumpHoldInput) { _holdingJump = false; if (!_isJumpCut && _jumping) { velocity.y *= jumpBreakVelYMult; _isJumpCut = true; } }
            if (_jumpPressInput) { _lastJumpPressTime = Time.time; }
            if (onground || movingPlatformHit) { _lastOnGroundTime = Time.time; _jumping = false; }

            if (movingPlatformHit)
            {
                transform.position += (Vector3)movingPlatformHit.collider.transform.GetComponent<MovingPlatform>().GetLastOffset();
                //_prevPosition = transform.position;
                if (velocity.y < 1)
                {
                    BoxCollider2D a = ((BoxCollider2D)movingPlatformHit.collider);
                    transform.position = new Vector3(transform.position.x, (a.size.y / 2) + a.offset.y + a.transform.position.y + 0.5f, transform.position.z);
                }
            }

            //gravity
            float currentGravity = _gravity;
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

                SoundManager.current.PlaySFXWithMusicMute(jumpAudios[Random.Range(0, jumpAudios.Count - 1)]);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((Vector2)transform.position + ongroundBoxOffset, ongroundBoxSize);
    }
}
