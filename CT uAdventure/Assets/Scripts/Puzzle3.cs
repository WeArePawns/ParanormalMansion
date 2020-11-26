using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle3 : MonoBehaviour
{
    GridBox[,] grid;
    public GameObject gridBoxPrefab;
    public GameObject solContainer;
    public GameObject solPrefab;
    public Vector2 gridOffset;
    GridBox pointedBox;
    bool[,] sol;

    public ParticleSystem finishParticles;

    //Each sol is a sizeXsize grid
    int columns;
    int rows;

    // Start is called before the first frame update
    void Start()
    {
        columns = 4;
        rows = 4;
        grid = new GridBox[rows, columns];
        Vector2 gridSize = new Vector2((gridBoxPrefab.transform.localScale.x + gridOffset.x) * columns, (gridBoxPrefab.transform.localScale.y + gridOffset.y) * rows);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 offset = new Vector3((gridBoxPrefab.transform.localScale.x + gridOffset.x) * j, -(gridBoxPrefab.transform.localScale.y + gridOffset.y) * i, 0);
                GameObject aux = Instantiate(gridBoxPrefab, transform.position + offset, Quaternion.identity, transform);
                grid[i, j] = aux.GetComponent<GridBox>();
            }
        }
        float height = Camera.main.orthographicSize * 2.0f;
        float width = height * Screen.width / Screen.height;

        float xRatio = width / gridSize.x;
        float yRatio = height / gridSize.y;
        float ratio = Mathf.Min(xRatio, yRatio);

        transform.localScale = new Vector3(ratio, ratio, 1);
        transform.position = new Vector3(-width / 2 + (gridBoxPrefab.transform.localScale.x / 2 + 0.2f) * ratio, height / 2 - (gridBoxPrefab.transform.localScale.y / 2 + 0.2f) * ratio, 0);

        createSol(out sol);
    }

    // Update is called once per frame
    void Update()
    {
        if (pointedBox != null && Input.GetMouseButtonDown(0))
            pointedBox.checkBox();

        checkMouse();

        //Cuando se soluciona el puzzle
        if (solved())
        {
            finishParticles.Play();
            Invoke("changeScene", 3.0f);
        }
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

    void createSol(out bool[,] sol)
    {
        sol = new bool[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                sol[i, j] = Random.Range(0, 2) != 0;
                GameObject go = Instantiate(solPrefab, solContainer.transform.position, Quaternion.identity, solContainer.transform);
                go.transform.GetChild(sol[i,j] ? 1 : 0).gameObject.SetActive(true);
            }
        }
    }

    public bool solved()
    {
        if(sol.Length == 0) return false;

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
        // set variables
        uAdventure.Runner.Game.Instance.GameState.SetFlag("metaPuzzle", 0);
        uAdventure.Runner.Game.Instance.RunTarget("Up");
    }
}
