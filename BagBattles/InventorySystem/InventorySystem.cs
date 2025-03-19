using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    // Start is called before the first frame update
    public static InventorySystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start() => DontDestroyOnLoad(gameObject);
    public void SetActive(bool isActive) => gameObject.SetActive(isActive);
}
