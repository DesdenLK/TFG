using UnityEngine;
using UnityEngine.UI;

public class ScrollSync : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform headerContent;

    void Update()
    {
        float contentX = scrollRect.content.anchoredPosition.x;
        Vector2 pos = headerContent.anchoredPosition;
        pos.x = contentX;
        headerContent.anchoredPosition = pos;
    }
}
