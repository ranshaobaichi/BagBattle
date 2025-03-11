using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [SerializeField]
    public enum GunType
    {
        None,
        Normal_Gun,
        Spear_Gun,
        Rocket_Gun,
        Lightning_Gun,
        Fire_Gun,
        Ice_Gun,
        Swallow_Gun,
        Bomb_Gun,
        Jump_Gun,
        Split_Gun
    };

    //public GameObject shellPrefab;
    //protected Transform shellPos;
    //protected Animator animator;
    public GameObject bulletPrefab;
    [SerializeField]
    new string name;
    [Header("攻击属性")]
    [Tooltip("攻速")] public float attack_speed;
    [Tooltip("是否发射角度随机偏移")] public bool random_angel;
    [Tooltip("默认指向方向")] public Vector2 pos_direct;

    [Header("子弹属性")]
    [Tooltip("子弹基础属性")] public Bullet.BulletBasicAttribute bullet_attr;

    protected Transform muzzlePos;
    protected Vector2 mousePos;
    protected Vector2 direction;
    protected float attack_timer;
    protected float flipY;

    [Header("标志位")]
    protected bool attack_flag;

    #region 枪械操控
    protected virtual void Start()
    {
        //animator = GetComponent<Animator>();
        muzzlePos = transform.Find("Muzzle");
        //shellPos = transform.Find("BulletShell");
        flipY = transform.localScale.y;
        random_angel = false;
        attack_flag = true;
        attack_timer = 0.0f;
    }

    protected virtual void Update()
    {
        if (PlayerController.Instance.Live() == false)
            return;
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = (mousePos - new Vector2(transform.position.x, transform.position.y)).normalized;

        transform.right = direction;
        if (mousePos.x < transform.position.x)
            transform.localScale = new Vector3(flipY, -flipY, 1);
        else
            transform.localScale = new Vector3(flipY, flipY, 1);


        if (attack_flag == false)
            attack_timer -= Time.deltaTime;
        if (attack_timer <= 0)
        {
            attack_flag = true;
            attack_timer = attack_speed;
        }

        Shoot();
    }

    protected virtual void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && attack_flag)
        {
            Fire();
        }
    }

    protected virtual void SetBullet(GameObject bullet)
    {
        // bullet.GetComponent<Bullet>().SetBullet(bullet_attr);
    }

    protected virtual void Fire()
    {
        //animator.SetTrigger("Shoot");
        GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
        // Debug.Log("self bullet" + bullet.GetInstanceID());
        bullet.transform.position = muzzlePos.position;
        SetBullet(bullet);

        // random angel
        if (random_angel == true)
        {
            float angel = Random.Range(-5f, 5f);
            bullet.GetComponent<Bullet>().SetSpeed(Quaternion.AngleAxis(angel, Vector3.forward) * direction);
        }
        else
            bullet.GetComponent<Bullet>().SetSpeed(direction);
        // GameObject shell = ObjectPool.Instance.GetObject(shellPrefab);
        // shell.transform.position = shellPos.position;
        // shell.transform.rotation = shellPos.rotation;
        attack_timer = attack_speed;
        attack_flag = false;
    }

    public virtual void Fire(Vector3 pos)
    {
        pos_direct = pos;
        if (attack_flag == true)
        {
            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = muzzlePos.position;
            SetBullet(bullet);

            // random angel
            if (random_angel == true)
            {
                float angel = Random.Range(-5f, 5f);
                bullet.GetComponent<Bullet>().SetSpeed(Quaternion.AngleAxis(angel, Vector3.forward) * pos);
            }
            else
                bullet.GetComponent<Bullet>().SetSpeed(pos);

            attack_timer = attack_speed;
            attack_flag = false;
        }
    }
    #endregion
}