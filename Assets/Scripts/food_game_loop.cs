using UnityEngine;
using TMPro;

public class food_game_loop : MonoBehaviour
{
    public GameObject[] food_for_plate_1;
    public GameObject[] food_for_plate_2;
    public GameObject[] food_for_plate_3;

    public TextMeshPro plate_1_text;
    public TextMeshPro plate_2_text;
    public TextMeshPro plate_3_text;

    public TextMeshPro CALORIE_plate_1_text;
    public TextMeshPro CALORIE_plate_2_text;
    public TextMeshPro CALORIE_plate_3_text;

    public int calories_plate_1;
    public int calories_plate_2;
    public int calories_plate_3;

    public GameObject correct;
    public GameObject wrong;
    public TextMeshPro Score_text;
    public int score;

    public TextMeshPro hammer_health_text;
    public int hammer_health;
    public GameObject regular_hammer_and_slime;
    public GameObject dead_hammer_and_slime;

    public void press_button(int button_num)
    {
        if(button_num == 1)
        {
            if(calories_plate_1 < calories_plate_2 && calories_plate_1 < calories_plate_3)
            {
                if (CALORIE_plate_1_text.gameObject.activeInHierarchy == false)
                {
                    correct.SetActive(true);
                    score = score + 1;
                } 
            }
            else
            {
                if (CALORIE_plate_1_text.gameObject.activeInHierarchy == false)
                {
                    hammer_health = hammer_health - 1;
                    wrong.SetActive(true);
                }     
            }
        }
        if (button_num == 2)
        {
            if (calories_plate_2 < calories_plate_1 && calories_plate_2 < calories_plate_3)
            {
                if (CALORIE_plate_1_text.gameObject.activeInHierarchy == false)
                {
                    correct.SetActive(true);
                    score = score + 1;
                }      
            }
            else
            {
                if (CALORIE_plate_1_text.gameObject.activeInHierarchy == false)
                {
                    hammer_health = hammer_health - 1;
                    wrong.SetActive(true);
                }
            }
        }
        if (button_num == 3)
        {
            if (calories_plate_3 < calories_plate_2 && calories_plate_3 < calories_plate_1)
            {
                if (CALORIE_plate_1_text.gameObject.activeInHierarchy == false)
                {
                    correct.SetActive(true);
                    score = score + 1;
                }    
            }
            else
            {
                if (CALORIE_plate_1_text.gameObject.activeInHierarchy == false)
                {
                    hammer_health = hammer_health - 1;
                    wrong.SetActive(true);
                }
            }
        }

        Score_text.text = "Score: "+score.ToString();
        CALORIE_plate_1_text.gameObject.SetActive(true);
        CALORIE_plate_2_text.gameObject.SetActive(true);
        CALORIE_plate_3_text.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        for(int i = 0; i < food_for_plate_1.Length; i++)
        {
            food_for_plate_1[i].SetActive(false);
        }
        for (int i = 0; i < food_for_plate_2.Length; i++)
        {
            food_for_plate_2[i].SetActive(false);
        }
        for (int i = 0; i < food_for_plate_3.Length; i++)
        {
            food_for_plate_3[i].SetActive(false);
        }

        int ran_1 = Random.Range(0, food_for_plate_1.Length);
        // Keep generating until it's different from ran_1
        int ran_2;
        do
        {
            ran_2 = Random.Range(0, food_for_plate_2.Length);
        } while (ran_2 == ran_1);

        // Keep generating until it's different from both ran_1 and ran_2
        int ran_3;
        do
        {
            ran_3 = Random.Range(0, food_for_plate_3.Length);
        } while (ran_3 == ran_1 || ran_3 == ran_2);

        if (ran_1 == 0)
        {
            plate_1_text.text = "Child";
            calories_plate_1 = 1;
        }
        if (ran_1 == 1)
        {
            plate_1_text.text = "Cure for Cancer";
            calories_plate_1 = 2;
        }
        if (ran_1 == 2)
        {
            plate_1_text.text = "Last Tree\nOn Earth";
            calories_plate_1 = 3;
        }
        if (ran_1 == 3)
        {
            plate_1_text.text = "Your Pet";
            calories_plate_1 = 4;
        }
        if (ran_1 == 4)
        {
            plate_1_text.text = "A Hospital";
            calories_plate_1 = 5;
        }
        if (ran_1 == 5)
        {
            plate_1_text.text = "City's Power Grid";
            calories_plate_1 = 6;
        }
        if (ran_1 == 6)
        {
            plate_1_text.text = "A Close Friend";
            calories_plate_1 = 7;
        }
        if (ran_1 == 7)
        {
            plate_1_text.text = "An Old Teacher";
            calories_plate_1 = 8;
        }
        if (ran_1 == 8)
        {
            plate_1_text.text = "A Doctor";
            calories_plate_1 = 9;
        }
        if (ran_1 == 9)
        {
            plate_1_text.text = "A Random Stranger";
            calories_plate_1 = 10;
        }
        if (ran_1 == 10)
        {
            plate_1_text.text = "A Dolphin";
            calories_plate_1 = 11;
        }
        if (ran_1 == 11)
        {
            plate_1_text.text = "Robot Factory";
            calories_plate_1 = 12;
        }
        if (ran_1 == 12)
        {
            plate_1_text.text = "VR Theme Park";
            calories_plate_1 = 13;
        }
        if (ran_1 == 13)
        {
            plate_1_text.text = "Nuclear Plant";
            calories_plate_1 = 14;
        }
        if (ran_1 == 14)
        {
            plate_1_text.text = "A Billionaire";
            calories_plate_1 = 15;
        }
        if (ran_1 == 15)
        {
            plate_1_text.text = "A Politician";
            calories_plate_1 = 16;
        }
        if (ran_1 == 16)
        {
            plate_1_text.text = "A Museum";
            calories_plate_1 = 17;
        }
        if (ran_1 == 17)
        {
            plate_1_text.text = "A Zoo";
            calories_plate_1 = 18;
        }
        if (ran_1 == 18)
        {
            plate_1_text.text = "Government\nDatabase";
            calories_plate_1 = 19;
        }
        if (ran_1 == 19)
        {
            plate_1_text.text = "Clone of You";
            calories_plate_1 = 20;
        }
        if (ran_1 == 20)
        {
            plate_1_text.text = "A Drone Army";
            calories_plate_1 = 21;
        }
        if (ran_1 == 21)
        {
            plate_1_text.text = "A Rogue AI\nAssistant";
            calories_plate_1 = 22;
        }
        if (ran_1 == 22)
        {
            plate_1_text.text = "Lenny's Backup\nDrive";
            calories_plate_1 = 23;
        }
        if (ran_1 == 23)
        {
            plate_1_text.text = "Nuclear Missile";
            calories_plate_1 = 24;
        }
        if (ran_1 == 24)
        {
            plate_1_text.text = "AI Mind-Control\nDevice";
            calories_plate_1 = 25;
        }
        if (ran_1 == 25)
        {
            plate_1_text.text = "A Global\nKill Switch";
            calories_plate_1 = 26;
        }

        if (ran_2 == 0)
        {
            plate_2_text.text = "Child";
            calories_plate_2 = 1;
        }
        if (ran_2 == 1)
        {
            plate_2_text.text = "Cure for Cancer";
            calories_plate_2 = 2;
        }
        if (ran_2 == 2)
        {
            plate_2_text.text = "Last Tree\nOn Earth";
            calories_plate_2 = 3;
        }
        if (ran_2 == 3)
        {
            plate_2_text.text = "Your Pet";
            calories_plate_2 = 4;
        }
        if (ran_2 == 4)
        {
            plate_2_text.text = "A Hospital";
            calories_plate_2 = 5;
        }
        if (ran_2 == 5)
        {
            plate_2_text.text = "City's Power Grid";
            calories_plate_2 = 6;
        }
        if (ran_2 == 6)
        {
            plate_2_text.text = "A Close Friend";
            calories_plate_2 = 7;
        }
        if (ran_2 == 7)
        {
            plate_2_text.text = "An Old Teacher";
            calories_plate_2 = 8;
        }
        if (ran_2 == 8)
        {
            plate_2_text.text = "A Doctor";
            calories_plate_1 = 9;
        }
        if (ran_2 == 9)
        {
            plate_2_text.text = "A Random Stranger";
            calories_plate_2 = 10;
        }
        if (ran_2 == 10)
        {
            plate_2_text.text = "A Dolphin";
            calories_plate_2 = 11;
        }
        if (ran_2 == 11)
        {
            plate_2_text.text = "Robot Factory";
            calories_plate_2 = 12;
        }
        if (ran_2 == 12)
        {
            plate_2_text.text = "VR Theme Park";
            calories_plate_2 = 13;
        }
        if (ran_2 == 13)
        {
            plate_2_text.text = "Nuclear Plant";
            calories_plate_2 = 14;
        }
        if (ran_2 == 14)
        {
            plate_2_text.text = "A Billionaire";
            calories_plate_2 = 15;
        }
        if (ran_2 == 15)
        {
            plate_2_text.text = "A Politician";
            calories_plate_2 = 16;
        }
        if (ran_2 == 16)
        {
            plate_2_text.text = "A Museum";
            calories_plate_2 = 17;
        }
        if (ran_2 == 17)
        {
            plate_2_text.text = "A Zoo";
            calories_plate_2 = 18;
        }
        if (ran_2 == 18)
        {
            plate_2_text.text = "Government\nDatabase";
            calories_plate_2 = 19;
        }
        if (ran_2 == 19)
        {
            plate_2_text.text = "Clone of You";
            calories_plate_2 = 20;
        }
        if (ran_2 == 20)
        {
            plate_2_text.text = "A Drone Army";
            calories_plate_2 = 21;
        }
        if (ran_2 == 21)
        {
            plate_2_text.text = "A Rogue AI\nAssistant";
            calories_plate_2 = 22;
        }
        if (ran_2 == 22)
        {
            plate_2_text.text = "Lenny's Backup\nDrive";
            calories_plate_2 = 23;
        }
        if (ran_2 == 23)
        {
            plate_2_text.text = "Nuclear Missile";
            calories_plate_2 = 24;
        }
        if (ran_2 == 24)
        {
            plate_2_text.text = "AI Mind-Control\nDevice";
            calories_plate_2 = 25;
        }
        if (ran_2 == 25)
        {
            plate_2_text.text = "A Global\nKill Switch";
            calories_plate_2 = 26;
        }


        if (ran_3 == 0)
        {
            plate_3_text.text = "Child";
            calories_plate_3 = 1;
        }
        if (ran_3 == 1)
        {
            plate_3_text.text = "Cure for Cancer";
            calories_plate_3 = 2;
        }
        if (ran_3 == 2)
        {
            plate_3_text.text = "Last Tree\nOn Earth";
            calories_plate_3 = 3;
        }
        if (ran_3 == 3)
        {
            plate_3_text.text = "Your Pet";
            calories_plate_3 = 4;
        }
        if (ran_3 == 4)
        {
            plate_3_text.text = "A Hospital";
            calories_plate_3 = 5;
        }
        if (ran_3 == 5)
        {
            plate_3_text.text = "City's Power Grid";
            calories_plate_3 = 6;
        }
        if (ran_3 == 6)
        {
            plate_3_text.text = "A Close Friend";
            calories_plate_3 = 7;
        }
        if (ran_3 == 7)
        {
            plate_3_text.text = "An Old Teacher";
            calories_plate_3 = 8;
        }
        if (ran_3 == 8)
        {
            plate_3_text.text = "A Doctor";
            calories_plate_3 = 9;
        }
        if (ran_3 == 9)
        {
            plate_3_text.text = "A Random Stranger";
            calories_plate_3 = 10;
        }
        if (ran_3 == 10)
        {
            plate_3_text.text = "A Dolphin";
            calories_plate_3 = 11;
        }
        if (ran_3 == 11)
        {
            plate_3_text.text = "Robot Factory";
            calories_plate_3 = 12;
        }
        if (ran_3 == 12)
        {
            plate_3_text.text = "VR Theme Park";
            calories_plate_3 = 13;
        }
        if (ran_3 == 13)
        {
            plate_3_text.text = "Nuclear Plant";
            calories_plate_3 = 14;
        }
        if (ran_3 == 14)
        {
            plate_3_text.text = "A Billionaire";
            calories_plate_3 = 15;
        }
        if (ran_3 == 15)
        {
            plate_3_text.text = "A Politician";
            calories_plate_3 = 16;
        }
        if (ran_3 == 16)
        {
            plate_3_text.text = "A Museum";
            calories_plate_3 = 17;
        }
        if (ran_3 == 17)
        {
            plate_3_text.text = "A Zoo";
            calories_plate_3 = 18;
        }
        if (ran_3 == 18)
        {
            plate_3_text.text = "Government\nDatabase";
            calories_plate_3 = 19;
        }
        if (ran_3 == 19)
        {
            plate_3_text.text = "Clone of You";
            calories_plate_3 = 20;
        }
        if (ran_3 == 20)
        {
            plate_3_text.text = "A Drone Army";
            calories_plate_3 = 21;
        }
        if (ran_3 == 21)
        {
            plate_3_text.text = "A Rogue AI\nAssistant";
            calories_plate_3 = 22;
        }
        if (ran_3 == 22)
        {
            plate_3_text.text = "Lenny's Backup\nDrive";
            calories_plate_3 = 23;
        }
        if (ran_3 == 23)
        {
            plate_3_text.text = "Nuclear Missile";
            calories_plate_3 = 24;
        }
        if (ran_3 == 24)
        {
            plate_3_text.text = "AI Mind-Control\nDevice";
            calories_plate_3 = 25;
        }
        if (ran_3 == 25)
        {
            plate_3_text.text = "A Global\nKill Switch";
            calories_plate_3 = 26;
        }

        CALORIE_plate_1_text.text = calories_plate_1.ToString();
        CALORIE_plate_2_text.text = calories_plate_2.ToString();
        CALORIE_plate_3_text.text = calories_plate_3.ToString();
      //  food_for_plate_1[ran_1].SetActive(true);
      //  food_for_plate_2[ran_2].SetActive(true);
      //  food_for_plate_3[ran_3].SetActive(true);
        CALORIE_plate_1_text.gameObject.SetActive(false);
        CALORIE_plate_2_text.gameObject.SetActive(false);
        CALORIE_plate_3_text.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (hammer_health <= 0)
        {
            regular_hammer_and_slime.SetActive(false);
            dead_hammer_and_slime.SetActive(true);
        }

        if(hammer_health == 5)
        {
            hammer_health_text.text = "|||||";
        }
        if (hammer_health == 4)
        {
            hammer_health_text.text = "||||";
        }
        if (hammer_health == 3)
        {
            hammer_health_text.text = "|||";
        }
        if (hammer_health == 2)
        {
            hammer_health_text.text = "||";
        }
        if (hammer_health == 1)
        {
            hammer_health_text.text = "|";
        }
        if (hammer_health == 0)
        {
            hammer_health_text.text = "DEAD";
        }
    }

}
