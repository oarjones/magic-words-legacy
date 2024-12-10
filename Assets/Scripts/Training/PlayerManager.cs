using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEvents;

public class PlayerManager : MonoBehaviour
{

    public static short Lifes { get; set; } = 10;
    
    [HideInInspector]
    public static Hex InitialPlayerTile { get; set; }
    
    [HideInInspector]
    public static Hex CurrentPlayerTile { get; set; }

    [HideInInspector]
    public static List<Hex> SelectedTilesPlayer { get; set; }


    [HideInInspector]
    public static Hex InitialOpponentTile { get; set; }

    [HideInInspector]
    public static Hex CurrentOpponentTile { get; set; }

    [HideInInspector]
    public static List<Hex> SelectedTilesOpponent { get; set; }

    //public GameObject LifeBox;

    public GameObject BtnChangeLetter;

    public GameObject swirl;
    //private ToolButton toolButtonScript;
    public Map map;
    private void OnEnable()
    {
        GameEvents.OnLifeDelete += GameEvents_OnLifeDelete;
        //GameEvents.OnChageLetter += GameEvents_OnChageLetter;
    }

    private void GameEvents_OnChageLetter(char letter)
    {
        //toolButtonScript = BtnChangeLetter.GetComponent<ToolButton>();
        //toolButtonScript.OnButtonClick();
        //if (toolButtonScript.mechanism == Assets.Scripts.Data.ToolTypeMechanism.ByNumEquiped && toolButtonScript.NumberOfItemsEquiped > 0)
        //{
        //    toolButtonScript.NumberOfItemsEquiped--;
        //    BtnChangeLetter.GetComponentInChildren<TMP_Text>().text = toolButtonScript.NumberOfItemsEquiped.ToString();

        //    if (toolButtonScript.NumberOfItemsEquiped == 0)
        //    {
        //        BtnChangeLetter.GetComponentInChildren<Button>().GetComponent<Image>().sprite = Resources.Load(toolButtonScript.InactiveImageBackground, typeof(Sprite)) as Sprite;
        //        BtnChangeLetter.GetComponentInChildren<Button>().interactable = false;
        //    }
        //}
        //else { }
    }

    private void OnDisable()
    {
        GameEvents.OnLifeDelete -= GameEvents_OnLifeDelete;
        //GameEvents.OnChageLetter -= GameEvents_OnChageLetter;
    }

    private void GameEvents_OnLifeDelete()
    {
        if(Lifes > 0)
        {
            Lifes--;
            //var lifeBoxText = LifeBox.GetComponentInChildren<TMP_Text>();
            //lifeBoxText.text = Lifes.ToString();
        }
    }


    //private void GameEvents_OnRuneRewriteLetter()
    //{
    //    toolButtonScript = BtnChangeLetter.GetComponent<ToolButton>();

    //    if (toolButtonScript.NumberOfItemsEquiped > 0)
    //    {
    //        toolButtonScript.NumberOfItemsEquiped--;
    //        BtnChangeLetter.GetComponentInChildren<TMP_Text>().text = toolButtonScript.NumberOfItemsEquiped.ToString();

    //        if (toolButtonScript.NumberOfItemsEquiped == 0)
    //        {
    //            BtnChangeLetter.GetComponentInChildren<Button>().GetComponent<Image>().sprite = Resources.Load(toolButtonScript.InactiveImageBackground, typeof(Sprite)) as Sprite;
    //            BtnChangeLetter.GetComponentInChildren<Button>().interactable = false;
    //        }
    //    }
    //}

    

    // Start is called before the first frame update
    void Start()
    {
        //var lifeBoxText = LifeBox.GetComponentInChildren<TMP_Text>();
        //lifeBoxText.text = Lifes.ToString();
    }

    
}
