using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = nameof(DialogCharacter), menuName = "CodeA/" + nameof(DialogCharacter), order = 0)]
public class SerializedCharacter : ScriptableObject
{
    [Header("캐릭터")]
    public List<SerializedCharacterStanding> standings = new List<SerializedCharacterStanding>();
}


[System.Serializable]
public class SerializedCharacterStanding
{
    [Tooltip("이름")]
    public string name;
    
    [Space(20)]
    [Tooltip("기본 자세(얼굴 없는거)")]
    public Sprite baseStanding;

    public Sprite logFace;

    [Space(10)]
    [Tooltip("표정들")]
    public List<Sprite> face;
}

public class CharacterStanding
{
    public readonly Dictionary<string, Standing> standings = new Dictionary<string, Standing>();
}

public class Standing
{
    public Sprite baseStanding;

    public Sprite logFace;
    public Dictionary<string, Sprite> faces;
}