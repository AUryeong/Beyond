using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIStanding : MonoBehaviour
    {
        [SerializeField] private Image baseStanding;
        [SerializeField] private Image sideStanding;
        [SerializeField] private Image face;
        [SerializeField] private Image sideFace;

        public DialogCharacter NowStanding { get; private set; }

        private RectTransform rectTransform;

        [Header("감정 표현용")][SerializeField] private Image emotionBase;

        [Space(10)][SerializeField] private Image[] thinkingPoints;

        [Space(10)][SerializeField] private Image flushBase;
        [SerializeField] private Image flushLine;

        [Space(10)][SerializeField] private Image funnyNote;

        [Space(10)][SerializeField] private Image worry;

        [Space(10)][SerializeField] private Image tiredSweat;
        [SerializeField] private Image tiredSmallSweat;

        [Space(10)][SerializeField] private Image surprise;

        [Space(10)][SerializeField] private Image question;

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Init()
        {
            if (NowStanding == null)
            {
                gameObject.SetActive(false);
                return;
            }

            NowStanding = null;

            if (!gameObject.activeSelf) return;

            baseStanding.DOKill();
            baseStanding.DOColor(Utility.fadeBlackColor, 0.5f).OnComplete(() => { gameObject.SetActive(false); });

            face.DOKill();
            face.DOColor(Utility.fadeBlackColor, 0.5f);

            emotionBase.gameObject.SetActive(false);
        }

        public void ShowCharacter(DialogCharacter dialogStanding)
        {
            if (!dialogStanding.dark)
                rectTransform.SetAsLastSibling();

            var character = GameManager.Instance.resourcesManager.GetCharacter(dialogStanding.name);
            var standing = character.standings[dialogStanding.clothes];

            var prevStanding = NowStanding;
            NowStanding = dialogStanding;

            var toColor = NowStanding.dark ? Utility.darkColor : Color.white;
            var toScale = Utility.SizeToScale(NowStanding.size);
            var toPos = Utility.PosToVector2(NowStanding.pos);

            bool isStandingEqual = prevStanding != null && prevStanding.name.Equals(NowStanding.name);
            if (isStandingEqual)
            {
                rectTransform.DOKill(true);

                if (!prevStanding.face.Equals(NowStanding.face))
                {
                    FaceChange(NowStanding.face, true);
                }
                else
                {
                    face.DOKill();
                    face.DOColor(toColor, 0.5f);
                }

                if (!prevStanding.clothes.Equals(NowStanding.clothes))
                {
                    ClothesChange(NowStanding.clothes, true);
                }
                else
                {
                    baseStanding.DOKill();
                    baseStanding.DOColor(toColor, 0.5f);
                }

                if (!prevStanding.size.Equals(NowStanding.size))
                    Scale(toScale, 1, true);

                if (!prevStanding.pos.Equals(NowStanding.pos))
                    Move(toPos, 1);
                return;
            }

            rectTransform.DOKill(true);
            rectTransform.localScale = toScale;
            rectTransform.anchoredPosition = new Vector2(toPos, rectTransform.anchoredPosition.y);

            baseStanding.DOKill();
            baseStanding.sprite = standing.baseStanding;
            baseStanding.color = Utility.fadeBlackColor;
            baseStanding.DOColor(toColor, 0.5f);

            face.DOKill();
            face.sprite = standing.faces[dialogStanding.face];
            face.color = Utility.fadeBlackColor;
            face.DOColor(toColor, 0.5f);

            sideFace.gameObject.SetActive(false);
            sideStanding.gameObject.SetActive(false);
        }

        public void SetDark(bool dark)
        {
            if (!dark)
                transform.SetAsLastSibling();
            var toColor = dark ? Utility.darkColor : Color.white;

            NowStanding.dark = dark;

            baseStanding.DOKill();
            baseStanding.DOColor(toColor, 0.5f);

            face.DOKill();
            face.DOColor(toColor, 0.5f);
        }

        public void FaceChange(string faceName, bool isSet = false)
        {
            var duration = 0.4f;
            var toColor = NowStanding.dark ? Utility.darkColor : Color.white;

            NowStanding.face = faceName;

            sideFace.DOKill();
            sideFace.sprite = face.sprite;
            sideFace.color = toColor;
            sideFace.gameObject.SetActive(true);
            sideFace.DOFade(0, duration).SetEase(Ease.Linear).OnComplete(() => { sideFace.gameObject.SetActive(false); });

            face.DOKill();
            face.color = Utility.GetFadeColor(toColor, 0);
            face.sprite = GameManager.Instance.resourcesManager.GetCharacter(NowStanding.name).standings[NowStanding.clothes].faces[faceName];
            face.DOFade(1, duration).SetEase(Ease.Linear);

            if (!isSet && !NowStanding.dark)
                rectTransform.SetAsLastSibling();
        }

        private void ClothesChange(string clothesName, bool isSet = false)
        {
            var duration = 0.4f;
            var toColor = NowStanding.dark ? Utility.darkColor : Color.white;

            NowStanding.face = clothesName;

            sideStanding.DOKill();
            sideStanding.sprite = baseStanding.sprite;
            sideStanding.color = toColor;
            sideStanding.gameObject.SetActive(true);
            sideStanding.DOFade(0, duration).SetEase(Ease.Linear).OnComplete(() => { sideStanding.gameObject.SetActive(false); });

            baseStanding.DOKill();
            baseStanding.color = Utility.GetFadeColor(toColor, 0);
            baseStanding.sprite = GameManager.Instance.resourcesManager.GetCharacter(NowStanding.name).standings[clothesName].baseStanding;
            baseStanding.DOFade(1, duration).SetEase(Ease.Linear);

            if (!isSet && !NowStanding.dark)
                rectTransform.SetAsLastSibling();
        }

        public void Move(float posX, float duration)
        {
            rectTransform.DOAnchorPosX(posX, duration);
        }

        public void Scale(Vector3 scale, float duration, bool isSet = false)
        {
            rectTransform.DOScale(scale, duration);
            if (!isSet && !NowStanding.dark)
                rectTransform.SetAsLastSibling();
        }

        public void Bounce(int repeat, float duration)
        {
            rectTransform.DOAnchorPosY(-50, duration).SetRelative().SetLoops(repeat * 2, LoopType.Yoyo).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 431.5f);
            });
            if (!NowStanding.dark)
                rectTransform.SetAsLastSibling();
        }

        public void Shake(float power, float duration)
        {
            if (power < 0)
                power = 6;
            rectTransform.DOShakeAnchorPos(duration, power, 30, 90, false, false).SetRelative();
            if (!NowStanding.dark)
                rectTransform.SetAsLastSibling();
        }

        public void Emotion(string emotionName, float duration)
        {
            if (!NowStanding.dark)
                rectTransform.SetAsLastSibling();

            if (duration < 0)
                duration = 1f;

            float fadeOutDelay = duration + 0.5f;
            float fadeOutDuration = 0.3f;

            emotionBase.gameObject.SetActive(true);
            emotionBase.DOKill();
            emotionBase.color = Color.white;
            emotionBase.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay).OnComplete(() => { emotionBase.gameObject.SetActive(false); });

            emotionBase.rectTransform.DOKill();
            emotionBase.rectTransform.localScale = Vector3.zero;
            emotionBase.rectTransform.DOScale(Vector3.one, fadeOutDuration);

            foreach (Image image in thinkingPoints)
            {
                image.DOKill();
                image.gameObject.SetActive(false);
            }

            flushBase.DOKill();
            flushBase.gameObject.SetActive(false);
            flushLine.DOKill();
            flushLine.rectTransform.DOKill();
            flushLine.gameObject.SetActive(false);

            funnyNote.DOKill();
            funnyNote.gameObject.SetActive(false);

            worry.DOKill();
            worry.rectTransform.DOKill();
            worry.gameObject.SetActive(false);

            tiredSweat.DOKill();
            tiredSweat.rectTransform.DOKill();
            tiredSweat.gameObject.SetActive(false);
            tiredSmallSweat.DOKill();
            tiredSmallSweat.rectTransform.DOKill();
            tiredSmallSweat.gameObject.SetActive(false);

            surprise.DOKill();
            surprise.rectTransform.DOKill();
            surprise.gameObject.SetActive(false);

            question.DOKill();
            question.rectTransform.DOKill();
            question.gameObject.SetActive(false);

            switch (emotionName)
            {
                case "Thinking":
                    for (var index = 0; index < thinkingPoints.Length; index++)
                    {
                        var image = thinkingPoints[index];
                        image.gameObject.SetActive(true);
                        image.color = Utility.GetFadeColor(Color.white, 0);
                        image.DOFade(1, duration / 6).SetDelay(duration / 4 * index);
                        image.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);
                    }

                    break;
                case "Flush":
                    flushBase.gameObject.SetActive(true);
                    flushBase.color = Utility.GetFadeColor(Color.white, 0);
                    flushBase.DOFade(1, duration / 10);
                    flushBase.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    flushLine.gameObject.SetActive(true);
                    flushLine.rectTransform.DOLocalRotate(new Vector3(0, 0, 10), duration / 3).OnComplete(() =>
                    {
                        flushLine.rectTransform.DOLocalRotate(new Vector3(0, 0, -10), duration / 3).OnComplete(() =>
                        {
                            flushLine.rectTransform.DOLocalRotate(new Vector3(0, 0, 0), duration / 3);
                        });
                    });
                    flushLine.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);
                    break;
                case "Funny":
                    funnyNote.gameObject.SetActive(true);
                    funnyNote.color = Utility.GetFadeColor(Color.white, 0);
                    funnyNote.DOFade(1, duration / 10);
                    funnyNote.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    funnyNote.rectTransform.anchoredPosition = new Vector2(20, 44);
                    funnyNote.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);

                    funnyNote.rectTransform.DOAnchorPos(new Vector2(-30, 64), fadeOutDelay);
                    funnyNote.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(5f, 15f)), duration / 2).OnComplete(() =>
                    {
                        funnyNote.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(-15f, -35f)), duration / 2).OnComplete(() =>
                        {
                            funnyNote.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(10f, 30f)), 0.5f);
                        });
                    });
                    break;
                case "Worry":
                    worry.gameObject.SetActive(true);
                    worry.color = Utility.GetFadeColor(Color.white, 0);
                    worry.DOFade(1, duration / 10);
                    worry.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    worry.rectTransform.anchoredPosition = Vector2.zero;
                    worry.rectTransform.localScale = new Vector3(1, 1, 1);
                    worry.rectTransform.localRotation = Quaternion.identity;

                    worry.rectTransform.DOShakeScale(fadeOutDelay, 0.4f);
                    worry.rectTransform.DOShakePosition(fadeOutDelay, 10, 10, 90F, false, false);
                    worry.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(35f, 15f)), duration).OnComplete(() =>
                    {
                        worry.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(-15f, -35f)), fadeOutDuration);
                    });
                    break;
                case "Sweat":
                    tiredSmallSweat.gameObject.SetActive(true);
                    tiredSmallSweat.color = Utility.GetFadeColor(Color.white, 0);
                    tiredSmallSweat.DOFade(1, duration / 10);
                    tiredSmallSweat.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    tiredSmallSweat.rectTransform.anchoredPosition = new Vector2(-32.9f, 34.7f);
                    tiredSmallSweat.rectTransform.DOAnchorPosY(24.7f, fadeOutDelay);

                    tiredSweat.gameObject.SetActive(true);
                    tiredSweat.color = Utility.GetFadeColor(Color.white, 0);
                    tiredSweat.DOFade(1, duration / 10);
                    tiredSweat.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    tiredSweat.rectTransform.anchoredPosition = new Vector2(25, 20.9f);
                    tiredSweat.rectTransform.DOAnchorPosY(-14.8f, fadeOutDelay);
                    break;
                case "Surprise":
                    surprise.gameObject.SetActive(true);
                    surprise.color = Utility.GetFadeColor(Color.white, 0);
                    surprise.DOFade(1, duration / 10);
                    surprise.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    surprise.rectTransform.localScale = Vector3.zero;
                    surprise.rectTransform.DOScale(1, duration).SetEase(Ease.OutBack);
                    break;
                case "Question":
                    question.gameObject.SetActive(true);
                    question.color = Utility.GetFadeColor(Color.white, 0);
                    question.DOFade(1, duration / 10);
                    question.DOFade(0, fadeOutDuration).SetDelay(fadeOutDelay);

                    question.rectTransform.localScale = Vector3.zero;
                    question.rectTransform.DOScale(1, duration).SetEase(Ease.OutBack);
                    question.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(0, 15f)), duration).OnComplete(() =>
                    {
                        question.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(-15f, 0)), fadeOutDuration).SetRelative();
                    });
                    break;
            }
        }
    }
}