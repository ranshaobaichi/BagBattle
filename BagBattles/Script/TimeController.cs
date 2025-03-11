using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class TimeController : MonoBehaviour
{
    public static TimeController Instance;
    [SerializeField]
    private float game_time;
    private float current_time;
    public Text T;
    public GameObject InventorySystem;
    private bool timeUP = false;
    public bool TimeUp() => timeUP;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        current_time = game_time;
    }

    // Update is called once per frame
    void Update()
    {
        if (current_time <= 0f)
        {
            PlayerController.Instance.Dead();
            timeUP = true;
            StartCoroutine(ChooseUI());
        }
        else
        {
            current_time -= Time.deltaTime;
            T.text = ((int)current_time).ToString();
        }
    }
    private IEnumerator ChooseUI()
    {
        yield return new WaitForSeconds(.5f);
        // Debug.Log("UI active");
        InventorySystem.SetActive(true);
        gameObject.SetActive(false);
    }
}
