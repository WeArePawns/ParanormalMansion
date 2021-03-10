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

    public Button hintButton;

    public StarsController starsController;
    private double nPasos = 0.0;
    private int nPasosMinimos = 2;

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
        // Valor -3 reservado para el delete

        dialedNumbersText.color = Color.black;
        if (value >= 0)
        {
            if (dialedNumbers.Count >= numDigits) return;

            AssetPackage.TrackerAsset.Instance.setVar("correct", sequence[dialedNumbers.Count] == value);
            AssetPackage.TrackerAsset.Instance.GameObject.Used("button_" + value.ToString(), GameObjectTracker.TrackedGameObject.GameObject);

            dialedNumbersText.text += value.ToString();
            dialedNumbers.Add(value);
        }
        else
        {
            if (value == -1)
            {
                ResetCode();
                AssetPackage.TrackerAsset.Instance.GameObject.Used("reset_button", GameObjectTracker.TrackedGameObject.GameObject);
            }
            else if (value == -2)
            {

                if (!CheckSolution())
                {
                    dialedNumbersText.color = Color.red;
                    AssetPackage.TrackerAsset.Instance.setSuccess(false);
                    AssetPackage.TrackerAsset.Instance.Completable.Progressed("laberinto_numeros_" + (int)(difficulty + 1), 0);
                }
                else
                {                  
                    dialedNumbersText.color = Color.green;
                    finishParticles.Play();

                    // estrella de pasos minimos
                    if (nPasos > nPasosMinimos)
                        starsController.deactivateMinimoStar();

                    starsController.gameObject.SetActive(true);

                    int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");
                    uAdventure.Runner.Game.Instance.GameState.SetVariable("N_STARS", nStars + starsController.getStars());
                }
            }
            else if (value == -3)
            {
                if (dialedNumbers.Count > 0)
                {
                    dialedNumbers.RemoveAt(dialedNumbers.Count - 1);
                    dialedNumbersText.text = "";
                    foreach (int n in dialedNumbers)
                        dialedNumbersText.text += n.ToString();

                    nPasos += 1.0 / numDigits;
                    AssetPackage.TrackerAsset.Instance.GameObject.Used("delete_button", GameObjectTracker.TrackedGameObject.GameObject);
                }

            }
        }
    }

    public void useHint()
    {
        if (hints.Count <= 2) return;

        starsController.deactivateNoPistasStar();

        int pos = hints.Dequeue();
        gridNumberLayout.transform.GetChild(pos).GetComponent<Image>().color = gridPatternLayout.transform.GetChild(pos).GetComponent<Image>().color;
        AssetPackage.TrackerAsset.Instance.GameObject.Used("hint_button", GameObjectTracker.TrackedGameObject.GameObject);

        if (hints.Count <= 2) hintButton.interactable = false;
    }

    void ResetCode()
    {
        nPasos += (double)dialedNumbers.Count / numDigits;

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

        AssetPackage.TrackerAsset.Instance.setVar("initial_state", GetState());
        AssetPackage.TrackerAsset.Instance.Completable.Initialized("laberinto_numeros_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);
    }

    string GetState()
    {
        string state = "";
        for (int j = 0; j < cells; j++)
        {
            state += "\n|";
            for (int i = 0; i < cells; i++)
            {
                Color color = patternColor[j][i].GetComponent<Image>().color;
                string c = (color == Color.red) ? "r" : ((color == colorPatternGrid) ? "g" : ((color == Color.blue) ? "b" : "w"));
                state += cellsValues[j][i].ToString() + c + "|";
            }
        }
        return state;
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
    public void changeScene()
    {
        AssetPackage.TrackerAsset.Instance.setScore(starsController.getStars());
        AssetPackage.TrackerAsset.Instance.Completable.Completed("laberinto_numeros_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

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
