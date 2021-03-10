using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsController : MonoBehaviour
{

    public GameObject minimoStar;
    public GameObject nivelCompletadoStar;
    public GameObject noPistasStar;

    public GameObject continueButton;

    public int getStars()
    {
        int nStars = 3;
        if (!noPistasStar.activeSelf)
            nStars--;
        if (!minimoStar.activeSelf)
            nStars--;

        return nStars;
    }

    public void deactivateNoPistasStar()
    {
        noPistasStar.SetActive(false);
    }

    public void deactivateMinimoStar()
    {
        minimoStar.SetActive(false);
    }
}
