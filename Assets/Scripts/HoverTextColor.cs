using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HoverTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public TextMeshProUGUI targetText;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.196f, 0.196f, 0.196f, 1f); // Dark gray matching Play button text

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        // Force reset color when the menu panel becomes active
        if (targetText != null)
        {
            targetText.color = normalColor;
        }
    }

    private void OnDisable()
    {
        if (targetText != null)
        {
            targetText.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetColor(normalColor);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetColor(hoverColor);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetColor(normalColor);
    }

    private void SetColor(Color color)
    {
        if (targetText != null)
        {
            targetText.color = color;
        }
    }
}
