using System;
using System.Collections.Generic;
using UnityEngine;

public class D100
{
    private static readonly List<(int min, int max, Action<int>)> _rangeActions = new List<(int, int, Action<int>)>
    {
        (0, 49, value => {
            // 50 %的概率
            HealthController.Instance.HealthRecover(1);
        }),
        (50, 98, value => {
            // 49 %的概率
            PlayerController.Instance.TakeDamage(1);
        }),
        (99, 99, value => {
            // 1 %的概率
            HealthController.Instance.HealthUp(1);
        })
    };

    public static void Triggered(int randomValue)
    {
        randomValue %= 100;
        foreach (var (min, max, action) in _rangeActions)
        {
            if (randomValue >= min && randomValue <= max)
            {
                action(randomValue);
                break;
            }
        }
    }
}