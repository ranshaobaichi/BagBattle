using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    //whether spawn as map
    public Tilemap map;
    public Transform leftlower, rightupper;
    public uint start_time;
    public float hold_time;
    protected float hold_timer;
    protected float start_timer;
    //true if spawn as map size
    public bool _default;
    //true if spawn randomly
    //false if only spawn on edge
    public bool random;
    public bool group;
    public float intensity;

    public uint spawn_nums;
    public float spawn_time;
    protected float spawn_timer;


    public GameObject enemyPrefab;


    protected virtual void Awake()
    {
        map = GameObject.FindWithTag("Map").GetComponent<Tilemap>();
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        spawn_timer = 0f;
        start_timer = 0f;
        hold_timer = 0f;
    }

    protected void Spawn(UnityEngine.Vector3 pos)
    {
        GameObject new_enemy = ObjectPool.Instance.GetObject(enemyPrefab);
        new_enemy.transform.position = pos;
        new_enemy.GetComponent<EnemyController>().Initialize();
    }   

    // Update is called once per frame
    protected virtual void Update()
    {
        if (PlayerController.Instance.Live() == false)
            return;

        if (hold_time != -1)
        {
            hold_timer += Time.deltaTime;
            if (hold_timer >= hold_time)
                Destroy(gameObject);
        }

        //start time
        if (start_timer < start_time)
        {
            start_timer += Time.deltaTime;
            return;
        }


        //spawn details
        if (spawn_timer < spawn_time)
            spawn_timer += Time.deltaTime;
        else
        {
            spawn_timer = 0f;
            if (group == false)
                for (int i = 0; i < spawn_nums; ++i)
                {
                    Spawn(SpawnPos());
                    //Instantiate(enemyPrefab, SpawnPos(), transform.rotation);
                }
            else
            {
                UnityEngine.Vector3 pos = SpawnPos();
                for (int i = 0; i < spawn_nums; ++i)
                {
                    if (Random.Range(0, 2) == 0)
                        pos.x += Random.Range(-intensity, intensity);
                    else
                        pos.y += Random.Range(-intensity, intensity);
                    Spawn(pos);
                    // Instantiate(enemyPrefab, pos, transform.rotation);
                }
            }
        }
    }

    protected UnityEngine.Vector3 SpawnPos()
    {
        UnityEngine.Vector2 ll, ru;
        UnityEngine.Vector3 ret = new UnityEngine.Vector3();
        if (_default == true)
        {
            Vector3Int minCell = map.cellBounds.min;
            Vector3Int maxCell = map.cellBounds.max;

            ll = map.CellToWorld(minCell);
            ru = map.CellToWorld(maxCell);
        }
        else
        {
            ll = leftlower.position;
            ru = rightupper.position;
        }

        if (random == true)
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
                    ret = new UnityEngine.Vector3(Random.Range(ll.x, ru.x), ru.y, 0);
                    break;
                case 1:
                    ret = new UnityEngine.Vector3(Random.Range(ll.x, ru.x), ll.y, 0);
                    break;
                case 2:
                    ret = new UnityEngine.Vector3(ll.x, Random.Range(ll.y, ru.y), 0);
                    break;
                case 3:
                    ret = new UnityEngine.Vector3(ru.x, Random.Range(ll.y, ru.y), 0);
                    break;
            }
        }
        return ret;
    }

}
