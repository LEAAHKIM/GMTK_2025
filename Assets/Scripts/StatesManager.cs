using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesManager : MonoBehaviour
{

    public PlayerStates CurrentState { get; private set; } = PlayerStates.Liquid;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    public Sprite _liquidSprite;
    public Sprite _solidSprite;
    public Sprite _gasSprite;
    public Sprite _cloudSprite; // respawn state

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    public void ChangeState(PlayerStates newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        _sr.sprite = CurrentState switch
        {
            PlayerStates.Liquid => _liquidSprite,
            PlayerStates.Solid => _solidSprite,
            PlayerStates.Gas => _gasSprite,
            PlayerStates.Cloud => _cloudSprite,
            _ => _sr.sprite
        };

        //Debug.Log($"Player state changed to: {CurrentState}");
        //Debug.Log($"Current speed: {_rb.velocity}");
        //Debug.Log($"Current gravity: {_rb.gravityScale}");
    }

    public float GetSpeedModifier()
    {
        return CurrentState switch
        {
            PlayerStates.Liquid => 1f,
            PlayerStates.Solid => 25f,
            PlayerStates.Gas => 0.03f,
            _ => 1f
        };
    }

    public float GetGravityModifier()
    {
        return CurrentState switch
        {
            PlayerStates.Liquid => 1f,
            PlayerStates.Solid => 2f,
            PlayerStates.Gas => -0.001f,
            _ => 1f
        };
    }

}

