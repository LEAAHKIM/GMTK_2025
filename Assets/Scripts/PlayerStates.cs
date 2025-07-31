using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour
{

    public enum PlayerState
    {
        Liquid,
        Solid,
        Gas
    }

    public PlayerState CurrentState { get; private set; } = PlayerState.Liquid;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    public Sprite _liquidSprite;
    public Sprite _solidSprite;
    public Sprite _gasSprite;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        UpdateState();
    }

    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        UpdateState();
    }
    private void UpdateState()
    {
        switch (CurrentState)
        {
            case PlayerState.Liquid:
                _rb.gravityScale = 1f;
                _sr.sprite = _liquidSprite;
                break;
            case PlayerState.Solid:
                _rb.gravityScale = 2f;
                _sr.sprite = _solidSprite;
                break;
            case PlayerState.Gas:
                _rb.gravityScale = -2f;
                _sr.sprite = _gasSprite;
                break;
        }
    }

    public void HandleMovement()
    {

    }
    void Update()
    {
        HandleMovement();
    }
}
