using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToMap : MonoBehaviour
{
    public void BackToMapScene()
    {
        SceneManager.LoadScene("Map");
        InventorySystem.Instance.SetActive(false);
        InventoryManager.Instance.TriggerTriggerItem();
        InventoryManager.Instance.AddTriggerItemToPlayer();
    }
}
