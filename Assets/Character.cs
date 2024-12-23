using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
    Idle,
    Walking,
    UTurning
}
public class Character : MonoBehaviour
{
    public CharacterState state = CharacterState.Idle;

    public bool IsUTurning()
    {
        return state == CharacterState.UTurning;
    }
}
