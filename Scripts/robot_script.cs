using UnityEngine;

public class robot_script : MonoBehaviour
{
    [Header("Materials")]
    public Material eyes_material;
    public Material mouth_material;
    public Material talking_mouth_material;
    public Material robot_material;

    [Header("Emotion Colors")]
    public Color neutral_Color;
    public Color happy_Color;
    public Color sad_Color;
    public Color mad_Color;
    public Color suprised_Color;

    [Header("Movement stuff")]
    public Transform player_to_look_at;
    public AudioSource drone_moving_sound;
    public Transform[] travel_locations;
    public float travel_speed;
    public float timer_to_travel;
    Transform place_to_transform;

    [Header("Animator")]
    public Animator robot_animator;

    public ParticleSystem transition_particle;

    void Start()
    {
        
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
    }
}
