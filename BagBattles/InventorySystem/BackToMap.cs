using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToMap : MonoBehaviour
{
    public void BackToMapScene()
    {
        SceneManager.LoadScene("Map");
        PlayerController.Instance.DestroyAllTriggers();
        InventorySystem.Instance.SetActive(false);
        InventoryManager.Instance.TriggerTriggerItem();
    }
}
