using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle1 : MonoBehaviour
{
    public GameObject redDisc;
    public GameObject greenDisc;
    public GameObject blueDisc;

    public ParticleSystem finishParticles;

    float fixedAngle = 45.0f;

    bool finished = false;

    private void Start()
    {
        while (checkIfSuccess())
        {
            int dir = Random.Range(0, 7);
            redDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir);
            dir = Random.Range(0, 7);
            blueDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir);
            dir = Random.Range(0, 7);
            greenDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir);
        }
    }

    private void Update()
    {
        //smooth
    }

    private bool checkIfSuccess()
    {
        if (Mathf.Round(redDisc.transform.rotation.eulerAngles.z) == Mathf.Round(blueDisc.transform.rotation.eulerAngles.z) &&
            Mathf.Round(blueDisc.transform.rotation.eulerAngles.z) == Mathf.Round(greenDisc.transform.rotation.eulerAngles.z) &&
            Mathf.Round(redDisc.transform.rotation.eulerAngles.z) == Mathf.Round(greenDisc.transform.rotation.eulerAngles.z))
        {
            return true;
        }

        print("red " + redDisc.transform.rotation.eulerAngles.z);
        print("blue " + blueDisc.transform.rotation.eulerAngles.z);
        print("green " + greenDisc.transform.rotation.eulerAngles.z);

        return false;
    }

    public void TriggerFirstLever(int dir)
    {
        if (finished) return;

        redDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir);
        blueDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir * 2.0f);

        if (checkIfSuccess())
            finish();
    }

    public void TriggerSecondLever(int dir)
    {
        if (finished) return;

        greenDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir);
        blueDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir * 2.0f);
        //redDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir * 2.0f);

        if (checkIfSuccess())
            finish();
    }

    public void TriggerThirdLever(int dir)
    {
        if (finished) return;

        greenDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir);
        redDisc.transform.Rotate(0.0f, 0.0f, fixedAngle * -dir * 2.0f);

        if (checkIfSuccess())
            finish();
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
        // set variables
        uAdventure.Runner.Game.Instance.GameState.AddInventoryItem("PuzzleClue");

        uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle1", 0);
        uAdventure.Runner.Game.Instance.RunTarget("RoomDownRight");
    }
}
