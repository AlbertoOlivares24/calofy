using UnityEngine;

public class AudioSanityBeep : MonoBehaviour
{
    public AudioSource speaker;
    void Start()
    {
        var clip = AudioClip.Create("beep", 44100, 1, 44100, false);
        float freq = 440f;
        var data = new float[44100];
        for (int i = 0; i < data.Length; i++) data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / 44100f) * 0.2f;
        clip.SetData(data, 0);
        speaker.clip = clip;
        speaker.Play();
        Debug.Log("[Beep] Played 1s test tone.");
    }
}
