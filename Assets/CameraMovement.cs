using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //singleteon class
    public CameraMovement current;

    public Camera cam;
    private float _camExtentsX;
    private float _camExtentsY;
    private Vector2 mousePos;
    public Vector2 mouseWorldPos;
    
    [Header("Camera follow")]
    public Transform target;
    public Vector2 offset;
    public float cameraSmoothTimeX;
    public float cameraSmoothTimeYMax;
    public float cameraSmoothTimeYMin;
    public float cameraOffsetWhenFalling=2;
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

    private void Awake()
    {
        cam = GetComponent<Camera>();
        current = this;
    }
    public void OffsetCameraTargetPos(Vector2 offset)
    {
        _cameraTargetPos += offset;
        _deadBoxPos += offset;
    }
    private void Start()
    {
        // not really cam height, cam height / 2. add this up/down to get the position of cam bounds
        _camExtentsY = cam.orthographicSize;
        _camExtentsX = _camExtentsY * cam.aspect;
        InputSystem.current.actions.Player.Move.performed += ctx => { _movementXInput = ctx.ReadValue<float>(); };
        InputSystem.current.actions.Player.Move.canceled += ctx => { _movementXInput = 0; };
        InputSystem.current.actions.Player.Look.performed += ctx => { mousePos = ctx.ReadValue<Vector2>(); mouseWorldPos = cam.ScreenToWorldPoint(mousePos); };
    }
    float SignWith0(float v)
    {
        if (v == 0) { return 0; }
        return (v > 0) ? 1.0f : -1.0f;
    }
    private void LateUpdate()
    {
        
        Vector2 targetPos = (Vector2)target.position + offset;
        float currentCameraSmoothY = cameraSmoothTimeYMax;

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



        // camera movement with cool smoothness function
        float smoothDampX = Mathf.SmoothDamp(transform.position.x, _cameraTargetPos.x, ref _currentCameraVelX, cameraSmoothTimeX, Mathf.Infinity, Time.deltaTime);
        float smoothDampY = Mathf.SmoothDamp(transform.position.y, _cameraTargetPos.y, ref _currentCameraVelY, currentCameraSmoothY, Mathf.Infinity, Time.deltaTime);
        transform.position = new Vector3(smoothDampX, smoothDampY, transform.position.z);
        _lastTargetPos = targetPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(levelPosition, levelExtents*2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(_camExtentsX*2, _camExtentsY*2));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_deadBoxPos, deadBoxExtents*2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastTargetPos, .25f);
    }
}
