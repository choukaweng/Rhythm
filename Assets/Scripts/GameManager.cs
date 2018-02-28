using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

    public Text comboText, perfectnessText;
    public Image healthBar;
    public UnityEvent GameEnd, GameStart;

    private int comboCount = 0;
    private int health = 100;

    // Use this for initialization
    void Start ()
    {
        GameStart.AddListener(RestartGame);
        GameEnd.AddListener(StopGame);

        comboText.text = "Combo : " + comboCount;
        perfectnessText.text = "";
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void AddCombo()
    {
        comboCount++;
        comboText.text = "Combo : " + comboCount;
    }

    public void ResetCombo()
    {
        comboCount = 0;
        comboText.text = "Combo : " + comboCount;
    }

    public void Heal()
    {
        health += 1;
        healthBar.fillAmount = health / 100f;

        if (health >= 100)
        {
            health = 100;
        }

        if (health < 30)
        {
            healthBar.color = Color.red;
        }
        else if (health < 70)
        {
            healthBar.color = Color.yellow;
        }
        else if (health <= 100)
        {
            healthBar.color = Color.green;
        }
    }

    public void Damage()
    {
        health -= 5;
        healthBar.fillAmount = health / 100f;

        if (health <= 0)
        {
            GameEnd.Invoke();
        }
        else if(health < 30)
        {
            healthBar.color = Color.red;
        }
        else if (health < 70)
        {
            healthBar.color = Color.yellow;
        }
        else if(health <= 100)
        {
            healthBar.color = Color.green;
        }
    }

    public void SetPerfectness(Perfectness perfectness)
    {
        switch (perfectness)
        {
            case Perfectness.Bad:
                perfectnessText.color = Color.red;
                perfectnessText.text = "Bad";
                break;
            case Perfectness.Good:
                perfectnessText.color = Color.yellow;
                perfectnessText.text = "Good";
                break;
            case Perfectness.Perfect:
                perfectnessText.color = Color.green;
                perfectnessText.text = "Perfect";
                break;
        }
    }

    public void ResetPerfectness()
    {
        perfectnessText.text = "";
    }

    public void StopGame()
    {
        perfectnessText.text = "GAME OVER";
        perfectnessText.color = Color.red;
    }

    public void RestartGame()
    {
        health = 100;
        healthBar.fillAmount = health;
        ResetCombo();
        ResetPerfectness();
    }
}
