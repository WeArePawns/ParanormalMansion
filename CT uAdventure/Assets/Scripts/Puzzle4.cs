using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle4 : MonoBehaviour
{
    public ParticleSystem finishParticles;

    bool finished = false;

    int numSequence = 8;
    Stack<int> correctOrder;
    Stack<int> order;

    public GameObject formaPrefab;
    public GameObject solucionPrefab;
    public GameObject container;

    public GameObject solutionContainer;

    List<GameObject> gos;

    private void Start()
    {
        Random.InitState(System.Environment.TickCount);

        correctOrder = new Stack<int>();
        gos = new List<GameObject>();

        for (int i = 0; i < numSequence; i++)
        {
            int k = Random.Range(1, 4);
            correctOrder.Push(k);

            GameObject go = Instantiate(solucionPrefab, solutionContainer.transform.position, Quaternion.identity, solutionContainer.transform);
            go.transform.GetChild(k - 1).gameObject.SetActive(true);

            //go.transform.position += new Vector3(110 * i, 200, 0);
        }

        fillOrder();
        //order = new Stack<int>(correctOrder);
    }

    void fillOrder()
    {
        order = new Stack<int>(correctOrder);
    }

    private bool checkIfSuccess()
    {
        if (order.Count == 0)
            return true;

        return false;
    }

    public void PressButton(int i)
    {
        if (finished || gos.Count >= numSequence) return;

        GameObject go = Instantiate(formaPrefab, container.transform.position, Quaternion.identity, container.transform);
        go.transform.GetChild(i - 1).gameObject.SetActive(true);
        gos.Add(go);

        if (order.Peek() == i)
            order.Pop();

        if (checkIfSuccess())
            finish();
    }

    public void resetButton()
    {
        reset();
        print("error");
    }

    void reset()
    {
        foreach (GameObject go in gos)
            Destroy(go);
        gos.Clear();

        fillOrder();
    }

    void finish()
    {
        finished = true;

        finishParticles.Play();
        print("FINISH");
        Invoke("changeScene", 3.0f);
    }

    void changeScene()
    {
        // set variables
        uAdventure.Runner.Game.Instance.GameState.AddInventoryItem("PuzzleClue");
        uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle4", 0);
        uAdventure.Runner.Game.Instance.RunTarget("RoomUpLeft");
    }
}
