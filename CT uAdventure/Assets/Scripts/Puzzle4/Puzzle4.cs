using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

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
    List<Color> shapesColors;

    public Button hintButton;

    public StarsController starsController;
    private int nPasos;
    private int nPasosMinimos = 2;

    private void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_4_DIFICULTY");

        UnityEngine.Random.InitState(System.Environment.TickCount);

        correctOrder = new Stack<int>();
        gos = new List<GameObject>();
        shapesColors = new List<Color>() { Color.red, Color.blue, Color.green, Color.cyan, Color.yellow };
        shapesColors = shapesColors.OrderBy(x => Guid.NewGuid()).ToList();

        numSequence = nSequences[(int)difficulty];
        int numOptions = nOptions[(int)difficulty];

        irregular = isIrregular[(int)difficulty];
        bool random = isRandom[(int)difficulty];

        string sol = "";
        for (int i = 0; i < numSequence; i++)
        {
            int k = UnityEngine.Random.Range(1, numOptions + 1);
            correctOrder.Push(k);
            sol += k.ToString() + ((i == numSequence - 1) ? "" : " ");

            GameObject go = Instantiate(solucionPrefab, solutionContainer.transform.position, Quaternion.identity, solutionContainer.transform);
            go.transform.GetChild(k - 1).gameObject.SetActive(true);

        }
        AssetPackage.TrackerAsset.Instance.setVar("initial_state", sol);
        AssetPackage.TrackerAsset.Instance.Completable.Initialized("formas_colores_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

        uint bin = (uint)(1 << numOptions) - 1;
        int j = 0;
        while (bin != 0)
        {
            if (random) j = UnityEngine.Random.Range(0, numOptions);
            uint iBin = (uint)1 << j;
            if ((iBin & bin) != iBin) continue;

            int index = j + 1;
            GameObject button = Instantiate(buttonPrefab, buttonContainer.transform);
            if (irregular) useAlternativeSprites(button.transform.GetChild(0).gameObject);
            button.transform.GetChild(0).transform.GetChild(j).gameObject.SetActive(true);
            button.GetComponent<Button>().onClick.AddListener(() => PressButton(index));
            button.transform.GetChild(0).transform.GetChild(j).GetComponent<Image>().color = shapesColors[j];

            bin = bin & (~iBin);
            if (!random) j++;
        }

        fillOrder();
    }

    public void useHint()
    {
        if (hintsOrder.Count <= 2) return;

        starsController.deactivateNoPistasStar();

        GameObject go = Instantiate(formaPrefab, hintsContainer.transform);
        if (irregular) useAlternativeSprites(go);
        int index = hintsOrder.Pop() - 1;
        go.transform.GetChild(index).GetComponent<Image>().color = shapesColors[index];
        go.transform.GetChild(index).gameObject.SetActive(true);

        AssetPackage.TrackerAsset.Instance.GameObject.Used("hint_button", GameObjectTracker.TrackedGameObject.GameObject);

        if (hintsOrder.Count <= 2) hintButton.interactable = false;
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

        AssetPackage.TrackerAsset.Instance.GameObject.Used("button_" + i.ToString(), GameObjectTracker.TrackedGameObject.GameObject);
        GameObject go = Instantiate(formaPrefab, container.transform.position, Quaternion.identity, container.transform);
        if (irregular) useAlternativeSprites(go);
        go.transform.GetChild(i - 1).gameObject.SetActive(true);
        go.transform.GetChild(i - 1).GetComponent<Image>().color = shapesColors[i - 1];
        gos.Add(go);

        if (order.Peek() == i)
            order.Pop();

        if (checkIfSuccess())
            finish();
        else if (gos.Count >= numSequence)
        {
            reset();
            AssetPackage.TrackerAsset.Instance.setSuccess(false);
            AssetPackage.TrackerAsset.Instance.Completable.Progressed("formas_colores_" + (int)(difficulty + 1), 0);
        }
    }

    public void resetButton()
    {
        reset();
        AssetPackage.TrackerAsset.Instance.GameObject.Used("reset_button", GameObjectTracker.TrackedGameObject.GameObject);
    }

    void reset()
    {
        nPasos++;
        foreach (GameObject go in gos)
            Destroy(go);
        gos.Clear();

        fillOrder();
    }

    void finish()
    {
        finished = true;
        finishParticles.Play();

        // estrella de pasos minimos
        if (nPasos > nPasosMinimos)
            starsController.deactivateMinimoStar();

        starsController.gameObject.SetActive(true);

        int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");
        uAdventure.Runner.Game.Instance.GameState.SetVariable("N_STARS", nStars + starsController.getStars());
    }

    public void changeScene()
    {
        AssetPackage.TrackerAsset.Instance.setScore(starsController.getStars());
        AssetPackage.TrackerAsset.Instance.Completable.Completed("formas_colores_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

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
