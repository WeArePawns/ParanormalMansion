using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle1 : MonoBehaviour
{
    public GameObject discPrefab;
    public GameObject discContainer;

    public GameObject leverPrefab;
    public GameObject UpLeverContainer;
    public GameObject DownLeverContainer;

    public GameObject hintPrefab;
    public GameObject hintContainer;

    public Difficulty difficulty;
    public ParticleSystem finishParticles;

    int[] numDiscs = { 2, 3, 4 };
    int nDiscs;
    GameObject[] discs;

    Text[] hints;
    int[] leverTurns;
    int lastHint = 0;

    float fixedAngle = 45.0f;

    public float timeToTurn = 0.5f;
    private Coroutine coroutine;

    bool finished = false;

    private void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_1_DIFICULTY");

        nDiscs = numDiscs[(int)difficulty];
        discs = new GameObject[nDiscs];
        hints = new Text[nDiscs];
        leverTurns = new int[nDiscs];
        initPuzzle();

        while (checkIfSuccess())
        {
            int nTurns, dir;
            for (int i = 0; i < nDiscs; i++)
            {
                leverTurns[i] = 0;
                nTurns = Random.Range(0, 7);
                for (int j = 0; j < nTurns; j++)
                {
                    dir = Random.Range(0, 1) == 0 ? -1 : 1;
                    RotateLever(i, dir);
                    addTurnToHint(i, dir);
                }
                updateHintText(i);
            }
        }
        AssetPackage.TrackerAsset.Instance.GameObject.Used("INITIAL STATE:" + getState(), GameObjectTracker.TrackedGameObject.GameObject); ;
    }

    private void addTurnToHint(int i, int dir)
    {
        leverTurns[i] -= dir;
        leverTurns[i] = leverTurns[i] % 8;
        leverTurns[i] = (Mathf.Abs(leverTurns[i]) < 8 - Mathf.Abs(leverTurns[i])) ? leverTurns[i] : leverTurns[i] - (8 * (int)Mathf.Sign(leverTurns[i]));
        updateHintText(i);
    }

    private void updateHintText(int i)
    {
        string anilla = leverTurns[i] == 0 ? "" : (leverTurns[i] < 0 ? " a la izquierda" : " a la derecha");
        string vez = Mathf.Abs(leverTurns[i]) == 1 ? "vez" : "veces";
        hints[i].text = "Gira la anilla " + (i + 1).ToString() + " " + (Mathf.Abs(leverTurns[i])).ToString() + " " + vez + anilla;
    }

    private void initPuzzle()
    {
        for (int i = 0; i < nDiscs; i++)
        {
            //Discos
            discs[i] = Instantiate(discPrefab, discContainer.transform);
            discs[i].transform.localScale = discs[i].transform.localScale + new Vector3(0.5f * i, 0.5f * i, 0);
            discs[i].transform.position = discs[i].transform.position + new Vector3(0, 0, i);
            discs[i].transform.GetChild(i).gameObject.SetActive(true);

            //Palancas
            int index = i;
            GameObject leverUp = Instantiate(leverPrefab, UpLeverContainer.transform);
            leverUp.GetComponent<Button>().onClick.AddListener(() => triggerLever(index, 1));

            GameObject leverDown = Instantiate(leverPrefab, DownLeverContainer.transform);
            leverDown.GetComponent<Button>().onClick.AddListener(() => triggerLever(index, -1));
            leverDown.transform.GetChild(0).Rotate(new Vector3(0, 180, 0), Space.Self);

            //Pistas
            hints[i] = Instantiate(hintPrefab, hintContainer.transform).GetComponent<Text>();
            hints[i].gameObject.SetActive(false);
            leverTurns[i] = 0;
        }
    }

    private bool checkIfSuccess()
    {
        int i = 1;
        bool cont = true;
        double zRot = Mathf.Round(discs[0].transform.rotation.eulerAngles.z);

        while (cont && i < nDiscs)
        {
            cont = zRot == Mathf.Round(discs[i].transform.rotation.eulerAngles.z);
            if (cont) i++;
        }

        return i >= nDiscs;
    }

    public void triggerLever(int iniDisc, int dir, bool checkFinish = true)
    {
        if (finished) return;
        if (coroutine != null) return;

        coroutine = StartCoroutine(Rotate(iniDisc, dir, checkFinish));
        addTurnToHint(iniDisc, dir);

        string direction = dir == -1 ? "Left" : "Right";
        AssetPackage.TrackerAsset.Instance.GameObject.Used(direction + " Lever " + iniDisc.ToString() + " Used", GameObjectTracker.TrackedGameObject.GameObject);
    }

    public void showHint()
    {
        if (lastHint >= hints.Length - 1) return;

        hints[lastHint].gameObject.SetActive(true);
        lastHint++;
        AssetPackage.TrackerAsset.Instance.GameObject.Used("Puzzle 1 Hint Used", GameObjectTracker.TrackedGameObject.GameObject);
    }

    private void RotateLever(int discIndex, int direction)
    {
        Vector3 dAngle1 = discs[discIndex].transform.eulerAngles;
        Vector3 dAngle2 = discs[(discIndex + 1) % nDiscs].transform.eulerAngles;
        discs[discIndex].transform.eulerAngles = new Vector3(dAngle1.x, dAngle1.y, dAngle1.z + fixedAngle * -direction);
        discs[(discIndex + 1) % nDiscs].transform.eulerAngles = new Vector3(dAngle2.x, dAngle2.y, dAngle2.z + fixedAngle * -direction * 2.0f);
    }

    private string getState()
    {
        string state = "";

        for (int i = 0; i < discs.Length; i++)
        {
            int rot = (int)discs[i].transform.eulerAngles.z;
            state += "\nDisc " + (i + 1).ToString() + " rotated " + rot.ToString() + "º anti-clockwise";
        }

        return state;
    }

    private IEnumerator Rotate(int discIndex, int direction, bool checkFinish = true)
    {
        float timer = 0.0f;
        Vector3 dAngle1 = discs[discIndex].transform.eulerAngles;
        Vector3 dAngle2 = discs[(discIndex + 1) % nDiscs].transform.eulerAngles;

        while (timer < timeToTurn)
        {
            timer += Time.deltaTime;
            float lerpAngle1 = Mathf.Lerp(dAngle1.z, dAngle1.z + fixedAngle * -direction, timer / timeToTurn);
            float lerpAngle2 = Mathf.Lerp(dAngle2.z, dAngle2.z + fixedAngle * -direction * 2.0f, timer / timeToTurn);

            discs[discIndex].transform.eulerAngles = new Vector3(dAngle1.x, dAngle1.y, lerpAngle1);
            discs[(discIndex + 1) % nDiscs].transform.eulerAngles = new Vector3(dAngle2.x, dAngle2.y, lerpAngle2);

            yield return null;
        }

        discs[discIndex].transform.eulerAngles = new Vector3(dAngle1.x, dAngle1.y, dAngle1.z + fixedAngle * -direction);
        discs[(discIndex + 1) % nDiscs].transform.eulerAngles = new Vector3(dAngle2.x, dAngle2.y, dAngle2.z + fixedAngle * -direction * 2.0f);

        if (checkIfSuccess() && checkFinish)
            finish();

        coroutine = null;
    }

    void finish()
    {
        finished = true;

        //AudioManager.instance.Play("Success");

        finishParticles.Play();
        print("FINISH");
        Invoke("changeScene", 3.0f);

    }

    void changeScene()
    {
        int diff = uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_1_DIFICULTY");
        uAdventure.Runner.Game.Instance.GameState.SetVariable("PUZZLE_1_DIFICULTY", ++diff);

        if (diff > (int)Difficulty.Hard)
        {
            // set variables
            var element = uAdventure.Runner.Game.Instance.GameState.FindElement<uAdventure.Core.Item>("Engranaje1");
            uAdventure.Runner.InventoryManager.Instance.AddElement(element);

            uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle1", 0);
            uAdventure.Runner.Game.Instance.Talk("Oh había un engranaje en la caja fuerte, lo guardare en la mochila", uAdventure.Core.Player.IDENTIFIER);
            uAdventure.Runner.Game.Instance.RunTarget("RoomUpLeft");
        }
        else
        {
            uAdventure.Runner.Game.Instance.RunTarget("Minijuego1");
        }
    }
}
