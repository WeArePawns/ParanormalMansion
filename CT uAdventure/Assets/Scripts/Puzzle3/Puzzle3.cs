using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle3 : MonoBehaviour
{
    public GameObject gridBoxPrefab;
    public Vector2 gridOffset;
    public Difficulty difficulty;

    public ParticleSystem finishParticles;

    public Text bestSolution;
    public Text currentMoves;

    GridBox[,] grid;
    GridBox pointedBox;
    List<GridBox> boxesClicked = new List<GridBox>(), solBoxes;
    bool[,] sol;
    int[,] initialState;

    int[] gridS = { 5, 4, 3 };
    int[] solClicks = { 5, 5, 7 };
    int[] maxValues = { 2, 2, 2 };
    int[] clicksToHint = { 4, 4, 3 };
    int clicks;

    //Each sol is a sizeXsize grid
    int columns;
    int rows;
    int maxValue;

    public Button hintButton;

    public StarsController starsController;
    private int nPasos;
    private int nPasosMinimos;
    private bool finished = false;

    private int nHintsClicks = 0;
    private bool addIndicators = false;
    private int nMoves = 0;

    void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_3_DIFICULTY");

        columns = gridS[(int)difficulty];
        rows = gridS[(int)difficulty];
        clicks = solClicks[(int)difficulty];
        maxValue = maxValues[(int)difficulty];

        CreateGrid();

        Invoke("CreateSol", 0.1f);
    }

    void Update()
    {
        if (finished) return;

        if (pointedBox != null && Input.GetMouseButtonDown(0))
        {
            nPasos++;
            nMoves++;
            currentMoves.text = "Actual: " + nMoves.ToString();

            Vector2Int index = pointedBox.GetIndex();
            bool correct = ClickBox(pointedBox);

            AssetPackage.TrackerAsset.Instance.setVar("is_correct", correct);
            AssetPackage.TrackerAsset.Instance.GameObject.Used("grid_box_" + index.x.ToString() + "_" + index.y.ToString() + "_checked", GameObjectTracker.TrackedGameObject.GameObject);

            if (boxesClicked.Count > clicksToHint[(int)difficulty]) hintButton.interactable = true;

            //Cuando se soluciona el puzzle
            CheckSolved();
        }

        CheckMouse();
    }

    private bool ClickBox(GridBox boxToClick)
    {
        boxToClick.BoxClicked();
        bool correct = boxesClicked.FindAll(box => box == boxToClick).Count > 0;
        if (correct)//Si esta se elimina una vez
        {
            boxesClicked.Remove(boxToClick);
            if (addIndicators) boxToClick.RemoveIndicator();
        }
        else//Si no esta se añade el numero de veces que clickar para volver al estado original
            for (int l = 1; l < maxValue; l++)
            {
                boxesClicked.Add(boxToClick);
                if (addIndicators) boxToClick.AddIndicator();
            }
        return correct;
    }

    public void UseHint()
    {
        if (boxesClicked.Count <= 3) return;

        starsController.deactivateNoPistasStar();

        while (boxesClicked.Count > 3)
            ClickBox(boxesClicked[boxesClicked.Count - 1]);

        addIndicators = ++nHintsClicks >= 3;
        if (nHintsClicks == 3)
            foreach (GridBox box in boxesClicked)
                box.AddIndicator();

        //Cuando se soluciona el puzzle
        CheckSolved();

        AssetPackage.TrackerAsset.Instance.setVar("state", GetState());
        AssetPackage.TrackerAsset.Instance.GameObject.Used("hint_button", GameObjectTracker.TrackedGameObject.GameObject);

        nMoves = 0;
        bestSolution.text = "Movimientos mínimos: " + boxesClicked.Count.ToString();
        currentMoves.text = "Actual: " + nMoves.ToString();

        hintButton.interactable = false;
    }

    void CheckSolved()
    {
        if (Solved())
        {
            finishParticles.Play();
            finished = true;

            // estrella de pasos minimos
            if (nPasos > 2 * nPasosMinimos)
                starsController.deactivateMinimoStar();

            starsController.gameObject.SetActive(true);

            int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");
            uAdventure.Runner.Game.Instance.GameState.SetVariable("N_STARS", nStars + starsController.getStars());
        }
    }


    void CheckMouse()
    {
        if (finished) return;

        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit cameraRayHit;
        // If the ray strikes an object...
        if (Physics.Raycast(cameraRay, out cameraRayHit, Mathf.Infinity))
        {
            // ...and if that object is a box...
            if (cameraRayHit.transform.tag == "GridBox")
            {
                pointedBox = cameraRayHit.transform.gameObject.GetComponent<GridBox>();
                return;
            }
        }
        pointedBox = null;
    }

    void CreateSol()
    {
        for (int k = 0; k < clicks; k++)
        {
            int i = Random.Range(0, rows);
            int j = Random.Range(0, columns);
            ClickBox(grid[i, j]);
        }
        solBoxes = new List<GridBox>(boxesClicked);

        nPasosMinimos = clicks;

        int sum = 0;
        sol = new bool[rows, columns];
        initialState = new int[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                initialState[i, j] = grid[i, j].GetValue();
                sol[i, j] = false;
                sum += grid[i, j].IsChecked() ? 1 : 0;
            }
        }

        //Al menos una casilla activa
        if (sum == 0)
        {
            nPasosMinimos = 2;

            ClickBox(grid[0, 0]);
            initialState[0, 0] = 1;
        }
        AssetPackage.TrackerAsset.Instance.setVar("initial_state_", GetState());
        AssetPackage.TrackerAsset.Instance.Completable.Initialized("electricista_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

        bestSolution.text = "Movimientos mínimos: " + boxesClicked.Count.ToString();
        currentMoves.text = "Actual: " + nMoves.ToString();

        if (boxesClicked.Count <= 3) hintButton.interactable = false;
    }

    public string GetState()
    {
        string state = "cells_active -> |Value(X Y)|: ";
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < columns; j++)
                if (grid[i, j].IsChecked())
                    state += "|" + grid[i, j].GetValue() + "(" + j.ToString() + " " + i.ToString() + ")| ";

        return state;
    }

    public void Reset()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (addIndicators) grid[i, j].RemoveAllIndicators();
                while (initialState[i, j] != grid[i, j].GetValue())
                    grid[i, j].CheckBox();
            }
        }

        boxesClicked = new List<GridBox>(solBoxes);
        AssetPackage.TrackerAsset.Instance.GameObject.Used("reset_button", GameObjectTracker.TrackedGameObject.GameObject);

        if (addIndicators)
            foreach (GridBox box in boxesClicked)
                box.AddIndicator();

        nMoves = 0;
        bestSolution.text = "Movimientos mínimos: " + boxesClicked.Count.ToString();
        currentMoves.text = "Actual: " + nMoves.ToString();

        hintButton.interactable = boxesClicked.Count > 3;
    }

    public void CreateGrid()
    {
        grid = new GridBox[rows, columns];
        Vector2 gridSize = new Vector2((gridBoxPrefab.transform.localScale.x + gridOffset.x) * columns, (gridBoxPrefab.transform.localScale.y + gridOffset.y) * rows);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 offset = new Vector3((gridBoxPrefab.transform.localScale.x + gridOffset.x) * j, -(gridBoxPrefab.transform.localScale.y + gridOffset.y) * i, 0);
                GameObject aux = Instantiate(gridBoxPrefab, transform.position + offset, Quaternion.identity, transform);
                grid[i, j] = aux.GetComponent<GridBox>();
                grid[i, j].SetIndex(new Vector2Int(j, i));
                grid[i, j].SetMaxValue(maxValue);
            }
        }

        CreateConnections();
        FitGrid(gridSize);
    }

    public void CreateConnections()
    {
        Vector2Int[] indexes = new Vector2Int[] { Vector2Int.up, Vector2Int.right };

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                foreach (Vector2Int index in indexes)
                {
                    Vector2Int newPos = index + pos;
                    if (newPos.x < rows && newPos.y < columns)
                    {
                        grid[i, j].AddConection(grid[newPos.x, newPos.y]);
                        grid[newPos.x, newPos.y].AddConection(grid[i, j]);
                    }
                }
            }
        }
    }

    public void FitGrid(Vector2 gridSize)
    {
        float height = Camera.main.orthographicSize * 2.0f;
        float width = height * Screen.width / Screen.height;

        float xRatio = width / gridSize.x;
        float yRatio = height / gridSize.y;
        float ratio = Mathf.Min(xRatio, yRatio);

        transform.localScale = new Vector3(ratio, ratio, 1);
        transform.position = new Vector3(-width / 2 + (gridBoxPrefab.transform.localScale.x / 2 + 0.2f) * ratio, height / 2 - (gridBoxPrefab.transform.localScale.y / 2 + 0.2f) * ratio, 0);
    }

    public bool Solved()
    {
        if (sol.Length == 0) return false;

        bool solved = true;
        int i = 0;
        while (solved && i < rows)
        {
            int j = 0;
            while (solved && j < columns)
            {
                solved = grid[i, j].IsChecked() == sol[i, j];
                j++;
            }
            i++;
        }

        return solved;
    }

    public void ChangeScene()
    {
        AssetPackage.TrackerAsset.Instance.setScore(starsController.getStars());
        AssetPackage.TrackerAsset.Instance.Completable.Completed("electricista_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

        int diff = uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_3_DIFICULTY");
        uAdventure.Runner.Game.Instance.GameState.SetVariable("PUZZLE_3_DIFICULTY", ++diff);

        if (diff > (int)Difficulty.Hard)
        {
            // set variables
            uAdventure.Runner.Game.Instance.GameState.SetFlag("metaPuzzle", 0);
            uAdventure.Runner.Game.Instance.RunTarget("Up");
        }
        else
        {
            uAdventure.Runner.Game.Instance.RunTarget("Minijuego3");
        }
    }
}
