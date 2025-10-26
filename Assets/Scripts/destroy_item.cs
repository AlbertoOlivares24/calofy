using UnityEngine;

public class destroy_item : MonoBehaviour
{
    public GameObject item_to_destroy;
    public float timer_to_destroy;

    private void Update()
    {
        timer_to_destroy = timer_to_destroy - Time.deltaTime;
        if (timer_to_destroy <= 0)
        {
            Destroy(item_to_destroy);
        }
    }
}
