using UnityEngine;

public class trigger_robot_emotion : MonoBehaviour
{
    public robot_script script_robot;
    public bool is_wrong;
    public bool is_correct;
    public bool is_finished_game;
    public trigger_robot_emotion this_component;
    float finish_game_timer = 6;

    private void OnEnable()
    {
        if(is_wrong == true)
        {
            int ran = Random.Range(0, 2);
            if(ran == 0)
            {
                script_robot.is_neutral = false;
                script_robot.is_sad = false;
                script_robot.is_happy = false;
                script_robot.is_suprised = false;
                script_robot.is_mad = false;
                script_robot.is_mad = true;
            }
            else
            {
                script_robot.is_neutral = false;
                script_robot.is_sad = false;
                script_robot.is_happy = false;
                script_robot.is_suprised = false;
                script_robot.is_mad = false;
                script_robot.is_sad = true;
            }
        }

        if (is_correct == true)
        {
            int ran = Random.Range(0, 2);
            if (ran == 0)
            {
                script_robot.is_neutral = false;
                script_robot.is_sad = false;
                script_robot.is_happy = false;
                script_robot.is_suprised = false;
                script_robot.is_mad = false;
                script_robot.is_happy = true;
            }
            else
            {
                script_robot.is_neutral = false;
                script_robot.is_sad = false;
                script_robot.is_happy = false;
                script_robot.is_suprised = false;
                script_robot.is_mad = false;
                script_robot.is_suprised = true;
            }
        }


     /*   if (is_finished_game == true)
        {
            script_robot.is_mad = false;
            script_robot.is_neutral = false;
            script_robot.is_sad = false;
            script_robot.is_happy = false;
            script_robot.is_suprised = false;
            script_robot.is_finished_game = true;
        }*/
    }

    private void Update()
    {
        if(is_finished_game == true)
        {
            finish_game_timer = finish_game_timer - Time.deltaTime;
            if(finish_game_timer <= 0)
            {
                script_robot.is_mad = false;
                script_robot.is_neutral = false;
                script_robot.is_sad = false;
                script_robot.is_happy = false;
                script_robot.is_suprised = false;
                script_robot.is_finished_game = true;
                Debug.Log("WE FINISHED THE GAME");
                this_component.enabled = false;
            }
        }
    }

}
