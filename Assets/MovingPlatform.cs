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
    int _prevIndex;
    public void InitializePositions(int size)
    {
        _positions = new Vector2[size];
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
            spriteTransform.position = Vector2.Lerp(_positions[_prevIndex], _positions[_currentIndex], (Time.time - _lastPosUpdateTime) / 0.02f);
        }
    }
    int GetNextIndex()
    {
        int indx = _currentIndex;
        if (_goingForward)
        {
            indx++;
            if (indx>= _positions.Length) { indx--; }

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

    int GetPrevIndex()
    {
        int indx = _currentIndex;
        if (_goingForward)
        {
            indx--;
            return Mathf.Max(indx, 0);

        }
        else
        {
            indx++;
            return Mathf.Min(indx, _positions.Length-1);
        }
    }


    private void FixedUpdate()
    {
        if (movementStarted)
        {
            transform.position = _positions[_currentIndex];
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
    /*
    public Vector2 GetLastOffset()
    {
        if (!movementStarted) { return Vector2.zero; }
        return (_positions[GetNextIndex()] - _positions[_currentIndex]);
    }
    */

    public Vector2 GetLastOffset()
    {
        if (!movementStarted) { return Vector2.zero; }
        return (_positions[_currentIndex] - _positions[_prevIndex]);
    }
    public void StartSettingUpMovingPlatform()
    {
        LevelManager.current.LoopCreateMovingPlatform(this);
    }
}
