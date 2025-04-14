using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyDamageNumberController : MonoBehaviour
{
    public static EnemyDamageNumberController Instance = null;
    public GameObject enemyDamageNumberPrefab;
    public Transform canvasTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 只做简单的预制体存在检查
        if (enemyDamageNumberPrefab == null)
            Debug.LogError("伤害数字预制体未设置!");
    }

    public void CreateDamageNumber(float damage, Vector3 worldPosition)
    {
        GameObject damageNumber = ObjectPool.Instance.GetObject(enemyDamageNumberPrefab);
        damageNumber.SetActive(true);
        
        damageNumber.transform.SetParent(canvasTransform);
        damageNumber.transform.localScale = Vector3.one;
        damageNumber.transform.position = worldPosition;
        
        // 设置文本值
        TextMeshProUGUI textComponent = damageNumber.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = damage.ToString();
            textComponent.ForceMeshUpdate();
        }

        // 初始化伤害数字
        EnemyDamageNumber enemyDamageNumber = damageNumber.GetComponent<EnemyDamageNumber>();
        if (enemyDamageNumber != null)
        {
            enemyDamageNumber.Initialize(damage, worldPosition);
        }
    }
}
