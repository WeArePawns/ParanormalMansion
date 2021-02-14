using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Difficulty { Easy, Medium, Hard };
public class Puzzle4 : MonoBehaviour
{
    public ParticleSystem finishParticles;

    bool finished = false;

    public Difficulty difficulty;

    int[] nSequences = { 6, 8, 10 };
    int[] nOptions = { 3, 4, 5 };
    bool[] isRandom = { false, true, true };
    bool[] isIrregular = { false, false, true };

    int numSequence;
    bool irregular;

    Stack<int> correctOrder;
    Stack<int> order;
    Stack<int> hintsOrder;

    public GameObject formaPrefab;
    public GameObject solucionPrefab;
    public GameObject container;
    public Sprite[] alternativeSprites;

    public GameObject buttonPrefab;
    public GameObject buttonContainer;

    public GameObject solutionContainer;

    public GameObject hintsContainer;

    List<GameObject> gos;

    private void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_4_DIFICULTY");

        Random.InitState(System.Environment.TickCount);

        correctOrder = new Stack<int>();
        gos = new List<GameObject>();

        numSequence = nSequences[(int)difficulty];
        int numOptions = nOptions[(int)difficulty];

        irregular = isIrregular[(int)difficulty];        
        bool random = isRandom[(int)difficulty];

        string sol = "";
        for (int i = 0; i < numSequence; i++)
        {
            int k = Random.Range(1, numOptions + 1);
            correctOrder.Push(k);
            sol = sol + k.ToString();

            GameObject go = Instantiate(solucionPrefab, solutionContainer.transform.position, Quaternion.identity, solutionContainer.transform);
            go.transform.GetChild(k - 1).gameObject.SetActive(true);

        }
        AssetPackage.TrackerAsset.Instance.GameObject.Interacted("Puzzle4Code " + sol, GameObjectTracker.TrackedGameObject.GameObject);

        uint bin = (uint)(1 << numOptions) - 1;
        int j = 0;
        while (bin != 0)
        {
            if (random) j = Random.Range(0, numOptions);
            uint iBin = (uint)1 << j;
            if ((iBin & bin) != iBin) continue;

            int index = j + 1;
            GameObject button = Instantiate(buttonPrefab, buttonContainer.transform);
            if (irregular) useAlternativeSprites(button.transform.GetChild(0).gameObject);
            button.transform.GetChild(0).transform.GetChild(j).gameObject.SetActive(true);
            button.GetComponent<Button>().onClick.AddListener(() => PressButton(index));

            bin = bin & (~iBin);
            if (!random) j++;
        }

        fillOrder();
    }

    public void useHint()
    {
        if (hintsOrder.Count <= 2) return;

        GameObject go = Instantiate(formaPrefab, hintsContainer.transform);
        if (irregular) useAlternativeSprites(go);
        go.transform.GetChild(hintsOrder.Pop() - 1).gameObject.SetActive(true);
        AssetPackage.TrackerAsset.Instance.GameObject.Used("Puzzle4HintUsed", GameObjectTracker.TrackedGameObject.GameObject);
    }

    private void useAlternativeSprites(GameObject forma)
    {
        int i = 0;
        foreach (Image image in forma.GetComponentsInChildren<Image>(true))
        {
            if (i < alternativeSprites.Length) image.sprite = alternativeSprites[i];
            i++;
        }
    }

    private void fillOrder()
    {
        order = new Stack<int>(correctOrder);
        hintsOrder = new Stack<int>(correctOrder);
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

        AssetPackage.TrackerAsset.Instance.GameObject.Used("Button " + i.ToString() + " Pressed", GameObjectTracker.TrackedGameObject.GameObject);
        GameObject go = Instantiate(formaPrefab, container.transform.position, Quaternion.identity, container.transform);
        if (irregular) useAlternativeSprites(go);
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
        AssetPackage.TrackerAsset.Instance.GameObject.Used("ResetButtonPressed", GameObjectTracker.TrackedGameObject.GameObject);
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
        int diff = uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_4_DIFICULTY");
        uAdventure.Runner.Game.Instance.GameState.SetVariable("PUZZLE_4_DIFICULTY", ++diff);

        if (diff > (int)Difficulty.Hard)
        {
            // set variables
            var element = uAdventure.Runner.Game.Instance.GameState.FindElement<uAdventure.Core.Item>("Engranaje4");
            uAdventure.Runner.InventoryManager.Instance.AddElement(element);
            uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle4", 0);
            uAdventure.Runner.Game.Instance.Talk("Oh había un engranaje en el armario, lo guardaré en la mochila", uAdventure.Core.Player.IDENTIFIER);
            uAdventure.Runner.Game.Instance.RunTarget("RoomDownRight");
        }
        else
        {
            uAdventure.Runner.Game.Instance.RunTarget("Minijuego4");
        }
    }
}
