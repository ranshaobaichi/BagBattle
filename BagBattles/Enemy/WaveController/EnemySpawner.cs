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
        
        // 开始生成协程前确保引用有效
        StartCoroutine(EnsureReferencesValid());
        
        // 开始生成协程
        StartSpawning();
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
            Spawn(SpawnPos());
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
                
            Spawn(pos);
        }
    }

    protected void Spawn(Vector3 pos)
    {
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
}