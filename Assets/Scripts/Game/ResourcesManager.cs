using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesManager : Manager
{
    public bool IsLoading { get; private set; }
    private readonly Dictionary<string, Sprite> backgroundSprites = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, CharacterStanding> characters = new Dictionary<string, CharacterStanding>();
    private readonly Dictionary<string, Dialogs> dialogs = new Dictionary<string, Dialogs>();
    private readonly Dictionary<string, Sprite> popupSprites = new Dictionary<string, Sprite>();

    [SerializeField] private List<SerializedCharacter> serializedCharacters;

    public override void OnCreated()
    {
        IsLoading = false;
        LoadFile();
    }

    private void LoadFile()
    {
        LoadCharacter();

        var backgrounds = Resources.LoadAll<Sprite>("Background");
        foreach(Sprite sprite in backgrounds)
            backgroundSprites.Add(sprite.name, sprite);

        var dialogList = Resources.LoadAll<Dialogs>("Dialogs");
        foreach (var dialog in dialogList)
            dialogs.Add(dialog.name, dialog);

        var popups = Resources.LoadAll<Sprite>("Popup");
        foreach (Sprite sprite in popups)
            popupSprites.Add(sprite.name, sprite);

    }

    private void LoadCharacter()
    {
        foreach (var serializedCharacter in serializedCharacters)
        {
            var character = new CharacterStanding();
            foreach (var serializedStanding in serializedCharacter.standings)
            {
                var standing = new Standing();
                var faceDictionary = new Dictionary<string, Sprite>();
                foreach (var faceSprite in serializedStanding.face)
                {
                    faceDictionary.Add(faceSprite.name, faceSprite);
                }

                standing.baseStanding = serializedStanding.baseStanding;
                standing.logFace = serializedStanding.logFace;
                standing.faces = faceDictionary;
                character.standings.Add(serializedStanding.name, standing);
            }

            characters.Add(serializedCharacter.name, character);
        }
    }

    public Sprite GetBackground(string backgroundName)
    {
        return backgroundSprites.TryGetValue(backgroundName, out var sprite) ? sprite : null;
    }

    public CharacterStanding GetCharacter(string characterName)
    {
        return characters[characterName];
    }

    public Dialogs GetDialog(string dialogName)
    {
        return dialogs.TryGetValue(dialogName, out var dialog) ? dialog : null;
    }

    public Sprite GetPopup(string popupName)
    {
        return popupSprites.TryGetValue(popupName, out var sprite) ? sprite : null;
    }
}