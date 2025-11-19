using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public Animator b_Controller;
    public Animator p_Controller;
    public Button restartButton;
    public float timeRemaining;
    public float roundTime = 10;
    public bool timerIsRunning = false;
    public bool firstRoundStarted = false;
    public TMP_Text timeText;
    public RotateCamera rotate_script;

   private void Start()
    {
        // Starts the timer automatically
        //
        timerIsRunning = true;
        timeRemaining = roundTime;
        firstRoundStarted = true;
        //timeText = GetComponent<TMP_Text>();
    }
    void Update()
    {
        if (rotate_script.notRotating)
        {
            if (timeRemaining > 0)
            {
                b_Controller.enabled = true;
                p_Controller.enabled = true;
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                //Take characters back to idle animation
                b_Controller.enabled = false;
                p_Controller.enabled = false;
                restartButton.gameObject.SetActive(true);
                timerIsRunning = false;
            }
        }
    }

    public void returnTimeRemaining(){
        
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timeText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}