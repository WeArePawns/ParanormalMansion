using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class End : MonoBehaviour
{
    public Text starsText;

    private void Start()
    {
        int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");

        starsText.text = nStars.ToString() + "/45";
    }

    public void SalirJuego()
    {
        //uAdventure.Runner.Game.
    }
}
