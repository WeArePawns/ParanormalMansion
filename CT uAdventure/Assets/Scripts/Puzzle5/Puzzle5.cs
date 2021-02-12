using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle5 : MonoBehaviour
{
    public Difficulty difficulty;
    public RectTransform gridPatternRect;
    public RectTransform gridNumberRect;
    public GameObject coloredCell;
    public GameObject textCell;
    public Color colorPatternGrid;

    int cells;
    float padding = 5;
    int numDigits;

    int[] nCells = { 3, 5, 7 };
    int[] nDigits = { 4, 6, 8 };

    public GridLayoutGroup gridPatternLayout;
    public GridLayoutGroup gridNumberLayout;

    List<List<int>> cellsValues;
    List<List<GameObject>> patternColor;
    List<int> sequence;

    Queue<int> hints = new Queue<int>();

    public Text dialedNumbersText;
    private List<int> dialedNumbers;

    public ParticleSystem finishParticles;

    void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_5_DIFICULTY");

        numDigits = nDigits[(int)difficulty];
        cells = nCells[(int)difficulty];

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
            string correct = sequence[dialedNumbers.Count] == value ? "correct" : "incorrect";
            AssetPackage.TrackerAsset.Instance.GameObject.Used("Button" + value.ToString() + "Pressed " + correct, GameObjectTracker.TrackedGameObject.GameObject);

            dialedNumbersText.text += value.ToString();
            dialedNumbers.Add(value);
        }
        else
        {
            if (value == -1)
            {
                ResetCode();
                AssetPackage.TrackerAsset.Instance.GameObject.Used("ResetPressed", GameObjectTracker.TrackedGameObject.GameObject);
            }
            else if (value == -2)
            {
                AssetPackage.TrackerAsset.Instance.GameObject.Used("EnterPressed", GameObjectTracker.TrackedGameObject.GameObject);

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

    public void useHint()
    {
        if (hints.Count <= 2) return;

        int pos = hints.Dequeue();
        gridNumberLayout.transform.GetChild(pos).GetComponent<Image>().color = gridPatternLayout.transform.GetChild(pos).GetComponent<Image>().color;
        AssetPackage.TrackerAsset.Instance.GameObject.Used("Puzzle5HintUsed", GameObjectTracker.TrackedGameObject.GameObject);
    }

    void ResetCode()
    {
        dialedNumbersText.text = string.Empty;
        dialedNumbers.Clear();
    }

    bool CheckSolution()
    {
        if (sequence.Count != dialedNumbers.Count) return false;

        for (int i = 0; i < sequence.Count; i++)
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
        for (int i = 0; i < cells; i++)
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
        hints.Enqueue(cells * y + x);

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
            hints.Enqueue(cells * y + x);
        }
        string combindedString = string.Join(",", sequence);
        AssetPackage.TrackerAsset.Instance.GameObject.Used("Puzzle5Sequence " + combindedString, GameObjectTracker.TrackedGameObject.GameObject);
    }

    void InitializeNumberedGrid()
    {
        // Creacion de patron de numeros
        float gridWidth = gridNumberRect.rect.width;
        float gridHeight = gridNumberRect.rect.height;
        gridNumberLayout.cellSize = new Vector2((gridWidth - (cells + 1) * padding) / (float)cells,
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
        int diff = uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_5_DIFICULTY");
        uAdventure.Runner.Game.Instance.GameState.SetVariable("PUZZLE_5_DIFICULTY", ++diff);

        if (diff > (int)Difficulty.Hard)
        {
            // set variables
            var element = uAdventure.Runner.Game.Instance.GameState.FindElement<uAdventure.Core.Item>("Engranaje3");
            uAdventure.Runner.InventoryManager.Instance.AddElement(element);
            uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle3", 0);
            uAdventure.Runner.Game.Instance.Talk("Oh había un engranaje detrás del cuadro, lo guardaré en la mochila", uAdventure.Core.Player.IDENTIFIER);
            uAdventure.Runner.Game.Instance.RunTarget("RoomDownLeft");
        }
        else
        {
            uAdventure.Runner.Game.Instance.RunTarget("Minijuego5");
        }
    }
}
