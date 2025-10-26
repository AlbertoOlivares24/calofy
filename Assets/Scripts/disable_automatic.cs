using UnityEngine;

public class disable_automatic : MonoBehaviour
{
    public GameObject item_to_disable;
    public float reset_timer;
    float timer_to_disable;

    private void Start()
    {
        timer_to_disable = reset_timer;
    }

    private void OnEnable()
    {
        timer_to_disable = reset_timer;
    }

    void Update()
    {
        timer_to_disable = timer_to_disable - Time.deltaTime;
        if (timer_to_disable <= 0)
        {
            item_to_disable.SetActive(false);
        }
    }
}
