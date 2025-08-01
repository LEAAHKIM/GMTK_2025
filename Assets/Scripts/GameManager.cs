using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this scripts controls the game state. There are currently 2 game states:
//1-player control, player walking around etc.
//2-turning platforms into moving platforms.
public class GameManager : MonoBehaviour
{
    enum CurrentGameState
    {
        PlayerControl=0,
        MovePlatform=1
    }
    private CurrentGameState _state;
    private void FixedUpdate()
    {
        switch (_state)
        {
            case CurrentGameState.PlayerControl:
                break;
            case CurrentGameState.MovePlatform:
                break;
            default:
                break;
        }
        void PlayerControlUpdate()
        {
        }
    }
}
