using Assets.Scripts.Data;
using Assets.Scripts.Training.Data;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{

    #region "Variables y propiedades públicas"

    public static IDictionaryManager dictionaryManager;
    public DialogBubble dialogBubble;

    public AudioManager audioManager;
    public GameObject playerIcon;
    public GameObject opponentIcon;

    public GameObject PointsChallengePanel;

    public Button resolveWordButton;
    public RectTransform canvasRectTransform;
    public RectTransform panelTools;

    public GameObject opponentPanel;
    public RectTransform opponentPanelTools;

    public ToolButton opponentChangeLetterButton = null;
    public ToolButton playerChangeLetterButton = null;

    public ToolButton FreezTrapOpponentButton = null;
    public ToolButton FreezTrapPlayerButton = null;

    public ToolButton MoveToTilePlayerButton = null;
    public ToolButton MoveToTileOpponentButton = null;

    public bool MoveToTileMessageDispalyed = false;
    public bool FreezeTrapMessageDisplayed = false;

    public TMP_Text OpponentWord;

    private List<string> PlayerWords = new List<string>();
    [HideInInspector]
    public List<string> OpponentWords = new List<string>();

    public RectTransform panelLetters;
    public float xOffset = 0.750f;
    public float yOffset = 0.850f;
    public float initialXpos = 0f;
    public float initialYpos = 0f;
    public float playerIconXoffset = 0.21f;
    public float playerIconYoffset = 0.32f;

    /// <summary>
    /// Celda inicial del jugador
    /// </summary>
    [HideInInspector]
    public Hex InitialPlayerTile = null;

    public Hex GetInitialPlayerTile()
    {
        return InitialPlayerTile;
    }

    /// <summary>
    /// Celda inicial del oponente para juego online
    /// </summary>
    [HideInInspector]
    public Hex InitialOpponentTile = null;

    public Hex GetInitialOpponentTile()
    {
        return InitialOpponentTile;
    }

    /// <summary>
    /// Celda actual del jugador
    /// </summary>
    [HideInInspector]
    public Hex CurrentPlayerTile = null;

    //public Hex GetCurrentPlayerTile()
    //{
    //    return CurrentPlayerTile;
    //}

    /// <summary>
    /// Celda actual del oponente
    /// </summary>
    [HideInInspector]
    public Hex CurrentOpponentTile = null;

    //public Hex GetCurrentOpponentTile()
    //{
    //    return CurrentOpponentTile;
    //}

    /// <summary>
    /// Celda objetivo con la que hay que formar una palabra para ganar
    /// </summary>
    [HideInInspector]
    public Hex ObjectiveTile = null;

    public Hex GetObjectiveTile()
    {
        return ObjectiveTile;
    }

    /// <summary>
    /// Habilita/deshabilita los eventos del game board cuando se está haciendo zoom/pan
    /// </summary>
    [HideInInspector]
    public static bool GameBoardEventsEnabled = true;


    /// <summary>
    /// Tablero del juego con todas las celdas
    /// </summary>
    public List<Hex> gridTiles = new List<Hex>();

    public Image Background;

    [HideInInspector]
    public float idleTimerPlayer = 0.0f;
    public float idleTimerOpponent = 0.0f;

    public float selectedTileScaleFactor = 1.05f;
    public float tileScale = 0.95f;
    public Text debugLabel;

    private float _cellScaleFactorY;

    [HideInInspector]
    public bool DeselectTilesByTimePlayer = false;
    [HideInInspector]
    public bool DeselectTilesByTimeOpponent = false;

    // Tiempo en segundos antes de que se deseleccionen las celdas
    [HideInInspector]
    public static float maxIdleTime = 20f;


    [HideInInspector]
    public static float maxIdleFrozenTime = 30f;
    [HideInInspector]
    public static float idleTimerFrozenPlayer = 0.0f;
    [HideInInspector]
    public static float idleTimerFrozenOpponent = 0.0f;

    public TextMeshPro FrozenPlayerCounter;
    public TextMeshPro FrozenOpponentCounter;


    public int currentLevel;
    public GameMode gameMode;
    public GameType gameType;

    public short wordMinLetter { get; set; }
    public float changeLetterTimer { get; set; }
    public float challenPointsToWin { get; set; } = 1000f;

    [HideInInspector]
    public bool PlayerIsFrozen = false;
    [HideInInspector]
    public bool OpponentIsFrozen = false;




    #endregion

    #region "Variables y propiedades públicas"

    /// <summary>
    /// Celdas del jugador seleccionadas
    /// </summary>
    public List<Hex> selectedTilesPlayer = new List<Hex>();
    //public short SelectedTilesCount()
    //{
    //    return (short)selectedTilesPlayer.Count;
    //}


    /// <summary>
    /// Celdas del oponente seleccionadas
    /// </summary>
    public List<Hex> selectedTilesOpponent = new List<Hex>();



    public Hex hexPrefab;
    public ToolButton toolButtonPrefab;
    public Language lang;


    private bool DoInvalidWordAnimationPlayer = false;
    private bool DoInvalidWordAnimationOpponent = false;

    public bool Initialized = false;
    public Vector3 _initialSwirlScale;
    public Vector3 _initialPlayerIconScale;

    List<SignLetter> wordLetters = new List<SignLetter>();
    Vector3 currentSignLetterPos = Vector3.zero;

    public GameObject arrowPrefab; // Asigna tu prefab de flecha aquí

    [HideInInspector]
    public bool isSettingFreezeTrap = false;

    [HideInInspector]
    public bool isMovingToTile = false;

    public GameObject FrozenOpponentImage = null;
    public GameObject FrozenPlayerImage = null;

    public ParticleSystem FreezeTrapEffectPlayer;
    public ParticleSystem FreezeTrapEffectOpponent;

    public OnlineManager multiplayerManager = null;

    #endregion


    float CalculateScaleFactorY(float screenWidth, float screenHeight)
    {
        float scaleFactorY = 1f; // Valor predeterminado si el alto no es superior al ancho

        // Verifica si el alto de la pantalla es superior al ancho
        if (screenHeight > screenWidth)
        {
            // Calcula el ratio dividiendo el ancho por el alto
            float ratio = screenWidth / screenHeight;

            // Pondera el factor final basado en el ratio
            // Asume que ratio = 0.45 corresponde a un factor ideal de 0.85
            // y ajusta linealmente según el ratio entre 0.1 y 0.9
            float minRatio = 0.1f;
            float maxRatio = 0.9f;
            float minFactor = 0.65f; // Factor ideal para el mínimo ratio observado
            float maxFactor = 1f; // Factor para el máximo ratio, asumiendo que queremos 1 como máximo

            // Interpolación lineal para calcular el factor basado en el ratio actual
            scaleFactorY = Mathf.Lerp(minFactor, maxFactor, (ratio - minRatio) / (maxRatio - minRatio));
        }

        return scaleFactorY;
    }

    Bounds CalculateCellsBounds()
    {
        Bounds totalBounds = new Bounds();
        bool hasBounds = false;

        foreach (Transform child in this.transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                if (!hasBounds)
                {
                    totalBounds = childRenderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    totalBounds.Encapsulate(childRenderer.bounds);
                }
            }
        }

        return totalBounds;
    }

    public void AdjustMapScale()
    {
        BoxCollider2D mapCollider = this.transform.GetComponent<BoxCollider2D>();

        Bounds cellsBounds = CalculateCellsBounds();
        Bounds mapBounds = cellsBounds;
        if (mapCollider != null)
        {
            mapCollider.enabled = true;
            mapBounds = mapCollider.bounds;
            mapCollider.enabled = false;
        }

        // Comprueba si los bounds de las celdas exceden los del Map en el eje X
        if (cellsBounds.size.x > mapBounds.size.x)
        {
            float requiredScaleFactor = mapBounds.size.x / cellsBounds.size.x;
            // Ajusta la escala del Map uniformemente para que las celdas encajen dentro de sus límites
            this.transform.localScale *= requiredScaleFactor;
        }
    }

    public void EnableResolveButton(bool enable)
    {

        // Establece el color objetivo basado en el estado del botón
        Color targetColor = enable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f); // Color oscurecido para deshabilitado



        var buttonImage = this.resolveWordButton.GetComponent<Image>();
        //var buttonColor = buttonImage.color;
        //buttonColor.a = enable ? 1f : 0.5f; // Establece la transparencia al 50%
        //buttonImage.color = buttonColor;

        // Anima el cambio de color
        buttonImage.DOColor(targetColor, 0.62f).SetEase(Ease.Flash); // Ajusta la duración de la animación según necesites

        /*
        
        this.resolveWordButton.interactable = enable;

         Modificar la transparencia del fondo del botón
        var buttonImage = this.resolveWordButton.GetComponent<Image>();
        var buttonColor = buttonImage.color;
        buttonColor.a = enable ? 1f : 0.5f; // Establece la transparencia al 50%
        buttonImage.color = buttonColor;

        // Modificar la transparencia del texto del botón, si es necesario
        var buttonText = this.resolveWordButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            var textColor = buttonText.color;
            textColor.a = enable ? 1f : 0.5f; // Establece la transparencia al 50%
            buttonText.color = textColor;
        }

        */

    }


    public void ToggleGameEvents(bool enable)
    {
        GameBoardEventsEnabled = enable;

        CanvasGroup canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = enable;
        canvasGroup.interactable = enable;
        canvasGroup.alpha = enable ? 1f : 0.2f;


        CanvasGroup resolveBtnCanvasGroup = resolveWordButton.gameObject.GetComponent<CanvasGroup>();
        resolveBtnCanvasGroup.blocksRaycasts = enable;
        resolveBtnCanvasGroup.interactable = enable;
        resolveBtnCanvasGroup.alpha = enable ? 1f : 0.75f;


        CanvasGroup panelToolsCanvasGroup = panelTools.gameObject.GetComponent<CanvasGroup>();
        panelToolsCanvasGroup.blocksRaycasts = enable;
        panelToolsCanvasGroup.interactable = enable;
        panelToolsCanvasGroup.alpha = enable ? 1f : 0.75f;
    }


    public void InitializeGame(int currentLevel, GameMode gameMode, /*GameLevelPhase gameLevelPhase,*/ GameType gametype)
    {


        PlayerWords = new List<string>();
        OpponentWords = new List<string>();

        wordLetters = new List<SignLetter>();
        currentSignLetterPos = Vector3.zero;

        ToggleGameEvents(false);

        Initialized = false;

        PointsChallengePanel.SetActive(false);

        _initialSwirlScale = playerIcon.GetComponent<PlayerManager>().swirl.transform.localScale;
        _initialPlayerIconScale = playerIcon.transform.localScale;

        this.currentLevel = currentLevel;
        this.gameMode = gameMode;
        //this.gameLevelPhase = gameLevelPhase;
        this.gameType = gametype;

        float mapSize = 4;
        float letterComplexity = 0.5f;

        switch (gameMode)
        {
            case GameMode.CatchLetter:
                {
                    challenPointsToWin = 500f;//gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.PointsToWin).FirstOrDefault().Value;
                    break;
                }

            case GameMode.PointsChallenge:
                {
                    challenPointsToWin = 500f; //gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.PointsToWin).FirstOrDefault().Value;
                    break;
                }

            default:
                {
                    break;
                }
        }

        //var timerMinutes = gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.Timer).FirstOrDefault().Value;
        changeLetterTimer = 0f;//gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.TileChangeLetter).FirstOrDefault().Value;

        mapSize = 4f;//gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.GameBoardSize).FirstOrDefault().Value;
        letterComplexity = 0.05f; //gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.LetterComplexity).FirstOrDefault().Value;
        wordMinLetter = (short)3;//(short)gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.WordMinLetter).FirstOrDefault().Value;

        //sandClock.roundDuration = timerMinutes * 60.0f;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenratio = (screenWidth / screenHeight);
        _cellScaleFactorY = CalculateScaleFactorY(screenWidth, screenHeight);
        //debugLabel.text = $"Resolución ({screenWidth}px x {screenHeight}px) : {screenratio} --> Scale factor: {_cellScaleFactorY}";

        PlayerManager.Lifes = 10;

        if (currentSwirlCoroutinePlayer != null)
        {
            StopCoroutine(currentSwirlCoroutinePlayer);
            currentSwirlCoroutinePlayer = null;
        }

        if (currentSwirlCoroutineOpponent != null)
        {
            StopCoroutine(currentSwirlCoroutineOpponent);
            currentSwirlCoroutineOpponent = null;
        }

        DestroyExistingTiles();
        gridTiles = new List<Hex>();

        //GameInitalLifes = 10;
        InitialPlayerTile = null;
        InitialOpponentTile = null;
        CurrentPlayerTile = null;
        CurrentOpponentTile = null;
        ObjectiveTile = null;
        selectedTilesPlayer = new List<Hex>();
        selectedTilesOpponent = new List<Hex>();
        idleTimerPlayer = 0.0f;
        idleTimerOpponent = 0.0f;

        //Idioma del usuario
        var userLang = PlayerPrefs.GetString("LANG");
        lang = Language.GetLanguages().Where(c => c.Code == userLang).FirstOrDefault();


        switch (userLang)
        {
            case LanguageCodes.ES_es:
                dictionaryManager = GameObject.Find("DictionaryManager").GetComponent<DictionaryManagerES_es>();
                break;

            case LanguageCodes.EN_en:
                dictionaryManager = GameObject.Find("DictionaryManager").GetComponent<DictionaryManagerEN_en>();
                break;

            default:
                dictionaryManager = GameObject.Find("DictionaryManager").GetComponent<DictionaryManagerES_es>();
                break;
        }

        lang.Initialize(userLang, letterComplexity);

        if (gameMode == GameMode.VsAlgorithm || gametype == GameType.Multiplayer)
            opponentPanel.SetActive(true);
        else
            opponentPanel.SetActive(false);

        var tools = new List<GameTool>()
        {
            new GameTool() { Tool = GameAidTool.ChangeCurrentLetter, ToolMode = GameAidToolModeType.Mixed, NumEquiped=3, RechargeTime = 30.0f},
            new GameTool() { Tool = GameAidTool.FreezeTrap, ToolMode = GameAidToolModeType.ByNumEquiped, NumEquiped=1, RechargeTime = 0f},
            new GameTool() { Tool = GameAidTool.MoveToTile, ToolMode = GameAidToolModeType.ByNumEquiped, NumEquiped=1, RechargeTime = 0f}
        };

        GenerateToolButtons(tools);

        /*
        int toolIndex = 1;
        foreach (var aidTool in gameLevelPhase.Tools)
        {
            if (aidTool.Tool == GameAidTool.ChangeCurrentLetter)
            {
                // Encuentra EmptyButton
                RectTransform emptyButtonRect = panelTools.transform.Find(string.Format("EmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                playerChangeLetterButton = Instantiate(toolButtonPrefab, panelTools.transform);

                RectTransform changeLetterInstanceRect = playerChangeLetterButton.GetComponent<RectTransform>();

                // Opcionalmente, ajusta la posición y escala del botón si es necesario
                // Ajusta la posición y tamaño basándose en EmptyButton
                changeLetterInstanceRect.localPosition = emptyButtonRect.localPosition;
                changeLetterInstanceRect.sizeDelta = emptyButtonRect.sizeDelta;
                playerChangeLetterButton.CurrentMap = this;
                playerChangeLetterButton.mechanism = aidTool.ToolMode;
                playerChangeLetterButton.aid = aidTool.Tool;
                playerChangeLetterButton.cooldownTime = aidTool.RechargeTime;
                playerChangeLetterButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                playerChangeLetterButton.actor = GameActor.Player;
                playerChangeLetterButton.GetComponentInChildren<Button>().name = "BtnChangeLetterPlayer";

                playerChangeLetterButton.ResetButton();
                emptyButtonRect.gameObject.SetActive(false);

                if (gameMode == GameMode.VsAlgorithm)
                {
                    // Encuentra EmptyButton
                    RectTransform emptyButtonRectOpponent = opponentPanelTools.transform.Find(string.Format("OpponentEmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                    opponentChangeLetterButton = Instantiate(toolButtonPrefab, opponentPanelTools.transform);
                    RectTransform changeLetterInstanceRectOpponent = opponentChangeLetterButton.GetComponent<RectTransform>();

                    // Opcionalmente, ajusta la posición y escala del botón si es necesario
                    // Ajusta la posición y tamaño basándose en EmptyButton
                    changeLetterInstanceRectOpponent.localPosition = emptyButtonRectOpponent.localPosition;
                    changeLetterInstanceRectOpponent.localScale = changeLetterInstanceRectOpponent.localScale * 0.85f;
                    opponentChangeLetterButton.CurrentMap = this;
                    opponentChangeLetterButton.mechanism = aidTool.ToolMode;
                    opponentChangeLetterButton.aid = aidTool.Tool;
                    opponentChangeLetterButton.cooldownTime = aidTool.RechargeTime;
                    opponentChangeLetterButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                    opponentChangeLetterButton.actor = GameActor.Opponent;
                    opponentChangeLetterButton.GetComponentInChildren<Button>().name = "BtnChangeLetterOpponent";
                    opponentChangeLetterButton.GetComponentInChildren<TMP_Text>().fontSize = 90f;

                    opponentChangeLetterButton.ResetButton();
                    emptyButtonRectOpponent.gameObject.SetActive(false);
                }
            }

            if (aidTool.Tool == GameAidTool.FreezeTrap)
            {
                // Encuentra EmptyButton
                RectTransform emptyButtonRectFreezeTrapPlayer = panelTools.transform.Find(string.Format("EmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                FreezTrapPlayerButton = Instantiate(toolButtonPrefab, panelTools.transform);

                RectTransform FreezTrapPlayerInstanceRect = FreezTrapPlayerButton.GetComponent<RectTransform>();

                // Opcionalmente, ajusta la posición y escala del botón si es necesario
                // Ajusta la posición y tamaño basándose en EmptyButton
                FreezTrapPlayerInstanceRect.localPosition = emptyButtonRectFreezeTrapPlayer.localPosition;
                FreezTrapPlayerInstanceRect.sizeDelta = emptyButtonRectFreezeTrapPlayer.sizeDelta;
                FreezTrapPlayerButton.CurrentMap = this;
                FreezTrapPlayerButton.mechanism = aidTool.ToolMode;
                FreezTrapPlayerButton.aid = aidTool.Tool;
                FreezTrapPlayerButton.cooldownTime = aidTool.RechargeTime;
                FreezTrapPlayerButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                FreezTrapPlayerButton.actor = GameActor.Player;
                FreezTrapPlayerButton.GetComponentInChildren<Button>().name = "BtnFreezeTrapPlayer";

                FreezTrapPlayerButton.ResetButton();
                emptyButtonRectFreezeTrapPlayer.gameObject.SetActive(false);

                if (gameMode == GameMode.VsAlgorithm)
                {
                    // Encuentra EmptyButton
                    RectTransform emptyButtonRectFreezeTrapOpponent = opponentPanelTools.transform.Find(string.Format("OpponentEmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                    FreezTrapOpponentButton = Instantiate(toolButtonPrefab, opponentPanelTools.transform);
                    RectTransform FreezTrapRectOpponent = FreezTrapOpponentButton.GetComponent<RectTransform>();

                    // Opcionalmente, ajusta la posición y escala del botón si es necesario
                    // Ajusta la posición y tamaño basándose en EmptyButton
                    FreezTrapRectOpponent.localPosition = emptyButtonRectFreezeTrapOpponent.localPosition;
                    FreezTrapRectOpponent.localScale = FreezTrapRectOpponent.localScale * 0.85f;
                    FreezTrapOpponentButton.CurrentMap = this;
                    FreezTrapOpponentButton.mechanism = aidTool.ToolMode;
                    FreezTrapOpponentButton.aid = aidTool.Tool;
                    FreezTrapOpponentButton.cooldownTime = aidTool.RechargeTime;
                    FreezTrapOpponentButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                    FreezTrapOpponentButton.actor = GameActor.Opponent;
                    FreezTrapOpponentButton.GetComponentInChildren<Button>().name = "BtnFreezeTrapOpponent";
                    FreezTrapOpponentButton.GetComponentInChildren<TMP_Text>().fontSize = 90f;

                    FreezTrapOpponentButton.ResetButton();
                    emptyButtonRectFreezeTrapOpponent.gameObject.SetActive(false);
                }


            }

            if (aidTool.Tool == GameAidTool.MoveToTile)
            {
                // Encuentra EmptyButton
                RectTransform emptyButtonRectMoveToTilePlayer = panelTools.transform.Find(string.Format("EmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                MoveToTilePlayerButton = Instantiate(toolButtonPrefab, panelTools.transform);

                RectTransform MoveToTilePlayerInstanceRect = MoveToTilePlayerButton.GetComponent<RectTransform>();

                // Opcionalmente, ajusta la posición y escala del botón si es necesario
                // Ajusta la posición y tamaño basándose en EmptyButton
                MoveToTilePlayerInstanceRect.localPosition = emptyButtonRectMoveToTilePlayer.localPosition;
                MoveToTilePlayerInstanceRect.sizeDelta = emptyButtonRectMoveToTilePlayer.sizeDelta;
                MoveToTilePlayerButton.CurrentMap = this;
                MoveToTilePlayerButton.mechanism = aidTool.ToolMode;
                MoveToTilePlayerButton.aid = aidTool.Tool;
                MoveToTilePlayerButton.cooldownTime = aidTool.RechargeTime;
                MoveToTilePlayerButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                MoveToTilePlayerButton.actor = GameActor.Player;
                MoveToTilePlayerButton.GetComponentInChildren<Button>().name = "BtnMoveToTilePlayer";
                MoveToTilePlayerButton.ResetButton();
                emptyButtonRectMoveToTilePlayer.gameObject.SetActive(false);

                
                if (gameMode == GameMode.VsAlgorithm)
                {
                    // Encuentra EmptyButton
                    RectTransform emptyButtonRectMoveToTileOpponent = opponentPanelTools.transform.Find(string.Format("OpponentEmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                    MoveToTileOpponentButton = Instantiate(toolButtonPrefab, opponentPanelTools.transform);
                    RectTransform MoveToTileRectOpponent = MoveToTileOpponentButton.GetComponent<RectTransform>();

                    // Opcionalmente, ajusta la posición y escala del botón si es necesario
                    // Ajusta la posición y tamaño basándose en EmptyButton
                    MoveToTileRectOpponent.localPosition = emptyButtonRectMoveToTileOpponent.localPosition;
                    MoveToTileRectOpponent.localScale = MoveToTileRectOpponent.localScale * 0.85f;
                    MoveToTileOpponentButton.CurrentMap = this;
                    MoveToTileOpponentButton.mechanism = aidTool.ToolMode;
                    MoveToTileOpponentButton.aid = aidTool.Tool;
                    MoveToTileOpponentButton.cooldownTime = aidTool.RechargeTime;
                    MoveToTileOpponentButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                    MoveToTileOpponentButton.actor = GameActor.Opponent;
                    MoveToTileOpponentButton.GetComponentInChildren<Button>().name = "BtnMoveToTileOpponent";
                    MoveToTileOpponentButton.GetComponentInChildren<TMP_Text>().fontSize = 90f;
                    MoveToTileOpponentButton.ResetButton();
                    emptyButtonRectMoveToTileOpponent.gameObject.SetActive(false);
                }
                

            }


            toolIndex++;
        }
        */


        //Escala del map inicial
        this.transform.localScale = new Vector3(1f, 1f, 1f);

        //Escala de la celda inicial
        hexPrefab.transform.localScale = new Vector3(1.31f, 1.31f, 1);

        //Generamos el tablero de celdas
        if (gametype == GameType.Standalone)
        {
            GenerateBoard(mapSize, this.gameMode);

            //Ajustamos la escala del mapa para ajustarse al tamaño del ablero de celdas
            AdjustMapScale();

            //Asignamos las celdas vecinas de cada celda
            AssignTileNeighbors();

            GameCountDown.timerIsRunning = true;

            if (resolveWordButton != null)
            {
                EnableResolveButton(false);
            }

            Initialized = true;

            ToggleGameEvents(true);
        }
        else
        {
            if (GameManager.GameData != null)
            {
                _player = GameManager.GameData.data.playersInfo.Where(d => d.Key == FirebaseInitializer.auth.CurrentUser.UserId).FirstOrDefault();
                _opponent = GameManager.GameData.data.playersInfo.Where(d => d.Key != FirebaseInitializer.auth.CurrentUser.UserId).FirstOrDefault();

                multiplayerManager.InitializeMultiplayerManager(this);

                //Cargamos las celdas del mapa/tablero
                //int index = 0;
                //foreach (var tile in GameManager.GameData.data.gameBoard.boardTiles)
                //{
                //    InstatiateHexTile(tile, index);
                //    index++;
                //}

                GenerateBoard(mapSize, this.gameMode, GameManager.GameData.data.gameBoard.boardTiles);





                //Ajustamos la escala del mapa para ajustarse al tamaño del ablero de celdas
                AdjustMapScale();

                //Asignamos las celdas vecinas de cada celda
                AssignTileNeighbors();

                GameCountDown.timerIsRunning = true;

                if (resolveWordButton != null)
                {
                    EnableResolveButton(false);
                }

                Initialized = true;

                ToggleGameEvents(true);

                //Habilitamos el tablero
                //TODO: Mostrar mensaje tipo: "El juego comenzará en 3,2,1..."

                /* TODO: Comprobar esto
                EnableBoard();
                _canContinueSelecting = true;
                */

                //Seleccionamos la celda actual
                //GameEvents.OnTileMultiPlayerSelectedMethod(FirstPlayerTile);




            }
            else
            {
                throw new ArgumentNullException(nameof(GameManager.GameData));
            }

        }

        /* 
        //Debug game board complexity
        List<LangLetter> letters = new List<LangLetter>();
        foreach (var cell in gridTiles)
        {
            letters.Add(cell.GetLangLetter(userLang));
        }

        
        var normMedia = letters.Where(c => !c.IsVocal).Sum(c => c.Normalized);
        var complex = letters.Count(c => !c.IsVocal && c.Normalized < 0.5);

        Debug.Log(string.Format("Diff group 1 number: {0}", letters.Count(c => c.DifficultyGroup == 1)));
        Debug.Log(string.Format("Diff group 2 number: {0}", letters.Count(c => c.DifficultyGroup == 2)));
        Debug.Log(string.Format("Diff group 3 number: {0}", letters.Count(c => c.DifficultyGroup == 3)));
        Debug.Log(string.Format("Vowels number: {0}", letters.Count(c => c.IsVocal)));
        */



    }

    #region "Firebase game events"

    //void HandleGameStatusIsReady(object sender, ValueChangedEventArgs args)
    //{
    //    if (args.DatabaseError != null)
    //    {
    //        Debug.LogError(args.DatabaseError.Message);
    //        return;
    //    }

    //    var jsonGameBoard = args.Snapshot.GetRawJsonValue();

    //    try
    //    {
    //        //Registramos el evento que se lanzará cuando haya alguna actualización edl oponente
    //        FirebaseInitializer.dbRef
    //        .GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/opponentActions")
    //        .ChildAdded += OpponentUpdateAction_ChildAdded;


    //        //Ajustamos la escala del mapa para ajustarse al tamaño del ablero de celdas
    //        AdjustMapScale();

    //        //Asignamos las celdas vecinas de cada celda
    //        AssignTileNeighbors();

    //        GameCountDown.timerIsRunning = true;

    //        if (resolveWordButton != null)
    //        {
    //            EnableResolveButton(false);
    //        }

    //        Initialized = true;

    //        ToggleGameEvents(true);

    //        FirebaseInitializer.dbRef.GetReference($"games/{GameManager.GameData.gameId}/stauts").ValueChanged -= HandleGameStatusIsReady;
    //        //Habilitamos el tablero
    //        //TODO: Mostrar mensaje tipo: "El juego comenzará en 3,2,1..."

    //        /* TODO: Comprobar esto
    //        EnableBoard();
    //        _canContinueSelecting = true;
    //        */

    //        //Seleccionamos la celda actual
    //        //GameEvents.OnTileMultiPlayerSelectedMethod(FirstPlayerTile);




    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError(e.Message);
    //        throw;
    //    }


    //}



    #endregion

    public static KeyValuePair<string, GamePlayerData> _player { get; set; } = new KeyValuePair<string, GamePlayerData>();
    public static KeyValuePair<string, GamePlayerData> _opponent { get; set; } = new KeyValuePair<string, GamePlayerData>();

    private void GenerateToolButtons(List<GameTool> tools)
    {
        int toolIndex = 1;
        foreach (var aidTool in tools)
        {
            if (aidTool.Tool == GameAidTool.ChangeCurrentLetter)
            {
                // Encuentra EmptyButton
                RectTransform emptyButtonRect = panelTools.transform.Find(string.Format("EmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                playerChangeLetterButton = Instantiate(toolButtonPrefab, panelTools.transform);

                RectTransform changeLetterInstanceRect = playerChangeLetterButton.GetComponent<RectTransform>();

                // Opcionalmente, ajusta la posición y escala del botón si es necesario
                // Ajusta la posición y tamaño basándose en EmptyButton
                changeLetterInstanceRect.localPosition = emptyButtonRect.localPosition;
                changeLetterInstanceRect.sizeDelta = emptyButtonRect.sizeDelta;
                playerChangeLetterButton.CurrentMap = this;
                playerChangeLetterButton.mechanism = aidTool.ToolMode;
                playerChangeLetterButton.aid = aidTool.Tool;
                playerChangeLetterButton.cooldownTime = aidTool.RechargeTime;
                playerChangeLetterButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                playerChangeLetterButton.actor = GameActor.Player;
                playerChangeLetterButton.GetComponentInChildren<Button>().name = "BtnChangeLetterPlayer";

                playerChangeLetterButton.ResetButton();
                emptyButtonRect.gameObject.SetActive(false);

                if (gameMode == GameMode.VsAlgorithm || gameType == GameType.Multiplayer)
                {
                    // Encuentra EmptyButton
                    RectTransform emptyButtonRectOpponent = opponentPanelTools.transform.Find(string.Format("OpponentEmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                    opponentChangeLetterButton = Instantiate(toolButtonPrefab, opponentPanelTools.transform);
                    RectTransform changeLetterInstanceRectOpponent = opponentChangeLetterButton.GetComponent<RectTransform>();

                    // Opcionalmente, ajusta la posición y escala del botón si es necesario
                    // Ajusta la posición y tamaño basándose en EmptyButton
                    changeLetterInstanceRectOpponent.localPosition = emptyButtonRectOpponent.localPosition;
                    changeLetterInstanceRectOpponent.localScale = changeLetterInstanceRectOpponent.localScale * 0.85f;
                    opponentChangeLetterButton.CurrentMap = this;
                    opponentChangeLetterButton.mechanism = aidTool.ToolMode;
                    opponentChangeLetterButton.aid = aidTool.Tool;
                    opponentChangeLetterButton.cooldownTime = aidTool.RechargeTime;
                    opponentChangeLetterButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                    opponentChangeLetterButton.actor = GameActor.Opponent;
                    opponentChangeLetterButton.GetComponentInChildren<Button>().name = "BtnChangeLetterOpponent";
                    opponentChangeLetterButton.GetComponentInChildren<TMP_Text>().fontSize = 90f;

                    opponentChangeLetterButton.ResetButton();
                    emptyButtonRectOpponent.gameObject.SetActive(false);
                }
            }

            if (aidTool.Tool == GameAidTool.FreezeTrap)
            {
                // Encuentra EmptyButton
                RectTransform emptyButtonRectFreezeTrapPlayer = panelTools.transform.Find(string.Format("EmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                FreezTrapPlayerButton = Instantiate(toolButtonPrefab, panelTools.transform);

                RectTransform FreezTrapPlayerInstanceRect = FreezTrapPlayerButton.GetComponent<RectTransform>();

                // Opcionalmente, ajusta la posición y escala del botón si es necesario
                // Ajusta la posición y tamaño basándose en EmptyButton
                FreezTrapPlayerInstanceRect.localPosition = emptyButtonRectFreezeTrapPlayer.localPosition;
                FreezTrapPlayerInstanceRect.sizeDelta = emptyButtonRectFreezeTrapPlayer.sizeDelta;
                FreezTrapPlayerButton.CurrentMap = this;
                FreezTrapPlayerButton.mechanism = aidTool.ToolMode;
                FreezTrapPlayerButton.aid = aidTool.Tool;
                FreezTrapPlayerButton.cooldownTime = aidTool.RechargeTime;
                FreezTrapPlayerButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                FreezTrapPlayerButton.actor = GameActor.Player;
                FreezTrapPlayerButton.GetComponentInChildren<Button>().name = "BtnFreezeTrapPlayer";

                FreezTrapPlayerButton.ResetButton();
                emptyButtonRectFreezeTrapPlayer.gameObject.SetActive(false);

                if (gameMode == GameMode.VsAlgorithm || gameType == GameType.Multiplayer)
                {
                    // Encuentra EmptyButton
                    RectTransform emptyButtonRectFreezeTrapOpponent = opponentPanelTools.transform.Find(string.Format("OpponentEmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                    FreezTrapOpponentButton = Instantiate(toolButtonPrefab, opponentPanelTools.transform);
                    RectTransform FreezTrapRectOpponent = FreezTrapOpponentButton.GetComponent<RectTransform>();

                    // Opcionalmente, ajusta la posición y escala del botón si es necesario
                    // Ajusta la posición y tamaño basándose en EmptyButton
                    FreezTrapRectOpponent.localPosition = emptyButtonRectFreezeTrapOpponent.localPosition;
                    FreezTrapRectOpponent.localScale = FreezTrapRectOpponent.localScale * 0.85f;
                    FreezTrapOpponentButton.CurrentMap = this;
                    FreezTrapOpponentButton.mechanism = aidTool.ToolMode;
                    FreezTrapOpponentButton.aid = aidTool.Tool;
                    FreezTrapOpponentButton.cooldownTime = aidTool.RechargeTime;
                    FreezTrapOpponentButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                    FreezTrapOpponentButton.actor = GameActor.Opponent;
                    FreezTrapOpponentButton.GetComponentInChildren<Button>().name = "BtnFreezeTrapOpponent";
                    FreezTrapOpponentButton.GetComponentInChildren<TMP_Text>().fontSize = 90f;

                    FreezTrapOpponentButton.ResetButton();
                    emptyButtonRectFreezeTrapOpponent.gameObject.SetActive(false);
                }


            }

            if (aidTool.Tool == GameAidTool.MoveToTile)
            {
                // Encuentra EmptyButton
                RectTransform emptyButtonRectMoveToTilePlayer = panelTools.transform.Find(string.Format("EmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                MoveToTilePlayerButton = Instantiate(toolButtonPrefab, panelTools.transform);

                RectTransform MoveToTilePlayerInstanceRect = MoveToTilePlayerButton.GetComponent<RectTransform>();

                // Opcionalmente, ajusta la posición y escala del botón si es necesario
                // Ajusta la posición y tamaño basándose en EmptyButton
                MoveToTilePlayerInstanceRect.localPosition = emptyButtonRectMoveToTilePlayer.localPosition;
                MoveToTilePlayerInstanceRect.sizeDelta = emptyButtonRectMoveToTilePlayer.sizeDelta;
                MoveToTilePlayerButton.CurrentMap = this;
                MoveToTilePlayerButton.mechanism = aidTool.ToolMode;
                MoveToTilePlayerButton.aid = aidTool.Tool;
                MoveToTilePlayerButton.cooldownTime = aidTool.RechargeTime;
                MoveToTilePlayerButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                MoveToTilePlayerButton.actor = GameActor.Player;
                MoveToTilePlayerButton.GetComponentInChildren<Button>().name = "BtnMoveToTilePlayer";
                MoveToTilePlayerButton.ResetButton();
                emptyButtonRectMoveToTilePlayer.gameObject.SetActive(false);


                if (gameMode == GameMode.VsAlgorithm || gameType == GameType.Multiplayer)
                {
                    // Encuentra EmptyButton
                    RectTransform emptyButtonRectMoveToTileOpponent = opponentPanelTools.transform.Find(string.Format("OpponentEmptyTool0{0}", toolIndex)).GetComponent<RectTransform>();
                    MoveToTileOpponentButton = Instantiate(toolButtonPrefab, opponentPanelTools.transform);
                    RectTransform MoveToTileRectOpponent = MoveToTileOpponentButton.GetComponent<RectTransform>();

                    // Opcionalmente, ajusta la posición y escala del botón si es necesario
                    // Ajusta la posición y tamaño basándose en EmptyButton
                    MoveToTileRectOpponent.localPosition = emptyButtonRectMoveToTileOpponent.localPosition;
                    MoveToTileRectOpponent.localScale = MoveToTileRectOpponent.localScale * 0.85f;
                    MoveToTileOpponentButton.CurrentMap = this;
                    MoveToTileOpponentButton.mechanism = aidTool.ToolMode;
                    MoveToTileOpponentButton.aid = aidTool.Tool;
                    MoveToTileOpponentButton.cooldownTime = aidTool.RechargeTime;
                    MoveToTileOpponentButton.NumberOfItemsEquiped = aidTool.NumEquiped;
                    MoveToTileOpponentButton.actor = GameActor.Opponent;
                    MoveToTileOpponentButton.GetComponentInChildren<Button>().name = "BtnMoveToTileOpponent";
                    MoveToTileOpponentButton.GetComponentInChildren<TMP_Text>().fontSize = 90f;
                    MoveToTileOpponentButton.ResetButton();
                    emptyButtonRectMoveToTileOpponent.gameObject.SetActive(false);
                }


            }

            toolIndex++;
        }
    }



    public void GenerateBoard(float mapSize, GameMode gameMode)
    {
        float yOffset1 = 0f;

        //#if UNITY_EDITOR
        //        yOffset1 = yOffset;
        //#else
        yOffset1 = yOffset * _cellScaleFactorY;
        //#endif


        float xPos = initialXpos;
        float yPos = initialYpos;

        var scaleX = hexPrefab.GetComponent<Hex>().transform.localScale.x;
        var scaleY = hexPrefab.GetComponent<Hex>().transform.localScale.y;

        float currentXOffset = 0.0f;
        float currentYOffset = 0.0f;



        //Modificamos escala
        if (scaleX != 0)
            currentXOffset = xOffset - (xOffset * (1f - scaleX));

        if (scaleY != 0)
            currentYOffset = yOffset1 - (yOffset1 * (1f - scaleY));

        Hex centerTile = default(Hex);

        //Por cada nivel (núermo de niveles + número de niveles vacíos)
        for (int level = 0; level < mapSize; level++)
        {
            //Es un nivel vacío
            var isEmptyLevel = level >= mapSize;


            //El nivel 0 solo contendrá una celda
            if (level == 0)
            {
                xPos = 0;
                yPos = 0;

                var posVector = new Vector3(xPos, yPos, 0);

                //Si se trata del modo de juego "Atrapar la letra"
                //if (gameMode == GameMode.CatchLetter)
                //{
                //    InstatiateHexTile(posVector, 0, 0, 0f, 0f, isObjectiveTile: true, isEmptyLevel: isEmptyLevel);
                //}
                //else
                //{
                InstatiateHexTile(posVector, 0, 0, 0f, 0f, isObjectiveTile: false, isEmptyLevel: isEmptyLevel);
                //}
            }
            else
            {
                //El número de celdas es el nivel por 6. Esto trazará un diseñio en forma de panel
                var levelTilesNumber = level * 6f;

                float yIncrement = 0.5f;
                float xIncrement = 1f;

                float xMultipler = 0;
                xMultipler = level * (xMultipler + xIncrement);

                float yMultipler = 0;
                yMultipler = level * (yMultipler + yIncrement);

                var negativeXtiles = 0;
                var positiveXtiles = 0;


                //Se instanciará cada celda comenzando por la derecaha y siguiendo la dirección contraria de las agujas del reloj
                for (int tileNum = 0; tileNum < levelTilesNumber; tileNum++)
                {
                    xPos = (xMultipler * currentXOffset) * 1.030f;
                    yPos = (yMultipler * currentYOffset) * 1.030f;

                    var posVector = new Vector3(xPos, yPos, 0);

                    //Marcamos la celda como actual, siempre se posivionará en el último nivel arriba -90 grados (x = 0, y = level)
                    bool isCurrentUserTile = false;

                    if (gameMode != GameMode.VsAlgorithm)
                        isCurrentUserTile = level == (mapSize - 1) && xMultipler == 0 && yMultipler == level;
                    else
                        isCurrentUserTile = level == (mapSize - 1) && tileNum == 0;

                    bool isCurrentOpponentTile = false;

                    if (gameMode != GameMode.VsAlgorithm)
                        isCurrentOpponentTile = level == (mapSize - 1) && xMultipler == 0 && yMultipler == -level;
                    else
                        isCurrentOpponentTile = level == (mapSize - 1) && tileNum == ((levelTilesNumber / 3));

                    //Instanciamos celda
                    Hex hex_go = InstatiateHexTile(posVector, level, tileNum, xMultipler, yMultipler, isCurrentUserTile: isCurrentUserTile, isEmptyLevel: isEmptyLevel, isCurrentOpponentTile: isCurrentOpponentTile);

                    if (gameMode == GameMode.VsAlgorithm && (level == (mapSize - 1) && tileNum == 12))
                    {
                        centerTile = hex_go;
                    }

                    //Calculamos los valore de xMultipler y yMultipler para calcular la posición de la próxima celda
                    if (tileNum < level)
                    {
                        xMultipler = xMultipler - xIncrement;
                        yMultipler = yMultipler + yIncrement;
                    }
                    else if (tileNum == level)
                    {
                        xMultipler = xMultipler - xIncrement;
                        yMultipler = yMultipler - yIncrement;
                    }
                    else
                    {
                        if (xMultipler < 0)
                        {
                            if (xMultipler == -(level) && negativeXtiles == 0)
                            {
                                negativeXtiles++;
                            }

                            if (negativeXtiles == 1)
                            {
                                yIncrement = 1;
                            }

                            if (negativeXtiles == 0)
                            {
                                xMultipler = xMultipler - xIncrement;
                                yMultipler = yMultipler - yIncrement;
                            }
                            else if (negativeXtiles > 0 && negativeXtiles < (level + 1))
                            {
                                negativeXtiles++;
                                yMultipler = yMultipler - yIncrement;
                            }
                            else
                            {
                                yIncrement = 0.5f;
                                xMultipler = xMultipler + xIncrement;
                                yMultipler = yMultipler - yIncrement;
                            }
                        }
                        else
                        {
                            if (xMultipler == (level) && positiveXtiles == 0)
                            {
                                positiveXtiles++;
                            }

                            if (positiveXtiles == 1)
                            {
                                yIncrement = 1;
                            }

                            if (positiveXtiles == 0)
                            {
                                xMultipler = xMultipler + xIncrement;
                                yMultipler = yMultipler + yIncrement;
                            }
                            else if (positiveXtiles > 0 && positiveXtiles < (level + 1))
                            {
                                positiveXtiles++;
                                yMultipler = yMultipler + yIncrement;
                            }
                            else
                            {
                                yIncrement = 0.5f;
                                xMultipler = xMultipler - xIncrement;
                                yMultipler = yMultipler + yIncrement;
                            }

                        }
                    }

                }
            }

        }

        Hex furthestHex = null;
        float furthestDistance = 0f;

        if (gameMode == GameMode.CatchLetter)
        {
            foreach (Hex tile in gridTiles.Where(c => !c.IsCurrentPlayerTile && c.tileState != GameTileState.Blocked))
            {
                float distance = Vector3.Distance(tile.transform.position, CurrentPlayerTile.transform.position);
                if (distance > furthestDistance)
                {
                    furthestDistance = distance;
                    furthestHex = tile;
                }
            }

            furthestHex.ToggleObjectiveTile(true);
            //furthestHex.transform.localScale = new Vector3(furthestHex.transform.localScale.x * 1.4f, furthestHex.transform.localScale.y * 1.4f, furthestHex.transform.localScale.z); 
            ObjectiveTile = furthestHex;
        }

        if (gameMode == GameMode.PointsChallenge)
        {
            //TODO: mostrar panel de puntos a conseguir
            if (PointsChallengePanel != null)
            {
                PointsChallengePanel.SetActive(true);
                PointsChallengePanel.GetComponentInChildren<TMP_Text>().text = ((int)challenPointsToWin).ToString();
            }
        }

        if (gameMode == GameMode.VsAlgorithm)
        {
            centerTile.ToggleObjectiveTile(true);
            ObjectiveTile = centerTile;
        }


    }

    public void GenerateBoard(float mapSize, GameMode gameMode, List<BoardTile> tiles)
    {
        float yOffset1 = 0f;

        //#if UNITY_EDITOR
        //        yOffset1 = yOffset;
        //#else
        yOffset1 = yOffset * _cellScaleFactorY;
        //#endif


        float xPos = initialXpos;
        float yPos = initialYpos;

        var scaleX = hexPrefab.GetComponent<Hex>().transform.localScale.x;
        var scaleY = hexPrefab.GetComponent<Hex>().transform.localScale.y;

        float currentXOffset = 0.0f;
        float currentYOffset = 0.0f;



        //Modificamos escala
        if (scaleX != 0)
            currentXOffset = xOffset - (xOffset * (1f - scaleX));

        if (scaleY != 0)
            currentYOffset = yOffset1 - (yOffset1 * (1f - scaleY));

        Hex centerTile = default(Hex);
        int tileIndex = 0;

        //Por cada nivel (núermo de niveles + número de niveles vacíos)
        for (int level = 0; level < mapSize; level++)
        {
            //Es un nivel vacío
            var isEmptyLevel = level >= mapSize;


            //El nivel 0 solo contendrá una celda
            if (level == 0)
            {
                xPos = 0;
                yPos = 0;

                var posVector = new Vector3(xPos, yPos, 0);
                var tile = tiles.Where(c => c.index == tileIndex).FirstOrDefault();
                InstatiateHexTile(posVector, 0, 0, 0f, 0f, isObjectiveTile: false, isEmptyLevel: isEmptyLevel, tile: tile);
                tileIndex++;
            }
            else
            {
                //El número de celdas es el nivel por 6. Esto trazará un diseñio en forma de panel
                var levelTilesNumber = level * 6f;

                float yIncrement = 0.5f;
                float xIncrement = 1f;

                float xMultipler = 0;
                xMultipler = level * (xMultipler + xIncrement);

                float yMultipler = 0;
                yMultipler = level * (yMultipler + yIncrement);

                var negativeXtiles = 0;
                var positiveXtiles = 0;


                //Se instanciará cada celda comenzando por la derecaha y siguiendo la dirección contraria de las agujas del reloj
                for (int tileNum = 0; tileNum < levelTilesNumber; tileNum++)
                {
                    xPos = (xMultipler * currentXOffset) * 1.030f;
                    yPos = (yMultipler * currentYOffset) * 1.030f;

                    var posVector = new Vector3(xPos, yPos, 0);



                    //Marcamos la celda como actual, siempre se posivionará en el último nivel arriba -90 grados (x = 0, y = level)
                    bool isCurrentUserTile = false;

                    if (gameMode != GameMode.VsAlgorithm)
                        isCurrentUserTile = level == (mapSize - 1) && xMultipler == 0 && yMultipler == level;
                    else
                        isCurrentUserTile = level == (mapSize - 1) && tileNum == 0;

                    bool isCurrentOpponentTile = false;

                    if (gameMode != GameMode.VsAlgorithm)
                        isCurrentOpponentTile = level == (mapSize - 1) && xMultipler == 0 && yMultipler == -level;
                    else
                        isCurrentOpponentTile = level == (mapSize - 1) && tileNum == ((levelTilesNumber / 3));



                    //Instanciamos celda
                    var tile = tiles.Where(c => c.index == tileIndex).FirstOrDefault();

                    Hex hex_go = InstatiateHexTile(posVector, level, tileNum, xMultipler, yMultipler, isCurrentUserTile: isCurrentUserTile,
                        isEmptyLevel: isEmptyLevel, isCurrentOpponentTile: isCurrentOpponentTile, tile: tile);

                    tileIndex++;

                    if (gameMode == GameMode.VsAlgorithm && (level == (mapSize - 1) && tileNum == 12))
                    {
                        centerTile = hex_go;
                    }

                    //Calculamos los valore de xMultipler y yMultipler para calcular la posición de la próxima celda
                    if (tileNum < level)
                    {
                        xMultipler = xMultipler - xIncrement;
                        yMultipler = yMultipler + yIncrement;
                    }
                    else if (tileNum == level)
                    {
                        xMultipler = xMultipler - xIncrement;
                        yMultipler = yMultipler - yIncrement;
                    }
                    else
                    {
                        if (xMultipler < 0)
                        {
                            if (xMultipler == -(level) && negativeXtiles == 0)
                            {
                                negativeXtiles++;
                            }

                            if (negativeXtiles == 1)
                            {
                                yIncrement = 1;
                            }

                            if (negativeXtiles == 0)
                            {
                                xMultipler = xMultipler - xIncrement;
                                yMultipler = yMultipler - yIncrement;
                            }
                            else if (negativeXtiles > 0 && negativeXtiles < (level + 1))
                            {
                                negativeXtiles++;
                                yMultipler = yMultipler - yIncrement;
                            }
                            else
                            {
                                yIncrement = 0.5f;
                                xMultipler = xMultipler + xIncrement;
                                yMultipler = yMultipler - yIncrement;
                            }
                        }
                        else
                        {
                            if (xMultipler == (level) && positiveXtiles == 0)
                            {
                                positiveXtiles++;
                            }

                            if (positiveXtiles == 1)
                            {
                                yIncrement = 1;
                            }

                            if (positiveXtiles == 0)
                            {
                                xMultipler = xMultipler + xIncrement;
                                yMultipler = yMultipler + yIncrement;
                            }
                            else if (positiveXtiles > 0 && positiveXtiles < (level + 1))
                            {
                                positiveXtiles++;
                                yMultipler = yMultipler + yIncrement;
                            }
                            else
                            {
                                yIncrement = 0.5f;
                                xMultipler = xMultipler - xIncrement;
                                yMultipler = yMultipler + yIncrement;
                            }

                        }
                    }

                }
            }

        }

        var objectiveTile = tiles.Where(c => c.isObjectiveTile).FirstOrDefault();
        if (objectiveTile != null)
        {
            var objTile = gridTiles.Where(c => c.name == objectiveTile.name).FirstOrDefault();
            if (objTile != null)
            {
                objTile.ToggleObjectiveTile(true);
                ObjectiveTile = objTile;
            }
        }
    }


    /// <summary>
    /// Se genera una nueva instancia de una celda
    /// </summary>
    /// <param name="posVector">Posición</param>
    /// <param name="level">Nível de profundidad en el que se ha creado</param>
    /// <param name="tileNum">Númer de la celda en ese nivel</param>
    /// <param name="x">Posición X relativa</param>
    /// <param name="y">Posición Y relativa</param>
    /// <param name="isObjectiveTile">Es celda objetivo</param>
    /// <param name="isCurrentUserTile">Es celda actual</param>
    /// <param name="isEmptyLevel">Pertenece a un nivel vacío</param>
    private Hex InstatiateHexTile(Vector3 posVector, int level, int tileNum, float x, float y, bool isObjectiveTile = false,
        bool isCurrentUserTile = false, bool isEmptyLevel = false, bool isCurrentOpponentTile = false, BoardTile tile = null)
    {


        //float screenWidth = Screen.width;
        //float screenHeight = Screen.height;
        //float scaleFactor = 0.85f;//(1f - (screenWidth / screenHeight));

        var ubicatePos = new Vector3(posVector.x, posVector.y, posVector.z);

        //#if UNITY_EDITOR
        //        hexPrefab.transform.localScale = new Vector3(tileScale, tileScale, 1.0f);
        //#else
        hexPrefab.transform.localScale = new Vector3(tileScale, tileScale * _cellScaleFactorY, 1.0f);
        //#endif


        Hex hex_go = (Hex)Instantiate(hexPrefab, ubicatePos, Quaternion.identity); //CreateAndAdjustCellSize(hexPrefab, posVector); 

        if (tile != null)
            hex_go.index = tile.index;

        hex_go._timerEnabled = changeLetterTimer > 0;

        if (hex_go._timerEnabled)
        {
            hex_go.timeToChange = changeLetterTimer * 60f;
            hex_go.ToggleTimer(true);
        }



        hex_go.name = $"Hex_{level}_{tileNum}";
        hex_go.level = level;
        hex_go.tileNumber = tileNum;
        hex_go.x = x;
        hex_go.y = y;
        hex_go.tileState = isEmptyLevel ? GameTileState.Blocked : GameTileState.Unselected;

        if (gameType == GameType.Multiplayer)
        {

            if (tile.playerInitial != null && tile.playerInitial == FirebaseInitializer.auth.CurrentUser.UserId)
            {
                PlayerManager.CurrentPlayerTile = CurrentPlayerTile = hex_go;
                PlayerManager.InitialPlayerTile = InitialPlayerTile = hex_go;
            }

            if (tile.playerInitial != null && tile.playerInitial != FirebaseInitializer.auth.CurrentUser.UserId)
            {
                PlayerManager.CurrentOpponentTile = CurrentOpponentTile = hex_go;
                PlayerManager.InitialOpponentTile = InitialOpponentTile = hex_go;
            }
        }
        else
        {
            if (isCurrentUserTile)
            {
                PlayerManager.CurrentPlayerTile = CurrentPlayerTile = hex_go;
                PlayerManager.InitialPlayerTile = InitialPlayerTile = hex_go;
            }

            if (isCurrentOpponentTile)
            {
                PlayerManager.CurrentOpponentTile = CurrentOpponentTile = hex_go;
                PlayerManager.InitialOpponentTile = InitialOpponentTile = hex_go;
            }
        }




        var letter = tile != null ? tile.letter[0] : lang.GetRandomLetter();
        hex_go.SetLetter(false, letter);


        if (isEmptyLevel)
        {
            hex_go.textoLetra.gameObject.SetActive(false); //.GetComponentInChildren<TMPro.TMP_Text>().gameObject.SetActive(false);
        }



        hex_go.transform.SetParent(this.transform, true);



        hex_go.hexlocalScale = hex_go.transform.localScale;

        gridTiles.Add(hex_go);

        //if(tileNum == 0 && level == 0)
        //{
        //    hex_go.Gift.SetActive(true);
        //}

        return hex_go;
    }


    public void DestroyExistingTiles()
    {
        // Destruye todas las celdas actuales
        foreach (var hexTile in gridTiles)
        {
            Destroy(hexTile.gameObject);
        }

        // Limpia la lista de celdas
        gridTiles.Clear();
    }

    void Start()
    {
    }

    public void FreezePlayer()
    {
        if (FrozenPlayerImage != null && !FrozenPlayerImage.activeInHierarchy)
        {
            if (FreezeTrapEffectPlayer != null)
            {
                FreezeTrapEffectPlayer.Play();
            }

            var freezeRenderer = FrozenPlayerImage.GetComponent<Image>();

            // Iniciar con el efecto invisible
            freezeRenderer.color = new Color(1f, 1f, 1f, 0f);

            FrozenPlayerImage.SetActive(true);

            // Animar para que "aparezca"
            freezeRenderer.DOFade(1f, 0.7f).SetDelay(0.6f);
        }
    }

    public void FreezeOpponent()
    {
        if (FrozenOpponentImage != null && !FrozenOpponentImage.activeInHierarchy)
        {
            if (FreezeTrapEffectOpponent != null)
            {
                FreezeTrapEffectOpponent.Play();
            }

            var freezeRenderer = FrozenOpponentImage.GetComponent<Image>();

            // Iniciar con el efecto invisible
            freezeRenderer.color = new Color(1f, 1f, 1f, 0f);

            FrozenOpponentImage.SetActive(true);

            // Animar para que "aparezca"
            freezeRenderer.DOFade(1f, 0.7f).SetDelay(0.6f); // Cambia la duración y el retraso como prefieras
        }
    }

    public void UnFreezeOpponent()
    {
        if (FrozenOpponentImage != null && FrozenOpponentImage.activeInHierarchy)
        {
            if (FreezeTrapEffectOpponent != null)
            {
                FreezeTrapEffectOpponent.Stop();
            }

            var freezeRenderer = FrozenOpponentImage.GetComponent<Image>();


            // Animar para que "aparezca"
            freezeRenderer.DOFade(0f, 0.7f).SetDelay(0.6f).OnComplete(() => { FrozenOpponentImage.SetActive(false); }); // Cambia la duración y el retraso como prefieras            
        }
    }


    public void UnFreezePlayer()
    {
        if (FrozenPlayerImage != null && FrozenPlayerImage.activeInHierarchy)
        {
            if (FreezeTrapEffectPlayer != null)
            {
                FreezeTrapEffectPlayer.Stop();
            }

            var freezeRenderer = FrozenPlayerImage.GetComponent<Image>();

            // Animar para que "aparezca"
            freezeRenderer.DOFade(0f, 0.7f).SetDelay(0.6f).OnComplete(() => { FrozenPlayerImage.SetActive(false); }); // Cambia la duración y el retraso como prefieras            
            ToggleGameEvents(true);
        }
    }


    public void FreezeGamePlayer(GameActor actor)
    {
        if (actor == GameActor.Player && !PlayerIsFrozen)
        {

            FreezePlayer();
            PlayerIsFrozen = true;

            idleTimerFrozenPlayer = maxIdleFrozenTime;
            FrozenPlayerCounter.gameObject.SetActive(true);

            FrozenPlayerCounter.text = Mathf.Ceil(idleTimerFrozenPlayer).ToString();
            ToggleGameEvents(false);

        }

        if (actor == GameActor.Opponent && !OpponentIsFrozen)
        {
            FreezeOpponent();

            OpponentIsFrozen = true;
            idleTimerFrozenOpponent = maxIdleFrozenTime;
            FrozenOpponentCounter.gameObject.SetActive(true);

            FrozenOpponentCounter.text = Mathf.Ceil(idleTimerFrozenOpponent).ToString();
        }
    }

    private bool isresetingTiles = false;

    void Update()
    {
        if (PlayerIsFrozen)
        {
            idleTimerFrozenPlayer -= Time.deltaTime;
            if (idleTimerFrozenPlayer <= 0)
            {
                idleTimerFrozenPlayer = 0.0f; // Reiniciar el temporizador
                                              //TODO: Activar jugador
                PlayerIsFrozen = false;
                CurrentPlayerTile.UnFreeze(GameActor.Player);
                UnFreezePlayer();
            }

            FrozenPlayerCounter.text = Mathf.Ceil(idleTimerFrozenPlayer).ToString();
        }


        if (OpponentIsFrozen)
        {
            idleTimerFrozenOpponent -= Time.deltaTime;
            if (idleTimerFrozenOpponent <= 0)
            {
                idleTimerFrozenOpponent = 0.0f; // Reiniciar el temporizador
                                                //TODO: Activar jugador
                OpponentIsFrozen = false;
                CurrentOpponentTile.UnFreeze(GameActor.Opponent);
                UnFreezeOpponent();
            }

            FrozenOpponentCounter.text = Mathf.Ceil(idleTimerFrozenOpponent).ToString();
        }


        //Si el jugador quiere poner una trampa de congelación
        if (isSettingFreezeTrap && !FreezeTrapMessageDisplayed)
        {
            Background.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            dialogBubble.ShowMessage(DictionaryUtilities.GetTranslation("FreezrTrapInfo", "Seleccione una celda para colocar la trampa."), 2);
            FreezeTrapMessageDisplayed = true;
        }

        //Si el jugador quiere moverse a una celda adyacente
        if (isMovingToTile && !MoveToTileMessageDispalyed)
        {
            Background.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            dialogBubble.ShowMessage(DictionaryUtilities.GetTranslation("MoveToTileInfo", "Seleccione una celda adyacente para moverte hasta ella."), 2);
            //dialogBubble.ShowMessage("Seleccione una celda adyacente para moverse hasta ella.", 2);
            MoveToTileMessageDispalyed = true;
        }



        DeselectTilesByTimePlayer = (selectedTilesPlayer.Count > 1);
        DeselectTilesByTimeOpponent = (selectedTilesOpponent.Count > 1);

        if (DeselectTilesByTimePlayer && !PlayerIsFrozen)
        {
            idleTimerPlayer += Time.deltaTime;
            if (idleTimerPlayer >= maxIdleTime && !isresetingTiles)
            {
                isresetingTiles = true;

                GameEvents.FireEndSelecteTilesTimer(GameActor.Player);

                if (gameType == GameType.Standalone)
                {
                    ResetSelectedTiles(GameActor.Player);
                    idleTimerPlayer = 0.0f; // Reiniciar el temporizador
                    isresetingTiles = false;
                }
                else
                {
                    //var tiles = new List<UpdateTile>();
                    //tiles.AddRange(selectedTilesPlayer.Where(c => CurrentPlayerTile).Select(d => new UpdateTile()
                    //{
                    //    index = d.index,
                    //    tileAction = (int)TileAction.Unselect
                    //}));

                    //Preparamos el objeto de actulización 
                    var updateAction = new GameUpdateAction()
                    {
                        gameId = GameManager.GameData.gameId,
                        createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        action = (int)PlayerAction.ClearTiles,
                        tiles = new List<UpdateTile>(),
                        OponnentUpdated = false,
                        oponnentId = _opponent.Key,
                        resetTimer = true
                    };

                    multiplayerManager.SendGameUpdateAction(updateAction, new List<Hex>());

                }
            }
        }


        if (DeselectTilesByTimeOpponent && !OpponentIsFrozen)
        {
            idleTimerOpponent += Time.deltaTime;
            if (gameType == GameType.Standalone && idleTimerOpponent >= maxIdleTime)
            {
                GameEvents.FireEndSelecteTilesTimer(GameActor.Opponent);
                ResetSelectedTiles(GameActor.Opponent);
                idleTimerOpponent = 0.0f; // Reiniciar el temporizador
            }
        }


        //Si hay que ejecutar la animación de palabra inválida
        if (DoInvalidWordAnimationPlayer)
        {
            StartCoroutine(AnimateInvalidWord(OnInvalidWordPlayerAnimFinished, GameActor.Player));
        }

        //Si hay que ejecutar la animación de palabra inválida
        if (DoInvalidWordAnimationOpponent)
        {
            StartCoroutine(AnimateInvalidWord(OnInvalidWordOpponentAnimFinished, GameActor.Opponent));
        }
    }

    public void ResetOpponentTiles(bool resetTimer)
    {
        if (gameType == GameType.Multiplayer)
        {
            ResetSelectedTiles(GameActor.Opponent);

            if (resetTimer)
                idleTimerOpponent = 0.0f; // Reiniciar el temporizador
        }
    }
    public void ResetTilesPlayer(bool resetTimer)
    {
        ResetSelectedTiles(GameActor.Player);

        if (resetTimer)
        {
            idleTimerPlayer = 0.0f;
            isresetingTiles = false;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnTileSelected += OnTileSelected;
        GameEvents.OnValidateWord += OnValidateWord;
        GameEvents.OnChageLetter += GameEvents_OnFireChageLetter;
        GameEvents.OnSetTileFreezeTrap += GameEvents_OnSetTileFreezeTrap;
        //GameEvents.OnBeginPanEvent += OnBeginPan;
        //GameEvents.OnEndPanEvent += OnEndPan;
        GameEvents.OnReubicateObjects += OnReubicateObjects;
        GameEvents.OnDeselectedTileEvent += GameEvents_OnDeselectedTileEvent;
        //GameEvents.OnEndTimerCountdownEvent += GameEvents_OnEndTimerCountdownEvent;
        //GameEvents.OnFirebaseInitialize += GameEvents_OnFirebaseInitialize;
        GameEvents.OnMoveToTileFinalized += GameEvents_OnMoveToTileFinalized;

    }



    private void OnDestroy()
    {
        GameEvents.OnTileSelected -= OnTileSelected;
        GameEvents.OnValidateWord -= OnValidateWord;
        GameEvents.OnChageLetter -= GameEvents_OnFireChageLetter;
        GameEvents.OnSetTileFreezeTrap -= GameEvents_OnSetTileFreezeTrap;
        //GameEvents.OnBeginPanEvent -= OnBeginPan;
        //GameEvents.OnEndPanEvent -= OnEndPan;
        GameEvents.OnReubicateObjects -= OnReubicateObjects;
        GameEvents.OnDeselectedTileEvent -= GameEvents_OnDeselectedTileEvent;
        //GameEvents.OnEndTimerCountdownEvent -= GameEvents_OnEndTimerCountdownEvent;
        //GameEvents.OnFirebaseInitialize -= GameEvents_OnFirebaseInitialize;
        GameEvents.OnMoveToTileFinalized -= GameEvents_OnMoveToTileFinalized;

    }


    /// <summary>
    /// Recoge el evento cuando se coloca una trampa en el tablero
    /// </summary>
    /// <param name="actor">Actor que invoca el evento</param>
    /// <exception cref="NotImplementedException"></exception>
    private void GameEvents_OnSetTileFreezeTrap(GameActor actor)
    {
        if (actor == GameActor.Player)
        {
            if (FreezTrapPlayerButton != null)
                FreezTrapPlayerButton.OnUseTool(' ', actor);

            if (Background != null && Background.color != new Color(1f, 1f, 1f, 1f))
            {
                Background.color = new Color(1f, 1f, 1f, 1f);
            }

            FreezeTrapMessageDisplayed = false;
        }

        //if (actor == GameActor.Opponent && CurrentOpponentTile != null)
        //{
        //    if (FreezTrapOpponentButton != null)
        //        FreezTrapOpponentButton.GameEvents_OnChageLetter(' ', actor);
        //}
    }


    /// <summary>
    /// Recoge el evento cuando se cambia la letra a la celda actual
    /// </summary>
    /// <param name="letter">Nueva letra</param>
    /// <exception cref="NotImplementedException"></exception>
    public void GameEvents_OnFireChageLetter(char letter, GameActor actor)
    {
        if (actor == GameActor.Player && CurrentPlayerTile != null)
        {

            if (gameType == GameType.Standalone)
            {
                ChangeLetterPlayer(letter);
            }
            else
            {
                var tiles = new List<UpdateTile>();
                tiles.Add(new UpdateTile()
                {
                    index = CurrentPlayerTile.index,
                    letter = letter.ToString(),
                    tileAction = (int)TileAction.ChangeLetter
                });

                //Preparamos el objeto de actulización 
                var updateAction = new GameUpdateAction()
                {
                    gameId = GameManager.GameData.gameId,
                    createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    action = (int)PlayerAction.ReplaceCurrentLetter,
                    tiles = tiles,
                    OponnentUpdated = false,
                    oponnentId = _opponent.Key
                };

                multiplayerManager.SendGameUpdateAction(updateAction, selectedTilesPlayer);
            }
        }

        if (actor == GameActor.Opponent && CurrentOpponentTile != null)
        {
            ChangeLetterOpponent(letter);
        }
    }

    public void ChangeLetterPlayer(char letter)
    {
        CurrentPlayerTile.SetLetter(true, letter);
        EliminaUltimaLetraCartel();
        AddLetraCartel(CurrentPlayerTile, (selectedTilesPlayer.Count - 1), false);

        if (playerChangeLetterButton != null)
            playerChangeLetterButton.OnUseTool(letter, GameActor.Player);
    }

    public void ChangeLetterOpponent(char letter)
    {
        if (gameType == GameType.Standalone)
            CurrentOpponentTile.SetLetter(true, letter);

        EliminaUltimaLetraCartelOpponent();
        AddLetraCartelOpponent(CurrentOpponentTile);

        if (opponentChangeLetterButton != null)
            opponentChangeLetterButton.OnUseTool(letter, GameActor.Opponent);
    }


    /// <summary>
    /// Limpia la palabra y celdas seleccionadas y permite cntinuar desde la última letra
    /// </summary>
    public void LimpiarYContinuarJuegoPlayer(bool updateLetter, bool clearTiles)
    {
        //Limpiamos las celdas seleccionadas
        foreach (var tile in selectedTilesPlayer)
        {
            if (clearTiles)
            {
                tile.ClearTile(updateLetter: updateLetter && tile.TileName() != CurrentPlayerTile.TileName());
            }

            if (!tile.IsObjectiveTile && !tile.IsCurrentPlayerTile && (changeLetterTimer > 0))
                tile.ToggleTimer(true);
        }

        ///Nueva lista de celdas seleccinada, añadimos la actual para comenzar la nueva búsqueda
        selectedTilesPlayer = new List<Hex>();
        selectedTilesPlayer.Add(CurrentPlayerTile);

        PlayerManager.SelectedTilesPlayer = selectedTilesPlayer;

        CurrentPlayerTile.SelectTile(selectedTileScaleFactor, GameActor.Player);
        SetPlayerIconPosition(CurrentPlayerTile.transform, GameActor.Player);

        //Limpiamos el cartel de la palabra y añadimos la actual
        CleanCartel();
        AddLetraCartel(CurrentPlayerTile, 0, animate: false);

        DrawArrowsBetweenSelectedTiles(GameActor.Player);

        idleTimerPlayer = 0f;
    }


    /// <summary>
    /// Limpia la palabra y celdas seleccionadas y permite cntinuar desde la última letra
    /// </summary>
    public void LimpiarYContinuarJuegoOpponent(bool updateLetter)
    {
        //Limpiamos las celdas seleccionadas
        foreach (var tile in selectedTilesOpponent)
        {
            tile.ClearTile(updateLetter: updateLetter && tile.TileName() != CurrentOpponentTile.TileName());

            if (!tile.IsObjectiveTile && !tile.IsCurrentOpponentTile && (changeLetterTimer > 0))
                tile.ToggleTimer(true);
        }

        ///Nueva lista de celdas seleccinada, añadimos la actual para comenzar la nueva búsqueda
        selectedTilesOpponent = new List<Hex>();
        selectedTilesOpponent.Add(CurrentOpponentTile);

        PlayerManager.SelectedTilesOpponent = selectedTilesOpponent;

        CurrentOpponentTile.SelectTile(selectedTileScaleFactor, GameActor.Opponent);
        SetPlayerIconPosition(CurrentOpponentTile.transform, GameActor.Opponent);

        //Limpiamos el cartel de la palabra y añadimos la actual
        CleanCartelOpponent();
        AddLetraCartelOpponent(CurrentOpponentTile);

        DrawArrowsBetweenSelectedTiles(GameActor.Opponent);

        idleTimerOpponent = 0f;
    }


    public IEnumerator ResetOponentWord(Action<bool> callback)
    {
        var selectedTiles = selectedTilesOpponent.Where(c => c != InitialOpponentTile);


        while (selectedTilesOpponent.Count > 1)
        {
            var hex = selectedTilesOpponent[selectedTilesOpponent.Count - 1];

            if (hex != InitialOpponentTile)
            {
                GameEvents_OnDeselectedTileEvent(hex, GameActor.Opponent);
                yield return new WaitForSeconds(1);
            }
            else
            {
                break;
            }
        }


        CurrentOpponentTile = InitialOpponentTile;

        ///Nueva lista de celdas seleccinada, añadimos la actual para comenzar la nueva búsqueda
        selectedTilesOpponent = new List<Hex>();
        selectedTilesOpponent.Add(InitialOpponentTile);

        PlayerManager.SelectedTilesOpponent = selectedTilesOpponent;
        SetPlayerIconPosition(CurrentOpponentTile.transform, GameActor.Opponent);

        //Limpiamos el cartel de la palabra y añadimos la actual
        CleanCartelOpponent();
        AddLetraCartelOpponent(CurrentOpponentTile);

        DrawArrowsBetweenSelectedTiles(GameActor.Opponent);

        idleTimerOpponent = 0f;

        callback(true);
    }

    private float _shakeAnimTiletimer;
    private Vector3 _shakeAnimTilerandomPos;
    [Header("Settings")]
    [Range(0f, 2f)]
    public float _shakeAnimTiletime = 1.2f;
    [Range(0f, 2f)]
    public float _shakeAnimTiledistance = 0.1f;
    [Range(0f, 0.1f)]
    public float _shakeAnimTiledelayBetweenShakes = 0f;
    private IEnumerator AnimateInvalidWord(Action<bool> callback, GameActor actor)
    {
        _shakeAnimTiletimer = 0f;

        while (_shakeAnimTiletimer < _shakeAnimTiletime)
        {
            _shakeAnimTiletimer += Time.deltaTime;

            if (actor == GameActor.Player)
            {
                foreach (var tile in selectedTilesPlayer)
                {
                    _shakeAnimTilerandomPos = tile._shakeAnimTilestartPos + (UnityEngine.Random.insideUnitSphere * _shakeAnimTiledistance);
                    tile.transform.position = _shakeAnimTilerandomPos;
                }
            }

            if (actor == GameActor.Opponent)
            {
                foreach (var tile in selectedTilesOpponent)
                {
                    _shakeAnimTilerandomPos = tile._shakeAnimTilestartPos + (UnityEngine.Random.insideUnitSphere * _shakeAnimTiledistance);
                    tile.transform.position = _shakeAnimTilerandomPos;
                }
            }


            if (_shakeAnimTiledelayBetweenShakes > 0f)
            {
                yield return new WaitForSeconds(_shakeAnimTiledelayBetweenShakes);
            }
            else
            {
                yield return null;
            }
        }

        callback(true);
    }


    /// <summary>
    /// Se jecuta cuando la palabra que se ha comprobado es inválida 
    /// </summary>
    public void PalabraInvalida(GameActor actor)
    {
        //Se podría restar vidas, quitar puntos, ...
        //if (PlayerManager.Lifes > 0)
        //    GameEvents.FireLifeDeleteEvent();
        //else
        //    GameOver();

        //TODO: Lanzamos animación y al acabar se lanza el callback para terminar el proceso.
        audioManager.PlaySound(SoundTypes.INVALID_WORD);

        if (actor == GameActor.Player)
        {
            foreach (var tile in selectedTilesPlayer)
            {
                tile.InvalidTile();
                tile._shakeAnimTilestartPos = tile.transform.position;
            }

            DoInvalidWordAnimationPlayer = true;
        }




        if (actor == GameActor.Opponent)
        {
            foreach (var tile in selectedTilesOpponent)
            {
                tile.InvalidTile();
                tile._shakeAnimTilestartPos = tile.transform.position;
            }

            DoInvalidWordAnimationOpponent = true;
        }





    }

    private void OnInvalidWordPlayerAnimFinished(bool finished)
    {
        if (finished)
        {
            DoInvalidWordAnimationPlayer = false;

            foreach (var tile in selectedTilesPlayer)
            {
                tile.transform.position = tile._shakeAnimTilestartPos;
            }

            PlayerManager.CurrentPlayerTile = CurrentPlayerTile = InitialPlayerTile;
            LimpiarYContinuarJuegoPlayer(updateLetter: false, clearTiles: true);
        }
    }


    public void OnInvalidWordOpponentAnimFinished(bool finished)
    {
        if (finished)
        {
            DoInvalidWordAnimationOpponent = false;

            foreach (var tile in selectedTilesOpponent)
            {
                tile.transform.position = tile._shakeAnimTilestartPos;
            }

            PlayerManager.CurrentOpponentTile = CurrentOpponentTile = InitialOpponentTile;
            LimpiarYContinuarJuegoOpponent(updateLetter: false);
        }
    }

    void ResetSelectedTiles(GameActor actor)
    {
        if (actor == GameActor.Player)
        {
            // Lógica para deseleccionar todas las celdas
            CurrentPlayerTile = InitialPlayerTile; // Suponiendo que tengas estas propiedades definidas en tu clase
            LimpiarYContinuarJuegoPlayer(updateLetter: false, clearTiles: true);
        }

        if (actor == GameActor.Opponent)
        {
            // Lógica para deseleccionar todas las celdas
            CurrentOpponentTile = InitialOpponentTile; // Suponiendo que tengas estas propiedades definidas en tu clase
            LimpiarYContinuarJuegoOpponent(updateLetter: false);
        }
    }


    /// <summary>
    /// Se jecuta cuando la palabra que se ha comporbado es válida pero no se ha conseguido todavía el objetivo
    /// </summary>
    public void PalabraValida(GameActor actor, string word, bool clearTiles)
    {
        if (actor == GameActor.Player)
        {
            audioManager.PlaySound(SoundTypes.VALID_WORD);
            //Si no se ha alcanzado la celda objetivo, marcaremos la última celda como la actual y como la inicial para continuar con la búsqueda
            var lastTile = selectedTilesPlayer[selectedTilesPlayer.Count - 1];
            //lastTile.SetCurrentUserTile();
            CurrentPlayerTile.IsCurrentPlayerTile = false;
            PlayerManager.CurrentPlayerTile = CurrentPlayerTile = lastTile;
            PlayerManager.InitialPlayerTile = InitialPlayerTile = lastTile;

            PlayerWords.Add(word.ToUpper());

            LimpiarYContinuarJuegoPlayer(updateLetter: false, clearTiles: clearTiles);
        }

        if (actor == GameActor.Opponent)
        {
            //audioManager.PlaySound(SoundTypes.VALID_WORD);
            //Si no se ha alcanzado la celda objetivo, marcaremos la última celda como la actual y como la inicial para continuar con la búsqueda
            var lastTile = selectedTilesOpponent[selectedTilesOpponent.Count - 1];
            CurrentOpponentTile.IsCurrentOpponentTile = false;
            PlayerManager.CurrentOpponentTile = CurrentOpponentTile = lastTile;
            PlayerManager.InitialOpponentTile = InitialOpponentTile = lastTile;

            OpponentWords.Add(word.ToUpper());

            LimpiarYContinuarJuegoOpponent(updateLetter: true);
        }





        if (gameMode == GameMode.CatchLetter || gameMode == GameMode.VsAlgorithm)
            CheckObjectiveDistance(actor);
    }

    /// <summary>
    /// Recoge el evento cuando se pulsa el botón de comrpobar palabra.
    /// </summary>
    public void OnValidateWord(GameActor actor)
    {
        string wordToValidate = string.Empty;

        try
        {
            if (actor == GameActor.Player)
            {
                wordToValidate = new string(selectedTilesPlayer.Select(c => c.GetLetter()).ToArray());

                if (selectedTilesPlayer.Count < wordMinLetter)
                {
                    return;
                }

                if (PlayerWords.Contains(wordToValidate.ToUpper()))
                {
                    //TODO: Mostrar mensaje palabra repetida
                    return;
                }

                ToggleGameEvents(false);

                if (resolveWordButton != null)
                {
                    EnableResolveButton(false);
                }
            }

            if (actor == GameActor.Opponent)
            {
                wordToValidate = new string(selectedTilesOpponent.Select(c => c.GetLetter()).ToArray());

                if (selectedTilesOpponent.Count < wordMinLetter)
                {
                    return;
                }

                if (OpponentWords.Contains(wordToValidate.ToUpper()))
                {
                    //TODO: Mostrar mensaje palabra repetida
                    return;
                }
            }


            StartCoroutine(dictionaryManager.ValidateWord(wordToValidate, IsValid =>
            {
                int points = 0;
                GameEvents.FireResolveWordEvent(IsValid, wordToValidate, actor, ref points);

                if (!IsValid)
                {
                    if (gameType == GameType.Standalone)
                    {
                        PalabraInvalida(actor);
                    }
                    else
                    {
                        var tiles = new List<UpdateTile>();
                        tiles.AddRange(selectedTilesPlayer.Select(c => new UpdateTile()
                        {
                            index = c.index,
                            letter = c.GetLetter().ToString(),
                            tileAction = c != InitialPlayerTile ? (int)TileAction.Unselect : (int)TileAction.Select,
                            playerOccupied = c != InitialPlayerTile ? null : _player.Key
                        }));

                        //Preparamos el objeto de actulización 
                        var updateAction = new GameUpdateAction()
                        {
                            gameId = GameManager.GameData.gameId,
                            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            action = (int)PlayerAction.InvalidWord,
                            tiles = tiles,
                            OponnentUpdated = false,
                            oponnentId = _opponent.Key,
                            word = wordToValidate
                        };

                        multiplayerManager.SendGameUpdateAction(updateAction, selectedTilesPlayer, word: wordToValidate);
                    }
                }
                else
                {
                    if (selectedTilesPlayer.Count > 0)
                    {

                        switch (gameMode)
                        {
                            case GameMode.CatchLetter:
                            case GameMode.VsAlgorithm:
                                {
                                    if (actor == GameActor.Player && selectedTilesPlayer.Any(c => c.IsObjectiveTile))
                                    {
                                        WinGame(actor);
                                    }
                                    else if (actor == GameActor.Opponent && selectedTilesOpponent.Any(c => c.IsObjectiveTile))
                                    {
                                        GameEvents.FireGameOverEvent();
                                    }
                                    else
                                    {
                                        if (gameType == GameType.Standalone)
                                        {
                                            PalabraValida(actor, wordToValidate, clearTiles: true);
                                        }
                                        else
                                        {

                                            //Preparamos las celdas
                                            foreach (var tile in selectedTilesPlayer)
                                            {
                                                tile.ClearTile(updateLetter: true && tile.TileName() != CurrentPlayerTile.TileName());

                                                if (!tile.IsObjectiveTile && !tile.IsCurrentPlayerTile && (changeLetterTimer > 0))
                                                    tile.ToggleTimer(true);
                                            }


                                            var tiles = new List<UpdateTile>();
                                            tiles.AddRange(selectedTilesPlayer.Select(c => new UpdateTile()
                                            {
                                                index = c.index,
                                                letter = c.GetLetter().ToString(),
                                                tileAction = c != CurrentPlayerTile ? (int)TileAction.Unselect : (int)TileAction.Select,
                                                playerOccupied = c != CurrentPlayerTile ? null : _player.Key
                                            }));

                                            //Preparamos el objeto de actulización 
                                            var updateAction = new GameUpdateAction()
                                            {
                                                gameId = GameManager.GameData.gameId,
                                                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                                action = (int)PlayerAction.ValidWord,
                                                tiles = tiles,
                                                OponnentUpdated = false,
                                                oponnentId = _opponent.Key,
                                                word = wordToValidate
                                            };

                                            multiplayerManager.SendGameUpdateAction(updateAction, selectedTilesPlayer, word: wordToValidate);
                                        }
                                    }
                                    break;
                                }

                            case GameMode.PointsChallenge:
                                {
                                    var currenPoints = Convert.ToInt32(PointsChallengePanel.GetComponentInChildren<TMP_Text>().text);

                                    if ((currenPoints - points) <= 0)
                                    {
                                        WinGame(actor);
                                    }
                                    else
                                    {
                                        PalabraValida(actor, wordToValidate, clearTiles: true);
                                        StartCoroutine(UpdateChallengePoints(currenPoints, points));
                                    }

                                    break;
                                }

                            default:
                                {
                                    PalabraValida(actor, wordToValidate, true);
                                    break;
                                }
                        }


                    }
                }
            }));
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("MW ERROR: OnValidateWord error: {0}", e.Message));
        }

    }

    private IEnumerator UpdateChallengePoints(int currentPoints, int pointsEarned)
    {
        float duration = 1.5f;
        float timeElapsed = 0f;
        int scoreAtStart = currentPoints;
        int scoreTarget = currentPoints - pointsEarned;

        while (timeElapsed < duration)
        {
            var newGameScore = (int)Mathf.Lerp(scoreAtStart, scoreTarget, timeElapsed / duration);
            PointsChallengePanel.GetComponentInChildren<TMP_Text>().text = newGameScore.ToString();

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        PointsChallengePanel.GetComponentInChildren<TMP_Text>().text = ((int)(currentPoints - pointsEarned)).ToString();

    }


    /// <summary>
    /// Se jecuta cuando se ha ganado el juego tras acertar una palabra con la letra objetivo
    /// </summary>
    private void WinGame(GameActor actor)
    {
        GameEvents.FireWinGameEvent(actor);
    }


    /// <summary>
    /// Comprueba la distancia entre la celda a actual y la celda objetivo y mueve la celda objetivo si ha alcanzado un mínimo
    /// </summary>
    private void CheckObjectiveDistance(GameActor actor)
    {
        float distance = 0f;

        if (actor == GameActor.Player)
        {
            distance = Vector3.Distance(CurrentPlayerTile.GetComponent<RectTransform>().position, ObjectiveTile.GetComponent<RectTransform>().position);
        }

        if (actor == GameActor.Opponent)
        {
            distance = Vector3.Distance(CurrentOpponentTile.GetComponent<RectTransform>().position, ObjectiveTile.GetComponent<RectTransform>().position);
        }

        Debug.Log("Distance to objective: " + distance);

        if (distance < 2f)
        {
            var objectiveLetter = ObjectiveTile.GetLetter();
            Hex farthestNeighbor = null;

            if (gameMode == GameMode.CatchLetter)
            {
                farthestNeighbor = ObjectiveTile.neighbors.ToList()
                    .Where(c => c != null && c.tileState != GameTileState.Blocked && !c.IsCurrentPlayerTile)
                    .OrderByDescending(c => Vector3.Distance(CurrentPlayerTile.GetComponent<RectTransform>().position, c.GetComponent<RectTransform>().position)).First();
            }
            else if (gameMode == GameMode.VsAlgorithm)
            {
                //farthestNeighbor = ObjectiveTile.neighbors.ToList()
                //    .Where(c => c != null && c.tileState != GameTileState.Blocked && !c.IsCurrentPlayerTile && !c.IsCurrentOpponentTile)
                //    .OrderByDescending(c => Vector3.Distance(CurrentPlayerTile.GetComponent<RectTransform>().position, c.GetComponent<RectTransform>().position)).First();

                farthestNeighbor = ObjectiveTile.neighbors.ToList()
                .Where(c => c != null && c.tileState != GameTileState.Blocked && !c.IsCurrentPlayerTile && !c.IsCurrentOpponentTile)
                .OrderByDescending(c =>
                    Vector3.Distance(CurrentPlayerTile.GetComponent<RectTransform>().position, c.GetComponent<RectTransform>().position) +
                    Vector3.Distance(CurrentOpponentTile.GetComponent<RectTransform>().position, c.GetComponent<RectTransform>().position))
                .FirstOrDefault();
            }

            if (farthestNeighbor != null)
            {
                //Si vamos a activar celdas vacías hayque comentar esta línea
                ObjectiveTile.SetLetter(false, farthestNeighbor.GetLetter());

                farthestNeighbor.SetLetter(false, objectiveLetter);
                ObjectiveTile.ToggleObjectiveTile(false);
                farthestNeighbor.ToggleObjectiveTile(true);
                ObjectiveTile = farthestNeighbor;
            }

        }

    }


    /// <summary>
    /// Hace visible una celda vacía
    /// </summary>
    /// <param name="tile">celda vacía</param>
    private void MakeTileVisible(Hex tile)
    {
        tile.ClearTile(updateLetter: false);
        //var textObj = tile.GetComponentInChildren<TMPro.TMP_Text>();
        //tile.GetComponentInChildren<TMPro.TMP_Text>().gameObject.SetActive(true);
        tile.textoLetra.gameObject.SetActive(true);
        tile.tileState = GameTileState.Unselected;

        //if (string.IsNullOrEmpty(tile.textContainer.text))
        //{
        //    var letter = Language.GetRandomLetter();
        //    tile.SetLetter(letter.Letter);
        //}

        if (changeLetterTimer > 0)
            tile.ToggleTimer(true);
    }

    public void DrawArrowsBetweenSelectedTiles(GameActor actor)
    {
        // Ensure we have the arrow prefab assigned
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab is not assigned.");
            return;
        }

        if (actor == GameActor.Player)
        {
            // Clear existing arrows if any
            foreach (Transform child in transform)
            {
                if (child.CompareTag("ArrowPlayer"))
                    Destroy(child.gameObject);
            }

            // Loop through all the selected tiles except the last one
            for (int i = 0; i < selectedTilesPlayer.Count - 1; i++)
            {
                // Use the GetCellMovement method from the Hex class to determine the direction
                var direction = selectedTilesPlayer[i].GetCellMovement(selectedTilesPlayer[i + 1].name);

                // Translate the Hex.Direction to a rotation angle
                float angle = DirectionToAngle(direction);

                // Instantiate the arrow prefab at the position of the current tile
                GameObject arrowInstance = Instantiate(arrowPrefab, selectedTilesPlayer[i].transform.position, Quaternion.identity, transform);

                // Rotate the arrow based on the angle
                arrowInstance.transform.Rotate(Vector3.forward, angle);

                // Optionally, adjust the position to be centered between the two tiles
                Vector3 middlePoint = (selectedTilesPlayer[i].transform.position + selectedTilesPlayer[i + 1].transform.position) / 2;
                arrowInstance.transform.position = middlePoint;
                arrowInstance.transform.localScale = arrowInstance.transform.localScale * 0.85f;

                // Tag the arrow instance if you need to reference it later
                arrowInstance.tag = "ArrowPlayer";
            }
        }


        if (actor == GameActor.Opponent)
        {
            // Clear existing arrows if any
            foreach (Transform child in transform)
            {
                if (child.CompareTag("ArrowOpponent"))
                    Destroy(child.gameObject);
            }

            // Loop through all the selected tiles except the last one
            for (int i = 0; i < selectedTilesOpponent.Count - 1; i++)
            {
                // Use the GetCellMovement method from the Hex class to determine the direction
                var direction = selectedTilesOpponent[i].GetCellMovement(selectedTilesOpponent[i + 1].name);

                // Translate the Hex.Direction to a rotation angle
                float angle = DirectionToAngle(direction);

                // Instantiate the arrow prefab at the position of the current tile
                GameObject arrowInstance = Instantiate(arrowPrefab, selectedTilesOpponent[i].transform.position, Quaternion.identity, transform);

                // Rotate the arrow based on the angle
                arrowInstance.transform.Rotate(Vector3.forward, angle);

                // Optionally, adjust the position to be centered between the two tiles
                Vector3 middlePoint = (selectedTilesOpponent[i].transform.position + selectedTilesOpponent[i + 1].transform.position) / 2;
                arrowInstance.transform.position = middlePoint;
                arrowInstance.transform.localScale = arrowInstance.transform.localScale * 0.85f;

                // Tag the arrow instance if you need to reference it later
                arrowInstance.tag = "ArrowOpponent";
            }
        }

    }

    // Helper method to translate Hex.Direction to a rotation angle
    private float DirectionToAngle(CellMovement direction)
    {
        switch (direction)
        {
            case CellMovement.Top:
                return 180f;
            case CellMovement.Bottom:
                return 0f;
            case CellMovement.RightUp:
                return 125f;
            case CellMovement.LeftUp:
                return -125f;
            case CellMovement.RightDown:
                return 45f;
            case CellMovement.LeftDown:
                return -45f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Evento que se lanza al pulsar sobre la última celda seleccionada o celda actual para desmarcarla y dejar la anterior como la actual celda.
    /// </summary>
    /// <param name="hex"></param>
    private void GameEvents_OnDeselectedTileEvent(Hex hex, GameActor actor)
    {
        try
        {
            if (actor == GameActor.Player)
            {
                if (selectedTilesPlayer.Count > 1)
                {
                    if (gameType == GameType.Multiplayer)
                    {
                        var tiles = new List<UpdateTile>();
                        tiles.Add(new UpdateTile()
                        {
                            index = hex.index,
                            tileAction = (int)TileAction.Unselect,
                            playerOccupied = null
                        });

                        //Preparamos el objeto de actulización 
                        var updateAction = new GameUpdateAction()
                        {
                            gameId = GameManager.GameData.gameId,
                            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            action = (int)PlayerAction.TileUnselected,
                            tiles = tiles,
                            OponnentUpdated = false,
                            oponnentId = _opponent.Key
                        };

                        multiplayerManager.SendGameUpdateAction(updateAction, new List<Hex>() { hex });
                    }
                    else
                    {
                        DeselectPlayerTile(hex);
                    }
                }
            }

            if (actor == GameActor.Opponent)
            {
                DeselectOpponentTile(hex);
            }

        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }


    public void DeselectPlayerTile(Hex hex)
    {
        EliminaUltimaLetraCartel();
        hex.ClearTile(updateLetter: false);
        selectedTilesPlayer.RemoveAt(selectedTilesPlayer.Count - 1);
        PlayerManager.SelectedTilesPlayer = selectedTilesPlayer;

        selectedTilesPlayer[selectedTilesPlayer.Count - 1].SelectTile(selectedTileScaleFactor, GameActor.Player);
        PlayerManager.CurrentPlayerTile = CurrentPlayerTile = selectedTilesPlayer[selectedTilesPlayer.Count - 1];
        audioManager.PlaySound(SoundTypes.DESELECT_TILE);


        var playerIconColor = playerIcon.GetComponent<Image>().color;
        playerIconColor.a = 0.75f;
        playerIcon.GetComponent<Image>().color = playerIconColor;

        GameObject torbellino = playerIcon.GetComponent<PlayerManager>().swirl;

        if (torbellino != null)
        {

            // Si hay una animación en curso, programar el inicio de la nueva animación después de un breve retardo
            if (currentSwirlCoroutinePlayer != null)
            {
                StopCoroutine(currentSwirlCoroutinePlayer);
                StartCoroutine(WaitAndStartNewSwirlAnimation(CurrentPlayerTile, TileAction.Unselect, 0.1f, GameActor.Player)); // Espera 0.1 segundos antes de comenzar la nueva animación
            }
            else
            {
                // Inicia la nueva animación del torbellino hacia la nueva posición
                currentSwirlCoroutinePlayer = StartCoroutine(MoveTorbellino(CurrentPlayerTile, TileAction.Unselect, GameActor.Player));
            }
        }

        DrawArrowsBetweenSelectedTiles(GameActor.Player);

        if (resolveWordButton != null)
        {
            EnableResolveButton(selectedTilesPlayer.Count >= wordMinLetter);
        }

        if (selectedTilesPlayer.Count == 1)
        {
            idleTimerPlayer = 0f;
        }
    }




    /// <summary>
    /// Recoge el evento al seleccionar una celda
    /// </summary>
    /// <param name="hex">celda seleccionada</param>
    public void OnTileSelected(Hex hex, GameActor actor)
    {
        if (actor == GameActor.Player)
        {
            var addedLetter = false;

            if (selectedTilesPlayer.Count > 0)
            {
                //Si la celdas seleccionadas no contienen la celda y ésta es vecina de la celda actual (o última celda seleccionada, la seleccionamos)
                if (!selectedTilesPlayer.Contains(hex) && !selectedTilesOpponent.Contains(hex) && hex.IsNeighbour(selectedTilesPlayer[selectedTilesPlayer.Count - 1].name))
                {
                    //hex.SelectTile(selectedTileScaleFactor, actor);
                    addedLetter = true;
                }
            }
            else
            {
                if (!selectedTilesOpponent.Contains(hex))
                {
                    //Si no hay celdas seleccionadas, se selecciona
                    //hex.SelectTile(selectedTileScaleFactor, actor);
                    addedLetter = true;
                }
            }

            //Si se ha permitido la selección de la celda
            if (addedLetter)
            {
                if (gameType == GameType.Multiplayer)
                {
                    var tiles = new List<UpdateTile>();
                    tiles.Add(new UpdateTile()
                    {
                        index = hex.index,
                        tileAction = (int)TileAction.Select,
                        playerOccupied = FirebaseInitializer.auth.CurrentUser.UserId
                    });

                    //Preparamos el objeto de actulización 
                    var updateAction = new GameUpdateAction()
                    {
                        gameId = GameManager.GameData.gameId,
                        createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        action = (int)PlayerAction.TileSelected,
                        tiles = tiles,
                        OponnentUpdated = false,
                        oponnentId = _opponent.Key
                    };

                    multiplayerManager.SendGameUpdateAction(updateAction, new List<Hex>() { hex });
                }
                else
                {
                    SelectPlayerTile(hex);
                }
            }
        }


        if (actor == GameActor.Opponent)
        {
            SelectOpponentTile(hex);
        }

    }

    public void SelectPlayerTile(Hex hex)
    {
        hex.SelectTile(selectedTileScaleFactor, GameActor.Player);

        audioManager.PlaySound(SoundTypes.SELECT_TILE);

        //Desactivamos timer de cambio de letra
        if (changeLetterTimer > 0)
            hex.ToggleTimer(false);

        selectedTilesPlayer.ForEach(c =>
        {
            c.IsCurrentPlayerTile = false;
            c.ResetScale(0.9f);
        });

        //selectedTiles.Any(c => { c.IsCurrentUserTile = false; c.ResetScale(); return true; });
        //La añadimos a la lista de seleccionadas
        selectedTilesPlayer.Add(hex);

        //La añadimos al cartel de la palabra
        AddLetraCartel(hex, (selectedTilesPlayer.Count - 1), animate: false);
        //La marcamos como celda actual
        PlayerManager.CurrentPlayerTile = CurrentPlayerTile = hex;


        var playerIconColor = playerIcon.GetComponent<Image>().color;
        playerIconColor.a = 0.75f;
        playerIcon.GetComponent<Image>().color = playerIconColor;

        GameObject torbellino = playerIcon.GetComponent<PlayerManager>().swirl;

        if (torbellino != null)
        {
            // Si hay una animación en curso, programar el inicio de la nueva animación después de un breve retardo
            if (currentSwirlCoroutinePlayer != null)
            {
                StopCoroutine(currentSwirlCoroutinePlayer);
                StartCoroutine(WaitAndStartNewSwirlAnimation(hex, TileAction.Select, 0.1f, GameActor.Player)); // Espera 0.1 segundos antes de comenzar la nueva animación
            }
            else
            {
                //// Activa el torbellino
                //torbellino.SetActive(true);

                // Inicia la nueva animación del torbellino hacia la nueva posición
                currentSwirlCoroutinePlayer = StartCoroutine(MoveTorbellino(hex, TileAction.Select, GameActor.Player));
            }

        }

        if (resolveWordButton != null)
        {
            EnableResolveButton(selectedTilesPlayer.Count >= wordMinLetter);
        }

        DrawArrowsBetweenSelectedTiles(GameActor.Player);
    }

    public void SelectOpponentTile(Hex hex)
    {
        //audioManager.PlaySound(SoundTypes.SELECT_TILE);
        var addedLetter = false;

        if (selectedTilesOpponent.Count > 0)
        {
            //Si la celdas seleccionadas no contienen la celda y ésta es vecina de la celda actual (o última celda seleccionada, la seleccionamos)
            if (!selectedTilesOpponent.Contains(hex) && !selectedTilesPlayer.Contains(hex) && hex.IsNeighbour(selectedTilesOpponent[selectedTilesOpponent.Count - 1].name))
            {
                hex.SelectTile(selectedTileScaleFactor, GameActor.Opponent);
                addedLetter = true;
            }
        }
        else
        {
            if (!selectedTilesPlayer.Contains(hex))
            {
                //Si no hay celdas seleccionadas, se selecciona
                hex.SelectTile(selectedTileScaleFactor, GameActor.Opponent);
                addedLetter = true;
            }
        }

        //Si se ha permitido la selección de la celda
        if (addedLetter)
        {

            //Desactivamos timer de cambio de letra
            if (changeLetterTimer > 0)
                hex.ToggleTimer(false);

            selectedTilesOpponent.ForEach(c =>
            {
                c.IsCurrentOpponentTile = false;
                c.ResetScale(0.9f);
            });

            //selectedTiles.Any(c => { c.IsCurrentUserTile = false; c.ResetScale(); return true; });
            //La añadimos a la lista de seleccionadas
            selectedTilesOpponent.Add(hex);

            //La añadimos al cartel de la palabra
            AddLetraCartelOpponent(hex);
            //La marcamos como celda actual
            PlayerManager.CurrentOpponentTile = CurrentOpponentTile = hex;


            var opponentIconColor = opponentIcon.GetComponent<Image>().color;
            opponentIconColor.a = 0.75f;
            opponentIcon.GetComponent<Image>().color = opponentIconColor;

            GameObject torbellino = opponentIcon.GetComponent<PlayerManager>().swirl;

            if (torbellino != null)
            {
                // Si hay una animación en curso, programar el inicio de la nueva animación después de un breve retardo
                if (currentSwirlCoroutineOpponent != null)
                {
                    StopCoroutine(currentSwirlCoroutineOpponent);
                    StartCoroutine(WaitAndStartNewSwirlAnimation(hex, TileAction.Select, 0.1f, GameActor.Opponent)); // Espera 0.1 segundos antes de comenzar la nueva animación
                }
                else
                {
                    //// Activa el torbellino
                    //torbellino.SetActive(true);

                    // Inicia la nueva animación del torbellino hacia la nueva posición
                    currentSwirlCoroutineOpponent = StartCoroutine(MoveTorbellino(hex, TileAction.Select, GameActor.Opponent));
                }

            }

            //if (resolveWordButton != null)
            //{
            //    EnableResolveButton(selectedTilesPlayer.Count >= wordMinLetter);
            //}

            DrawArrowsBetweenSelectedTiles(GameActor.Opponent);

        }
    }

    public void DeselectOpponentTile(Hex hex)
    {
        if (selectedTilesOpponent.Count > 1)
        {
            EliminaUltimaLetraCartelOpponent();
            hex.ClearTile(updateLetter: false);
            selectedTilesOpponent.RemoveAt(selectedTilesOpponent.Count - 1);
            PlayerManager.SelectedTilesOpponent = selectedTilesOpponent;

            selectedTilesOpponent[selectedTilesOpponent.Count - 1].SelectTile(selectedTileScaleFactor, GameActor.Opponent);
            PlayerManager.CurrentOpponentTile = CurrentOpponentTile = selectedTilesOpponent[selectedTilesOpponent.Count - 1];
            //audioManager.PlaySound(SoundTypes.DESELECT_TILE);


            var opponentIconColor = opponentIcon.GetComponent<Image>().color;
            opponentIconColor.a = 0.75f;
            opponentIcon.GetComponent<Image>().color = opponentIconColor;

            GameObject torbellino = opponentIcon.GetComponent<PlayerManager>().swirl;

            if (torbellino != null)
            {

                // Si hay una animación en curso, programar el inicio de la nueva animación después de un breve retardo
                if (currentSwirlCoroutineOpponent != null)
                {
                    StopCoroutine(currentSwirlCoroutineOpponent);
                    StartCoroutine(WaitAndStartNewSwirlAnimation(CurrentOpponentTile, TileAction.Unselect, 0.1f, GameActor.Opponent)); // Espera 0.1 segundos antes de comenzar la nueva animación
                }
                else
                {
                    // Inicia la nueva animación del torbellino hacia la nueva posición
                    currentSwirlCoroutineOpponent = StartCoroutine(MoveTorbellino(CurrentOpponentTile, TileAction.Unselect, GameActor.Opponent));
                }
            }

            DrawArrowsBetweenSelectedTiles(GameActor.Opponent);
        }

        if (selectedTilesOpponent.Count == 1)
        {
            idleTimerOpponent = 0f;
        }


    }

    private IEnumerator WaitAndStartNewSwirlAnimation(Hex hex, TileAction action, float delay, GameActor actor)
    {
        yield return new WaitForSeconds(delay); // Espera por el tiempo de retardo

        if (actor == GameActor.Player)
            currentSwirlCoroutinePlayer = StartCoroutine(MoveTorbellino(hex, action, actor));

        if (actor == GameActor.Opponent)
            currentSwirlCoroutineOpponent = StartCoroutine(MoveTorbellino(hex, action, actor));
    }


    public Coroutine currentSwirlCoroutinePlayer;
    public Coroutine currentSwirlCoroutineOpponent;
    private IEnumerator MoveTorbellino(Hex hex, TileAction action, GameActor actor)
    {
        if (actor == GameActor.Player)
        {
            GameObject torbellino = playerIcon.GetComponent<PlayerManager>().swirl;

            // Activa el torbellino
            torbellino.SetActive(true);

            torbellino.transform.localScale = _initialSwirlScale;
            playerIcon.transform.localScale = _initialPlayerIconScale;

            float elapsedTime = 0f;
            float duration = 1.0f; // Duración de la animación
            float alphaRampDuration = duration * 0.15f; // Duración de la interpolación de alfa para aparecer y desaparecer
            Vector3 startPosition = playerIcon.transform.position;
            Vector3 endPosition = new Vector3(hex.transform.position.x + playerIconXoffset, hex.transform.position.y + playerIconYoffset, hex.transform.position.z);//hex.transform.position; // Asegúrate de que esta es la posición correcta para el hex seleccionado.
            Vector3 startScale = torbellino.transform.localScale; // Escala inicial
            Vector3 midScale = startScale * 4.2f; // Hacer el Torbellino 2 veces más grande a mitad de camino
                                                  // Guarda la opacidad inicial y final
                                                  // Establece la opacidad inicial y final para la desaparición y aparición
            float initialAlpha = 0.75f;
            float disappearAlpha = 0f;
            float finalAlpha = 1.0f;

            Vector3 playerIconStartScale = playerIcon.transform.localScale; // Escala inicial
            Vector3 playerIconMidScale = playerIconStartScale * 0.25f; // Hacer el Torbellino 2 veces más grande a mitad de camino


            while (elapsedTime < duration)
            {
                // Interpola la posición del playerIcon hacia la nueva celda
                playerIcon.transform.position = torbellino.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));

                if (elapsedTime <= duration / 2f)
                {
                    // Aumenta la escala hasta la mitad del tiempo
                    torbellino.transform.localScale = Vector3.Lerp(startScale, midScale, (elapsedTime / (duration / 2f)));
                }
                else
                {
                    // Reduce la escala de vuelta al tamaño original después de la mitad del tiempo
                    torbellino.transform.localScale = Vector3.Lerp(midScale, startScale, ((elapsedTime - duration / 2f) / (duration / 2f)));
                }


                // Durante el primer 25% de la animación, disminuir el alpha del playerIcon
                if (elapsedTime <= alphaRampDuration)
                {
                    playerIcon.transform.localScale = Vector3.Lerp(playerIconStartScale, playerIconMidScale, elapsedTime / alphaRampDuration);
                    float alpha = Mathf.Lerp(initialAlpha, disappearAlpha, elapsedTime / alphaRampDuration);
                    playerIcon.GetComponent<Image>().color = new Color(playerIcon.GetComponent<Image>().color.r, playerIcon.GetComponent<Image>().color.g, playerIcon.GetComponent<Image>().color.b, alpha);
                }
                // Durante el último 25% de la animación, aumentar el alpha del playerIcon
                else if (elapsedTime >= duration - alphaRampDuration)
                {
                    playerIcon.transform.localScale = Vector3.Lerp(playerIconMidScale, playerIconStartScale, (elapsedTime - (duration - alphaRampDuration)) / alphaRampDuration);
                    float alpha = Mathf.Lerp(disappearAlpha, finalAlpha, (elapsedTime - (duration - alphaRampDuration)) / alphaRampDuration);
                    playerIcon.GetComponent<Image>().color = new Color(playerIcon.GetComponent<Image>().color.r, playerIcon.GetComponent<Image>().color.g, playerIcon.GetComponent<Image>().color.b, alpha);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            currentSwirlCoroutinePlayer = null;

            torbellino.SetActive(false);

            // Asegura que playerIcon y torbellino están en la posición final y que el torbellino tiene su escala original
            playerIcon.transform.position = endPosition; // Actualiza esta línea si playerIcon no debe moverse con el torbellino
            playerIcon.transform.localScale = playerIconStartScale;
            torbellino.transform.position = endPosition;
            torbellino.transform.localScale = startScale;

            switch (action)
            {
                case TileAction.Select:

                    break;

                case TileAction.Unselect:
                    {
                        //hex.ClearTile(updateLetter: false);
                        //selectedTiles.RemoveAt(selectedTiles.Count - 1);
                        //PlayerManager.SelectedTiles = selectedTiles;
                        //EliminaUltimaLetraCartel();
                        //selectedTiles[selectedTiles.Count - 1].SelectTile(selectedTileScaleFactor);
                        //PlayerManager.CurrentTile = CurrentUserTile = selectedTiles[selectedTiles.Count - 1];
                        //audioManager.PlaySound(SoundTypes.DESELECT_TILE);
                    }
                    break;
            }

            //if (resolveWordButton != null)
            //{
            //    EnableResolveButton(selectedTiles.Count >= wordMinLetter);
            //}
        }

        if (actor == GameActor.Opponent)
        {
            GameObject torbellino = opponentIcon.GetComponent<PlayerManager>().swirl;

            // Activa el torbellino
            torbellino.SetActive(true);

            torbellino.transform.localScale = _initialSwirlScale;
            playerIcon.transform.localScale = _initialPlayerIconScale;

            float elapsedTime = 0f;
            float duration = 1.0f; // Duración de la animación
            float alphaRampDuration = duration * 0.15f; // Duración de la interpolación de alfa para aparecer y desaparecer
            Vector3 startPosition = opponentIcon.transform.position;
            Vector3 endPosition = new Vector3(hex.transform.position.x + playerIconXoffset, hex.transform.position.y + playerIconYoffset, hex.transform.position.z);//hex.transform.position; // Asegúrate de que esta es la posición correcta para el hex seleccionado.
            Vector3 startScale = torbellino.transform.localScale; // Escala inicial
            Vector3 midScale = startScale * 4.2f; // Hacer el Torbellino 2 veces más grande a mitad de camino
                                                  // Guarda la opacidad inicial y final
                                                  // Establece la opacidad inicial y final para la desaparición y aparición
            float initialAlpha = 0.75f;
            float disappearAlpha = 0f;
            float finalAlpha = 1.0f;

            Vector3 playerIconStartScale = opponentIcon.transform.localScale; // Escala inicial
            Vector3 playerIconMidScale = playerIconStartScale * 0.25f; // Hacer el Torbellino 2 veces más grande a mitad de camino


            while (elapsedTime < duration)
            {
                // Interpola la posición del playerIcon hacia la nueva celda
                opponentIcon.transform.position = torbellino.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));

                if (elapsedTime <= duration / 2f)
                {
                    // Aumenta la escala hasta la mitad del tiempo
                    torbellino.transform.localScale = Vector3.Lerp(startScale, midScale, (elapsedTime / (duration / 2f)));
                }
                else
                {
                    // Reduce la escala de vuelta al tamaño original después de la mitad del tiempo
                    torbellino.transform.localScale = Vector3.Lerp(midScale, startScale, ((elapsedTime - duration / 2f) / (duration / 2f)));
                }


                // Durante el primer 25% de la animación, disminuir el alpha del playerIcon
                if (elapsedTime <= alphaRampDuration)
                {
                    opponentIcon.transform.localScale = Vector3.Lerp(playerIconStartScale, playerIconMidScale, elapsedTime / alphaRampDuration);
                    float alpha = Mathf.Lerp(initialAlpha, disappearAlpha, elapsedTime / alphaRampDuration);
                    opponentIcon.GetComponent<Image>().color = new Color(opponentIcon.GetComponent<Image>().color.r, opponentIcon.GetComponent<Image>().color.g, opponentIcon.GetComponent<Image>().color.b, alpha);
                }
                // Durante el último 25% de la animación, aumentar el alpha del playerIcon
                else if (elapsedTime >= duration - alphaRampDuration)
                {
                    opponentIcon.transform.localScale = Vector3.Lerp(playerIconMidScale, playerIconStartScale, (elapsedTime - (duration - alphaRampDuration)) / alphaRampDuration);
                    float alpha = Mathf.Lerp(disappearAlpha, finalAlpha, (elapsedTime - (duration - alphaRampDuration)) / alphaRampDuration);
                    opponentIcon.GetComponent<Image>().color = new Color(opponentIcon.GetComponent<Image>().color.r, opponentIcon.GetComponent<Image>().color.g, opponentIcon.GetComponent<Image>().color.b, alpha);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            currentSwirlCoroutineOpponent = null;

            torbellino.SetActive(false);

            // Asegura que playerIcon y torbellino están en la posición final y que el torbellino tiene su escala original
            opponentIcon.transform.position = endPosition; // Actualiza esta línea si playerIcon no debe moverse con el torbellino
            opponentIcon.transform.localScale = playerIconStartScale;
            torbellino.transform.position = endPosition;
            torbellino.transform.localScale = startScale;

            switch (action)
            {
                case TileAction.Select:

                    break;

                case TileAction.Unselect:
                    {
                    }
                    break;
            }

        }

    }



    /// <summary>
    /// Se añade la celda al cartel de la palabra
    /// </summary>
    /// <param name="hex">celda</param>
    /// <param name="animate">Indica si se realiza la animación</param>
    public void AddLetraCartel(Hex hex, int index, bool animate = true)
    {
        if (currentSignLetterPos == Vector3.zero || (index == 0 && wordLetters.Count == 0))
        {
            var rectTra = resolveWordButton.GetComponent<RectTransform>();
            currentSignLetterPos = new Vector3(rectTra.position.x, rectTra.position.y, rectTra.position.z);
        }

        // Obtener el componente TextMeshPro del objeto Hex seleccionado
        TextMeshPro textMesh = hex.GetComponentInChildren<TextMeshPro>();

        // Duplicar el objeto Hex
        TextMeshPro duplicateHex = Instantiate(textMesh, hex.transform);
        duplicateHex.name = string.Format("Letter_{0}", index);

        var signLetter = new SignLetter() { Index = index, LetterObj = duplicateHex };


        duplicateHex.transform.SetParent(panelLetters.transform, true);
        duplicateHex.transform.localScale = duplicateHex.transform.localScale * 0.7f;

        // Obtener el componente RectTransform de duplicateHex
        RectTransform rectTransform = duplicateHex.GetComponent<RectTransform>();

        duplicateHex.ForceMeshUpdate(); // Asegura que la información del texto esté actualizada
        var textWidth = duplicateHex.GetRenderedValues(true).x; // Obtiene el ancho del texto renderizado sin considerar el zoom

        // Obtener la anchura del objeto en píxeles
        float xPos = 0f;

        switch (hex.GetLetter())
        {
            case 'M':
            case 'W':
                xPos = textWidth * 0.95f;
                break;
            case 'I':
            case 'J':
                xPos = textWidth * 1.81f;
                break;
            default:
                xPos = textWidth * 1.02f;
                break;
        }

        rectTransform.sizeDelta = new Vector2(xPos, rectTransform.sizeDelta.y);
        signLetter.Width = xPos;

        // Agregar el objeto duplicado a la lista
        wordLetters.Add(signLetter);

        currentSignLetterPos = new Vector3((currentSignLetterPos.x + xPos), currentSignLetterPos.y, currentSignLetterPos.z);


        // Animar el objeto duplicado hacia el objetivo usando DG.Tweening
        //DG.Tweening.Sequence sequence = DOTween.Sequence();
        duplicateHex.transform.DOMove(currentSignLetterPos, 1f).SetEase(Ease.OutBounce); // Movimiento con efecto bounceOut

        //duplicateHex.transform.DOScale(Vector3.one * 0.95f, 0.8f).SetEase(Ease.InOutSine);

        // Opcional: agregar efecto de rotación
        duplicateHex.transform.DORotate(new Vector3(0f, 0f, 360f), 1f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine).SetLoops(1, LoopType.Restart); // Rotación continua


    }

    public void AddLetraCartelOpponent(Hex hex)
    {
        OpponentWord.text = OpponentWord.text + hex.GetLetter().ToString();
    }

    // Método para destruir un objeto duplicado por su índice
    public void DestroyDuplicateHex(int index)
    {
        if (index >= 0 && index < wordLetters.Count)
        {
            var hexToDelete = wordLetters.Where(c => c.Index == index).FirstOrDefault();
            wordLetters.Remove(hexToDelete);
            Destroy(hexToDelete.LetterObj);

            currentSignLetterPos = new Vector3(((index == 0) ? currentSignLetterPos.x : (currentSignLetterPos.x - hexToDelete.Width)), currentSignLetterPos.y, currentSignLetterPos.z);
        }
    }

    public void CleanCartel()
    {
        foreach (var hexToDelete in wordLetters)
        {
            //TextMeshProUGUI hexToDelete = duplicatedHexes.Where(c => c.name == string.Format("Letter_{0}", index)).FirstOrDefault();
            Destroy(hexToDelete.LetterObj);
            currentSignLetterPos = Vector3.zero;
        }

        wordLetters.Clear();
    }

    public void CleanCartelOpponent()
    {
        OpponentWord.text = "";
    }

    /// <summary>
    /// Elimina la última letra del cartel de la palabra
    /// </summary>
    public void EliminaUltimaLetraCartel()
    {
        DestroyDuplicateHex((selectedTilesPlayer.Count - 1));
    }

    public void EliminaUltimaLetraCartelOpponent()
    {
        OpponentWord.text = new string(selectedTilesOpponent.Where(c => c.name != selectedTilesOpponent[selectedTilesOpponent.Count - 1].name).
            Select(c => c.GetLetter()).ToArray());
    }



    /// <summary>
    /// Evento que se recoge al modificar la escala del panel de juego para reubicar objetos
    /// </summary>
    /// <param name="newScale"></param>
    private void OnReubicateObjects(GameActor actor, float? newScale = null)
    {
        SetPlayerIconPosition(CurrentPlayerTile.transform, actor, newScale);
    }

    /// <summary>
    /// Establece la posición y escala del icono del jugador
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="newScale"></param>
    public void SetPlayerIconPosition(Transform trans, GameActor actor, float? newScale = null)
    {
        if (actor == GameActor.Player)
        {
            if (newScale.HasValue)
            {
                var newScaleV3 = new Vector3(newScale.Value * 1.25f, newScale.Value * 1.25f, 1f);
                playerIcon.transform.localScale = newScaleV3;
            }

            playerIcon.transform.position = new Vector3(trans.position.x + playerIconXoffset, trans.position.y + playerIconYoffset, trans.position.z);
        }


        if (actor == GameActor.Opponent)
        {
            if (newScale.HasValue)
            {
                var newScaleV3 = new Vector3(newScale.Value * 1.25f, newScale.Value * 1.25f, 1f);
                opponentIcon.transform.localScale = newScaleV3;
            }

            opponentIcon.transform.position = new Vector3(trans.position.x + playerIconXoffset, trans.position.y + playerIconYoffset, trans.position.z);
        }



    }


    /// <summary>
    /// Establece la posición y escala del icono del jugador
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="newScale"></param>
    public void SetPlayerIconPosition(GameActor actor, float? newScale = null)
    {
        if (actor == GameActor.Player)
        {
            if (newScale.HasValue)
            {
                var newScaleV3 = new Vector3(newScale.Value * 1.25f, newScale.Value * 1.25f, 1f);
                playerIcon.transform.localScale = newScaleV3;
            }

            playerIcon.transform.position = new Vector3(CurrentPlayerTile.transform.position.x + playerIconXoffset, CurrentPlayerTile.transform.position.y + playerIconYoffset, CurrentPlayerTile.transform.position.z);
        }


        if (actor == GameActor.Opponent)
        {
            if (newScale.HasValue)
            {
                var newScaleV3 = new Vector3(newScale.Value * 1.25f, newScale.Value * 1.25f, 1f);
                opponentIcon.transform.localScale = newScaleV3;
            }

            opponentIcon.transform.position = new Vector3(CurrentOpponentTile.transform.position.x + playerIconXoffset, CurrentOpponentTile.transform.position.y + playerIconYoffset, CurrentOpponentTile.transform.position.z);
        }

    }

    /// <summary>
    /// Método para asiganar las celdas vecinas de cada celda del tablero
    /// </summary>
    public void AssignTileNeighbors()
    {

        foreach (var tile in gridTiles)
        {
            tile.neighbors.neighbor_TOP = gridTiles.Where(c => c.x == tile.x && c.y == (tile.y + 1f)).Any() ? gridTiles.Where(c => c.x == tile.x && c.y == (tile.y + 1f)).First() : null;
            tile.neighbors.neighbor_BOTTOM = gridTiles.Where(c => c.x == tile.x && c.y == (tile.y - 1f)).Any() ? gridTiles.Where(c => c.x == tile.x && c.y == (tile.y - 1f)).First() : null;

            tile.neighbors.neighbor_LEFTUP = gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y + 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y + 0.5f)).First() : null;
            tile.neighbors.neighbor_LEFTDOWN = gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y - 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y - 0.5f)).First() : null;

            tile.neighbors.neighbor_RIGHTUP = gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y + 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y + 0.5f)).First() : null;
            tile.neighbors.neighbor_RIGHTDOWN = gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y - 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y - 0.5f)).First() : null;
        }

    }


    public void MoveToTile(GameActor actor, Hex newTile)
    {
        if (actor == GameActor.Player)
        {
            if (gameType == GameType.Standalone)
            {
                MoveToTilePlayer(newTile);
            }
            else
            {
                var tiles = new List<UpdateTile>();
                tiles.Add(new UpdateTile()
                {
                    index = CurrentPlayerTile.index,
                    tileAction = (int)TileAction.Unselect
                });

                tiles.Add(new UpdateTile()
                {
                    index = newTile.index,
                    tileAction = (int)TileAction.Select
                });

                //Preparamos el objeto de actulización 
                var updateAction = new GameUpdateAction()
                {
                    gameId = GameManager.GameData.gameId,
                    createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    action = (int)PlayerAction.MoveToTile,
                    tiles = tiles,
                    OponnentUpdated = false,
                    oponnentId = _opponent.Key
                };

                multiplayerManager.SendGameUpdateAction(updateAction, new List<Hex>() { newTile });
            }
        }
    }

    public void MoveToTilePlayer(Hex newTile)
    {
        CurrentPlayerTile.ClearTile(false);

        selectedTilesPlayer = new List<Hex>();
        selectedTilesPlayer.Add(newTile);

        PlayerManager.CurrentPlayerTile = CurrentPlayerTile = newTile;
        PlayerManager.InitialPlayerTile = InitialPlayerTile = newTile;

        PlayerManager.SelectedTilesPlayer = selectedTilesPlayer;

        newTile.SelectTile(selectedTileScaleFactor, GameActor.Player);
        SetPlayerIconPosition(CurrentPlayerTile.transform, GameActor.Player);

        //Limpiamos el cartel de la palabra y añadimos la actual
        CleanCartel();
        AddLetraCartel(newTile, 0, animate: false);

        DrawArrowsBetweenSelectedTiles(GameActor.Player);

        idleTimerPlayer = 0f;

        isMovingToTile = false;

        GameEvents.FireMoveToTileFinalizedEvent(GameActor.Player);
    }

    public void MoveToTileOpponent(Hex newTile)
    {
        CurrentOpponentTile.ClearTile(false);

        selectedTilesOpponent = new List<Hex>
        {
            newTile
        };

        PlayerManager.CurrentOpponentTile = CurrentOpponentTile = newTile;
        PlayerManager.InitialOpponentTile = InitialOpponentTile = newTile;
        PlayerManager.SelectedTilesOpponent = selectedTilesOpponent;

        newTile.SelectTile(selectedTileScaleFactor, GameActor.Opponent);
        SetPlayerIconPosition(CurrentOpponentTile.transform, GameActor.Opponent);

        //Limpiamos el cartel de la palabra y añadimos la actual
        CleanCartelOpponent();
        AddLetraCartelOpponent(newTile);

        DrawArrowsBetweenSelectedTiles(GameActor.Opponent);

        idleTimerOpponent = 0f;

        if (MoveToTileOpponentButton != null)
            MoveToTileOpponentButton.OnUseTool(' ', GameActor.Opponent);
    }

    private void GameEvents_OnMoveToTileFinalized(GameActor actor)
    {
        if (actor == GameActor.Player && CurrentPlayerTile != null)
        {
            if (MoveToTilePlayerButton != null)
                MoveToTilePlayerButton.OnUseTool(' ', actor);

            if (Background != null && Background.color != new Color(1f, 1f, 1f, 1f))
            {
                Background.color = new Color(1f, 1f, 1f, 1f);
            }

            MoveToTileMessageDispalyed = false;
        }
    }


    public void SetFreeztrapOnTilePlayer(Hex hex, float time = 60)
    {
        var tiles = new List<UpdateTile>();
        tiles.Add(new UpdateTile()
        {
            index = hex.index,
            tileAction = (int)TileAction.FreezeTile,
            actionTime = time
        });

        //Preparamos el objeto de actulización 
        var updateAction = new GameUpdateAction()
        {
            gameId = GameManager.GameData.gameId,
            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            action = (int)PlayerAction.FreezeTile,
            tiles = tiles,
            OponnentUpdated = false,
            oponnentId = Map._opponent.Key
        };

        multiplayerManager.SendGameUpdateAction(updateAction, new List<Hex>() { { hex } });
    }


    public void UpdateFreeztrapOnTilePlayer(Hex tile, float time = 60)
    {
        tile.tileState = GameTileState.FreezeTrapFromPlayer;

        tile._displayedImage.sprite = Resources.Load("Art/UI/cell_bg_freeze", typeof(Sprite)) as Sprite;
        if (tile.effectTime != null)
        {
            tile.effectTime.text = ((short)time).ToString();
            tile.isFrozen = true;
            tile.frozenTimeRemaining = time;
            tile.effectTime.gameObject.SetActive(true);
        }


        isSettingFreezeTrap = false;
        GameEvents.FireSetTileFreezeTrapEvent(GameActor.Player);
    }

    public void UpdateFreeztrapOnTileOpponent(Hex tile, float time = 60)
    {
        tile.tileState = GameTileState.FreezeTrapFromOpponent;

        if (FreezTrapOpponentButton != null)
            FreezTrapOpponentButton.OnUseTool(' ', GameActor.Opponent);

    }

    public void FreezePlayer(Hex hex)
    {
        var tiles = new List<UpdateTile>();
        tiles.Add(new UpdateTile()
        {
            index = hex.index,
            tileAction = (int)TileAction.FreezePlayer
        });

        //Preparamos el objeto de actulización 
        var updateAction = new GameUpdateAction()
        {
            gameId = GameManager.GameData.gameId,
            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            action = (int)PlayerAction.FreezePlayer,
            tiles = tiles,
            OponnentUpdated = false,
            oponnentId = Map._opponent.Key
        };

        multiplayerManager.SendGameUpdateAction(updateAction, new List<Hex>() { { hex } });
    }

    public void UpdateFreezePlayer(Hex hex)
    {
        hex.FrozenTrapParticles.Play();
        //TODO: Lanzar efecto de sonido
        hex._displayedImage.sprite = Resources.Load("Art/UI/cell_bg_freeze", typeof(Sprite)) as Sprite;
        FreezeGamePlayer(GameActor.Player);
        hex.isFrozen = false;
    }

}
