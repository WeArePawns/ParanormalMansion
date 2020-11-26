using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Puzzle2 : MonoBehaviour, IPointerClickHandler
{
    private bool completed = false;

    // Card donde va el resultado
    public Card result;
    public Card objetive;
    public CardHolder[] holders;
    public List<Card> cards = new List<Card>();

    List<Vector2> holderPositions;
    List<Vector2> cardPositions;
    private List<Card> placed;

    public ParticleSystem finishParticles;

    private void Start()
    {
        holderPositions = new List<Vector2>();
        cardPositions = new List<Vector2>();
        placed = new List<Card>();

        foreach (CardHolder holder in holders)
            holderPositions.Add(holder.GetComponent<RectTransform>().anchoredPosition);
        foreach (Card card in cards)
            cardPositions.Add(card.GetComponent<RectTransform>().anchoredPosition);
    }

    private void Update()
    {
        if (!completed)
        {
            int i = 0;
            List<int> objetiveValues = objetive.GetValues();
            List<int> resultValues = result.GetValues();

            while (i < resultValues.Count && objetiveValues[i] == resultValues[i]) i++;

            //Si i esta fuera de limite, ha llegado al final es una victoria
            if (i >= resultValues.Count) 
            { 
                print("VICTORIA");
                completed = true;
                finishParticles.Play();
                Invoke("changeScene", 3.0f);

            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("Click hecho");
        List<RaycastResult> resultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultList);

        Card clickedCard = null;
        // Si encuentra CardHolder entonces se descoloca
        CardHolder holder = null;

        foreach (RaycastResult result in resultList)
        {
            if (result.gameObject.CompareTag("Card"))
            {
                print("Card encontrado");
                clickedCard = result.gameObject.GetComponent<Card>();
            }

            if (result.gameObject.CompareTag("CardHolder"))
            {
                print("Holder encontrado");
                holder = result.gameObject.GetComponent<CardHolder>();
            }
        }

        if (holder != null && clickedCard != null)
        {
            print("Descolocar");
            DispatchCard(clickedCard);

        }
        else if (holder == null && clickedCard != null)
        {
            print("Colocar");
            PlaceCard(clickedCard);

        }
        else
            print("Clickado fuera de objetos interactuables");

        // Actualizamos el resultado
        UpdateResult();
    }

    private void UpdateResult()
    {
        result.ResetValues();
        
        foreach(Card card in placed)
        {
            result.AddValues(card.GetValues());
        }
    }

    private void PlaceCard(in Card card)
    {
        if(placed.Count >= holders.Length)
        {
            print("Ha habido un problema, no se puede colocar una carta, esta todo lleno");
            return;
        }

        //Colocamos la carta
        card.GetComponent<RectTransform>().anchoredPosition = holderPositions[placed.Count];
        placed.Add(card);

        // Actualizar resultado
        UpdateResult();

    }

    private void DispatchCard(in Card card)
    {
        // Encontrar el indice y devolverlo a la posicion
        int indexPlaced = placed.IndexOf(card);
        int indexCard = cards.IndexOf(card);

        if (indexPlaced >= cards.Count || indexPlaced < 0) 
        { 
            print("Ha habido un problema, indice fuera de limites");
            return;
        }

        // Posicionamos la tarjeta
        card.GetComponent<RectTransform>().anchoredPosition = cardPositions[indexCard];
        placed.RemoveAt(indexPlaced);

        // Recolocar el resto (poco eficiente, descoloca y coloca todo otra vez)
        List<Card> auxCard = new List<Card>(placed);
        placed.Clear();
        for(int i = 0; i < auxCard.Count; i++)
        {
            PlaceCard(auxCard[i]);
        }
    }

    void changeScene()
    {
        // set variables
        uAdventure.Runner.Game.Instance.GameState.AddInventoryItem("PuzzleClue");
        uAdventure.Runner.Game.Instance.GameState.SetFlag("puzzle2", 0);
        uAdventure.Runner.Game.Instance.RunTarget("RoomDownLeft");
    }

}
