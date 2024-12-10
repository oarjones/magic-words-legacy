using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;



public class InfiniteScroll : MonoBehaviour
{
    private Language currentLanguage;
    public GameObject letterButtonPrefab;
    public GameObject backgroundPanel;
    //public ScrollRect scrollRect;
    public RectTransform viewportTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup HLG;
    public Button frameButton; // El marco que actúa como botón
    public Map map;
    //public GameObject scrollView;
    public GameObject runeRewriteButton;

    [HideInInspector]
    public static bool runeRewiteActive = false;
    bool canUseMouse;

    private Outline frameOutline;
    public static Transform letterTransformInside = null;

    private bool lettersCreated = false;

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        if (!lettersCreated)
        {
            currentLanguage = GetCurrentLanguage();
            frameOutline = frameButton.GetComponent<Outline>();

            float totalWidth = 0;
            foreach (var letter in currentLanguage.Abecedary)
            {
                GameObject rrLetter = Instantiate(letterButtonPrefab, contentPanelTransform);
                var buttonText = rrLetter.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = letter.Letter.ToString();
                //itemList.Add(rrLetter);

                totalWidth += rrLetter.GetComponent<RectTransform>().rect.width + 5f;
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject RT = Instantiate(letterButtonPrefab, contentPanelTransform);
                RT.GetComponentInChildren<TextMeshProUGUI>().text = "";
                RT.GetComponent<RectTransform>().SetAsLastSibling();
            }

            for (int i = 0; i < 4; i++)
            {
                GameObject RT = Instantiate(letterButtonPrefab, contentPanelTransform);
                RT.GetComponentInChildren<TextMeshProUGUI>().text = "";
                RT.GetComponent<RectTransform>().SetAsFirstSibling();
            }

            var pruebas = contentPanelTransform.transform.position.x;
            lettersCreated = true;
        }

    }

    private void OnDestroy()
    {
        Debug.Log("InfiniteScroll destroyed!");
    }

    
    public void InitializeRunerewrite()
    {
        
        this.gameObject.SetActive(false);
        frameButton.interactable = false; // Desactivamos el marco al inicio
        canUseMouse = Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer && Input.mousePresent;
        frameOutline = frameButton.GetComponent<Outline>();
        frameOutline.enabled = false;
        backgroundPanel.SetActive(false);
        runeRewiteActive = false;
        
    }

    private void Update()
    {

        if (runeRewiteActive)
        {
            CheckCloseRunerewrite();
            CheckLetterInFrame();
        }
    }


    private void CheckCloseRunerewrite()
    {
        if (canUseMouse)
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerRunerewriteUIObject())
            {
                CloseRunerewite();
            }
        }
        else
        {
            // Manejo de toques en pantalla
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && !IsPointerOverRunerewriteUIObject(touch.position))
                {
                    CloseRunerewite();
                }
            }
        }
    }

    public bool IsPointerOverRunerewriteUIObject(Vector2 position)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0 && results.Any(c => c.gameObject == this.gameObject || c.gameObject == runeRewriteButton);
    }

    public bool IsPointerRunerewriteUIObject()
    {

        if (EventSystem.current == null) return false;
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0 && results.Any(c => c.gameObject == this.gameObject || c.gameObject == runeRewriteButton);
    }



    //public void ToggleGameEvents(bool enable)
    //{
    //    Map.GameBoardEventsEnabled = enable;

    //    CanvasGroup canvasGroup = gameBoard.GetComponent<CanvasGroup>();
    //    canvasGroup.blocksRaycasts = enable;
    //    canvasGroup.interactable = enable;
    //    canvasGroup.alpha = enable ? 1f : 0.2f;
        
    //}

    private void CheckLetterInFrame()
    {
        RectTransform frameRect = frameButton.GetComponent<RectTransform>();

        if (letterTransformInside != null)
        {
            if (!IsLetterInsideFrame(letterTransformInside.GetComponent<RectTransform>(), frameRect))
            {
                letterTransformInside.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                letterTransformInside.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                frameButton.GetComponentInChildren<TextMeshProUGUI>().text = "";
                letterTransformInside = null;
            }
        }

        if (letterTransformInside == null)
        {
            foreach (Transform child in contentPanelTransform)
            {
                if (child.GetComponentInChildren<TextMeshProUGUI>().text != "")
                {
                    RectTransform letterRect = child.GetComponent<RectTransform>();

                    if (IsLetterInsideFrame(letterRect, frameRect))
                    {
                        if (letterTransformInside == null || letterTransformInside != child)
                        {
                            letterTransformInside = child;
                            SetLetterInFrame();
                        }
                        break; // Una vez que encontramos una letra dentro del marco, salimos del bucle
                    }
                }

            }
        }

        if (letterTransformInside == null)
        {
            frameButton.interactable = false;
            frameOutline.enabled = false;
            frameButton.onClick.RemoveAllListeners();
        }

    }

    private void SetLetterInFrame()
    {
        if (letterTransformInside.GetComponent<RectTransform>().localScale == new Vector3(1f, 1f, 1f))
        {
            letterTransformInside.GetComponent<RectTransform>().localScale *= 1.8f;
        }
        frameButton.interactable = true;
        frameOutline.enabled = true;

        //frameButton.onClick.AddListener(() => OnLetterSelected(letterTransformInside.GetComponentInChildren<TextMeshProUGUI>().text[0]));


        letterTransformInside.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        frameButton.GetComponentInChildren<TextMeshProUGUI>().text = letterTransformInside.GetComponentInChildren<TextMeshProUGUI>().text;
    }



    private bool IsLetterInsideFrame(RectTransform letter, RectTransform frame)
    {
        Vector2 letterWorldPosition = letter.position;
        Vector2 frameWorldPosition = frame.position;

        //float halfLetterWidth = letter.rect.width * 0.5f * letter.lossyScale.x;
        float halfFrameWidth = frame.rect.width * 0.5f * frame.lossyScale.x;

        // Margen de tolerancia
        float tolerance = 0.22f;

        var isLetterInside = letterWorldPosition.x > (frameWorldPosition.x - halfFrameWidth + tolerance) && letterWorldPosition.x < (frameWorldPosition.x + halfFrameWidth - tolerance);
               
        return isLetterInside;
    }


    public void OnRuneRewriteLetter()
    {
        if (!runeRewiteActive)
        {
            this.gameObject.SetActive(true);
            backgroundPanel.SetActive(true);
            map.ToggleGameEvents(false);
            CheckCloseRunerewrite();
            CheckLetterInFrame();

            runeRewiteActive = true;

            //GameEvents.FireRuneRewriteLetterEvent();
        }
        else
        {
            CloseRunerewite();
        }
    }


    public void OnLetterSelected()
    {
        char selectedLetter = letterTransformInside.GetComponentInChildren<TextMeshProUGUI>().text[0];

        GameEvents.FireChageLetter(selectedLetter, GameActor.Player);
        CloseRunerewite();
    }

    public void CloseRunerewite()
    {
        this.gameObject.SetActive(false);
        backgroundPanel.SetActive(false);
        map.ToggleGameEvents(true);

        runeRewiteActive = false;
    }

    public void ScrollContentPanel(SwipeDirection direction, float distance)
    {
        // Aquí asumiré que tienes alguna lógica para desplazar el contenido
        // del panel basado en la dirección y la distancia. Puede que necesites
        // ajustar esto según cómo esté estructurado tu código actualmente.

        if (direction == SwipeDirection.Right)
        {
            // Desplazar el contenido a la derecha
            contentPanelTransform.transform.Translate(Vector3.right * distance);
        }
        else if (direction == SwipeDirection.Left)
        {
            // Desplazar el contenido a la izquierda
            contentPanelTransform.transform.Translate(Vector3.left * distance);
        }
    }


    private Language GetCurrentLanguage()
    {
        var userLang = PlayerPrefs.GetString("LANG");

        if (string.IsNullOrEmpty(userLang))
        {
            userLang = LanguageCodes.ES_es;
        }

        return Language.GetLanguages().Where(c => c.Code == userLang).FirstOrDefault();
    }
}
