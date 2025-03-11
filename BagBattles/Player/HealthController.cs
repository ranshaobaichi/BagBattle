using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class HeartController : MonoBehaviour
{
    public static HeartController Instance { get; private set; }

    [Header("配置")]
    public int maxHearts;
    private static int currentHearts;
    public int healthPerHeart;
    public GameObject heartPrefab;

    //实心
    private static List<Heart> Hearts = new();
    private static List<int> remain_health = new();
    private static int current;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(this != Instance)
            Destroy(gameObject);
    }

    void Start()
    {
        current = maxHearts - 1;
        currentHearts = maxHearts;
        for (int i = 0; i < maxHearts; i++)
            remain_health.Add(healthPerHeart);
        PrewarmPool();
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < maxHearts; i++)
        {
            var heart = Instantiate(heartPrefab, transform).GetComponent<Heart>();
            heart.Initialize(healthPerHeart, remain_health[i]);
            heart.gameObject.SetActive(true);
            Hearts.Add(heart);
        }
    }

    public static void TakeDamage(int damage)
    {
        int remainingDamage = damage;

        while (remainingDamage > 0)
        {
            //Debug.Log("damage");
            int actualDamage = Hearts[current].ReduceHealth(remainingDamage);
            remainingDamage -= actualDamage;

            if (Hearts[current].IsEmpty)
            {
                if(current > 0)
                    current--;
                else
                {
                    GameOver();
                    break;
                }
            }
        }

        if (Hearts[current].IsEmpty)
            GameOver();
    }

    private static void GameOver()
    {
        PlayerController.Instance.Dead();
    }

    public void HealthUp(int value)
    {
        if (value <= 0) return;
        currentHearts++;
        remain_health.Add(0);
        for (int i = remain_health.Count - 1; i > 0; i++)
        {
            remain_health[i] = remain_health[i - 1];
        }
        remain_health[0] = healthPerHeart;
        current++;

        var heart = Instantiate(heartPrefab, transform).GetComponent<Heart>();
        heart.gameObject.SetActive(true);
        Hearts.Add(heart);

        for (int i = 0; i < Hearts.Count; i++)
        {
            Hearts[i].Initialize(healthPerHeart, remain_health[i]);
        }
        HealthUp(value - 1);
    }

    private void OnDestroy()
    {
        
    }
}