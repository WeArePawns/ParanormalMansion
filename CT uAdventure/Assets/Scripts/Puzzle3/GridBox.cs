using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBox : MonoBehaviour
{
    bool check = false;
    Vector2Int index;
    List<GridBox> adjacent = new List<GridBox>();

    public void Start()
    {
        check = false;
        GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public void addConection(GridBox box)
    {
        if (adjacent.Count >= 4) return;
        adjacent.Add(box);
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
        check = !check;
        GetComponent<MeshRenderer>().material.color = (check) ? Color.yellow : Color.white;
    }

    public bool isChecked()
    {
        return check;
    }

    public void setIndex(Vector2Int ind)
    {
        index = ind;
    }

    public Vector2Int getIndex()
    {
        return index;
    }
}
