using UnityEngine;
using RG.LabBot;
using System.Collections;

public class robot_script : MonoBehaviour
{
    [Header("Materials")]
    public Material eyes_material;
    public Material mouth_material;
    public Material talking_mouth_material;
    public Material robot_material;

    [Header("Emotions")]
    public bool is_neutral;
    public bool is_happy;
    public bool is_sad;
    public bool is_mad;
    public bool is_suprised;
    public bool is_finished_game;

    [Header("Emotion Colors")]
    public Color neutral_Color;
    public Color happy_Color;
    public Color sad_Color;
    public Color mad_Color;
    public Color suprised_Color;

    [Header("Animator")]
    public Animator robot_animator;

    public ParticleSystem transition_particle;

    [Header("Movement stuff")]
    public Transform player_to_look_at;
    public AudioSource drone_moving_sound;
    public Transform[] travel_locations;
    public float travel_speed;
    public float timer_to_travel;
    Transform place_to_transform;


    [Header("Talk stuff")]
    public AudioSource talking_audio;
    public RobotSpeech speech;
    [TextArea] public string prompt = "";
    bool has_spoken = false;
    public AudioSource reaction_sound;
    public GameObject mouth;

    //private variables
    bool reset_animator;
    float reset_animation_timer = 5;

    async void talk_robot()
    {
        if (is_neutral == true)
        {
            prompt = "narrate the line as if you are the one talking, keep it pg13 and do not say asterisk: you are a flying sphere, now respond to the player with a super short neutral tone sarcastic reaction line to this mini demo where the player has to try to select out of 3 dishes which food has the least amount of " +
                "calories, guessing correctly will earn them points, incorrectly will make you get this super cute slime creature closer to its invevitable demise by dropping a massive " +
                "hammer on top of it, you are a flying funny spehere that insults the player and in this case you have to react to the player in a neutral way, please remeber to keep the jokes short and simple for easier understanding, DO NOT INCLUDE ANY SNYMBOLS EXCEPT COMMAS OR PERIODS IN YOUR RESPONSE, JUST WORDS AND LETTERS, NO SYMBOLS";
        }
        if (is_mad == true)
        {
            prompt = "narrate the line as if you are the one talking, keep it pg13 and do not say asterisk: you are a flying sphere, now respond to the player with a super short mad tone sarcastic reaction line to this mini demo where the player has to try to select out of 3 dishes which food has the least amount of " +
               "calories, guessing correctly will earn them points, incorrectly will make you get this super cute slime creature closer to its invevitable demise by dropping a massive " +
               "hammer on top of it, you are a flying funny spehere that insults the player and in this case you have to react to the player in a sarcastic angry way and make fun of the player for choosing the wrong answer, tease the player and make fun of the slime that is about to get squashed, please remeber to keep the jokes short and simple for easier understanding, DO NOT INCLUDE ANY SNYMBOLS EXCEPT COMMAS OR PERIODS IN YOUR RESPONSE, JUST WORDS AND LETTERS, NO SYMBOLS";
        }
        if (is_happy == true)
        {
            prompt = "narrate the line as if you are the one talking, keep it pg13 and do not say asterisk: you are a flying sphere, now respond to the player with a super short happy tone sarcastic reaction line to this mini demo where the player has to try to select out of 3 dishes which food has the least amount of " +
               "calories, guessing correctly will earn them points, incorrectly will make you get this super cute slime creature closer to its invevitable demise by dropping a massive " +
               "hammer on top of it, you are a flying funny spehere that insults the player and in this case you have to react to the player in a happy way that the player chose the correct answer, give the player some props but make fun of them that they will most likely enp up squashing the slime, please remeber to keep the jokes short and simple for easier understanding, DO NOT INCLUDE ANY SNYMBOLS EXCEPT COMMAS OR PERIODS IN YOUR RESPONSE, JUST WORDS AND LETTERS, NO SYMBOLS";
        }
        if (is_sad == true)
        {
            prompt = "narrate the line as if you are the one talking, keep it pg13 and do not say asterisk: you are a flying sphere, now respond to the player with a super short sad tone sarcastic reaction line to this mini demo where the player has to try to select out of 3 dishes which food has the least amount of " +
               "calories, guessing correctly will earn them points, incorrectly will make you get this super cute slime creature closer to its invevitable demise by dropping a massive " +
               "hammer on top of it, you are a flying funny spehere that insults the player and in this case you have to react to the player in a sad sarcastic way that you cannot believe the player chose that answer and got it so incorrect when the right answer is so obvious, please remeber to keep the jokes short and simple for easier understanding, DO NOT INCLUDE ANY SNYMBOLS EXCEPT COMMAS OR PERIODS IN YOUR RESPONSE, JUST WORDS AND LETTERS, NO SYMBOLS";
        }
        if (is_suprised == true)
        {
            prompt = "narrate the line as if you are the one talking, keep it pg13 and do not say asterisk: you are a flying sphere, now respond to the player with a super short suprised tone sarcastic reaction line to this mini demo where the player has to try to select out of 3 dishes which food has the least amount of " +
               "calories, guessing correctly will earn them points, incorrectly will make you get this super cute slime creature closer to its invevitable demise by dropping a massive " +
               "hammer on top of it, you are a flying funny spehere that insults the player and in this case you have to react to the player in a suprised way that the player got the answer correctly, give the player props but make sure they do not feel too confident, please remeber to keep the jokes short and simple for easier understanding, DO NOT INCLUDE ANY SNYMBOLS EXCEPT COMMAS OR PERIODS IN YOUR RESPONSE, JUST WORDS AND LETTERS, NO SYMBOLS";
        }
        if(is_finished_game == true)
        {
            prompt = "narrate the line as if you are the one talking, keep it pg13 and do not say asterisk: you are a flying sphere, now respond to the player with a super short suprised tone sarcastic reaction line to this mini demo where the player has to try to select out of 3 dishes which food has the least amount of " +
               "calories, guessing correctly will earn them points, incorrectly will make you get this super cute slime creature closer to its invevitable demise by dropping a massive " +
               "hammer on top of it, in this specific prompt is the end of the game where the player got all the questions wrong and the slime has been squashed by the hammer, make a joke about this and a quick outro of the demo and thank the player for playing and hope to see them play the game again, please remeber to keep the jokes short and simple for easier understanding, DO NOT INCLUDE ANY SNYMBOLS EXCEPT COMMAS OR PERIODS IN YOUR RESPONSE, JUST WORDS AND LETTERS, NO SYMBOLS";
        }
        await speech.SpeakFromPromptAsync(prompt);
    }

    private void Start()
    {
        is_neutral = true;
        place_to_transform = travel_locations[0];
    }

    void Update()
    {
        //look at player
        this.transform.LookAt(player_to_look_at.position);

        //travel locations
        timer_to_travel = timer_to_travel - Time.deltaTime;
        if (timer_to_travel <= 0)
        {
            int ran = Random.Range(0, travel_locations.Length);
            place_to_transform = travel_locations[ran];
            timer_to_travel = 5;
        }
        this.transform.position = Vector3.Lerp(transform.position, place_to_transform.position, travel_speed * Time.deltaTime);

        /* if (Vector3.Distance(this.transform.position, place_to_transform.position) < 1 && drone_moving_sound.isPlaying)
         {
             drone_moving_sound.Stop();
         }
         else if (Vector3.Distance(this.transform.position, place_to_transform.position) > 1 && !drone_moving_sound.isPlaying)
         {
             drone_moving_sound.Play();
         }*/


        if (is_neutral == true)
        {
            //transition_particle.Play();
            is_mad = false;
            is_happy = false;
            is_sad = false;
            is_suprised = false;
            eyes_material.SetTextureOffset("_BaseMap", new Vector2(0f, 0f));
            mouth_material.SetTextureOffset("_BaseMap", new Vector2(0f, 0f));
            eyes_material.SetColor("_EmissionColor", neutral_Color);
            mouth_material.SetColor("_EmissionColor", neutral_Color);
            talking_mouth_material.SetColor("_EmissionColor", neutral_Color);
            robot_material.SetColor("_EmissionColor", neutral_Color);
            robot_animator.SetInteger("state", 0);
            //mouth.SetActive(true);
            has_spoken = false;
        }
        if (is_happy == true && has_spoken == false)
        {
            talk_robot();
            reaction_sound.Play();
            transition_particle.Play();
            has_spoken = true;
            is_mad = false;
            is_neutral = false;
            is_sad = false;
            is_suprised = false;
            eyes_material.SetTextureOffset("_BaseMap", new Vector2(0.1f, 0f));
            mouth_material.SetTextureOffset("_BaseMap", new Vector2(0.1f, 0f));
            eyes_material.SetColor("_EmissionColor", happy_Color);
            mouth_material.SetColor("_EmissionColor", happy_Color);
            talking_mouth_material.SetColor("_EmissionColor", happy_Color);
            robot_material.SetColor("_EmissionColor", happy_Color);
            robot_animator.SetInteger("state", 1);
            mouth.SetActive(true);
            reset_animator = true;
        }
        if (is_sad == true && has_spoken == false)
        {
            talk_robot();
            reaction_sound.Play();
            transition_particle.Play();
            is_mad = false;
            is_happy = false;
            is_neutral = false;
            is_suprised = false;
            eyes_material.SetTextureOffset("_BaseMap", new Vector2(0.2f, 0f));
            mouth_material.SetTextureOffset("_BaseMap", new Vector2(0.2f, 0f));
            eyes_material.SetColor("_EmissionColor", sad_Color);
            mouth_material.SetColor("_EmissionColor", sad_Color);
            talking_mouth_material.SetColor("_EmissionColor", sad_Color);
            robot_material.SetColor("_EmissionColor", sad_Color);
            robot_animator.SetInteger("state", 2);
            reset_animator = true;
            mouth.SetActive(true);
            has_spoken = true;          
        }
        if (is_mad == true && has_spoken == false)
        {
            talk_robot();
            reaction_sound.Play();
            transition_particle.Play();
            has_spoken = true;
            is_happy = false;
            is_neutral = false;
            is_sad = false;
            is_suprised = false;
            eyes_material.SetTextureOffset("_BaseMap", new Vector2(0.7f, 0f));
            mouth_material.SetTextureOffset("_BaseMap", new Vector2(0.3f, 0f));
            eyes_material.SetColor("_EmissionColor", mad_Color);
            mouth_material.SetColor("_EmissionColor", mad_Color);
            talking_mouth_material.SetColor("_EmissionColor", mad_Color);
            robot_material.SetColor("_EmissionColor", mad_Color);
            robot_animator.SetInteger("state", 3);
            mouth.SetActive(true);
            reset_animator = true;
        }
        if (is_suprised == true && has_spoken == false)
        {
            talk_robot();
            reaction_sound.Play();
            transition_particle.Play();
            has_spoken = true;
            is_mad = false;
            is_happy = false;
            is_neutral = false;
            is_sad = false;
            eyes_material.SetTextureOffset("_BaseMap", new Vector2(0.4f, 0f));
            mouth_material.SetTextureOffset("_BaseMap", new Vector2(0.4f, 0f));
            eyes_material.SetColor("_EmissionColor", suprised_Color);
            mouth_material.SetColor("_EmissionColor", suprised_Color);
            talking_mouth_material.SetColor("_EmissionColor", suprised_Color);
            robot_material.SetColor("_EmissionColor", suprised_Color);
            robot_animator.SetInteger("state", 4);
            mouth.SetActive(true);
            reset_animator = true;
        }
        //is_finished_game
        if (is_finished_game == true && has_spoken == false)
        {
            talk_robot();
            reaction_sound.Play();
            transition_particle.Play();
            has_spoken = true;
            is_suprised = false;
            is_mad = false;
            is_happy = false;
            is_neutral = false;
            is_sad = false;
            eyes_material.SetTextureOffset("_BaseMap", new Vector2(0.4f, 0f));
            mouth_material.SetTextureOffset("_BaseMap", new Vector2(0.4f, 0f));
            eyes_material.SetColor("_EmissionColor", suprised_Color);
            mouth_material.SetColor("_EmissionColor", suprised_Color);
            talking_mouth_material.SetColor("_EmissionColor", suprised_Color);
            robot_material.SetColor("_EmissionColor", suprised_Color);
            robot_animator.SetInteger("state", 4);
            mouth.SetActive(true);
            reset_animator = true;
        }

        if (reset_animator == true)
        {
            reset_animation_timer = reset_animation_timer - Time.deltaTime;
            if (reset_animation_timer <= 0)
            {
                robot_animator.SetInteger("state", 0);
                mouth.SetActive(false);
                   is_neutral = true;
                   is_mad = false;
                    is_happy = false;
                    is_sad = false;
                   is_suprised = false;
                    is_finished_game = false;
                reset_animation_timer = 5;
                reset_animator = false;
            }
        }
    }
}
