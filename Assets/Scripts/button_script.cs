using UnityEngine;
using UnityEngine.SceneManagement;

public class button_script : MonoBehaviour
{
    public Animator this_animator;
    public AudioSource click_sound;
    public int button_num;
    public food_game_loop game_loop;
    public bool is_restart_button;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            this_animator.SetInteger("state", 1);
            click_sound.Play();
            if (is_restart_button == false)
            {
                game_loop.press_button(button_num);
            }

            if (is_restart_button == true)
            {
                SceneManager.LoadScene(0);
            }
        }
      
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            this_animator.SetInteger("state", 0);
        }    
    }


}
