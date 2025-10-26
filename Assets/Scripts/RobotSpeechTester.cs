using UnityEngine;
using System.Threading.Tasks;
using RG.LabBot;

public class RobotSpeechTester : MonoBehaviour
{
    public RobotSpeech speech;
    [TextArea] public string prompt = "Give one short, funny line about rubber ducks in a lab. Max 12 words.";

    async void Start()
    {
       // Debug.Log("[Tester] Starting speech test…");
        await speech.SpeakFromPromptAsync(prompt);
    //    Debug.Log("[Tester] Done.");
    }
}
