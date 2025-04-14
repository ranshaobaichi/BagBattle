using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
