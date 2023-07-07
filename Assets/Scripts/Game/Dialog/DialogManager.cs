using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIEffects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UI;

public class DialogManager : Manager
{
    private readonly Queue<Dialog> dialogQueue = new Queue<Dialog>();
    private Dialog nowDialog;
    private Dialog prevDialog;

    [SerializeField] private EventTrigger dialogEventTrigger;

    [Header("대화문")] [SerializeField] private GameObject dialogWindow;

    [SerializeField] private Image dialogueImage;
    [SerializeField] private TextMeshProUGUI dialogOwnerNameText;

    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("연출")] [SerializeField] private UITransitionEffect blackFadeIn;
    [SerializeField] private UITransitionEffect blackFadeOut;
    [SerializeField] private TextMeshProUGUI endTextEffect;

    private bool isEnding;

    [Space(20)] [SerializeField] private Image backgroundTitle;
    [SerializeField] private TextMeshProUGUI backgroundTitleText;
    [SerializeField] private TextMeshProUGUI backgroundTitleLore;

    private const float backgroundTitleDuration = 1.5f;

    [SerializeField] private Image popupImage;

    [Header("배경")] [SerializeField] private Image backgroundImage;
    [SerializeField] private Image subBackgroundImage;
    private UITransitionEffect subBackgroundEffect;

    [Header("스탠딩")] [SerializeField] private UIStanding uiStanding;
    [SerializeField] private RectTransform standingParent;
    private readonly List<UIStanding> uiStandings = new List<UIStanding>();

    [Header("애니메이션")] private readonly Queue<DialogAnimation> animations = new Queue<DialogAnimation>();
    private float animationWaitTime;

    private float dialogDuration;
    private const float defaultDialogDuration = 0.05f;

    private float autoDuration;
    private const float autoWaitTime = 2f;

    private readonly Dictionary<string, Action<UIStanding, DialogAnimation>> charAnimationPairs = new Dictionary<string, Action<UIStanding, DialogAnimation>>();
    private readonly Dictionary<string, Action<DialogAnimation>> camAnimationPairs = new Dictionary<string, Action<DialogAnimation>>();
    private readonly Dictionary<string, Action<DialogAnimation>> dialAnimationPairs = new Dictionary<string, Action<DialogAnimation>>();
    private readonly Dictionary<string, Action<DialogAnimation>> utilAnimationPairs = new Dictionary<string, Action<DialogAnimation>>();

    [Header("선택지")] [SerializeField] private UIOption uiOption;
    [SerializeField] private RectTransform optionParent;
    private readonly List<UIOption> uiOptions = new List<UIOption>();
    private bool isHasOption;
    private bool optionActive;
    public override void OnCreated()
    {
        subBackgroundEffect = subBackgroundImage.GetComponent<UITransitionEffect>();

        foreach (RectTransform rect in standingParent)
            uiStandings.Add(rect.GetComponent<UIStanding>());

        foreach (RectTransform rect in optionParent)
            uiOptions.Add(rect.GetComponent<UIOption>());

        dialogEventTrigger.triggers.Clear();

        var entry = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.PointerClick,
            callback = new EventTrigger.TriggerEvent()
        };
        entry.callback.AddListener(PointerClick);
        dialogEventTrigger.triggers.Add(entry);
        
        dialogWindow.gameObject.SetActive(false);
        
        AnimationInit();
    }

    public override void OnReset()
    {
        nowDialog = null;
        prevDialog = null;

        dialogQueue.Clear();
        animations.Clear();

        dialogDuration = 0;
        autoDuration = 0;

        dialogWindow.gameObject.SetActive(false);
        
        popupImage.gameObject.SetActive(false);

        blackFadeIn.gameObject.SetActive(false);
        blackFadeOut.gameObject.SetActive(false);

        isEnding = false;

        OptionReset();

        foreach (var obj in uiStandings)
        {
            obj.gameObject.SetActive(false);
            obj.Init();
        }
    }

    private void Update()
    {
        if (isEnding) return;

        DialogUpdate();
        AnimationWait();
    }

    private void EndingSetUp()
    {
        isEnding = true;
        foreach (var obj in uiStandings)
        {
            obj.gameObject.SetActive(false);
            obj.Init();
        }

        dialogWindow.gameObject.SetActive(false);

        popupImage.gameObject.SetActive(false);

        isEnding = false;
    }

    public void PointerClick(BaseEventData data)
    {
        if (isEnding) return;
        if (!dialogueImage.gameObject.activeSelf) return;

        if (descriptionText.maxVisibleCharacters < descriptionText.textInfo.characterCount)
        {
            int nowIndex = descriptionText.maxVisibleCharacters;
            descriptionText.maxVisibleCharacters = descriptionText.textInfo.characterCount;
            if (nowDialog.dialogText.dialogAnimations.Count <= 0) return;

            var dialogTextAnimations = nowDialog.dialogText.dialogAnimations.FindAll((dialogTextAnimation => dialogTextAnimation.startIndex >nowIndex));
            
            foreach (var dialogTextAnimation in dialogTextAnimations)
            {
                if (dialogTextAnimation.type == DialogTextAnimationType.ANIM)
                    EffectSetting(Mathf.RoundToInt(dialogTextAnimation.parameter));
                if (dialogTextAnimation.type == DialogTextAnimationType.SKIP)
                    NewDialog();
            }
        }
        else
        {
            if (nowDialog.optionList == null || nowDialog.optionList.Count <= 0)
                NewDialog();
        }
    }

    private void CheckNextNoDialogue()
    {
        if (animations.Count > 0) return;

        if (isHasOption)
        {
            if (!optionActive)
            {
                OptionSetting();
                optionActive = true;
            }

            return;
        }

        autoDuration += Time.deltaTime;
        if (autoDuration >= autoWaitTime)
        {
            autoDuration -= autoWaitTime;

            NewDialog();
        }
    }

    private void DialogUpdate()
    {
        if (!dialogWindow.gameObject.activeSelf) return;
        if (nowDialog == null) return;

        if (!dialogueImage.gameObject.activeSelf || nowDialog.dialogText == null)
        {
            CheckNextNoDialogue();
            return;
        }

        if (descriptionText.maxVisibleCharacters < descriptionText.textInfo.characterCount)
        {
            dialogDuration += Time.deltaTime;
            float nextDuration = defaultDialogDuration;
            if (dialogDuration >= nextDuration)
            {
                dialogDuration -= nextDuration;
                descriptionText.maxVisibleCharacters++;
                foreach (var dialogAnimation in nowDialog.dialogText.dialogAnimations)
                {
                    if (dialogAnimation.startIndex == descriptionText.maxVisibleCharacters)
                    {
                        switch (dialogAnimation.type)
                        {
                            case DialogTextAnimationType.WAIT:
                                dialogDuration -= dialogAnimation.parameter;
                                break;
                            case DialogTextAnimationType.ANIM:
                                EffectSetting(Mathf.RoundToInt(dialogAnimation.parameter));
                                break;
                            case DialogTextAnimationType.SKIP:
                                NewDialog();
                                break;
                        }
                    }
                }
            }
        }
        else
        {
            if (!endTextEffect.gameObject.activeSelf)
            {
                endTextEffect.gameObject.SetActive(true);
                endTextEffect.DOKill();
                endTextEffect.color = Utility.GetFadeColor(endTextEffect.color, 1);
                endTextEffect.DOFade(0, 0.8f).SetLoops(-1, LoopType.Yoyo);
            }

            var characterPos = new Vector3(-824.4f, 32.39394f);
            if (!string.IsNullOrWhiteSpace(descriptionText.text) && !descriptionText.text.Equals(string.Empty))
            {
                if (descriptionText.textInfo.characterInfo != null && descriptionText.maxVisibleCharacters > 0 &&
                    descriptionText.textInfo.characterInfo.Length >= descriptionText.maxVisibleCharacters - 1)
                {
                    characterPos = Utility.GetVector3Aver(
                        descriptionText.textInfo.characterInfo[descriptionText.maxVisibleCharacters - 1].topRight,
                        descriptionText.textInfo.characterInfo[descriptionText.maxVisibleCharacters - 1].bottomRight);
                }
            }

            endTextEffect.rectTransform.anchoredPosition = new Vector2(characterPos.x + 40, characterPos.y - 20);

            if (isHasOption)
            {
                if (!optionActive)
                {
                    OptionSetting();
                    optionActive = true;
                }

                return;
            }
        }

        EffectUpdate();
    }

    private void NewDialog(Dialog newDialog = null)
    {
        if (dialogQueue.Count <= 0)
        {
            EndingSetUp();

            animations.Clear();

            prevDialog = null;
            nowDialog = null;

            OptionReset();

            return;
        }

        prevDialog = nowDialog;
        dialogWindow.gameObject.SetActive(true);
        dialogueImage.DOKill();

        descriptionText.maxVisibleCharacters = 0;
        dialogDuration = 0;
        autoDuration = 0;
        animationWaitTime = 0;
        animations.Clear();

        optionParent.gameObject.SetActive(true);

        endTextEffect.gameObject.SetActive(false);
        nowDialog = newDialog ?? dialogQueue.Dequeue();

        isHasOption = nowDialog.optionList.Count > 0;
        optionActive = false;

        if (nowDialog.dialogText != null)
        {
            dialogOwnerNameText.text = string.IsNullOrEmpty(nowDialog.dialogText.name)
                ? " "
                : nowDialog.dialogText.name;
            descriptionText.text = string.IsNullOrEmpty(nowDialog.dialogText.text)
                ? " "
                : nowDialog.dialogText.text;
            dialogueImage.gameObject.SetActive(nowDialog.dialogText.active);
            dialogueImage.color = nowDialog.dialogText.invisible
                ? Color.clear
                : new Color(0.08627451F, 0.08627451F, 0.08627451F, 0.9137255F);
        }
        else
        {
            dialogueImage.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(nowDialog.bgm))
        {
            if (prevDialog == null || !nowDialog.bgm.Equals(prevDialog.bgm))
            {
                GameManager.Instance.soundManager.PlaySound(nowDialog.bgm);
            }
        }

        var background = GameManager.Instance.resourcesManager.GetBackground(nowDialog.dialogBackground.name);
        if (prevDialog != null)
        {
            if (prevDialog.dialogBackground.name != nowDialog.dialogBackground.name ||
                Math.Abs(prevDialog.dialogBackground.scale - nowDialog.dialogBackground.scale) > 0.01f ||
                prevDialog.dialogBackground.title != nowDialog.dialogBackground.title)
            {
                if (!string.IsNullOrEmpty(nowDialog.dialogBackground.name))
                {
                    subBackgroundImage.DOKill(true);
                    switch (nowDialog.dialogBackground.effect)
                    {
                        default:
                        case DialogBackgroundEffect.NONE:
                            backgroundImage.sprite = background;
                            backgroundImage.rectTransform.localScale = Vector3.one * nowDialog.dialogBackground.scale;
                            subBackgroundImage.gameObject.SetActive(false);
                            break;
                        case DialogBackgroundEffect.TRANS:
                            subBackgroundImage.gameObject.SetActive(true);
                            subBackgroundImage.sprite = background;
                            backgroundImage.rectTransform.localScale = Vector3.one * prevDialog.dialogBackground.scale;
                            subBackgroundImage.rectTransform.localScale = Vector3.one * nowDialog.dialogBackground.scale;
                            subBackgroundEffect.effectFactor = 0;
                            break;
                        case DialogBackgroundEffect.FADE:
                            subBackgroundEffect.effectFactor = 1;

                            backgroundImage.rectTransform.localScale = Vector3.one * prevDialog.dialogBackground.scale;

                            subBackgroundImage.gameObject.SetActive(true);
                            subBackgroundImage.sprite = background;
                            subBackgroundImage.rectTransform.localScale = Vector3.one * nowDialog.dialogBackground.scale;
                            subBackgroundImage.color = Utility.GetFadeColor(Color.white, 0);

                            subBackgroundImage.DOFade(1, nowDialog.dialogBackground.effectDuration).OnComplete(() =>
                            {
                                backgroundImage.sprite = subBackgroundImage.sprite;
                                backgroundImage.rectTransform.localScale = subBackgroundImage.rectTransform.localScale;
                                subBackgroundEffect.gameObject.SetActive(false);
                            });
                            break;
                    }
                }
            }
        }
        else
        {
            subBackgroundImage.DOKill(true);
            backgroundImage.sprite = background;
            backgroundImage.rectTransform.localScale = Vector3.one * nowDialog.dialogBackground.scale;
            subBackgroundImage.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(nowDialog.dialogBackground.title))
        {
            backgroundTitle.rectTransform.DOKill();
            backgroundTitle.rectTransform.anchoredPosition =
                new Vector2(-1377, backgroundTitle.rectTransform.anchoredPosition.y);
            backgroundTitle.rectTransform.DOAnchorPosX(0, backgroundTitleDuration).SetEase(Ease.OutQuart).OnComplete(() =>
            {
                backgroundTitle.rectTransform.DOAnchorPosX(1377, backgroundTitleDuration).SetEase(Ease.InQuart);
            });
            backgroundTitleText.text = nowDialog.dialogBackground.title;
            if (!string.IsNullOrEmpty(nowDialog.dialogBackground.description))
            {
                backgroundTitleLore.gameObject.SetActive(true);
                backgroundTitleLore.text = nowDialog.dialogBackground.description;
            }
            else
            {
                backgroundTitleLore.gameObject.SetActive(false);
            }
        }

        if (nowDialog.dialogPopup != null && !string.IsNullOrEmpty(nowDialog.dialogPopup.name))
        {
            Sprite popupSprite = GameManager.Instance.resourcesManager.GetPopup(nowDialog.dialogPopup.name);
            if (popupSprite != null)
            {
                popupImage.DOKill();
                popupImage.gameObject.SetActive(true);
            
                Vector2 pivot = nowDialog.dialogPopup.pivot.GetVector2();
                popupImage.rectTransform.anchorMin = pivot;
                popupImage.rectTransform.anchorMax = pivot;

                popupImage.rectTransform.anchoredPosition = nowDialog.dialogPopup.pos.GetVector2();

                popupImage.sprite = popupSprite;
                
                float x = popupSprite.rect.width / popupImage.pixelsPerUnit;
                float y = popupSprite.rect.height / popupImage.pixelsPerUnit;
                
                popupImage.rectTransform.sizeDelta = nowDialog.dialogPopup.scale.GetScale(x, y);
                popupImage.SetAllDirty();

                if (prevDialog.dialogPopup == null || string.IsNullOrEmpty(prevDialog.dialogPopup.name))
                {
                    popupImage.rectTransform.localScale = Vector3.zero;
                    popupImage.rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }
        else
        {
            if (prevDialog != null && prevDialog.dialogPopup != null && !string.IsNullOrEmpty(prevDialog.dialogPopup.name))
            {
                popupImage.rectTransform.DOKill();
                popupImage.rectTransform.localScale = Vector3.one;

                popupImage.rectTransform.DOScale(Vector3.zero, 0.3f).OnComplete(() => popupImage.gameObject.SetActive(false));
            }
            else
            {
                popupImage.gameObject.SetActive(false);
            }
        }

        if (nowDialog.dialogBackground.name.StartsWith("CG_"))
        {
            foreach (var standing in uiStandings)
            {
                standing.gameObject.SetActive(false);
                standing.Init();
            }

            EffectSetting();
            return;
        }

        if (uiStandings.Count < nowDialog.characters.Count)
        {
            int repeatCount = nowDialog.characters.Count - uiStandings.Count;
            for (int i = 0; i < repeatCount; i++)
            {
                var temp = Instantiate(uiStanding, standingParent);
                temp.Init();
                uiStandings.Add(temp);
            }
        }

        var standings = new List<UIStanding>(uiStandings);
        var dialogStandings = new List<DialogCharacter>(nowDialog.characters);

        foreach (var standing in nowDialog.characters)
        {
            var prevStanding = standings.Find(x =>
                x.NowStanding != null && x.NowStanding.name.Equals(standing.name) &&
                x.NowStanding.clothes.Equals(standing.clothes));

            if (prevStanding == null) continue;

            standings.Remove(prevStanding);
            prevStanding.gameObject.SetActive(true);
            prevStanding.ShowCharacter(standing);
            dialogStandings.Remove(standing);
        }

        foreach (var standing in dialogStandings)
        {
            var newStanding = standings[0];

            if (newStanding == null) continue;

            standings.Remove(newStanding);
            newStanding.gameObject.SetActive(true);
            newStanding.ShowCharacter(standing);
        }

        foreach (var standing in standings)
        {
            standing.Init();
        }

        EffectSetting();
    }

    #region Option

    private void OptionSetting()
    {
        if (nowDialog == null || nowDialog.optionList == null || nowDialog.optionList.Count <= 0) return;

        if (uiOptions.Count < nowDialog.optionList.Count)
        {
            int repeatCount = nowDialog.optionList.Count - uiOptions.Count;
            for (int i = 0; i < repeatCount; i++)
            {
                var temp = Instantiate(uiOption, optionParent);
                uiOptions.Add(temp);
            }
        }

        var options = new List<UIOption>(uiOptions);
        var dialogOptions = new List<DialogOption>(nowDialog.optionList);

        foreach (var option in dialogOptions)
        {
            var changeOption = options[0];

            if (changeOption == null) continue;

            options.Remove(changeOption);
            changeOption.gameObject.SetActive(true);
            changeOption.SetOption(option);
        }

        foreach (var option in options)
            option.gameObject.SetActive(false);
    }

    private void OptionReset()
    {
        foreach (var option in uiOptions)
            option.gameObject.SetActive(false);
    }


    public void SelectOption(UIOption setOption)
    {
        foreach (UIOption option in uiOptions)
        {
            if (option.gameObject.activeSelf)
            {
                if (option != setOption)
                {
                    option.Disable();
                }
            }
        }

        var dialogs = GameManager.Instance.resourcesManager.GetDialog(setOption.DialogOption.dialog);

        DialogAdd(setOption.DialogOption.dialogEventType, dialogs == null ? null : dialogs.dialogs);
    }

    #endregion

    #region Event

    #endregion

    #region Add / Save Dialog

    public void AddDialog(string dialogName)
    {
        var dialogs = GameManager.Instance.resourcesManager.GetDialog(dialogName);

        if (!string.IsNullOrEmpty(dialogs.cgTitle))

        AddDialog(dialogs);
    }

    public void AddDialog(Dialogs dialogs)
    {
        bool flag = dialogQueue.Count <= 0;
        foreach (var dialog in dialogs.dialogs)
            dialogQueue.Enqueue(dialog);

        if (flag) NewDialog();
    }

    public void AddDialog(List<Dialog> dialogs)
    {
        bool flag = dialogQueue.Count <= 0;
        foreach (var dialog in dialogs)
            dialogQueue.Enqueue(dialog);

        if (flag) NewDialog();
    }

    public void DialogAdd(DialogEventType dialogEventType, List<Dialog> dialogs)
    {
        if (dialogs == null || dialogs.Count <= 0)
        {
            NewDialog();
            return;
        }

        switch (dialogEventType)
        {
            case DialogEventType.BEFORE:
            {
                var leftDialogs = dialogQueue.ToList();
                dialogQueue.Clear();

                foreach (var dialog in dialogs)
                    dialogQueue.Enqueue(dialog);

                foreach (var dialog in leftDialogs)
                    dialogQueue.Enqueue(dialog);

                if (dialogs.Count != 0 && leftDialogs.Count != 0)
                    NewDialog();
                break;
            }
            case DialogEventType.AFTER:
            {
                bool flag = dialogQueue.Count <= 0;

                AddDialog(dialogs);
                if (!flag)
                    NewDialog();
                break;
            }
            default:
            case DialogEventType.CHANGE:
            {
                dialogQueue.Clear();
                AddDialog(dialogs);
                break;
            }
        }
    }

    #endregion

    #region Animation

    private void AnimationInit()
    {
        charAnimationPairs.Add("Move", (findStanding, anim) => findStanding.Move(Utility.PosToVector2(anim.parameter), anim.duration));
        charAnimationPairs.Add("Face", (findStanding, anim) => findStanding.FaceChange(anim.parameter));
        charAnimationPairs.Add("Bounce", (findStanding, anim) => findStanding.Bounce(int.Parse(anim.parameter), anim.duration));
        charAnimationPairs.Add("Shake", (findStanding, anim) => findStanding.Shake(float.Parse(anim.parameter), anim.duration));
        charAnimationPairs.Add("Scale", (findStanding, anim) => findStanding.Scale(Utility.SizeToScale(anim.parameter), anim.duration));
        charAnimationPairs.Add("Emotion", (findStanding, anim) => findStanding.Emotion(anim.parameter, anim.duration));
        charAnimationPairs.Add("Dark", (findStanding, anim) => findStanding.SetDark(bool.Parse(anim.parameter)));


        camAnimationPairs.Add("Black_FadeIn", (anim) =>
        {
            blackFadeIn.gameObject.SetActive(true);
            blackFadeIn.effectFactor = 0;
        });

        camAnimationPairs.Add("Black_FadeOut", (anim) =>
        {
            if (blackFadeIn.gameObject.activeSelf)
                blackFadeIn.gameObject.SetActive(false);
            blackFadeOut.gameObject.SetActive(true);
            blackFadeOut.effectFactor = 1;
        });


        utilAnimationPairs.Add("Wait", (anim) => animationWaitTime = anim.duration);


        dialAnimationPairs.Add("On", (anim) =>
        {
            if (nowDialog.dialogText == null) return;

            dialogueImage.DOKill();
            dialogueImage.gameObject.SetActive(true);
            if (nowDialog.dialogText.invisible)
            {
                dialogueImage.color = Color.clear;
            }
            else
            {
                dialogueImage.color = new Color(0.08627451F, 0.08627451F, 0.08627451F, 0);
                dialogueImage.DOFade(0.9137255F, 0.2f);
            }

            if (!string.IsNullOrEmpty(nowDialog.dialogText.owner))
            {
                foreach (var standing in uiStandings)
                {
                    if (!standing.gameObject.activeSelf) continue;

                    standing.SetDark(standing.NowStanding.name != nowDialog.dialogText.owner);
                }
            }
        });

        dialAnimationPairs.Add("Shake", (anim) =>
        {
            float power = float.Parse(anim.parameter);

            if (power < 0)
                power = 6;
            dialogueImage.rectTransform.DOKill(true);
            dialogueImage.rectTransform
                .DOShakeAnchorPos(anim.duration, power, 30, 90, false, false)
                .SetRelative();
        });
    }

    private void AnimationWait()
    {
        if (animationWaitTime > 0)
        {
            animationWaitTime -= Time.deltaTime;
            if (animationWaitTime <= 0)
                AnimationUpdate();
        }
    }

    private void AnimationUpdate()
    {
        if (!dialogWindow.gameObject.activeSelf) return;
        if (nowDialog == null) return;
        if (animations.Count <= 0) return;

        var anim = animations.Dequeue();
        switch (anim.type)
        {
            case DialogAnimationType.CHAR:
                var findStanding = uiStandings.Find((standing) =>
                    standing.gameObject.activeSelf && standing.NowStanding != null &&
                    standing.NowStanding.name == anim.name);
                if (findStanding == null) break;

                charAnimationPairs[anim.effect]?.Invoke(findStanding, anim);
                break;
            case DialogAnimationType.DIAL:
                dialAnimationPairs[anim.effect]?.Invoke(anim);
                break;
            case DialogAnimationType.CAM:
                camAnimationPairs[anim.effect]?.Invoke(anim);
                break;
            default:
            case DialogAnimationType.UTIL:
                utilAnimationPairs[anim.effect]?.Invoke(anim);
                return;
        }

        AnimationUpdate();
    }

    #endregion

    #region Effect

    private void EffectUpdate()
    {
        if (subBackgroundImage.gameObject.activeSelf)
        {
            if (subBackgroundEffect.effectFactor < 1)
            {
                subBackgroundEffect.effectFactor = Mathf.Min(1,
                    subBackgroundEffect.effectFactor + Time.deltaTime / nowDialog.dialogBackground.effectDuration);
                if (subBackgroundEffect.effectFactor >= 1)
                {
                    backgroundImage.sprite = subBackgroundImage.sprite;
                    backgroundImage.rectTransform.localScale = subBackgroundImage.rectTransform.localScale;
                    subBackgroundEffect.gameObject.SetActive(false);
                }
            }
        }

        if (blackFadeIn.gameObject.activeSelf)
        {
            blackFadeIn.effectFactor = Mathf.Min(1, blackFadeIn.effectFactor + Time.deltaTime);
        }

        if (blackFadeOut.gameObject.activeSelf)
        {
            blackFadeOut.effectFactor = Mathf.Max(0, blackFadeOut.effectFactor - Time.deltaTime);
            if (blackFadeOut.effectFactor <= 0)
                blackFadeOut.gameObject.SetActive(false);
        }
    }

    private void EffectSetting(int index = 0)
    {
        if (nowDialog.animationLists.Count <= 0) return;

        var dialogAnimation = nowDialog.animationLists.Find((anim) => anim.index == index);
        if (dialogAnimation == null) return;

        bool flag = animations.Count == 0;

        foreach (var anim in dialogAnimation.animations)
        {
            animations.Enqueue(anim);
        }

        if (flag)
            AnimationUpdate();
    }

    #endregion
}