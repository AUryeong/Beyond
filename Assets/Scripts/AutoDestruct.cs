using UnityEngine;

public class AutoDestruct : MonoBehaviour
{
    public float duration;
    private float leftDuration;
    private void OnEnable()
    {
        leftDuration = duration;
    }

    private void Update()
    {
        leftDuration -= Time.deltaTime;
        if (leftDuration <= 0)
            gameObject.SetActive(false);
    }
}
