using System;
using UnityEngine;

public abstract class Othering : MonoBehaviour
{
    public void DestroyOthering() => ObjectPool.Instance.PushObject(gameObject);
}