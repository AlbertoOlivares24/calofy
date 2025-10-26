using UnityEngine;

public class talking_script : MonoBehaviour
{
    public GameObject regular_mouth;
    public GameObject talking_mouth;
    public AudioSource speech_audio;
    public robot_script script_robot;


    //detect silence
    public float silenceThreshold = 0.01f; // Adjust this depending on how sensitive you want it
    public int sampleSize = 1024;          // Number of samples to analyze
    private float[] samples;
    private bool isCurrentlySilent = false;

    void Start()
    {
        samples = new float[sampleSize];
    }

    void Update()
    {
        if (speech_audio.isPlaying && talking_mouth.activeInHierarchy == false)
        {
        //    talking_mouth.SetActive(true);
            regular_mouth.SetActive(false);
        }
        else if(!speech_audio.isPlaying && regular_mouth.activeInHierarchy == false)
        {
          //  talking_mouth.SetActive(false);
           // regular_mouth.SetActive(true);
        }

        //detect silence
        if (speech_audio.isPlaying)
        {
            speech_audio.GetOutputData(samples, 0);

            float sum = 0f;
            for (int i = 0; i < sampleSize; i++)
                sum += Mathf.Abs(samples[i]);

            float average = sum / sampleSize;

            bool silentNow = average < silenceThreshold;

            if (silentNow && !isCurrentlySilent)
            {
                isCurrentlySilent = true;
               // Debug.Log("Audio is silent");
                talking_mouth.SetActive(false);
            }
            else if (!silentNow && isCurrentlySilent)
            {
                isCurrentlySilent = false;
              //  Debug.Log("Audio resumed");
                talking_mouth.SetActive(true);
            }
        }

    }
}
