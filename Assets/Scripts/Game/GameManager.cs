using UnityEngine;


public class GameManager : Singleton<GameManager>
{
    protected override bool IsDontDestroying => true;
    private readonly Vector2 CAMERA_RENDER_SIZE = new Vector2(1070, 2532);

    public Camera UICamera => uiCamera;
    [SerializeField] private Camera uiCamera;

    [Header("매니저들")]
    public DialogManager dialogManager;
    public SoundManager soundManager;
    public ResourcesManager resourcesManager;
    protected override void OnCreated()
    {
        SetResolution();
        
        Application.targetFrameRate = Application.platform == RuntimePlatform.Android ? 30 : 120;
        
        dialogManager.OnCreated();
        soundManager.OnCreated();
        resourcesManager.OnCreated();
    }

    private void SetResolution()
    {
        int setWidth = Mathf.CeilToInt(CAMERA_RENDER_SIZE.x);
        int setHeight = Mathf.CeilToInt(CAMERA_RENDER_SIZE.y);

        int deviceWidth = Screen.width;
        int deviceHeight = Screen.height;

        Screen.SetResolution(setWidth, (int)((float)deviceHeight / deviceWidth * setWidth), true);

        float screenMultiplier = (float)setWidth / setHeight;
        float deviceMultiplier = (float)deviceWidth / deviceHeight;

        if (screenMultiplier < deviceMultiplier)
        {
            float newWidth = screenMultiplier / deviceMultiplier;
            foreach(var cam in Camera.allCameras)
                cam.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else
        {
            float newHeight = deviceMultiplier / screenMultiplier;
            foreach(var cam in Camera.allCameras)
                cam.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }
    }

    private void Start()
    {
        dialogManager.AddDialog("editor");
    }

    protected override void OnReset()
    {
        dialogManager.OnReset();
        soundManager.OnReset();
        resourcesManager.OnReset();
    }
}