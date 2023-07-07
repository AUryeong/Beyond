using UnityEngine;


public class GameManager : Singleton<GameManager>
{
    protected override bool IsDontDestroying => true;

    public Camera UICamera => uiCamera;
    [SerializeField] private Camera uiCamera;

    [Header("매니저들")]
    public DialogManager dialogManager;
    public SoundManager soundManager;
    public ResourcesManager resourcesManager;
    protected override void OnCreated()
    {
        Application.targetFrameRate = Application.platform == RuntimePlatform.Android ? 30 : 120;
        
        dialogManager.OnCreated();
        soundManager.OnCreated();
        resourcesManager.OnCreated();
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