using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBox : MonoBehaviour
{
    bool check = false;

    public void Start()
    {
        check = false;
        GetComponent<MeshRenderer>().material.color = Color.white;
    }

    public void checkBox()
    {
        check = !check;
        GetComponent<MeshRenderer>().material.color = (check) ? Color.black : Color.white;
    }

    public bool isChecked()
    {
        return check;
    }
}
