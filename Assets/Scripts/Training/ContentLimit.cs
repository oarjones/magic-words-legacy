using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class ContentLimit : MonoBehaviour
{
    private RectTransform contentRectTransform;
    private RectTransform parentRectTransform; 
    public RectTransform panelContainer;
    public ScrollRect scrollRect;

    private float originalDecelerationRate;

    public float threshold = 10f;
    private void Awake()
    {
        contentRectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent.parent.GetComponent<RectTransform>();
        originalDecelerationRate = scrollRect.decelerationRate;
    }

    private void Update()
    {
        LimitScroll();
    }

    private void LimitScroll()
    {
        float contentWidth = contentRectTransform.rect.width;
        float parentWidth = parentRectTransform.rect.width;

        Debug.Log("ScrollViewContent x pos:" + contentRectTransform.transform.position.x);
        Debug.Log("ScrollViewContent x(local) pos:" + contentRectTransform.transform.localPosition.x);

        var xLocalPos = contentRectTransform.transform.localPosition.x;

        if ((contentRectTransform.transform.localPosition.x - threshold) < panelContainer.rect.width)
        {
            scrollRect.decelerationRate = 0f; 
        }
        else if((contentRectTransform.transform.localPosition.x + threshold) > parentWidth)
        {
            scrollRect.decelerationRate = 0f;
        }
        else
        {
            scrollRect.decelerationRate = originalDecelerationRate;
        }


        if (contentRectTransform.transform.localPosition.x < panelContainer.rect.width)
        {
            contentRectTransform.transform.localPosition = new Vector2(panelContainer.rect.width, contentRectTransform.transform.localPosition.y);
        }
        else
        {
            if(contentRectTransform.transform.localPosition.x > parentWidth)
            {
                contentRectTransform.transform.localPosition = new Vector2(parentWidth, contentRectTransform.transform.localPosition.y);
            }
        }

        // Si el contenido es más pequeño que el viewport, simplemente lo centramos.
        //if (contentWidth <= parentWidth)
        //{
        //    contentRectTransform.anchoredPosition = new Vector2(0, contentRectTransform.anchoredPosition.y);
        //    return;
        //}

        // Si el contenido es más grande que el viewport, limitamos su posición
        //float minX = (contentWidth - parentWidth) / 2 * -1;
        //float maxX = (contentWidth - parentWidth) / 2;

        //contentRectTransform.anchoredPosition = new Vector2(Mathf.Clamp(contentRectTransform.anchoredPosition.x, minX, maxX), contentRectTransform.anchoredPosition.y);
    }
}
