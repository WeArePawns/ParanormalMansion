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

    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;


    public void Start()
    {
        value = 0;
        initColors();
        GetComponent<MeshRenderer>().material.color = gradient.Evaluate(value / (float)(maxValue-1));
    }

    public void addConection(GridBox box)
    {
        if (adjacent.Count >= 4) return;
        adjacent.Add(box);
    }

    public void setMaxValue(int mValue)
    {
        if (mValue < 2) return;
        maxValue = mValue;
    }

    public int getValue()
    {
        return value;
    }

    public void boxClicked()
    {
        checkBox();
        if (adjacent.Count >= 0)
            foreach (GridBox box in adjacent)
                box.checkBox();
    }

    public void checkBox()
    {
        value = (value + 1) % maxValue;
        GetComponent<MeshRenderer>().material.color = gradient.Evaluate(value / (float)(maxValue-1));
    }

    public bool isChecked()
    {
        return value > 0;
    }

    public void setIndex(Vector2Int ind)
    {
        index = ind;
    }

    public Vector2Int getIndex()
    {
        return index;
    }
    void initColors()
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
