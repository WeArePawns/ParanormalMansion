using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Puzzle2 : MonoBehaviour, IPointerClickHandler
{
    private bool completed = false;

    // Card donde va el resultado
    public Card result;
    public Card objective;
    public GameObject holderPrefab;
    public GameObject holdersContainer;
    public List<Card> cards = new List<Card>();
    public Difficulty difficulty;

    int[] nResultCards = { 2, 3, 3 };
    int[] nBars = { 4, 5, 6 };
    int nValues, nResult, hintsToUse = 1;

    List<Vector2> holderPositions;
    List<Vector2> cardPositions;
    private CardHolder[] holders;
    private List<Card> placed;
    private List<Card> solutionCards = new List<Card>();

    public ParticleSystem finishParticles;
    public Button hintButton;

    public StarsController starsController;
    private int nPasos = 0;
    private int nPasosMinimos = 3;

    private void Start()
    {
        difficulty = (Difficulty)uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_2_DIFICULTY");

        nValues = nBars[(int)difficulty];
        nResult = nResultCards[(int)difficulty];

        createSolution();
        createResult();
        createOptions();

        AssetPackage.TrackerAsset.Instance.setVar("initial_state", GetState());
        AssetPackage.TrackerAsset.Instance.Completable.Initialized("hacking_etico_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

        holderPositions = new List<Vector2>();
        cardPositions = new List<Vector2>();
        placed = new List<Card>();
        holders = new CardHolder[nResult];
        for (int i = 0; i < nResult; i++)
            holders[i] = Instantiate(holderPrefab, holdersContainer.transform).GetComponent<CardHolder>();

        LayoutRebuilder.ForceRebuildLayoutImmediate(holdersContainer.GetComponent<RectTransform>());
        foreach (CardHolder holder in holders)
            holderPositions.Add(holder.GetComponent<RectTransform>().position);
        foreach (Card card in cards)
            cardPositions.Add(card.GetComponent<RectTransform>().position);
    }

    public string GetState()
    {
        string state = "";
        for (int i = 0; i < cards.Count; i++)
        {
            Card c = cards[i];
            state += "\ncard_" + i.ToString()+"_" + c.GetState() + ((solutionCards.Contains(c)) ? "_correct" : "_incorrect");
        }
        state += "\nsolution_" + objective.GetState();
        return state;
    }

    public void useHint()
    {
        if (hintsToUse == 0) return;

        starsController.deactivateNoPistasStar();

        int hintIndex = 0;
        //Si hay cartas colocadas
        if (placed.Count > 0)
        {
            //Elegimos una solucion que no este colocada
            int i = 0;
            while (hintIndex >= 0 && i < solutionCards.Count)
                hintIndex = placed.IndexOf(solutionCards[i++]);
            hintIndex = i - 1;

            //Descolocamos
            int j = 0;
            bool free = false;
            while (j < placed.Count && !free)
            {
                int k = 0;
                free = true;
                while (free && k < solutionCards.Count)
                    free = placed[j] != solutionCards[k++];
                j++;
            }
            if (!free)
                DispatchCard(placed[j - 1]);
        }
        PlaceCard(solutionCards[hintIndex]);
        hintsToUse--;

        AssetPackage.TrackerAsset.Instance.setVar("card", hintIndex.ToString());
        AssetPackage.TrackerAsset.Instance.GameObject.Used("hint_button", GameObjectTracker.TrackedGameObject.GameObject);

        if (hintsToUse == 0) hintButton.interactable = false;
    }

    private void createSolution()
    {
        List<int> values = new List<int>();
        for (int i = 0; i < nValues; i++)
            values.Add(Random.Range(-2, 3));
        objective.SetValues(values);
    }

    private void createResult()
    {
        List<int> values = new List<int>();
        for (int i = 0; i < nValues; i++)
            values.Add(0);
        result.SetValues(values);
    }

    private void createOptions()
    {
        List<int> lastValue = new List<int>(objective.GetValues());
        List<int>[] resultValues = new List<int>[nResult];
        for (int i = 0; i < nResult - 1; i++)
        {
            List<int> res = new List<int>(lastValue);
            for (int j = 0; j < res.Count; j++)
            {
                int offset = Random.Range(-2, 3);
                res[j] += offset;
                lastValue[j] = -offset;
            }
            resultValues[i] = res;
        }
        resultValues[nResult - 1] = lastValue;

        int n = nResult;
        for (int i = 0; i < cards.Count; i++)
        {
            bool isResult = n > 0 && (Random.Range(0, 2) == 1 || i >= cards.Count - 1 - n);
            if (isResult)
            {
                cards[i].SetValues(resultValues[--n]);
                solutionCards.Add(cards[i]);
            }
            else
            {
                List<int> values = new List<int>();
                for (int j = 0; j < nValues; j++)
                    values.Add(Random.Range(-2, 3));
                cards[i].SetValues(values);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        List<RaycastResult> resultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultList);

        Card clickedCard = null;
        // Si encuentra CardHolder entonces se descoloca
        CardHolder holder = null;

        foreach (RaycastResult raycastResult in resultList)
        {
            if (raycastResult.gameObject.CompareTag("Card"))
            {
                clickedCard = raycastResult.gameObject.GetComponent<Card>();
                if (clickedCard == result || clickedCard == objective) clickedCard = null;
            }

            if (raycastResult.gameObject.CompareTag("CardHolder"))
                holder = raycastResult.gameObject.GetComponent<CardHolder>();
        }

        if (holder != null && clickedCard != null)
            DispatchCard(clickedCard, true);
        else if (holder == null && clickedCard != null)
            PlaceCard(clickedCard, true);
        else
            print("Clickado fuera de objetos interactuables");

        // Actualizamos el resultado
        UpdateResult();
    }

    private void checkCompleted()
    {
        if (!completed)
        {
            int i = 0;
            List<int> objectiveValues = objective.GetValues();
            if (objectiveValues.Count == 0) return;

            List<int> resultValues = result.GetValues();

            while (i < resultValues.Count && objectiveValues[i] == resultValues[i]) i++;

            //Si i esta fuera de limite, ha llegado al final es una victoria
            if (i >= resultValues.Count)
            {
                completed = true;
                finishParticles.Play();

                // estrella de pasos minimos
                if (nPasos > nPasosMinimos)
                    starsController.deactivateMinimoStar();

                starsController.gameObject.SetActive(true);

                int nStars = uAdventure.Runner.Game.Instance.GameState.GetVariable("N_STARS");
                uAdventure.Runner.Game.Instance.GameState.SetVariable("N_STARS", nStars + starsController.getStars());
            }
        }
    }

    private void UpdateResult()
    {
        result.ResetValues();

        foreach (Card card in placed)
        {
            result.AddValues(card.GetValues());
        }
    }

    private void PlaceCard(in Card card, bool trace = false)
    {
        if (placed.Count >= holders.Length)
        {
            print("Ha habido un problema, no se puede colocar una carta, esta todo lleno");
            return;
        }

        //Colocamos la carta
        card.GetComponent<RectTransform>().position = holderPositions[placed.Count];
        placed.Add(card);

        // Actualizar resultado
        UpdateResult();

        if (trace)
        {
            AssetPackage.TrackerAsset.Instance.setVar("correct", solutionCards.Contains(card));
            AssetPackage.TrackerAsset.Instance.GameObject.Used("card_" + cards.IndexOf(card).ToString() + "_placed", GameObjectTracker.TrackedGameObject.GameObject);
        }

        //Se comprueba si se ha llegado a la solucion
        checkCompleted();
        if (placed.Count >= holders.Length && !completed)
        {
            AssetPackage.TrackerAsset.Instance.setSuccess(false);
            AssetPackage.TrackerAsset.Instance.Completable.Progressed("hacking_etico_" + (int)(difficulty + 1), 0);
            nPasos++;
        }
    }

    private void DispatchCard(in Card card, bool trace = false)
    {
        // Encontrar el indice y devolverlo a la posicion
        int indexPlaced = placed.IndexOf(card);
        int indexCard = cards.IndexOf(card);

        if (indexPlaced >= cards.Count || indexPlaced < 0)
            return;

        // Posicionamos la tarjeta
        card.GetComponent<RectTransform>().position = cardPositions[indexCard];
        placed.RemoveAt(indexPlaced);

        if (trace)
        {
            AssetPackage.TrackerAsset.Instance.setVar("isSolution", solutionCards.Contains(card));
            AssetPackage.TrackerAsset.Instance.GameObject.Used("card_" + cards.IndexOf(card).ToString() + "_dispatched", GameObjectTracker.TrackedGameObject.GameObject);
        }
        // Recolocar el resto (poco eficiente, descoloca y coloca todo otra vez)
        List<Card> auxCard = new List<Card>(placed);
        placed.Clear();
        for (int i = 0; i < auxCard.Count; i++)
            PlaceCard(auxCard[i]);
    }

    public void changeScene()
    {
        AssetPackage.TrackerAsset.Instance.setScore(starsController.getStars());
        AssetPackage.TrackerAsset.Instance.Completable.Completed("hacking_etico_" + (int)(difficulty + 1), CompletableTracker.Completable.Level);

        int diff = uAdventure.Runner.Game.Instance.GameState.GetVariable("PUZZLE_2_DIFICULTY");
        uAdventure.Runner.Game.Instance.GameState.SetVariable("PUZZLE_2_DIFICULTY", ++diff);

        if (diff > (int)Difficulty.Hard)
        {
            // set variables
            var element = uAdventure.Runner.Game.Instance.GameState.FindElement<uAdventure.Core.Item>("Engranaje2");
            uAdventure.Runner.InventoryManager.Instance.AddElement(element);

            uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle2", 0);
            uAdventure.Runner.Game.Instance.Talk("Oh un engranaje ha aparecido en la mesa, lo guardaré en la mochila", uAdventure.Core.Player.IDENTIFIER);
            uAdventure.Runner.Game.Instance.RunTarget("RoomUpRight");
        }
        else
        {
            uAdventure.Runner.Game.Instance.RunTarget("Minijuego2");
        }
    }

}
