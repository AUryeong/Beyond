using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

public class Dialogs : ScriptableObject
{
    public string cgTitle;
    
    [TextArea] public string skipText;
    public List<Dialog> dialogs = new List<Dialog>();
}

[System.Serializable]
public class Dialog
{
    [XmlElement("Bgm")] 
    public string bgm = "";

    [XmlElement("BG")] 
    public DialogBackground dialogBackground = new DialogBackground();

    [XmlElement("Text")] 
    public DialogText dialogText;

    [XmlElement("Event")] 
    public List<DialogEvent> eventList = new List<DialogEvent>();

    [XmlElement("Option")] 
    public List<DialogOption> optionList;

    [XmlElement("Popup")] 
    public DialogPopup dialogPopup;

    [XmlArray("Characters")]
    [XmlArrayItem("Character")]
    public List<DialogCharacter> characters = new List<DialogCharacter>();

    [FormerlySerializedAs("animations")] [XmlElement("Animations")] 
    public List<DialogAnimationList> animationLists = new List<DialogAnimationList>();
}

[System.Serializable]
public class DialogTextAnimation
{
    [XmlAttribute("Index")]
    public int startIndex = 0;

    [XmlAttribute("Type")] 
    public DialogTextAnimationType type = DialogTextAnimationType.ANIM;
    
    [XmlAttribute("Parameter")]
    public float parameter;
}

[System.Serializable]
public class DialogAnimationList
{
    [XmlAttribute("Index")]
    public int index = 0;
    
    [XmlElement("Animation")]
    public List<DialogAnimation> animations = new List<DialogAnimation>();
}

[System.Serializable]
public class DialogOption
{
    [XmlAttribute("Script")] 
    public string script;

    [XmlElement("SkipText")] 
    public string skipText;
    
    [XmlAttribute("Dialog")] 
    public string dialog;

    [FormerlySerializedAs("eventType")] [XmlAttribute("DialogType")]
    public DialogEventType dialogEventType;
    
    [XmlAttribute("Special")]
    public bool special;

    [XmlArray("Dialogs")]
    [XmlArrayItem("Dialog")]
    [HideInInspector]
    public List<Dialog> dialogs;

    [XmlElement("Event")] 
    public List<DialogEvent> eventList = new List<DialogEvent>();
}


[System.Serializable]
public class DialogTipEvent
{
    [XmlAttribute("Name")] 
    public string eventName;

    [XmlElement("SkipText")] 
    public string skipText;

    [XmlAttribute("Dialog")] 
    public string dialogName;

    [XmlArray("Dialogs")]
    [XmlArrayItem("Dialog")]
    [HideInInspector]
    public List<Dialog> dialogs;

    [FormerlySerializedAs("eventType")] [XmlAttribute("Type")]
    public DialogEventType dialogEventType = DialogEventType.CHANGE;
}

[System.Serializable]
public class DialogText
{
    [XmlElement("Name")] 
    public string name;

    [XmlElement("Text")] 
    public string text;

    [XmlElement("TipEvent")]  
    public List<DialogTipEvent> tipEvent = new List<DialogTipEvent>();

    [XmlElement("DialogAnimation")]  
    public List<DialogTextAnimation> dialogAnimations = new List<DialogTextAnimation>();

    [XmlAttribute("Active")] 
    public bool active = true;

    [XmlAttribute("Invisible")] 
    public bool invisible;

    [XmlAttribute("Owner")] 
    public string owner;

}

[System.Serializable]
public class DialogEvent
{
    [XmlAttribute("Name")] 
    public string name;
    
    [XmlAttribute("Type")] 
    public Type type;
    public enum Type
    {
        ADD_TIP,
    }
}

[System.Serializable]
public class SerializedVector2
{
    [XmlAttribute("X")]
    public float x;
    
    [XmlAttribute("Y")]
    public float y;

    public Vector2 GetVector2()
    {
        return new Vector2(x, y);
    }
}

[System.Serializable]
public class SerializedHtmlScale
{
    [XmlAttribute("X")]
    public string x;
    
    [XmlAttribute("Y")]
    public string y;

    public Vector2 GetScale(float defaultWidth, float defaultHeight)
    {
        bool isXNotSet = string.IsNullOrEmpty(x);
        bool isYNotSet = string.IsNullOrEmpty(y);

        float width = defaultWidth;
        float height = defaultHeight;
        
        if (!isXNotSet)
        {
            if (x.EndsWith("%"))
            {
                float percent = float.Parse(x.Replace("%", ""))/100f;
                width = defaultWidth * percent;
                if (isYNotSet)
                    height = defaultHeight * percent;
            }
            else
            {
                width = float.Parse(x.Replace("px", ""));
                if (isYNotSet)
                    height = width / defaultWidth * defaultHeight;
            }
        }
        
        if (!isYNotSet)
        {
            if (y.EndsWith("%"))
            {
                float percent = float.Parse(y.Replace("%", ""))/100f;
                height = defaultHeight * percent;
                if (isXNotSet)
                    width = defaultWidth * percent;
            }
            else
            {
                height = float.Parse(x.Replace("px", ""));
                if (isXNotSet)
                    width = width / defaultHeight * defaultHeight;
            }
        }

        return new Vector2(width, height);
    }
}

[System.Serializable]
public class DialogPopup
{
    [XmlAttribute("Name")] 
    public string name;

    [XmlElement("Pos")] 
    public SerializedVector2 pos = new SerializedVector2() { x = 0, y = 100 };

    [XmlElement("Pivot")] 
    public SerializedVector2 pivot = new SerializedVector2() { x = 0.5f, y = 0.5f };

    [XmlElement("Scale")] 
    public SerializedHtmlScale scale;
}


[System.Serializable]
public class DialogBackground
{
    [XmlAttribute("Name")] 
    public string name;
    
    [XmlAttribute("Effect")]
    public DialogBackgroundEffect effect = DialogBackgroundEffect.NONE;

    [XmlAttribute("Title")]
    public string title;

    [XmlAttribute("Description")]
    public string description;
    
    [XmlAttribute("Duration")] 
    public float effectDuration = 1;

    [XmlAttribute("Scale")] 
    public float scale = 1;
}

[System.Serializable]
public class DialogAnimation
{
    [XmlAttribute("Type")] 
    public DialogAnimationType type = DialogAnimationType.UTIL;

    [XmlAttribute("Effect")] 
    public string effect;

    [XmlAttribute("Name")] 
    public string name;

    [XmlAttribute("Parameter")] 
    public string parameter;

    [XmlAttribute("Duration")] 
    public float duration = -1;
}

[System.Serializable]
public class DialogCharacter
{
    [XmlAttribute("Name")] 
    public string name;

    [XmlAttribute("Clothes")] 
    public string clothes;

    [XmlAttribute("Face")] 
    public string face;

    [XmlAttribute("Pos")] 
    public DialogCharacterPos pos = DialogCharacterPos.N;

    [XmlAttribute("Size")] 
    public DialogCharacterSize size = DialogCharacterSize.N;

    [XmlAttribute("Dark")] 
    public bool dark = true;
}

public enum DialogBackgroundEffect
{
    NONE,
    TRANS,
    FADE
}

public enum DialogEventType
{
    BEFORE,
    CHANGE,
    AFTER
}

public enum DialogAnimationType
{
    CHAR,
    UTIL,
    DIAL,
    CAM
}

public enum DialogTextAnimationType
{
    WAIT,
    ANIM,
    SKIP
}
public enum DialogCharacterPos
{
    N,
    R3,
    R2,
    R1,
    REND,
    C,
    L1,
    L2,
    L3,
    LEND
}

public enum DialogCharacterSize
{
    N,
    S,
    M,
    L,
    XL
}