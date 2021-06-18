using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    public float moveSpeed = 12.0f;
    public float turnSpeed = 250.0f;
    public float currentSpeed = 0.0f;

    
    //For chasing
    float stamina = 100.0f;
    public float maxStamina = 100.0f;
    public float staminaDrain = 20.0f;
    public float staminaRegen = 10.0f;
    public float runSpeed = 1.5f;
    public float delayTime = 1.0f;

    private void Start()
    {
        //Stamina will be used for calculation
        stamina = maxStamina;
    }

    //To add a delay to stamina regeneration
    IEnumerator StaminaRegen()
    {
        //creating a lock here to delay the loop
        bool isDelay = false;
        yield return new WaitForSeconds(delayTime);
        isDelay = true;
        if (isDelay)
        {
            stamina += staminaRegen * Time.deltaTime;
            //This will go over max stamina 
            if (stamina >= maxStamina)
            {
                stamina = maxStamina;
                //to stop the regen process
                isDelay = false;
            }
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        //input handling
        float translation = Input.GetAxis("Vertical") * moveSpeed;
        float rotation = Input.GetAxisRaw("Horizontal") * turnSpeed;


        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        if (Input.GetAxisRaw("Run") > 0 && stamina > 0)
        {
            translation *= runSpeed;
            stamina -= staminaDrain * Time.deltaTime ;
        }

        if(Input.GetAxisRaw("Run") == 0 && stamina != maxStamina )
        {
            StartCoroutine("StaminaRegen");
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        transform.Translate(0, 0, translation);
        //This will be used to send data to navmesh agents
        currentSpeed = translation;

        transform.Rotate(0, rotation, 0);

    }


}
