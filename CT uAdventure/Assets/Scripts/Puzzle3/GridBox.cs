using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBox : MonoBehaviour
{
    int value = 0;
    int maxValue = 2;
    Vector2Int index;
    List<GridBox> adjacent = new List<GridBox>();

    public Color turnedOffColor = Color.black;
    public Color turnedOnColor = Color.yellow;
    public GameObject indicator;

    int indicatorsActive = 0;

    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    public void Start()
    {
        value = 0;
        InitColors();
        GetComponent<MeshRenderer>().material.color = gradient.Evaluate(value / (float)(maxValue - 1));
    }

    public void AddConection(GridBox box)
    {
        if (adjacent.Count >= 4) return;
        adjacent.Add(box);
    }

    public void SetMaxValue(int mValue)
    {
        if (mValue < 2) return;
        maxValue = mValue;
    }

    public int GetValue()
    {
        return value;
    }

    public void BoxClicked()
    {
        CheckBox();
        if (adjacent.Count >= 0)
            foreach (GridBox box in adjacent)
                box.CheckBox();
    }

    public void AddIndicator()
    {
        indicatorsActive++;
        GameObject ind = (transform.childCount < indicatorsActive) ? Instantiate(indicator, transform) : transform.GetChild(indicatorsActive - 1).gameObject;
        ind.SetActive(true);
        ind.transform.localPosition = new Vector3(0, (transform.localScale.y / 2.0f - ind.transform.localScale.y / 2.0f), -1);
        SortIndicators();
    }

    public void RemoveAllIndicators()
    {
        indicatorsActive = 0;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    public void RemoveIndicator()
    {
        if (indicatorsActive <= 0) return;
        indicatorsActive--;
        SortIndicators();
    }

    public void SortIndicators()
    {
        float indSize = 0.2f;
        float initialX = -(indicatorsActive / 2.0f) * (indSize / 2);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject ind = transform.GetChild(i).gameObject;
            ind.transform.localPosition = new Vector3(initialX + (i * indSize), ind.transform.localPosition.y, ind.transform.localPosition.z);
            ind.SetActive(i < indicatorsActive);
        }
    }

    public void CheckBox()
    {
        value = (value + 1) % maxValue;
        GetComponent<MeshRenderer>().material.color = gradient.Evaluate(value / (float)(maxValue - 1));
    }

    public bool IsChecked()
    {
        return value > 0;
    }

    public void SetIndex(Vector2Int ind)
    {
        index = ind;
    }

    public Vector2Int GetIndex()
    {
        return index;
    }
    void InitColors()
    {
        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = turnedOffColor;
        colorKey[0].time = 0.0f;
        colorKey[1].color = turnedOnColor;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 0.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
    }
}
