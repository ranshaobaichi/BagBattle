using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
 
public class Heart : MonoBehaviour 
{
    [Header("状态贴图")]
    public Sprite[] heart;
 
    private Image image;
    public int currentHealth;
    private int maxHealth;
 
    void Awake()
    {
        image = GetComponent<Image>();
        image.preserveAspect  = true;
    }
 
    public void Initialize(int capacity, int current = -1)
    {
        if(capacity != -1)
            maxHealth = capacity;
        if(current == -1)
            currentHealth = maxHealth;
        else
            currentHealth = current;
        UpdateDisplay();
    }
 
    public int ReduceHealth(int damage)
    {
        int actualDamage = Mathf.Min(currentHealth, damage);
        currentHealth -= actualDamage;
        UpdateDisplay();
        return actualDamage;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        image.sprite  = currentHealth switch
        {
            0 => heart[0],
            1 => heart[1],
            2 => heart[2],
            _ => null
        };
    }

    public bool IsEmpty => currentHealth <= 0;
}