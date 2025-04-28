using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Startbutton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image buttonImage;
    private Color imageOriginalColor;
    private Image textImage;
    private Color textOriginalColor;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            imageOriginalColor = buttonImage.color;
        }
        textImage = GetComponentInChildren<Image>();
        if (textImage != null)
        {
            textOriginalColor = textImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            // 提高亮度并增加透明度
            Color highlightColor = imageOriginalColor * 1.2f;
            highlightColor.a = 0.6f; // 设置透明度
            buttonImage.color = highlightColor;

            if (textImage != null)
            {
                Color textColor = textImage.color;
                textColor.a = 0.6f;
                textImage.color = textColor;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = imageOriginalColor;
        }
        if (textImage != null)
        {
            textImage.color = textOriginalColor;
        }
    }
}