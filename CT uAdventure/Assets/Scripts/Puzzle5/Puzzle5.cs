using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle5 : MonoBehaviour
{
    public RectTransform gridPatternRect;
    public RectTransform gridNumberRect;
    public GameObject coloredCell;
    public GameObject textCell;
    public Color colorPatternGrid;
    public int cells = 5;
    float padding = 5;
    public GridLayoutGroup gridPatternLayout;
    public GridLayoutGroup gridNumberLayout;
    public int numDigits = 9;

    List<List<int>> cellsValues;
    List<List<GameObject>> patternColor;
    List<int> sequence;

    public Text dialedNumbersText;
    private List<int> dialedNumbers;

    public ParticleSystem finishParticles;

    void Start()
    {
        dialedNumbers = new List<int>();
        InitializeNumberedGrid();
        InitializeColoredGrid();
        InitializeColoredPattern();
    }

    public void PressButton(int value)
    {
        // Valor -1 reservado para el reset
        // Valor -2 reservado para el enter

        if (value >= 0)
        {
            if (dialedNumbers.Count >= numDigits) return;
            dialedNumbersText.text += value.ToString();
            dialedNumbers.Add(value);
        }
        else
        {
            if(value == -1)
            {
                ResetCode();
            }
            else if(value == -2)
            {
                if (!CheckSolution())
                    ResetCode();
                else
                {
                    print("VICTORY");
                    finishParticles.Play();
                    Invoke("changeScene", 3.0f);
                }
            }
        }
    }

    void ResetCode()
    {
        dialedNumbersText.text = string.Empty;
        dialedNumbers.Clear();
    }

    bool CheckSolution()
    {
        if (sequence.Count != dialedNumbers.Count) return false;

        for(int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i] != dialedNumbers[i]) return false;
        }
        return true;
    }

    void InitializeColoredGrid()
    {
        // Creacion de patron por colores
        float gridWidth = gridPatternRect.rect.width;
        float gridHeight = gridPatternRect.rect.height;
        gridPatternLayout.cellSize = new Vector2((gridWidth - (cells + 1) * padding) / (float)cells,
                                                    (gridHeight - (cells + 1) * padding) / (float)cells);
        gridPatternLayout.spacing = new Vector2(padding, padding);
        gridPatternLayout.padding.top = (int)padding;
        gridPatternLayout.padding.bottom = (int)padding;
        gridPatternLayout.padding.left = (int)padding;
        gridPatternLayout.padding.right = (int)padding;

        patternColor = new List<List<GameObject>>();
        for (int j = 0; j < cells; j++)
        {
            patternColor.Add(new List<GameObject>());
            for (int i = 0; i < cells; i++)
            {
                GameObject cell = Instantiate(coloredCell, gridPatternRect.transform);
                Image cellImage = cell.GetComponent<Image>();
                cellImage.color = Color.gray;
                patternColor[j].Add(cell);
            }
        }
    }

    void InitializeColoredPattern()
    {
        sequence = new List<int>();
        List<int> columnsVisited = new List<int>(cells);
        List<int> rowsVisited = new List<int>(cells);
        for(int i = 0; i < cells; i++)
        {
            columnsVisited.Add(0);
            rowsVisited.Add(0);
        }

        bool vertical = false;
        //Random entrance point
        int x = cells - 1;
        int y = Random.Range(1, cells);
        columnsVisited[x]++;
        rowsVisited[y]++;
        sequence.Add(cellsValues[y][x]);
        patternColor[y][x].GetComponent<Image>().color = Color.blue;

        for (int i = 0; i < numDigits - 1; i++)
        {
            if (!vertical)
            {
                do
                {
                    x = Random.Range(0, cells);
                }
                while (columnsVisited[x] >= 1);
            }
            else
            {
                do
                {
                    y = Random.Range(0, cells);
                }
                while (rowsVisited[y] >= 1);
            }

            columnsVisited[x]++;
            rowsVisited[y]++;
            vertical = !vertical;

            sequence.Add(cellsValues[y][x]);
            patternColor[y][x].GetComponent<Image>().color = (i != numDigits - 2) ? colorPatternGrid : Color.red;
        }

    }

    void InitializeNumberedGrid()
    {
        // Creacion de patron de numeros
        float gridWidth = gridNumberRect.rect.width;
        float gridHeight = gridNumberRect.rect.height;
        gridNumberLayout.cellSize = new Vector2(   (gridWidth - (cells + 1) * padding) / (float)cells,
                                                    (gridHeight - (cells + 1) * padding) / (float)cells);
        gridNumberLayout.spacing = new Vector2(padding, padding);
        gridNumberLayout.padding.top = (int)padding;
        gridNumberLayout.padding.bottom = (int)padding;
        gridNumberLayout.padding.left = (int)padding;
        gridNumberLayout.padding.right = (int)padding;

        cellsValues = new List<List<int>>();
        for (int j = 0; j < cells; j++)
        {
            cellsValues.Add(new List<int>());
            for (int i = 0; i < cells; i++)
            {
                GameObject cell = Instantiate(textCell, gridNumberRect.transform);
                Image cellImage = cell.GetComponent<Image>();
                cellImage.color = Color.white;
                Text text = cell.transform.GetChild(0).GetComponent<Text>();
                int value = Random.Range(0, 10);
                cellsValues[j].Add(value);
                text.text = value.ToString();
            }
        }
    }
    void changeScene()
    {
        // set variables
        uAdventure.Runner.Game.Instance.GameState.AddInventoryItem("PuzzleClue");
        uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle3", 0);
        uAdventure.Runner.Game.Instance.RunTarget("RoomUpRight");
    }
}
