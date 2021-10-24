using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instructions : MonoBehaviour
{
    public PlayerController controller;
    public GameObject space;
    public GameObject directions;
    [HideInInspector]public bool shouldTutorial = false;
    bool inTutorial = false;

    void Update()
    {
        if (shouldTutorial)
        {
            space.SetActive(!inTutorial);
            
            if (Input.GetKey(KeyCode.Space))
            {
                inTutorial = true;

                Time.timeScale = 0.3f;

                if (controller.pathLengthTimer <= 0.4f)
                {
                    directions.SetActive(true);
                    space.SetActive(false);

                    Time.timeScale = 0;

                    if (Input.GetAxisRaw("Horizontal") >= 0.6f)
                    {
                        Time.timeScale = 1;

                        shouldTutorial = false;
                        directions.SetActive(false);
                        space.SetActive(false);
                    }
                }
            } else if (inTutorial)
            {
                inTutorial = false;

                Time.timeScale = 1;
                directions.SetActive(false);
                controller.SetStanding();
            }
        }
    }
}
