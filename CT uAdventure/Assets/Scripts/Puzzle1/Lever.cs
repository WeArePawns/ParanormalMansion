using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public Image firstRingLabel;
    public Image secondRingLabel;
    public Text secondRingText;
    public Text firstRingText;

    public void SetLever(Color first, Color second, int dir)
    {
        firstRingLabel.color = first;
        secondRingLabel.color = second;
        firstRingText.text = (dir * 45).ToString() + "º";
        secondRingText.text = (dir * 90).ToString() + "º";
    }

    public void ActivateLabels()
    {
        firstRingLabel.gameObject.SetActive(true);
        secondRingLabel.gameObject.SetActive(true);
    }

    public void ActivateTexts()
    {
        firstRingText.gameObject.SetActive(true);
        secondRingText.gameObject.SetActive(true);
    }
}
