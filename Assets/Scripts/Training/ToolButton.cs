using Assets.Scripts.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolButton : MonoBehaviour
{

    public Map CurrentMap = null;
    public Button myButton;
    //public MultiplayerMap CurrentMap = null;
    public int NumberOfItemsEquiped = 2;

    public string ActiveImageBackground;
    public string InactiveImageBackground;
    public GameAidToolModeType mechanism = GameAidToolModeType.ByNumEquiped;
    public GameAidTool aid = GameAidTool.ChangeCurrentLetter;
    public GameActor actor = GameActor.Player;

    public float cooldownTime = 30f; // Tiempo total de cooldown
    public float remainingTimePlayer;    // Tiempo restante de cooldown
    public float remainingTimeOpponent;    // Tiempo restante de cooldown
    public bool isCooldown;        // Estado de cooldown
    public Text cooldownText;       // Texto del contador de tiempo
    public Image mask;              // Máscara alpha del botón

    //public bool IsLoading = false;

    private void OnEnable()
    {

        //GameEvents.OnChageLetter += GameEvents_OnChageLetter;
        //GameEvents.OnSetTileFreezeTrap += GameEvents_OnSetTileFreezeTrap;
    }

    

    private void OnDestroy()
    {
        //GameEvents.OnChageLetter -= GameEvents_OnChageLetter;
        //GameEvents.OnSetTileFreezeTrap -= GameEvents_OnSetTileFreezeTrap;
    }

    public void ResetButton()
    {
        if (myButton == null)
            myButton = this.GetComponentInChildren<Button>();

        myButton.interactable = true;

        if (mechanism == GameAidToolModeType.ByNumEquiped || mechanism == GameAidToolModeType.Mixed)
        {
            //NumberOfItemsEquiped = 2;
            this.GetComponentInChildren<TMP_Text>().text = NumberOfItemsEquiped.ToString();
            this.GetComponentInChildren<TMP_Text>().enabled = true;

        }
        else if (mechanism == GameAidToolModeType.ByTime)
        {
            this.GetComponentInChildren<TMP_Text>().enabled = false;
            mask.fillAmount = 1;
            mask.enabled = false;
            isCooldown = false;
            myButton.interactable = true;
            mask.fillAmount = 1;
            cooldownText.text = "";
            cooldownText.enabled = false;
            remainingTimePlayer = cooldownTime;
            remainingTimeOpponent = cooldownTime;
        }

        switch(aid)
        {
            case GameAidTool.ChangeCurrentLetter:
                myButton.GetComponent<Image>().sprite = Resources.Load("Art/UI/buttons/changeletter", typeof(Sprite)) as Sprite;
                break;

            case GameAidTool.FreezeTrap:
                myButton.GetComponent<Image>().sprite = Resources.Load("Art/UI/buttons/freezetrap", typeof(Sprite)) as Sprite;
                break;

            case GameAidTool.MoveToTile:
                myButton.GetComponent<Image>().sprite = Resources.Load("Art/UI/buttons/movetotile", typeof(Sprite)) as Sprite;
                break;

            case GameAidTool.locktile:
                myButton.GetComponent<Image>().sprite = Resources.Load("Art/UI/buttons/locktile", typeof(Sprite)) as Sprite;
                break;
        }
        

    }

    // Start is called before the first frame update
    void Start()
    {
        if (myButton == null)
            myButton = this.GetComponentInChildren<Button>();

        mask.enabled = false;
        cooldownText.enabled = false;

        if (mechanism == GameAidToolModeType.ByNumEquiped || mechanism == GameAidToolModeType.Mixed)
        {
            this.GetComponentInChildren<TMP_Text>().text = NumberOfItemsEquiped.ToString();
            this.GetComponentInChildren<TMP_Text>().enabled = true;

        }
        else if (mechanism == GameAidToolModeType.ByTime)
        {
            this.GetComponentInChildren<TMP_Text>().enabled = false;
            mask.fillAmount = 1;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isCooldown)
        {
            // Actualizar el tiempo restante
            if (actor == GameActor.Player)
                remainingTimePlayer -= Time.deltaTime;
            else
                remainingTimeOpponent -= Time.deltaTime;


            UpdateMaskAndText();

            // Verificar si el cooldown ha terminado
            if (actor == GameActor.Player && remainingTimePlayer <= 0)
            {
                mask.enabled = false;
                isCooldown = false;
                myButton.interactable = true;
                mask.fillAmount = 1;
                cooldownText.text = "";
                cooldownText.enabled = false;
            }

            // Verificar si el cooldown ha terminado
            if (actor == GameActor.Opponent && remainingTimeOpponent <= 0)
            {
                mask.enabled = false;
                isCooldown = false;
                myButton.interactable = true;
                mask.fillAmount = 1;
                cooldownText.text = "";
                cooldownText.enabled = false;
            }
        }
    }



    //private void GameEvents_OnSetTileFreezeTrap(GameActor actor)
    //{
    //    throw new NotImplementedException();

    //}


    public void OnUseTool(char letter, GameActor actor)
    {
        var pruebas = this.name;
        //if (actor == GameActor.Player)
        //{
        if ((mechanism == Assets.Scripts.Data.GameAidToolModeType.ByNumEquiped || mechanism == Assets.Scripts.Data.GameAidToolModeType.Mixed)
        && NumberOfItemsEquiped >= 0)
        {
            if (NumberOfItemsEquiped > 0)
            {
                NumberOfItemsEquiped--;
                this.GetComponentInChildren<TMP_Text>().text = NumberOfItemsEquiped.ToString();
            }

            if (NumberOfItemsEquiped == 0)
            {
                if (mechanism == Assets.Scripts.Data.GameAidToolModeType.ByNumEquiped)
                {
                    //myButton.GetComponent<Image>().sprite = Resources.Load(InactiveImageBackground, typeof(Sprite)) as Sprite;
                    myButton.interactable = false;
                }
                else if (mechanism == Assets.Scripts.Data.GameAidToolModeType.Mixed)
                {
                    mechanism = GameAidToolModeType.ByTime;

                    this.GetComponentInChildren<TMP_Text>().enabled = false;
                    mask.enabled = true;
                    mask.fillAmount = 1;
                    isCooldown = true;

                    if (actor == GameActor.Player)
                        remainingTimePlayer = cooldownTime;
                    else
                        remainingTimeOpponent = cooldownTime;

                    myButton.interactable = false;
                    cooldownText.enabled = true;
                }
            }

        }
        else if (mechanism == GameAidToolModeType.ByTime)
        {
            // Iniciar el cooldown
            mask.enabled = true;
            mask.fillAmount = 1;
            isCooldown = true;

            if (actor == GameActor.Player)
                remainingTimePlayer = cooldownTime;
            else
                remainingTimeOpponent = cooldownTime;

            myButton.interactable = false;
            cooldownText.enabled = true;
        }
        //}

    }

    public void OnButtonClick()
    {
        if (aid == GameAidTool.ChangeCurrentLetter && actor == GameActor.Player)
        {
            GameEvents.FireRuneRewriteLetterEvent();
        }


        if (aid == GameAidTool.FreezeTrap && actor == GameActor.Player)
        {
            if (!CurrentMap.isSettingFreezeTrap)
            {
                GameEvents.FireFreezeTrapEvent();
            }
            else
            {
                CurrentMap.isSettingFreezeTrap = false;
            }
        }


        if (aid == GameAidTool.MoveToTile && actor == GameActor.Player)
        {
            if (!CurrentMap.isMovingToTile)
            {
                GameEvents.FireMoveToTileEvent();
            }
            else
            {
                CurrentMap.isMovingToTile = false;
            }
        }


    }

    void UpdateMaskAndText()
    {
        if(actor == GameActor.Player)
        {
            // Calcular el porcentaje del cooldown restante
            float fillAmount = remainingTimePlayer / cooldownTime;
            mask.fillAmount = 0 + fillAmount;

            // Actualizar el texto del contador
            cooldownText.text = Mathf.Ceil(remainingTimePlayer).ToString();
        }
        else
        {
            // Calcular el porcentaje del cooldown restante
            float fillAmount = remainingTimeOpponent / cooldownTime;
            mask.fillAmount = 0 + fillAmount;

            // Actualizar el texto del contador
            cooldownText.text = Mathf.Ceil(remainingTimeOpponent).ToString();
        }
        

    }
}
