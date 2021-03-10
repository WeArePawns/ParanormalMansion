using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class Card : MonoBehaviour
{
    public List<int> values = new List<int>();

    public Color positiveColor = Color.blue;
    public Color negativeColor = Color.red;
    public Color neutralColor = Color.white;

    private float width;

    public float lineWidthPerUnit = 10;
    public GameObject linePrefab;
    List<GameObject> children;

    Image frame = null;

    private void Start()
    {
        children = new List<GameObject>();
        frame = GetComponent<Image>();
    }

    private void Update()
    {
        if (linePrefab == null) return;

        if (children == null) children = new List<GameObject>();

        // Si hay mas valores que hijos, se destruyen para volver a crearlos
        if(transform.childCount != values.Count)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        // Miramos los hijos
        children.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }

        // Si no hay hijos, crea las lineas
        if (children.Count == 0)
        {
            CreateLines();
        }

        // Actualiza las lineas
        if (children.Count == values.Count)
            CheckLines();
        else print("No se puede modificar");
    }

    private void CreateLines()
    {
        for (int i = 0; i < values.Count; i++)
        {
            GameObject line = Instantiate(linePrefab, transform);
            children.Add(line);
        }
    }

    public string GetState()
    {
        string state = "[";
        for (int i = 0; i < values.Count; i++)
            state += values[i].ToString() + ((i == values.Count - 1) ? "" : " ");
        state += "]";
        return state;
    }

    private void CheckLines()
    {
        if (frame == null) frame = GetComponent<Image>();

        width = frame.rectTransform.rect.width;
        lineWidthPerUnit = width / 30.0f;

        float widthBetweenLines = (float)width / (float)(values.Count + 1);

        for (int i = 0; i < values.Count; i++)
        {
            GameObject line = children[i];
            RectTransform rectTransform = line.GetComponent<RectTransform>();
            Image image = line.GetComponent<Image>();

            Color color = values[i] < 0 ? negativeColor : (values[i] == 0 ? neutralColor : positiveColor);

            image.color = color;

            rectTransform.anchoredPosition = new Vector2(widthBetweenLines * (i + 1), rectTransform.anchoredPosition.y);
            rectTransform.sizeDelta = new Vector2(lineWidthPerUnit * Mathf.Abs(values[i]), frame.rectTransform.rect.height);
        }
    }

    public List<int> GetValues()
    {
        return values;
    }

    public void SetValues(List<int> values)
    {
        this.values = values;
    }

    public void AddValues(List<int> values)
    {
        if(values.Count != this.values.Count)
        {
            print("No tienen el mismo tamaño, se sobreescribe o se quedara corto");
        }

        for(int i = 0; i < values.Count; i++)
        {
            if (i < this.values.Count)
                this.values[i] += values[i];
            else
                this.values.Add(values[i]);
        }
    }

    // Pone todos los valores a cero
    public void ResetValues()
    {
        for (int i = 0; i < values.Count; i++)
        {
            values[i] = 0;
        }
    }

}
