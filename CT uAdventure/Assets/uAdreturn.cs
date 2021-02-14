using System.Collections;
using System.Collections.Generic;
using uAdventure.Core;
using uAdventure.Runner;
using UnityEngine;
using UnityEngine.EventSystems;

public class uAdreturn : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Game.Instance.Execute(new EffectHolder(new Effects()
        {
            new SpeakPlayerEffect("Naruto")
        }), _ =>
        {
            Game.Instance.GameState.SetFlag("Cosa", FlagCondition.FLAG_ACTIVE);
            Debug.Log(Game.Instance.GameState.CheckFlag("Cosa"));
            Game.Instance.RunTarget("SceneId");
        });

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
