using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//handles looping & 
public class LevelManager : MonoBehaviour
{
    public static LevelManager current;
    public enum GameState
    {
        PlayerControl = 0,
        MovingPlatform = 1
    }
    public float collectibleAmount = 0;

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


    public bool zoomOutInput;

    private float _cameraStartSize;
    public float cameraZoomOutSpeed;
    public float cameraZoomOutSize;
    public bool goDownInput;
    private void Awake()
    {
        current = this;
        _playerTransform = playerMovement.transform;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool _isLoading = false;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isLoading = false;
        collectibleAmount = FindObjectsOfType<Collectible>().Length;
        Debug.Log($"Scene loaded: {scene.name}, collectibles found: {collectibleAmount}");
    }

    private void Start()
    {
        InputSystem.current.actions.Player.MovingPlatformMove.performed += ctx => { moveMovingPlatformInput = ctx.ReadValue<Vector2>(); };
        InputSystem.current.actions.Player.MovingPlatformMove.canceled += ctx => { moveMovingPlatformInput = Vector2.zero; };

        InputSystem.current.actions.Player.ZoomOutKey.performed += ctx => { zoomOutInput = true; };
        InputSystem.current.actions.Player.ZoomOutKey.canceled += ctx => { zoomOutInput = false; };

        InputSystem.current.actions.Player.GoDown.performed += ctx => { goDownInput = true; };
        InputSystem.current.actions.Player.GoDown.canceled += ctx => { goDownInput = false; };
        currentGameState = GameState.PlayerControl;

        _cameraStartSize = cameraMovement.cam.orthographicSize;
        float width = currentLevelExtents.y * cameraMovement.cam.aspect;
        if (width > currentLevelExtents.x) { cameraZoomOutSize = currentLevelExtents.y; }
        else { cameraZoomOutSize = currentLevelExtents.x * (1.0f / cameraMovement.cam.aspect); }
    }
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels to load.");
        }
    }
    public void LoopCreateMovingPlatform(MovingPlatform p)
    {
        _currentMovingPlatform = p;
        _currentMovingPlatformPos = p.transform.position;
        currentGameState = GameState.MovingPlatform;
        currentPlatformIndex = 0;
        currentPlatformSize = (int)(p.platformMoveTime * 50.0f);
        p.InitializePositions(currentPlatformSize);
        cameraMovement.applyLookahead = false;
        cameraMovement.target = p.spriteTransform;
        playerMovement.FreezePlayerMovement();
        p.HideGhostPlaftorms();
    }
    public void StopMovingPlatform()
    {
        _currentMovingPlatform.StartMovement();
        _currentMovingPlatform.UnhideGhostPlaftorms();
        _currentMovingPlatform = null;
        _currentMovingPlatformPos = Vector2.zero;
        currentPlatformIndex = 0;
        currentPlatformSize = 0;
        currentGameState = GameState.PlayerControl;
        cameraMovement.target = playerMovement.spriteRendererTransform;
        cameraMovement.applyLookahead = true;
        playerMovement.UnFreezePlayerMovement();
    }
    private void Update()
    {
        switch (currentGameState)
        {
            case GameState.PlayerControl:
                PlayerControl();
                break;
            case GameState.MovingPlatform:
                MovePlatform();
                break;
            default:
                break;
        }

        void MovePlatform()
        {
            if (currentPlatformIndex < 2) { return; }
            _currentMovingPlatform.spriteTransform.position = Vector2.Lerp(_currentMovingPlatform._positions[currentPlatformIndex - 2], _currentMovingPlatform._positions[currentPlatformIndex - 1], (Time.time - _lastFUpdateTime) / 0.02f);

        }
        void PlayerControl()
        {
            cameraMovement.SetCameraSize(Mathf.Lerp(cameraMovement.cam.orthographicSize, zoomOutInput ? cameraZoomOutSize : _cameraStartSize, Time.deltaTime * cameraZoomOutSpeed));
            if (zoomOutInput)
            {
                cameraMovement.target = null;
            }
            else
            {
                cameraMovement.target = playerMovement.spriteRendererTransform;
            }
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
            if (playerPos.x > currentLevelExtents.x) { playerPos.x -= currentLevelExtents.x * 2; camPos.x -= currentLevelExtents.x * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(-currentLevelExtents.x * 2, 0), !zoomOutInput); }
            else if (playerPos.x < -currentLevelExtents.x) { playerPos.x += currentLevelExtents.x * 2; camPos.x += currentLevelExtents.x * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(currentLevelExtents.x * 2, 0), !zoomOutInput); }
            if (playerPos.y > currentLevelExtents.y) { playerPos.y -= currentLevelExtents.y * 2; camPos.y -= currentLevelExtents.y * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(0, -currentLevelExtents.y * 2), !zoomOutInput); }
            if (playerPos.y < -currentLevelExtents.y) { playerPos.y += currentLevelExtents.y * 2; camPos.y += currentLevelExtents.y * 2; cameraMovement.OffsetCameraTargetPos(new Vector2(0, currentLevelExtents.y * 2), !zoomOutInput); }
            if (!zoomOutInput)
            {
                cameraMovement.transform.position = camPos;
            }
            _playerTransform.position = playerPos;
            playerMovement._prevPosition = playerPos;
        }

        void MovingDirectionLogic()
        {
            Vector2 dir;
            if (moveMovingPlatformInput.magnitude > 0) { dir = moveMovingPlatformInput.normalized; }
            else { dir = Vector2.zero; return; }
            Vector3 newPos = _currentMovingPlatform.transform.position + (Vector3)(dir * (_currentMovingPlatform.platformMoveSpeed * Time.fixedDeltaTime));
            BoxCollider2D boxCol = _currentMovingPlatform.GetComponent<BoxCollider2D>();
            if (Physics2D.BoxCast(newPos + (Vector3)boxCol.offset, boxCol.size, 0, Vector2.zero, 0, PlayerMovement.GROUNDLAYER )) { return; }

            _currentMovingPlatform.transform.position = newPos;
            _currentMovingPlatformPos = _currentMovingPlatform.transform.position;
            _currentMovingPlatform._positions[currentPlatformIndex] = _currentMovingPlatformPos;
            currentPlatformIndex++;
            if (currentPlatformIndex >= currentPlatformSize) { StopMovingPlatform(); }
            _lastFUpdateTime = Time.time;

        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, currentLevelExtents * 2);
    }
    public void CollectItem()
    {
        collectibleAmount--;
        if (collectibleAmount <= 0)
        {
            // Advance to next level or trigger win condition
            Debug.Log("All collectibles found!");
            LoadNextLevel();
        }
    }
}
