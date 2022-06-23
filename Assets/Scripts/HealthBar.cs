using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    public Player playerHealth;
    

    private void Start()
    {
        playerHealth = MovingObject.FindObjectOfType<Player>();
        healthBar = GetComponent<Slider>();
        healthBar.maxValue = 100;
        healthBar.value = playerHealth.food;
    }

    public void SetHealth(int hp)
    {
        healthBar.value = hp;
    }
}