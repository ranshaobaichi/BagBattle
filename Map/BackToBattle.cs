using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToBattle : MonoBehaviour
{
    public void BackToBattleScene()
    {
        SceneManager.LoadScene("BattleScene");
        InventorySystem.Instance.SetActive(false);
        PlayerController.Instance.SetActive(true);
    }
}
