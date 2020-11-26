using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardHolder : MonoBehaviour
{
    private RectTransform mRectTransform;
    public Card card = null;

    void Start()
    {
        mRectTransform = GetComponent<RectTransform>();
        PositionCard();
    }

    public void SetCard(in Card card)
    {
        this.card = card;
        PositionCard();
    }

    public void Dispatch()
    {
        this.card = null;
    }

    private void PositionCard()
    {
        if (card == null) return;

        RectTransform rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        rectTransform.anchoredPosition = mRectTransform.anchoredPosition;
    }
}
