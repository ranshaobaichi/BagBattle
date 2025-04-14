using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToMap : MonoBehaviour
{
    public void BackToMapScene()
    {
        Debug.Log("Back to Map Scene");
        SceneManager.LoadScene("Map");
        MapCellManager.Instance.transform.parent.gameObject.SetActive(true);
        MapCellManager.Instance.gameObject.SetActive(true);
        InventorySystem.Instance.gameObject.SetActive(false);
        InventoryManager.Instance.TriggerTriggerItem();
        WaveManager.Instance.SetActive(false);
    }
}
