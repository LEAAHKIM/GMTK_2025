using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//handles looping & 
public class LevelManager : MonoBehaviour
{
    public static LevelManager current;
    public enum GameState
    {
        PlayerControl = 0,
        MovingPlatform = 1
    }
    public GameState currentGameState = GameState.PlayerControl;
    public Vector2 currentLevelExtents;

    public PlayerMovement playerMovement;
    public CameraMovement cameraMovement;
    private Transform _playerTransform;

    //moving platform controls
    private MovingPlatform _currentMovingPlatform;
    private Vector2 _currentMovingPlatformPos;
    private Vector2 moveMovingPlatformInput;
    private int currentPlatformIndex;
    private int currentPlatformSize;
    private float _lastFUpdateTime;

    public GameObject timeLeftGameObject;
    public TMPro.TextMeshProUGUI timeLeftText;
    private void Awake()
    {
        current = this;
    }
    private void Start()
    {
        timeLeftGameObject.SetActive(false);
        _playerTransform = playerMovement.transform;
        InputSystem.current.actions.Player.MovingPlatformMove.performed += ctx => { moveMovingPlatformInput = ctx.ReadValue<Vector2>(); };
        InputSystem.current.actions.Player.MovingPlatformMove.canceled += ctx => { moveMovingPlatformInput = Vector2.zero; };
    }
    public void LoopCreateMovingPlatform(MovingPlatform p)
    {
        timeLeftGameObject.SetActive(true);
        _currentMovingPlatform = p;
        _currentMovingPlatformPos = p.transform.position;
        currentGameState = GameState.MovingPlatform;
        currentPlatformIndex = 0;
        currentPlatformSize = (int)(p.platformMoveTime * 50.0f) ;
        p.InitializePositions(currentPlatformSize);
        cameraMovement.applyLookahead = false;
        cameraMovement.target = p.spriteTransform;
        playerMovement.FreezePlayerMovement();
        p.HideGhostPlaftorms();
        timeLeftText.text = p.platformMoveTime.ToString("F1");
    }
    public void StopMovingPlatform()
    {
        timeLeftGameObject.SetActive(false);
        _currentMovingPlatform.StartMovement();
        _currentMovingPlatform.UnhideGhostPlaftorms();
        _currentMovingPlatform = null;
        _currentMovingPlatformPos = Vector2.zero;
        currentPlatformIndex = 0;
        currentPlatformSize = 0;
        currentGameState = GameState.PlayerControl;
        cameraMovement.target = playerMovement.transform;
        cameraMovement.applyLookahead = true;
        playerMovement.UnFreezePlayerMovement();
    }
    private void Update()
    {
        if(currentGameState==GameState.MovingPlatform&&currentPlatformIndex>1)
        {
            _currentMovingPlatform.spriteTransform.position = 
                Vector2.Lerp(_currentMovingPlatform._positions[currentPlatformIndex-2], _currentMovingPlatform._positions[currentPlatformIndex-1], (Time.time-_lastFUpdateTime)/0.02f);
        }
    }
    private void FixedUpdate()
    {
        ApplyLevelLooping();
        switch (currentGameState)
        {
            case GameState.PlayerControl:
                break;
            case GameState.MovingPlatform:
                MovingDirectionLogic();
                break;
            default:
                break;
        }
        void ApplyLevelLooping()
        {
            Vector3 playerPos = _playerTransform.position;
            Vector3 camPos = cameraMovement.transform.position;
            if (playerPos.x > currentLevelExtents.x) { playerPos.x -= currentLevelExtents.x * 2; camPos.x -= currentLevelExtents.x * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(-currentLevelExtents.x * 2, 0)); }
            else if (playerPos.x < -currentLevelExtents.x) { playerPos.x += currentLevelExtents.x * 2; camPos.x += currentLevelExtents.x * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(currentLevelExtents.x * 2, 0)); }
            if (playerPos.y > currentLevelExtents.y) { playerPos.y -= currentLevelExtents.y * 2; camPos.y -= currentLevelExtents.y * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(0, -currentLevelExtents.y * 2)); }
            if (playerPos.y < -currentLevelExtents.y) { playerPos.y += currentLevelExtents.y * 2; camPos.y += currentLevelExtents.y * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(0, currentLevelExtents.y * 2)); }
            cameraMovement.transform.position = camPos;
            _playerTransform.position = playerPos;
            playerMovement._prevPosition = playerPos;   
        }

        void MovingDirectionLogic()
        {
            Vector2 dir;
            if (moveMovingPlatformInput.magnitude > 0) { dir = moveMovingPlatformInput.normalized; }
            else { dir = Vector2.zero; return; }
            _currentMovingPlatform.transform.position = _currentMovingPlatform.transform.position +  (Vector3)(dir * (_currentMovingPlatform.platformMoveSpeed * Time.fixedDeltaTime));
            _currentMovingPlatformPos = _currentMovingPlatform.transform.position;
            _currentMovingPlatform._positions[currentPlatformIndex] = _currentMovingPlatformPos;
            currentPlatformIndex++;
            float timeLeft = _currentMovingPlatform.platformMoveTime - ((((float)currentPlatformIndex + 1) / (float)currentPlatformSize) * _currentMovingPlatform.platformMoveTime);
            timeLeftText.text = timeLeft.ToString("F1");
            if (currentPlatformIndex >= currentPlatformSize) { StopMovingPlatform(); }
            _lastFUpdateTime = Time.time;

        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, currentLevelExtents * 2);
    }
}
