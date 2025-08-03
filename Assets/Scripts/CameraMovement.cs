using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    //singleteon class
    public static CameraMovement current;

    public Camera cam;
    private float _camExtentsX;
    private float _camExtentsY;
    private Vector2 mousePos;
    public Vector2 mouseWorldPos;

    [Header("Camera follow")]
    public Transform target;
    public Vector2 offset;
    public bool dontapplyoffset = false;
    public float cameraSmoothTimeX;
    public float cameraSmoothTimeYMax;
    public float cameraSmoothTimeYMin;
    public float cameraOffsetWhenFalling = 2;
    [Header("Camera limiting boxes")]
    // not really deadboxsize, deadboxsize/2 to avoid runtime division
    public Vector2 deadBoxExtents;
    private Vector2 _deadBoxPos;

    // not really levelsize, levelsize/2 to avoid runtime division
    public Vector2 levelExtents;
    public Vector2 levelPosition;

    private Vector2 _cameraTargetPos;
    private float _currentCameraVelX;
    private float _currentCameraVelY;


    [Header("Look ahead")]
    // if true apply input offset like mouse / lookahead
    public bool applyLookahead;

    public float lookAheadWeight;
    public float lookAheadProgressSpeed = 2.0f;
    //value between [-1,1]
    private float _currentLookAheadProgress;
    private float _movementXInput;


    //i currently disabled this because it doesnt feel good
    public float mouseLookAheadWeight;
    
    //for debugging
    private Vector2 _lastTargetPos;

    public Vector2 realPositionOffset;
    public float parallaxMultX;
    public float parallaxMultY;
    public float bgSize;
    public Sprite bgSprite;
    private float cameraStartSize;
    private float _cameraCurrentSize;
    public bool dontapplycamerabox = false;
    private InputAction moveAction;
    private InputAction lookAction;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        cam = GetComponent<Camera>();
        cameraStartSize = cam.orthographicSize;
    }

    private void OnEnable()
    {
        var actionAsset = InputSystem.current?.actions;
        if (actionAsset == null) return;

        moveAction = actionAsset.FindAction("Player/Move");
        lookAction = actionAsset.FindAction("Player/Look");

        if (moveAction != null)
        {
            moveAction.Enable(); // Important: enable it
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        if (lookAction != null)
        {
            lookAction.Enable(); // Important: enable it
            lookAction.performed += OnLookPerformed;
        }
    }


    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            moveAction.Disable();
        }

        if (lookAction != null)
        {
            lookAction.performed -= OnLookPerformed;
            lookAction.Disable();
        }
    }


    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _movementXInput = ctx.ReadValue<float>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _movementXInput = 0;
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        if (cam == null) return;

        mousePos = ctx.ReadValue<Vector2>();
        mouseWorldPos = cam.ScreenToWorldPoint(mousePos);
    }


    // i know this is terrible but there is only 1 day left. i hope no one sees this
    public void OffsetCameraTargetPos(Vector2 offset, bool actuallyOffset)
    {
        if (actuallyOffset)
        {
            _cameraTargetPos += offset;
            _deadBoxPos += offset;
            realPositionOffset += offset;
        }
    }
    private void Start()
    {
        // not really cam height, cam height / 2. add this up/down to get the position of cam bounds
        _camExtentsY = cam.orthographicSize;
        _camExtentsX = _camExtentsY * cam.aspect;
        
        levelExtents = LevelManager.current.currentLevelExtents * 3;
    }
    public void SetCameraSize(float h)
    {
        cam.orthographicSize = h;
        _camExtentsY = cam.orthographicSize;
        _camExtentsX = _camExtentsY * cam.aspect;
        _cameraCurrentSize = h;
    }
    float SignWith0(float v)
    {
        if (v == 0) { return 0; }
        return (v > 0) ? 1.0f : -1.0f;
    }
    private void LateUpdate()
    {
        Vector2 currentOffset = dontapplyoffset ? Vector2.zero : offset;
        Vector2 targetPos;
        float currentCameraSmoothY = cameraSmoothTimeYMax;
        float currentCameraSmoothX = cameraSmoothTimeX;
        if (target != null)
        {
            targetPos = (Vector2)target.position + currentOffset;
            currentCameraSmoothY = cameraSmoothTimeYMax;

            // this part is only called when camera is bound to player, so i access player only here.
            if (applyLookahead)
            {
                //inputx
                if (SignWith0(_movementXInput) != SignWith0(_currentLookAheadProgress)) { _currentLookAheadProgress = 0; }
                //it is fine if this is frame dependent, since it is camera
                _currentLookAheadProgress = Mathf.Lerp(_currentLookAheadProgress, _movementXInput, Time.deltaTime * lookAheadProgressSpeed);
                targetPos.x += lookAheadWeight * _currentLookAheadProgress;

                //mouse
                Vector2 mouseOffset = (mouseWorldPos - (Vector2)target.transform.position).normalized;
                targetPos += mouseOffset * mouseLookAheadWeight;
                float t = -LevelManager.current.playerMovement._rb.velocity.y / 20;
                currentCameraSmoothY = Mathf.Lerp(cameraSmoothTimeYMax, cameraSmoothTimeYMin, t);
                targetPos.y -= Mathf.Lerp(0, cameraOffsetWhenFalling, t);
            }
            else { _currentLookAheadProgress = 0; }
        }
        else { targetPos = Vector2.zero; _cameraTargetPos = Vector2.zero; }

        //level bound checkin
        if ((targetPos.x - _camExtentsX) < (levelPosition.x - levelExtents.x)) { targetPos.x = levelPosition.x - levelExtents.x + _camExtentsX; }
        else if ((targetPos.x + _camExtentsX) > (levelPosition.x + levelExtents.x)) { targetPos.x = levelPosition.x + levelExtents.x - _camExtentsX; }
        if ((targetPos.y - _camExtentsY) < (levelPosition.y - levelExtents.y)) { targetPos.y = levelPosition.y - levelExtents.y + _camExtentsY; }
        else if ((targetPos.y + _camExtentsY) > (levelPosition.y + levelExtents.y)) { targetPos.y = levelPosition.y + levelExtents.y - _camExtentsY; }

        bool outofboundsX = false;
        bool outofboundsY = false;

        //check if target position is out of bounds of deadbox
        if ((targetPos.x) < (_deadBoxPos.x - deadBoxExtents.x)) { _deadBoxPos.x = targetPos.x + deadBoxExtents.x; outofboundsX = true; }
        else if ((targetPos.x) > (_deadBoxPos.x + deadBoxExtents.x)) { _deadBoxPos.x = targetPos.x - deadBoxExtents.x; outofboundsX = true; }
        if ((targetPos.y) < (_deadBoxPos.y - deadBoxExtents.y)) { _deadBoxPos.y = targetPos.y + deadBoxExtents.y; outofboundsY = true; }
        else if ((targetPos.y) > (_deadBoxPos.y + deadBoxExtents.y)) { _deadBoxPos.y = targetPos.y - deadBoxExtents.y; outofboundsY = true; }

        if (outofboundsX) { _cameraTargetPos.x = targetPos.x; }
        if (outofboundsY) { _cameraTargetPos.y = targetPos.y; }

        if (dontapplycamerabox) { _cameraTargetPos = targetPos; }

        // camera movement with cool smoothness function
        float smoothDampX = Mathf.SmoothDamp(transform.position.x, _cameraTargetPos.x, ref _currentCameraVelX, currentCameraSmoothX, Mathf.Infinity, Time.deltaTime);
        float smoothDampY = Mathf.SmoothDamp(transform.position.y, _cameraTargetPos.y, ref _currentCameraVelY, currentCameraSmoothY, Mathf.Infinity, Time.deltaTime);
        transform.position = new Vector3(smoothDampX, smoothDampY, transform.position.z);
        _lastTargetPos = targetPos; 
        float scaleChange = _cameraCurrentSize / cameraStartSize;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(levelPosition, levelExtents * 2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(_camExtentsX * 2, _camExtentsY * 2));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_deadBoxPos, deadBoxExtents * 2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastTargetPos, .25f);
    }
}
