using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[System.Serializable]
public class EnemySpawnData
{
    public Enemy.EnemyType enemyType;
    public uint spawnCount;
    // 生成间隔时间
    public float spawnTime;
    // 敌人生成方式
    public bool isGroup;
    public float intensity;
    public bool random;
    public bool isDefault;
    // 开始生成的时间（相对于波次开始）
    public float startTime;
    // 生成持续时间
    public float holdTime;
}

public class EnemySpawner : MonoBehaviour
{
    // 基础地图和边界引用
    public Tilemap map;
    public Transform leftlower, rightupper;

    // 保存完整的敌人生成数据
    protected EnemySpawnData spawnData = new EnemySpawnData();

    // 敌人预制体
    public GameObject enemyPrefab;

    // 控制协程的变量
    protected bool isSpawning = false;
    protected Coroutine spawnRoutine = null;

    [Header("生成预警设置")]
    public GameObject warningMarkPrefab; // 红色X标记的预制体
    public float warningDuration = 1.5f; // 警告持续时间
    public float blinkInterval = 0.15f;  // 闪烁间隔
    [Header("生成安全区设置")]
    public float playerSafeRadius = 2f; // 玩家周围敌人不会生成的半径

#if UNITY_EDITOR
    private System.Collections.Generic.List<Vector3> activeSpawnPoints = new();
#endif

    protected virtual void Awake()
    {
        // 初始尝试获取引用，但不在这里报错
        TryGetMapReferences();
    }

    // 使用EnemySpawnData初始化生成器
    public virtual void Initialize(EnemySpawnData data)
    {
        // 直接存储完整的配置
        this.spawnData = data;

        StartCoroutine(InitializeAfterReferencesReady());
    }

    private IEnumerator InitializeAfterReferencesReady()
    {
        yield return StartCoroutine(EnsureReferencesValid());
        
        // 再次确认引用有效后才开始生成
        if (TryGetMapReferences())
            StartSpawning();
        else
            Debug.LogError("无法获取必要的地图引用，敌人生成已取消");
    }

    // 尝试获取地图和边界引用
    protected bool TryGetMapReferences()
    {
        if (map == null)
        {
            GameObject mapObj = GameObject.FindWithTag("Map");
            if (mapObj != null)
                map = mapObj.GetComponent<Tilemap>();
        }

        if (leftlower == null)
        {
            GameObject llObj = GameObject.FindWithTag("EnemySpawnLL");
            if (llObj != null)
                leftlower = llObj.transform;
        }

        if (rightupper == null)
        {
            GameObject ruObj = GameObject.FindWithTag("EnemySpawnRU");
            if (ruObj != null)
                rightupper = ruObj.transform;
        }

        return map != null && leftlower != null && rightupper != null;
    }

    // 确保引用有效，必要时等待
    protected IEnumerator EnsureReferencesValid()
    {
        // 尝试获取引用
        if (!TryGetMapReferences())
        {
            Debug.Log("尝试获取地图引用失败，将在3帧后重试...");

            // 等待一帧，让场景有机会完全加载
            yield return null;

            // 最多尝试5次
            int attempts = 0;
            while (!TryGetMapReferences() && attempts < 5)
            {
                yield return new WaitForSeconds(Time.deltaTime * 3);
                attempts++;
                Debug.Log($"正在重试获取地图引用...第{attempts}次尝试");
            }

            // 如果还是失败，记录错误
            if (!TryGetMapReferences())
            {
                Debug.LogError("无法获取地图引用！请确保场景中存在标签为Map、EnemySpawnLL和EnemySpawnRU的对象。");
            }
        }
    }

    public virtual void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnRoutine = StartCoroutine(SpawnRoutine());
        }
    }

    public virtual void StopSpawning()
    {
        if (isSpawning && spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            isSpawning = false;
            spawnRoutine = null;
        }
    }

    protected IEnumerator SpawnRoutine()
    {
        // 等待开始时间
        if (spawnData.startTime > 0)
        {
            yield return new WaitForSeconds(spawnData.startTime);
        }

        float elapsedTime = 0;

        // 主要生成循环
        while (true)
        {
            // 检查玩家是否存活
            if (!PlayerController.Instance.Live())
            {
                yield return null;
                continue;
            }

            // 生成敌人
            if (spawnData.isGroup)
            {
                SpawnGroup();
            }
            else
            {
                SpawnIndividual();
            }


            // 检查持续时间
            if (spawnData.holdTime > 0)
            {
                elapsedTime += spawnData.spawnTime;
                if (elapsedTime >= spawnData.holdTime)
                {
                    // 生成时间结束
                    break;
                }
            }
            // 等待下一次生成
            yield return new WaitForSeconds(spawnData.spawnTime);
        }

        // 结束生成器
        Destroy(gameObject);
    }

    protected void SpawnIndividual()
    {
        for (int i = 0; i < spawnData.spawnCount; ++i)
        {
            StartCoroutine(SpawnWithWarning(SpawnPos()));
        }
    }

    protected void SpawnGroup()
    {
        Vector3 basePos = SpawnPos();
        for (int i = 0; i < spawnData.spawnCount; ++i)
        {
            Vector3 pos = basePos;
            if (Random.Range(0, 2) == 0)
                pos.x += Random.Range(-spawnData.intensity, spawnData.intensity);
            else
                pos.y += Random.Range(-spawnData.intensity, spawnData.intensity);

            StartCoroutine(SpawnWithWarning(SpawnPos()));
        }
    }

    protected IEnumerator SpawnWithWarning(Vector3 pos)
    {
#if UNITY_EDITOR
        // 只在编辑器中跟踪生成点
        activeSpawnPoints.Add(pos);
#endif

        // 创建警告标记
        GameObject warningMark = null;

        if (warningMarkPrefab != null)
        {
            warningMark = ObjectPool.Instance.GetObject(warningMarkPrefab);
            warningMark.transform.position = pos;
        }
        else
        {
            // 如果没有预制体，创建一个临时的红色X标记
            warningMark = new GameObject("EnemySpawnWarning");
            warningMark.transform.position = pos;
        }

        // 添加一个TextMesh来显示X
        TextMesh textMesh = warningMark.AddComponent<TextMesh>();
        textMesh.text = "X";
        textMesh.color = Color.red;
        textMesh.fontSize = 12;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        // 透明度闪烁效果
        float elapsedTime = 0f;
        SpriteRenderer spriteRenderer = warningMark.GetComponent<SpriteRenderer>();
        bool increasing = false; // 控制透明度是增加还是减少
        float minAlpha = 0.2f;   // 最小透明度
        float maxAlpha = 1.0f;   // 最大透明度
        float currentAlpha = maxAlpha;

        // 设置初始透明度
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = currentAlpha;
            spriteRenderer.color = color;
        }
        else if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = currentAlpha;
            textMesh.color = color;
        }

        while (elapsedTime < warningDuration)
        {
            // 计算透明度变化
            float alphaChange = (maxAlpha - minAlpha) * (Time.deltaTime / blinkInterval);

            if (increasing)
                currentAlpha = Mathf.Min(currentAlpha + alphaChange, maxAlpha);
            else
                currentAlpha = Mathf.Max(currentAlpha - alphaChange, minAlpha);

            // 到达极限值时反转方向
            if (currentAlpha >= maxAlpha || currentAlpha <= minAlpha)
                increasing = !increasing;

            // 设置新透明度
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = currentAlpha;
                spriteRenderer.color = color;
            }
            else if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = currentAlpha;
                textMesh.color = color;
            }

            elapsedTime += Time.deltaTime;
            yield return null; // 每帧更新
        }

        // 销毁警告标记
        if (warningMark != null)
        {
            if (warningMarkPrefab != null)
                ObjectPool.Instance.PushObject(warningMark);
            else
                Destroy(warningMark);
        }

        // 生成敌人
        Spawn(pos);
#if UNITY_EDITOR
        // 只在编辑器中移除生成点
        activeSpawnPoints.Remove(pos);
#endif

        yield break;
    }
    protected void Spawn(Vector3 pos)
    {
        // 检查生成位置是否在玩家安全区内
        if (PlayerController.Instance != null)
        {
            float distanceToPlayer = Vector2.Distance(pos, PlayerController.Instance.transform.position);
            if (distanceToPlayer < playerSafeRadius)
            {
                // 在安全区内，不生成敌人
                Debug.Log($"敌人生成点太靠近玩家(距离: {distanceToPlayer})，取消生成");
                return;
            }
        }
        GameObject new_enemy = ObjectPool.Instance.GetObject(enemyPrefab);
        new_enemy.transform.position = pos;
        new_enemy.GetComponent<EnemyController>().Initialize();
    }

    protected Vector3 SpawnPos()
    {
        // 在使用前再次确保引用有效
        if (!TryGetMapReferences())
        {
            // Debug.LogError("无法获取生成位置！地图引用无效。");
            return Vector3.zero;
        }

        // 保持原有的SpawnPos逻辑
        Vector2 ll, ru;
        Vector3 ret = new Vector3();

        if (spawnData.isDefault)
        {
            Vector3Int minCell = map.cellBounds.min;
            Vector3Int maxCell = map.cellBounds.max;

            ll = map.CellToWorld(minCell);
            ru = map.CellToWorld(maxCell);

            ll += new Vector2(1f, 1f);
            ru -= new Vector2(1f, 1f);
        }
        else
        {
            ll = leftlower.position;
            ru = rightupper.position;
        }

        if (spawnData.random)
        {
            ret = new(Random.Range(ll.x, ru.x), Random.Range(ll.y, ru.y), 0);
        }
        else
        {
            //0-3 up down left right
            int choose = Random.Range(0, 4);
            switch (choose)
            {
                case 0:
                    ret = new Vector3(Random.Range(ll.x, ru.x), ru.y, 0);
                    break;
                case 1:
                    ret = new Vector3(Random.Range(ll.x, ru.x), ll.y, 0);
                    break;
                case 2:
                    ret = new Vector3(ll.x, Random.Range(ll.y, ru.y), 0);
                    break;
                case 3:
                    ret = new Vector3(ru.x, Random.Range(ll.y, ru.y), 0);
                    break;
            }
        }
        return ret;
    }

    private void OnDestroy()
    {
        StopSpawning();
    }

#if UNITY_EDITOR
    // 在Scene视图中绘制所有活跃的生成点 - 仅在编辑器中编译
    private void OnDrawGizmos()
    {
        // // 绘制玩家安全区
        // if (Application.isPlaying && PlayerController.Instance != null)
        // {
        //     // 绘制玩家安全区域
        //     Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // 半透明黄色
        //     Gizmos.DrawSphere(PlayerController.Instance.transform.position, playerSafeRadius);
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawWireSphere(PlayerController.Instance.transform.position, playerSafeRadius);
        // }

        // 绘制所有活跃的生成点
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 半透明红色
        foreach (Vector3 spawnPoint in activeSpawnPoints)
        {
            // 绘制实心圆表示生成区域
            Gizmos.DrawSphere(spawnPoint, 0.5f);
            // 绘制线框圆增强可视性
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint, 0.5f);
        }
    }
#endif
}