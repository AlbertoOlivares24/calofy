using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

public class cookie_shooter : MonoBehaviour
{
    public GameObject cookie_bullet;
    public GameObject position_to_spawn_cookie;
    public float bullet_speed;
    public Animator gun_animator;

    InputDevice leftHand;
    InputDevice rightHand;

    float fire_rate = 0.5f;

    void Start()
    {
        // Get both hand controllers
        var leftDevices = new List<InputDevice>();
        var rightDevices = new List<InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftDevices);
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightDevices);

        if (leftDevices.Count > 0)
            leftHand = leftDevices[0];

        if (rightDevices.Count > 0)
            rightHand = rightDevices[0];
    }

    void Update()
    {
        // Check if triggers are pressed
        if (leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftPressed) && leftPressed && fire_rate<=0)
        {
            //Debug.Log("Left trigger pressed!");
            gun_animator.SetInteger("state", 1);
            GameObject cookie = Instantiate(cookie_bullet, position_to_spawn_cookie.transform.position, position_to_spawn_cookie.transform.rotation);
            Rigidbody cookie_rig = cookie.GetComponent<Rigidbody>();
            if (cookie_rig != null)
            {
                cookie_rig.AddForce(position_to_spawn_cookie.transform.forward * bullet_speed);
            }
            fire_rate = 0.5f;
        }

      /*  if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightPressed) && rightPressed && fire_rate <= 0)
        {
            //Debug.Log("Right trigger pressed!");
            GameObject cookie = Instantiate(cookie_bullet, position_to_spawn_cookie.transform.position, position_to_spawn_cookie.transform.rotation);
            Rigidbody cookie_rig = cookie.GetComponent<Rigidbody>();
            if (cookie_rig != null)
            {
                cookie_rig.AddForce(position_to_spawn_cookie.transform.forward * bullet_speed);
            }
            fire_rate = 0.5f;
        }*/

        if (fire_rate > 0)
        {
            fire_rate = fire_rate - Time.deltaTime;      
        }
        else
        {
            gun_animator.SetInteger("state", 0);
        }
    }
}

