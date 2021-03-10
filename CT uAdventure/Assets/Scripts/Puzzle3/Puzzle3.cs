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

    GridBox[,] grid;
    GridBox pointedBox;
    Stack<GridBox> boxesClicked = new Stack<GridBox>(), solBoxes;
    bool[,] sol;
    int[,] initialState;

    int[] gridS = { 3, 4, 3 };
    int[] solClicks = { 5, 7, 7 };
    int[] maxValues = { 2, 2, 3 };
    int[] clicksToHint = { 7, 10, 14 };
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

    void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_3_DIFICULTY");

        boxesClicked.Push(null);

        columns = gridS[(int)difficulty];
        rows = gridS[(int)difficulty];
        clicks = solClicks[(int)difficulty];
        maxValue = maxValues[(int)difficulty];

        createGrid();

        Invoke("createSol", 0.1f);
    }

    void Update()
    {
        if (finished) return;

        if (pointedBox != null && Input.GetMouseButtonDown(0))
        {
            nPasos++;

            pointedBox.boxClicked();
            Vector2Int index = pointedBox.getIndex();

            string correct = pointedBox == boxesClicked.Peek() ? "correct" : "incorrect";
            AssetPackage.TrackerAsset.Instance.GameObject.Used("grid_box_" + index.x.ToString() + "_" + index.y.ToString() + "_checked", GameObjectTracker.TrackedGameObject.GameObject);
            if (correct == "incorrect")
                for (int l = 1; l < maxValue; l++)
                    boxesClicked.Push(pointedBox);
            else
                boxesClicked.Pop();

            if (boxesClicked.Count > clicksToHint[(int)difficulty]) hintButton.interactable = true;

            //Cuando se soluciona el puzzle
            if (solved())
            {
                finishParticles.Play();
                //Invoke("changeScene", 3.0f);

                finished = true;

                // estrella de pasos minimos
                if (nPasos > nPasosMinimos + 10)
                    starsController.deactivateMinimoStar();

                starsController.gameObject.SetActive(true);

                int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");
                uAdventure.Runner.Game.Instance.GameState.SetVariable("N_STARS", nStars + starsController.getStars());
            }
        }

        checkMouse();
    }

    public void useHint()
    {
        if (boxesClicked.Count < 3) return;

        starsController.deactivateNoPistasStar();

        while (boxesClicked.Count > 4)
        {
            boxesClicked.Peek().boxClicked();
            boxesClicked.Pop();
        }

        //Cuando se soluciona el puzzle
        if (solved())
        {
            finishParticles.Play();
            //Invoke("changeScene", 3.0f);

            // estrella de pasos minimos
            if (nPasos > nPasosMinimos + 3)
                starsController.deactivateMinimoStar();

            starsController.gameObject.SetActive(true);

            int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");
            uAdventure.Runner.Game.Instance.GameState.SetVariable("N_STARS", nStars + starsController.getStars());
        }
        AssetPackage.TrackerAsset.Instance.setVar("state", getState());
        AssetPackage.TrackerAsset.Instance.GameObject.Used("hint_button", GameObjectTracker.TrackedGameObject.GameObject);

        hintButton.interactable = false;
    }


    void checkMouse()
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

    void createSol()
    {
        for (int k = 0; k < clicks; k++)
        {
            int i = Random.Range(0, rows);
            int j = Random.Range(0, columns);
            if (grid[i, j] != boxesClicked.Peek())
            {
                grid[i, j].boxClicked();
                for (int l = 1; l < maxValue; l++)
                    boxesClicked.Push(grid[i, j]);
            }
        }
        solBoxes = new Stack<GridBox>(boxesClicked);

        nPasosMinimos = clicks;

        int sum = 0;
        sol = new bool[rows, columns];
        initialState = new int[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                initialState[i, j] = grid[i, j].getValue();
                sol[i, j] = false;
                sum += grid[i, j].isChecked() ? 1 : 0;
            }
        }

        //Al menos una casilla activa
        if (sum == 0)
        {
            nPasosMinimos = 2;

            grid[0, 0].boxClicked();
            initialState[0, 0] = 1;
        }
        AssetPackage.TrackerAsset.Instance.setVar("initial_state_", getState());
        AssetPackage.TrackerAsset.Instance.Completable.Initialized("electricista_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);
        //AssetPackage.TrackerAsset.Instance.setSuccess(false);
        //AssetPackage.TrackerAsset.Instance.Completable.Progressed("Electricista_" + (int)difficulty , CompletableTracker.Completable.Level);

    }

    public string getState()
    {
        string state = "cells_active -> |Value(X Y)|: ";
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < columns; j++)
                if (grid[i, j].isChecked())
                    state += "|" + grid[i, j].getValue() + "(" + j.ToString() + " " + i.ToString() + ")| ";

        return state;
    }

    public void reset()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                while (initialState[i, j] != grid[i, j].getValue())
                    grid[i, j].checkBox();
            }
        }

        boxesClicked = new Stack<GridBox>(solBoxes);
        AssetPackage.TrackerAsset.Instance.GameObject.Used("reset_button", GameObjectTracker.TrackedGameObject.GameObject);

        hintButton.interactable = true;
    }

    public void createGrid()
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
                grid[i, j].setIndex(new Vector2Int(j, i));
                grid[i, j].setMaxValue(maxValue);
            }
        }

        createConnections();
        fitGrid(gridSize);
    }

    public void createConnections()
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
                        grid[i, j].addConection(grid[newPos.x, newPos.y]);
                        grid[newPos.x, newPos.y].addConection(grid[i, j]);
                    }
                }
            }
        }
    }

    public void fitGrid(Vector2 gridSize)
    {
        float height = Camera.main.orthographicSize * 2.0f;
        float width = height * Screen.width / Screen.height;

        float xRatio = width / gridSize.x;
        float yRatio = height / gridSize.y;
        float ratio = Mathf.Min(xRatio, yRatio);

        transform.localScale = new Vector3(ratio, ratio, 1);
        transform.position = new Vector3(-width / 2 + (gridBoxPrefab.transform.localScale.x / 2 + 0.2f) * ratio, height / 2 - (gridBoxPrefab.transform.localScale.y / 2 + 0.2f) * ratio, 0);
    }

    public bool solved()
    {
        if (sol.Length == 0) return false;

        bool solved = true;
        int i = 0;
        while (solved && i < rows)
        {
            int j = 0;
            while (solved && j < columns)
            {
                solved = grid[i, j].isChecked() == sol[i, j];
                j++;
            }
            i++;
        }

        return solved;
    }

    public void changeScene()
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
