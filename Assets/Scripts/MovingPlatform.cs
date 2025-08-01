using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// moving platforms don't need a ghost object script, it is handled in this script already.
public class MovingPlatform : MonoBehaviour
{
    public float platformMoveSpeed;
    //how long the platform can move
    public float platformMoveTime;

    //there are 50 positions per second (since fixedupdate gets called 50 times per second). Turning back is going through the array in reverse.i
    //positions don't need to be inside the level bounds, since we are looping.
    //however this might cause issues if we somehow go through the level twice with a single moving platform.
    public Vector2[] _positions;
    private int _currentIndex;
    private bool _goingForward;
    private bool movementStarted = false;

    private float _lastPosUpdateTime;
    public Transform spriteTransform;

    MovingPlatform[] ghostPlatforms;
    int _prevPrevIndex;
    int _prevIndex;

    [HideInInspector] public bool isGhostPlatform = false;
    private MovingPlatform _originalPlatform;
    public void InitializePositions(int size)
    {
        _positions = new Vector2[size];
    }
    public void HideGhostPlaftorms()
    {
        for (int i = 0; i < 8; i++)
        {
            ghostPlatforms[i].spriteTransform.gameObject.SetActive(false);
        }
    }
    public void UnhideGhostPlaftorms()
    {
        for (int i = 0; i < 8; i++)
        {
            ghostPlatforms[i].spriteTransform.gameObject.SetActive(true);
        }
    }
    private void Start()
    {
        if (!isGhostPlatform)
        {
            ghostPlatforms = new MovingPlatform[8];
            int idx = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) { continue; }
                    GameObject a = new GameObject();
                    GameObject b = new GameObject();
                    Vector2 offset = (new Vector2(i, j)) * LevelManager.current.currentLevelExtents * 2;
                    if (spriteTransform.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRend))
                    {
                        SpriteRenderer newSpriteRend = b.AddComponent<SpriteRenderer>();
                        newSpriteRend.sprite = spriteRend.sprite;
                        newSpriteRend.color = spriteRend.color;
                    }
                    if (TryGetComponent<BoxCollider2D>(out BoxCollider2D boxCol))
                    {
                        BoxCollider2D newBoxCol = a.AddComponent<BoxCollider2D>();
                        newBoxCol.offset = boxCol.offset;
                        newBoxCol.size = boxCol.size;
                    }
                    MovingPlatform newMovingPlatform = a.AddComponent<MovingPlatform>();
                    newMovingPlatform.isGhostPlatform = true;
                    newMovingPlatform._originalPlatform = this;
                    newMovingPlatform.spriteTransform = b.transform;
                    ghostPlatforms[idx] = newMovingPlatform;
                    a.layer = gameObject.layer;
                    a.transform.position = transform.position + (Vector3)offset;
                    a.transform.localScale = transform.localScale;
                    a.transform.localRotation = transform.localRotation;

                    b.transform.position = spriteTransform.position + (Vector3)offset;
                    b.transform.localScale = spriteTransform.lossyScale;
                    b.transform.localRotation = spriteTransform.localRotation;

                    idx++;
                }
            }
        }
    }
    public void StartMovement()
    {
        _currentIndex = _positions.Length - 1;
        _goingForward = false;
        movementStarted = true;
        PlayerInteractManager.current.RemoveInteractable(GetComponent<Interactable>().uid);
    }
    private void Update()
    {
        if (movementStarted)
        {
            Vector3 p = Vector2.Lerp(_positions[_prevPrevIndex], _positions[_prevIndex], (Time.time - _lastPosUpdateTime) / 0.02f);
            spriteTransform.position = p;
            UpdateGhostSpritePositions(p);
        }
    }
    void UpdateGhostPositions(Vector3 pos)
    {
        int idx = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) { continue; }

                Vector2 offset = (new Vector2(i, j)) * LevelManager.current.currentLevelExtents * 2;

                ghostPlatforms[idx].transform.position = pos + (Vector3)offset;
                idx++;
            }
        }
    }
    void UpdateGhostSpritePositions(Vector3 pos)
    {
        int idx = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) { continue; }
                Vector2 offset = (new Vector2(i, j)) * LevelManager.current.currentLevelExtents * 2;

                ghostPlatforms[idx].spriteTransform.position = pos + (Vector3)offset;
                idx++;
            }
        }
    }
    int GetNextIndex()
    {
        int indx = _currentIndex;
        if (_goingForward)
        {
            indx++;
            if (indx >= _positions.Length) { indx--; }

        }
        else
        {
            indx--;
            if (indx < 0)
            {
                indx++;
            }
        }
        return indx;
    }
    private void FixedUpdate()
    {
        if (movementStarted)
        {
            transform.position = _positions[_currentIndex];
            UpdateGhostPositions(transform.position);
            _prevPrevIndex = _prevIndex;
            _prevIndex = _currentIndex;
            if (_goingForward)
            {
                _currentIndex++;
                if (_currentIndex >= _positions.Length) { _currentIndex--; _goingForward = false; }
            }
            else
            {
                _currentIndex--;
                if (_currentIndex < 0) { _currentIndex++; _goingForward = true; }
            }
            _lastPosUpdateTime = Time.time;
        }
    }

    public Vector2 GetLastOffset()
    {
        if (isGhostPlatform) { return _originalPlatform.GetLastOffset(); }
        if (!movementStarted) { return Vector2.zero; }
        return (_positions[_prevIndex] - _positions[_prevPrevIndex]);
    }
    //*

    /*
    public Vector2 GetLastOffset()
    {
        if (isGhostPlatform) { return _originalPlatform.GetLastOffset(); }
        if (!movementStarted) { return Vector2.zero; }
        return (_positions[_currentIndex] - _positions[_prevIndex]);
    }*/
    public void StartSettingUpMovingPlatform()
    {
        LevelManager.current.LoopCreateMovingPlatform(this);
    }
}
