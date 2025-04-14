using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class TimeController : MonoBehaviour
{
    public static TimeController Instance;
    [SerializeField]
    private float game_time;
    private float current_time;
    private float timeScale;
    public Text T;
    private bool timeUP = false;
    public bool TimeUp() => timeUP;
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (active == true)
        {
            current_time = game_time;
            timeUP = false;
        }
    }
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }   
    }

    // Start is called before the first frame update
    void Start()
    {
        current_time = game_time;
        timeUP = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeUP) return;
        if (PlayerController.Instance.Live() == false) gameObject.SetActive(false);

        if (current_time <= 0f)
            TimeUpEvent();
        else
        {
            current_time -= Time.deltaTime;
            T.text = ((int)current_time).ToString();
        }
    }

    [ContextMenu("TimeUp")]
    public void TimeUpEvent()
    {
        timeUP = true;
        PlayerController.Instance.FinishRound();
        StartCoroutine(ChooseUI());
    }
    private IEnumerator ChooseUI()
    {
        yield return new WaitForSeconds(.5f);
        // Debug.Log("UI active");
        InventorySystem.Instance.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    public void PauseGame()
    {
        timeScale = Time.timeScale;
        Time.timeScale = 0f; // 暂停游戏
    }

    public void ResumeGame()
    {
        Time.timeScale = timeScale; // 恢复游戏速度
    }
}
