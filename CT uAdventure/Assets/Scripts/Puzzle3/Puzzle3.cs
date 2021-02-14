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
    bool[,] initialState;

    int[] gridS = { 3, 4, 4 };
    int[] solClicks = { 5, 7, 10 };
    int clicks;

    //Each sol is a sizeXsize grid
    int columns;
    int rows;

    // Start is called before the first frame update
    void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_3_DIFICULTY");

        boxesClicked.Push(null);

        columns = gridS[(int)difficulty];
        rows = gridS[(int)difficulty];
        clicks = solClicks[(int)difficulty];

        createGrid();

        Invoke("createSol", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (pointedBox != null && Input.GetMouseButtonDown(0))
        {
            pointedBox.boxClicked();
            Vector2Int index = pointedBox.getIndex();
            string correct = pointedBox == boxesClicked.Peek() ? "correct" : "incorrect";
            AssetPackage.TrackerAsset.Instance.GameObject.Used(correct + " GridBox " + index.x.ToString() + " " + index.y.ToString() + " checked", GameObjectTracker.TrackedGameObject.GameObject);
            if (correct == "incorrect")
                boxesClicked.Push(pointedBox);
            else
                boxesClicked.Pop();

            //Cuando se soluciona el puzzle
            if (solved())
            {
                finishParticles.Play();
                Invoke("changeScene", 3.0f);
            }
        }

        checkMouse();
    }

    public void useHint()
    {
        if (boxesClicked.Count < 3) return;

        while (boxesClicked.Count > 4)
        {
            boxesClicked.Peek().boxClicked();
            boxesClicked.Pop();
        }

        //Cuando se soluciona el puzzle
        if (solved())
        {
            finishParticles.Play();
            Invoke("changeScene", 3.0f);
        }
        AssetPackage.TrackerAsset.Instance.GameObject.Used("MetaPuzzleHintUsed", GameObjectTracker.TrackedGameObject.GameObject);
    }


    void checkMouse()
    {
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
                boxesClicked.Push(grid[i, j]);
            }
        }
        solBoxes = new Stack<GridBox>(boxesClicked);

        int sum = 0;
        sol = new bool[rows, columns];
        initialState = new bool[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                initialState[i, j] = grid[i, j].isChecked();
                sol[i, j] = false;
                sum += grid[i, j].isChecked() ? 1 : 0;
            }
        }
        //Al menos una casilla activa
        if (sum == 0)
        {
            grid[0, 0].boxClicked();
            initialState[0, 0] = true;
        }
    }

    public void reset()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (initialState[i, j] && !grid[i, j].isChecked() || !initialState[i, j] && grid[i, j].isChecked())
                    grid[i, j].checkBox();
            }
        }

        boxesClicked = new Stack<GridBox>(solBoxes);
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
                grid[i, j].setIndex(new Vector2Int(i, j));
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

    void changeScene()
    {
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
