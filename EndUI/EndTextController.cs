using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTextController : MonoBehaviour
{
    private Image titleImage;
    public Button returnButton;
    [Header("每几秒变化一次")] public float fadeTimes;
    [Header("初始透明度")] public float initAlpha = .15f;
    [Header("每次变化多少")] public float fadeStep = .15f;
    void Awake()
    {
        titleImage = GetComponent<Image>();
    }
    void Start()
    {
        titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b, initAlpha);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        while (true)
        {
            yield return new WaitForSeconds(fadeTimes);
            titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b, Mathf.Min(1, titleImage.color.a + fadeStep));
            if (titleImage.color.a >= 1)
            {
                break;
            }
        }
        returnButton.interactable = true;
    }
}
