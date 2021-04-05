using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class HUD : NetworkBehaviour
{
    [SerializeField] private Text time;
    [SerializeField] private Text scoreTeam1;
    [SerializeField] private Text scoreTeam2;
    [SerializeField] private int minutes = 10;
    private int seconds = 0;
    private bool takingAway = false;
    private bool gameEnded = false;

    [SerializeField]  private Slider manaBar;
    [SerializeField] private float fillSpeed = 0.5f;
    [SerializeField] private ParticleSystem particleSystem;
    private float fullMana = 1f;
    

    void Start()
    {
        time.text = minutes + ":0" + seconds;
        //manaBar = gameObject.GetComponent<ManaBar>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!takingAway && minutes >= 0 && seconds >= 0) 
        {
            StartCoroutine(Timer());
        }
        if (seconds >= 10)
            time.text = minutes + ":" + seconds;
        else
            time.text = minutes + ":0" + seconds;

        if (minutes == 0 && seconds == 0)
        {
            EndGame();
        }

        if (manaBar.value < fullMana)
        {
            manaBar.value += fillSpeed * Time.deltaTime;
            if (!particleSystem.isPlaying)
                particleSystem.Play();  
        }
        else
            particleSystem.Stop();

        if (Input.GetKeyDown(KeyCode.F)) manaBar.value -= 0.6f;

    }

    void EndGame()
    {
        gameEnded = true;
        StopCoroutine(Timer());
        //gameoverUI.SetActive(true);
    }
    
    IEnumerator Timer()
    {
        takingAway = true;
        yield return new WaitForSeconds(1);
        if (seconds == 0)
        {
            minutes--;
            seconds = 59;
        }
        else
        seconds--;
        takingAway = false;
    }
}
