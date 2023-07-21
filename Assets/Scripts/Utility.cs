using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T GetEnum<T>(string enumName)
    {
        return (T)Enum.Parse(typeof(T), enumName);
    }

    public static Vector3 GetVector3Aver(Vector3 vector, Vector3 vector2)
    {
        return (vector + vector2) / 2;
    }

    public static Color GetFadeColor(Color color, float fade)
    {
        color.a = fade;
        return color;
    }

    public static DialogEventType GetStringToEventType(string eventType)
    {
        if (!string.IsNullOrEmpty(eventType))
        {
            switch (eventType)
            {

                case "Before":
                case "BEFORE":
                    return DialogEventType.BEFORE;
                case "Change":
                case "CHANGE":
                    return DialogEventType.CHANGE;
                case "After":
                case "AFTER":
                    return DialogEventType.AFTER;
            }
        }
        return DialogEventType.CHANGE;
    }

    public static float PosToVector2(string pos)
    {
        switch (pos)
        {
            case "REND":
                return 1580;
            case "R3":
                return 540f;
            case "R2":
                return 360f;
            case "R1":
                return 180;
            case "C":
                return 0;
            case "L1":
                return -180;
            case "L2":
                return -360;
            case "L3":
                return -540;
            case "LEND":
                return -1580;
            default:
                return 0;
        }
    }

    public static float PosToVector2(DialogCharacterPos pos)
    {
        switch (pos)
        {
            case DialogCharacterPos.LEND:
                return -1580;
            case DialogCharacterPos.L3:
                return -540f;
            case DialogCharacterPos.L2:
                return -360f;
            case DialogCharacterPos.L1:
                return -180;
            case DialogCharacterPos.C:
                return 0;
            case DialogCharacterPos.R1:
                return 180;
            case DialogCharacterPos.R2:
                return 360;
            case DialogCharacterPos.R3:
                return 540;
            case DialogCharacterPos.REND:
                return 1580;
            default:
                return 0;
        }
    }

    public static Vector3 SizeToScale(string size)
    {
        switch (size)
        {
            default:
                return new Vector3(0.8f, 0.8f, 0.8f);
            case "M":
                return new Vector3(1f, 1f, 1f);
            case "L":
                return new Vector3(1.2f, 1.2f, 1.2f);
            case "XL":
                return new Vector3(1.4f, 1.4f, 1.4f);
        }
    }

    public static Vector3 SizeToScale(DialogCharacterSize size)
    {
        switch (size)
        {
            default:
            case DialogCharacterSize.S:
                return new Vector3(0.8f, 0.8f, 0.8f);
            case DialogCharacterSize.M:
                return new Vector3(1f, 1f, 1f);
            case DialogCharacterSize.L:
                return new Vector3(1.2f, 1.2f, 1.2f);
            case DialogCharacterSize.XL:
                return new Vector3(1.4f, 1.4f, 1.4f);
        }
    }

    public static T SelectOne<T>(List<T> tList)
    {
        return tList[UnityEngine.Random.Range(0, tList.Count)];
    }

    public static T SelectOne<T>(params T[] tList)
    {
        return tList[UnityEngine.Random.Range(0, tList.Length)];
    }
    
    public static Color fadeBlackColor = new Color(0.16f, 0.16f, 0.16f, 0);
    public static Color darkColor = new Color(0.75f, 0.75f, 0.75f, 1);
}