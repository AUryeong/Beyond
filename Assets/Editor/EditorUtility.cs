using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

[Serializable]
public class XMLDialogs
{
    [XmlElement("Dialog")] public List<Dialog> dialogs = new List<Dialog>();
}

[Serializable]
public class XMLDialogOwners
{
    [XmlElement("Owner")] public List<Owner> owners = new List<Owner>();
}

[Serializable]
public class Owner
{
    [XmlAttribute("Name")] public string name;
    [XmlAttribute("OwnerName")] public string ownerName;
}


public class EditorUtility
{
    private const string remainderRegex = "(.*?((?=})|(/|$)))";
    private const string eventRegexString = "{(?<Event>" + remainderRegex + ")}";
    private static readonly Regex eventRegex = new Regex(eventRegexString);
    
    [MenuItem("Assets/Convert Xml To ScriptableObject Dialog")]
    public static void CreateDialogScriptableObject()
    {
        var ownerDictionaries = new Dictionary<string, string>();
        var str = File.ReadAllText(Application.dataPath + "/Editor/Xmls/DialogOwners.xml");
        XMLDialogOwners dialogOwners;
        using (var stringReader = new StringReader(str))
        {
            dialogOwners = (XMLDialogOwners)new XmlSerializer(typeof(XMLDialogOwners)).Deserialize(stringReader);
        }

        foreach (var owner in dialogOwners.owners)
            ownerDictionaries.Add(owner.name, owner.ownerName);

        const string xmlPath = "/Editor/Xmls/Dialog";
        var dir = new DirectoryInfo(Application.dataPath + xmlPath);
        ConvertXmlDialog(dir, ownerDictionaries);

        AssetDatabase.SaveAssets();
    }

    private static void ConvertXmlDialog(DirectoryInfo dir, Dictionary<string, string> ownerDictionaries)
    {
        var directories = dir.GetDirectories();
        if (directories.Length > 0)
        {
            foreach (var directory in directories)
            {
                ConvertXmlDialog(directory, ownerDictionaries);
            }
        }

        foreach (var fileInfo in dir.GetFiles())
        {
            if (fileInfo.FullName.EndsWith(".meta")) continue;

            string str = File.ReadAllText(fileInfo.FullName);

            XMLDialogs dialogs;
            using (var stringReader = new StringReader(str))
            {
                dialogs = (XMLDialogs)new XmlSerializer(typeof(XMLDialogs)).Deserialize(stringReader);
            }

            CreateNewAsset(Path.GetFileNameWithoutExtension(fileInfo.FullName), dialogs.dialogs, ownerDictionaries);
        }
    }

    private static void CreateNewAsset(string assetName, List<Dialog> dialogList, Dictionary<string, string> ownerDictionaries)
    {
        Debug.Log(assetName);
        var newDialogs = ScriptableObject.CreateInstance<Dialogs>();

        ParsingDialogs(assetName, ref dialogList, ownerDictionaries);
        newDialogs.dialogs = dialogList;

        AssetDatabase.CreateAsset(newDialogs, $"Assets/Resources/Dialogs/{assetName}.asset");
    }

    private static void ParsingDialogs(string assetName, ref List<Dialog> dialogList, Dictionary<string, string> ownerDictionaries)
    {
        string background = string.Empty;
        float backgroundScale = 1;
        string bgm = string.Empty;

        var characterDictionary = new Dictionary<string, string>();
        var faceDictionary = new Dictionary<string, string>();
        var posDictionary = new Dictionary<string, DialogCharacterPos>();
        var sizeDictionary = new Dictionary<string, DialogCharacterSize>();
        foreach (var dialog in dialogList)
        {
            if (string.IsNullOrEmpty(dialog.dialogBackground.name))
            {
                dialog.dialogBackground.name = background;
                dialog.dialogBackground.scale = backgroundScale;
            }
            else
            {
                background = dialog.dialogBackground.name;
                backgroundScale = dialog.dialogBackground.scale;
            }

            if (string.IsNullOrEmpty(dialog.bgm))
                dialog.bgm = bgm;
            else
                bgm = dialog.bgm;

            if (dialog.dialogText != null)
            {
                if (!string.IsNullOrEmpty(dialog.dialogText.name))
                {
                    if (ownerDictionaries.TryGetValue(dialog.dialogText.name, out var dictionary))
                    {
                        if (string.IsNullOrEmpty(dialog.dialogText.owner))
                        {
                            dialog.dialogText.owner = dictionary;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(dialog.dialogText.owner))
                        {
                            ownerDictionaries.Add(dialog.dialogText.name, dialog.dialogText.owner);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dialog.dialogText.text))
                {
                    var matches = eventRegex.Matches(dialog.dialogText.text);
                    int indexAdd = 0;
                    int indexAnimAdd = 0;
                    for (var i = 0; i < matches.Count; i++)
                    {
                        var match = matches[i];
                        var commands = match.Groups["Event"].Value.Split(':');

                        switch (commands[0])
                        {
                            case "Anim":
                            {
                                var dialogTextAnimation = new DialogTextAnimation()
                                {
                                    startIndex = match.Index - indexAnimAdd,
                                    type = Utility.GetEnum<DialogTextAnimationType>(commands[1])
                                };
                                if (commands.Length < 3)
                                {
                                    switch (dialogTextAnimation.type)
                                    {
                                        case DialogTextAnimationType.WAIT:
                                            dialogTextAnimation.parameter = 0.5f;
                                            break;
                                        case DialogTextAnimationType.ANIM:
                                            dialogTextAnimation.parameter = 1;
                                            break;
                                    }
                                }
                                else
                                {
                                    dialogTextAnimation.parameter = float.Parse(commands[2]);
                                }

                                dialog.dialogText.dialogAnimations.Add(dialogTextAnimation);
                                break;
                            }
                        }
                        indexAnimAdd += match.Groups["Event"].Value.Length + 2;
                    }

                    dialog.dialogText.text = Regex.Replace(dialog.dialogText.text, eventRegexString, "");
                }

                if (dialog.dialogText.active && dialog.characters != null)
                {
                    if (!string.IsNullOrEmpty(dialog.dialogText.owner))
                    {
                        var findCharacter = dialog.characters.Find((character) => character.dark && dialog.dialogText.owner == character.name);
                        if (findCharacter != null)
                            findCharacter.dark = false;
                    }
                }
            }

            if (dialog.optionList != null && dialog.optionList.Count > 0)
            {
                for (int i = 0; i < dialog.optionList.Count; i++)
                {
                    DialogOption dialogOption = dialog.optionList[i];
                    if (dialogOption.dialogs != null && dialogOption.dialogs.Count > 0)
                    {
                        string tipEventDialogName = $"{assetName}_Option_{i}";
                        ParsingDialogs(tipEventDialogName, ref dialogOption.dialogs, ownerDictionaries);
                        CreateNewAsset(tipEventDialogName, dialogOption.dialogs, ownerDictionaries);
                        dialogOption.dialogs = null;
                        dialogOption.dialog = tipEventDialogName;
                    }
                }
            }

            foreach (var character in dialog.characters)
            {
                if (string.IsNullOrEmpty(character.clothes))
                {
                    if (!characterDictionary.ContainsKey(character.name))
                    {
                        characterDictionary.Add(character.name, "Default");
                    }

                    character.clothes = characterDictionary[character.name];
                }
                else
                {
                    characterDictionary[character.name] = character.clothes;
                }

                string face = character.face;
                if (string.IsNullOrEmpty(face))
                {
                    if (!faceDictionary.ContainsKey(character.name))
                    {
                        faceDictionary.Add(character.name, "Default");
                    }

                    face = faceDictionary[character.name];
                }

                character.face = face;

                DialogCharacterPos pos = character.pos;
                if (pos == DialogCharacterPos.N)
                {
                    if (!posDictionary.ContainsKey(character.name))
                    {
                        posDictionary.Add(character.name, DialogCharacterPos.C);
                    }

                    pos = posDictionary[character.name];
                }

                character.pos = pos;

                DialogCharacterSize size = character.size;
                if (size == DialogCharacterSize.N)
                {
                    if (!sizeDictionary.ContainsKey(character.name))
                    {
                        sizeDictionary.Add(character.name, DialogCharacterSize.M);
                    }

                    size = sizeDictionary[character.name];
                }

                character.size = size;

                if (dialog.animationLists.Count > 0)
                {
                    var firstAnimation = dialog.animationLists.Find((DialogAnimationList anim) => anim.index == 0);
                    if (firstAnimation == null) return;

                    foreach (var anim in firstAnimation.animations)
                    {
                        if (anim.type != DialogAnimationType.CHAR) continue;

                        if (anim.name == character.name)
                        {
                            switch (anim.effect)
                            {
                                case "Face":
                                    face = anim.parameter;
                                    break;
                                case "Scale":
                                    size = Utility.GetEnum<DialogCharacterSize>(anim.parameter);
                                    break;
                                case "Move":
                                    pos = Utility.GetEnum<DialogCharacterPos>(anim.parameter);
                                    break;
                            }
                        }
                    }

                    if (dialog.dialogText.dialogAnimations.Count > 0)
                    {
                        foreach (var dialogAnimation in dialog.dialogText.dialogAnimations)
                        {
                            if (dialogAnimation.type != DialogTextAnimationType.ANIM) continue;

                            var animation = dialog.animationLists.Find(animation => animation.index == Mathf.RoundToInt(dialogAnimation.parameter));
                            foreach (var anim in animation.animations)
                            {
                                if (anim.type != DialogAnimationType.CHAR) continue;

                                if (anim.name == character.name)
                                {
                                    switch (anim.effect)
                                    {
                                        case "Face":
                                            face = anim.parameter;
                                            break;
                                        case "Scale":
                                            size = Utility.GetEnum<DialogCharacterSize>(anim.parameter);
                                            break;
                                        case "Move":
                                            pos = Utility.GetEnum<DialogCharacterPos>(anim.parameter);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                faceDictionary[character.name] = face;

                posDictionary[character.name] = pos;

                sizeDictionary[character.name] = size;
            }
        }
    }
}