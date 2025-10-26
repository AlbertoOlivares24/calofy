using UnityEngine;

public class game_starter : MonoBehaviour
{
    public GameObject robot;
    public GameObject hammer_parent;
    public GameObject game_stuff;
    public AudioSource spawn_sound;
    public GameObject CALORIE_plate_1_text;
    public food_game_loop game_loop;
    public float spawn_speed;
    float respawn_timer = 10;
    bool start_respawn_timer;
    float timer_to_start = 20;

    private void Start()
    {
        robot.transform.localScale = new Vector3(0, 0, 0);
     //   game_stuff.transform.localScale = new Vector3(0, 0, 0);
       // hammer_parent.transform.localScale = new Vector3(0, 0, 0);
        robot.SetActive(false);
        game_stuff.SetActive(false);
        hammer_parent.SetActive(false);
    }

    void Update()
    {
        if (timer_to_start > 0)
        {
            timer_to_start = timer_to_start - Time.deltaTime;
            if (timer_to_start <= 15)
            {
                if (robot.activeInHierarchy == false)
                {
                    robot.SetActive(true);
                    spawn_sound.Play();
                }
            }

            if (timer_to_start <= 10)
            {
                if (game_stuff.activeInHierarchy == false)
                {
                    game_stuff.SetActive(true);
                    spawn_sound.Play();
                }
            }

            if (timer_to_start <= 5)
            {
                if (hammer_parent.activeInHierarchy == false)
                {
                    hammer_parent.SetActive(true);
                    spawn_sound.Play();
                }
            }
        }

        if (robot.activeInHierarchy == true && robot.transform.localScale.x <= 1.9f)
        {
            robot.transform.localScale = robot.transform.localScale + new Vector3(Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime), Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime), Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime));
        }
        /* if (game_stuff.activeInHierarchy == true && game_stuff.transform.localScale.x <= 1)
         {
             game_stuff.transform.localScale = robot.transform.localScale + new Vector3(Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime), Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime), Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime));
         }
         if (hammer_parent.activeInHierarchy == true && hammer_parent.transform.localScale.x <= 1)
         {
             hammer_parent.transform.localScale = robot.transform.localScale + new Vector3(Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime), Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime), Mathf.Lerp(0, 1, spawn_speed * Time.deltaTime));
         }*/

        if (CALORIE_plate_1_text.gameObject.activeInHierarchy == true && game_loop.hammer_health > 0)
        {
            start_respawn_timer = true;  
        }

        if(start_respawn_timer == true)
        {
            respawn_timer = respawn_timer - Time.deltaTime;
            if (respawn_timer < 6 && respawn_timer > 4 && game_stuff.activeInHierarchy == true)
            {
                spawn_sound.Play();
                game_stuff.SetActive(false);
            }
            if (respawn_timer <= 1.5f)
            {
                spawn_sound.Play();
                game_stuff.SetActive(true);
                respawn_timer = 10;
                start_respawn_timer = false;
            }
        }
    }
}
