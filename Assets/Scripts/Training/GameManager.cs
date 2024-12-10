using Assets.Scripts.Data;
using Assets.Scripts.Training.Data;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Referencia al script Map para acceder a la información del tablero.
    public Map map;
    public AuthFirebaseManager authFirebaseManager;
    public AudioManager audioManager = null;
    public User userData = null;
    public GameObject efectoPalabraValida;
    private TMP_Text textUI;
    private ParticleSystem efecto;
    private Transform palabraValida;

    public GameObject scoreBoxOpponent;

    public GameObject scoreBox;
    public GameObject userBox;
    //public InfiniteScroll rewriteLetterTool;
    public Transform lettersScrollCanvasRect;
    public GameObject lettersScrollPrefab;
    public GameObject lettersScrollbgPanel;
    private int _currentGameScore = 0;
    private int _currentGameScoreOpponent = 0;
    private int _currentPlayerScore = 0;

    public ParticleSystem particleSys;
    public ParticleSystem particleSysOpponent;
    public ParticleSystem scoreGlowParticles;
    public ParticleSystem scoreGlowParticlesOpponent;
    public Transform target; // Asigna el objeto de UI de puntuación (score) en el inspector
    public Transform sourcePlayer; // Asigna el objeto de UI de puntuación (score) en el inspector
    public Transform sourceOpponent; // Asigna el objeto de UI de puntuación (score) en el inspector
    public Transform parent;
    public float duration = 2.0f; // Duración total de la animación
    public float starSpawnFrequency = 0.32f; // Frecuencia con la que se generan estrellas
    public float rotationSpeed = 180f; // Velocidad de rotación
    public Vector3 maxScaleMultiplier = new(4f, 4f, 4f); // Multiplicador de escala máxima
    public Color startColor = new(1, 1, 1, 0.7f); // Color inicial de la estrella
    public Color endColor = new(1, 1, 1, 1); // Color final de la estrella

    private GameObject letterScroll;
    private InfiniteScroll scroll;
    private bool loadLevelScene = false;
    private bool updateScoreAnimationFinalized = true;

    public GameObject phaseCompletedParticles;
    public ParticleSystem[] phaseCompletedConfetties;

    public SandClock sandClock;
    public GameObject PnlGametype;

    private string InitialWord = string.Empty;

    string userLang = LanguageCodes.ES_es;
    Language language = new SpanishLang();

    private GameType gameType;
    //bool userInitialized = false;

    GameMode gameMode = GameMode.VsAlgorithm;
    //GameLevelPhase gameLevelPhase = null;

    //GameStatus gameStatus = GameStatus.Pending;
    public static Game GameData { get; set; }
    private bool multiplayerGameLoaded = false;
    private bool multiplayerReady = false;
    

    private void Awake()
    {

        gameType = (GameType)PlayerPrefs.GetInt("GameType");

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("LANG")))
            userLang = PlayerPrefs.GetString("LANG");

        if (!string.IsNullOrEmpty(userLang))
            language = Language.GetLanguages().Where(c => c.Code == userLang).FirstOrDefault();
    }

    private void Start()
    {

        if (FirebaseInitializer.FirebaseInitialized)
        {
            GameEvents_OnFirebaseInitialize();
        }

        letterScroll = Instantiate(lettersScrollPrefab, lettersScrollCanvasRect);
        scroll = letterScroll.GetComponent<InfiniteScroll>();

        if (scroll != null)
        {
            GameEvents.OnRuneRewriteLetter += scroll.OnRuneRewriteLetter;
            scroll.map = map;
            scroll.backgroundPanel = lettersScrollbgPanel;
            scroll.InitializeRunerewrite();
        }

        efectoPalabraValida.SetActive(false);

        if (scoreBox != null)
        {
            foreach (var textBox in scoreBox.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (textBox.name == "TextTop")
                {
                    textBox.text = "0";
                }
            }
        }

        if (phaseCompletedParticles != null)
        {
            phaseCompletedConfetties = phaseCompletedParticles.GetComponentsInChildren<ParticleSystem>();
        }

    }

    private void Update()
    {
        if (loadLevelScene && updateScoreAnimationFinalized)
        {
            SceneManager.LoadScene("LevelMenu");
        }

        if (gameType == GameType.Standalone)
        {
            if (!string.IsNullOrEmpty(InitialWord))
            {
                var palabra = InitialWord;
                InitialWord = string.Empty;

                var gameTypeImage = PnlGametype.transform.Find("GameTypeImage").GetComponent<Image>();
                var gameTypeText = PnlGametype.GetComponentInChildren<TMP_Text>();


                gameTypeImage.sprite = Resources.Load(string.Format("Art/UI/fases/{0}", (int)gameMode), typeof(Sprite)) as Sprite;
                gameTypeText.text = language.GetGameModeText(gameMode);

                map.CleanCartel();
                map.InitializeGame(userData.level, gameMode, gameType);


                List<Hex> tiles = new List<Hex>();

                short i = 0;
                Hex currentTile = null;

                foreach (char letter in palabra.ToUpper())
                {
                    if (i == 0)
                    {
                        currentTile = map.CurrentPlayerTile;
                    }
                    else
                    {
                        var cellCount = currentTile.neighbors.ToList().Where(c => c != null && !tiles.Contains(c)).Count();
                        int randomCellIndex = UnityEngine.Random.Range(0, (cellCount - 1));

                        currentTile = currentTile.neighbors.ToList().Where(c => c != null && !tiles.Contains(c)).ToList()[randomCellIndex];

                    }

                    currentTile.SetLetter(false, DictionaryUtilities.RemoveDiacritics(letter.ToString())[0]);
                    tiles.Add(currentTile);

                    i++;
                }

                GameEvents.FireGameLoaded();
            }
        }
        else
        {
            if (multiplayerGameLoaded && !multiplayerReady)
            {
                multiplayerReady = true;
                var gameTypeImage = PnlGametype.transform.Find("GameTypeImage").GetComponent<Image>();
                var gameTypeText = PnlGametype.GetComponentInChildren<TMP_Text>();


                gameTypeImage.sprite = Resources.Load(string.Format("Art/UI/fases/{0}", (int)gameMode), typeof(Sprite)) as Sprite;
                gameTypeText.text = language.GetGameModeText(gameMode);

                map.CleanCartel();

                FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/gameBoardLoaded").SetValueAsync(true)
                .ContinueWithOnMainThread(task =>
                {
                    FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/status").ValueChanged += HandleGameStatusIsReady;
                    //TODO: Disable events
                    map.ToggleGameEvents(false);                    

                });
            }
        }
    }

    public void addToGameWaitRoom(Firebase.Auth.FirebaseUser user)
    {
        try
        {
            if (FirebaseInitializer.dbRef != null)
            {
                PlayerWaitRoom playerWaitRoom = null;


                FirebaseInitializer.dbRef.GetReference($"users/{user.UserId}/username").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.Log($"[addToGameWaitRoom] Error retrieving user data: {task.Exception.Message}");
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;

                        playerWaitRoom = new PlayerWaitRoom()
                        {
                            createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            langCode = PlayerPrefs.GetString("LANG"),
                            level = PlayerPrefs.GetInt("LEVEL"),
                            userName = task.Result.Value.ToString()
                        };


                        //Eliminamos las posibles ocurrencians anteriores
                        //await dbRef.RootReference.Child("gameWaitRoom").Child(user.UserId).RemoveValueAsync();

                        //Se añade una nueva entrada
                        string jsonPlayerWaitRoom = Newtonsoft.Json.JsonConvert.SerializeObject(playerWaitRoom);
                        FirebaseInitializer.dbRef.RootReference.Child("gameWaitRoom").Child(user.UserId).SetRawJsonValueAsync(jsonPlayerWaitRoom).ContinueWithOnMainThread(task =>
                        {

                            //Cuando se una otro jugador se generará automaticamente una partida
                            InitializeMultiplayerGame(userData);

                        });

                    }
                });

            }
            else
            {
                throw new Exception("No se ha inicializado FirebaseDatabase dbRef!");
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }

    public void HandleGameStatusIsReady(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        var jasonGameStatus = args.Snapshot.GetRawJsonValue();
        GameStatus gameSattus = Newtonsoft.Json.JsonConvert.DeserializeObject<GameStatus>(jasonGameStatus);

        if(gameSattus != GameData.data.status)
        {
            GameData.data.status = gameSattus;

            if(gameSattus == GameStatus.GameBoardCompleted)
            {
                try
                {
                    //FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/status").ValueChanged -= HandleGameStatusIsReady;
                    map.InitializeGame(userData.level, gameMode, gameType);
                    GameEvents.FireGameLoaded();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    throw;
                }
            }
        }

    }

    private void OnEnable()
    {
        GameEvents.OnEndTimerCountdownEvent += GameEvents_OnEndTimerCountdownEvent; ; //Cuando se acaba el tiempo del juego
        GameEvents.OnWinGame += OnWinGame; //Al ganar el juego
        GameEvents.OnGameOver += OnGameOver; //Al perder el juego.
        GameEvents.OnEndSelecteTilesTimer += GameEvents_OnEndSelecteTilesTimer; //Al acabar el tiempo de selección de celdas
        GameEvents.OnInvalidTileClick += OnInvalidTileClick; //Al acabar el temporizador sin seleccinar ninguna letra, vuelve a la letra de la que partía
        //GameEvents.OnFirebaseInitialize += GameEvents_OnFirebaseInitialize; //Al inicializarse Firebase
        GameEvents.OnResolveWord += GameEvents_OnResolveWord; //Cuando se resuelve una palabra (válida o inválida)
        GameEvents.OnGameLoaded += GameEvents_OnGameLoaded;
        GameEvents.OnFreezeTrap += GameEvents_OnFreezeTrap;
        GameEvents.OnMoveToTile += GameEvents_OnMoveToTile;

    }

    private void GameEvents_OnFreezeTrap()
    {
        if (!map.isSettingFreezeTrap)
        {
            map.isSettingFreezeTrap = true;
        }
    }

    private void GameEvents_OnMoveToTile()
    {
        if (!map.isMovingToTile)
        {
            map.isMovingToTile = true;
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnEndTimerCountdownEvent -= GameEvents_OnEndTimerCountdownEvent;
        GameEvents.OnWinGame -= OnWinGame;
        GameEvents.OnGameOver -= OnGameOver;
        GameEvents.OnEndSelecteTilesTimer -= GameEvents_OnEndSelecteTilesTimer;
        GameEvents.OnInvalidTileClick -= OnInvalidTileClick;
        //GameEvents.OnFirebaseInitialize -= GameEvents_OnFirebaseInitialize;
        GameEvents.OnResolveWord -= GameEvents_OnResolveWord;
        GameEvents.OnRuneRewriteLetter -= scroll.OnRuneRewriteLetter;
        GameEvents.OnGameLoaded -= GameEvents_OnGameLoaded;
        GameEvents.OnFreezeTrap -= GameEvents_OnFreezeTrap;
        GameEvents.OnMoveToTile -= GameEvents_OnMoveToTile;

    }

    private void GameEvents_OnGameLoaded()
    {
        //Seleccionamos la celda actual
        GameEvents.OnTileSelectedMethod(map.CurrentPlayerTile, GameActor.Player);

        //Posicionamos el icono del jugador en la celda actual
        map.SetPlayerIconPosition(map.CurrentPlayerTile.transform, GameActor.Player);

        var timerMinutes = 8.0f; //gameLevelPhase.DifficultyValues.Where(c => c.Key == GameDifficulty.Timer).FirstOrDefault().Value;

        if (gameMode == GameMode.VsAlgorithm || gameType == GameType.Multiplayer)
        {
            map.opponentIcon.SetActive(true);


            //Seleccionamos la celda actual
            GameEvents.OnTileSelectedMethod(map.CurrentOpponentTile, GameActor.Opponent);

            //Posicionamos el icono del oponente en la celda actual
            map.SetPlayerIconPosition(map.CurrentOpponentTile.transform, GameActor.Opponent);

            GameEvents.FireOpponentLoaded();
        }

        sandClock.roundDuration = timerMinutes * 60.0f;
        sandClock.Begin();

        audioManager.PlayMusic(SoundTypes.GAME_MELODY);
    }

    private void SetInitialWord()
    {
        InitialWord = GetRandomWord();
    }

    private string GetRandomWord()
    {
        var randomWord = "";

        var letters = Language.GetLangAbecedary(userLang).Where(c => c.DifficultyGroup == 1 || c.DifficultyGroup == 2).ToList();
        var letter = letters[UnityEngine.Random.Range(0, (letters.Count - 1))];


        var dictionaryPath = $"/dictionary/{userLang}/{letter.Letter.ToString().ToLower()}.txt";
        string filePath = Application.persistentDataPath + dictionaryPath;
        IEnumerable<string> dictionaryContent = null;

        if (System.IO.File.Exists(filePath))
        {
            dictionaryContent = System.IO.File.ReadAllText(filePath).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(c => c.Length > 3 && c.Length < 7);

            // Contamos todos los nodos hijos, que cada uno representaría una palabra
            int wordsCount = dictionaryContent.Count();
            int randomIndex = UnityEngine.Random.Range(0, (wordsCount - 1));

            randomWord = dictionaryContent.ElementAt(randomIndex);
        }
        else
        {
            Debug.LogError($"El archivo de diccionario {string.Format("dictionary/{0}/{1}.txt", userLang, letter.Letter.ToString().ToLower())} no se encontró en el contenedor persistente.");
        }

        return randomWord;
    }

    private void GameEvents_OnEndSelecteTilesTimer(GameActor actor)
    {

    }

    private void GameEvents_OnEndTimerCountdownEvent()
    {
        OnGameOver();
    }

    private void InitializeMultiplayerGame(User userdata)
    {
        var gamesRef = FirebaseInitializer.dbRef.GetReference("games");
        gamesRef.ChildAdded += HandleGameAdded;
    }


    

    public void HandleGameAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        //var game = args.Snapshot;
        var jsonGame = args.Snapshot.GetRawJsonValue();

        try
        {
            GameData gameData = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(jsonGame);

            //Si el juego me incluye como player...
            if (gameData != null && gameData.playersInfo.Where(d => d.Key == FirebaseInitializer.auth.CurrentUser.UserId).Any())
            {
                GameData = new Game();
                GameData.gameId = args.Snapshot.Key;
                GameData.data = gameData;

                GamePlayerData playerInfo = gameData.playersInfo.Where(d => d.Key == FirebaseInitializer.auth.CurrentUser.UserId).FirstOrDefault().Value;

                var otherPlayerId = GameData.data.playersInfo.Where(d => d.Key != FirebaseInitializer.auth.CurrentUser.UserId).FirstOrDefault().Key;

                //Si es el jugador MASTER, se encargará de generar y guardar el tablero
                if (playerInfo != null && playerInfo.master)
                {
                    GameBoard gameBoard = GameBoardMultiplayer.GenerateBoard(4, GameMode.CatchLetter,
                        playerId1: FirebaseInitializer.auth.CurrentUser.UserId, playerId2: otherPlayerId,
                        1f, 1f, language);


                    //Añadir palabras iniciales a los 2 jugadores
                    var playerInitalWord = GetRandomWord();
                    var opponentInitalWord = GetRandomWord();

                    List<BoardTile> tiles = new List<BoardTile>();

                    short i = 0;
                    BoardTile currentTile = null;

                    //Player
                    foreach (char letter in playerInitalWord.ToUpper())
                    {
                        if (i == 0)
                        {
                            currentTile = gameBoard.boardTiles.Where(c => !string.IsNullOrEmpty(c.playerInitial) && c.playerInitial == FirebaseInitializer.auth.CurrentUser.UserId).FirstOrDefault(); //map.CurrentPlayerTile;
                        }
                        else
                        {
                            var neighborns = GetNeighbors(currentTile, gameBoard.boardTiles);

                            var cellCount = neighborns.Where(c => c != null && !tiles.Contains(c)).Count();
                            int randomCellIndex = UnityEngine.Random.Range(0, (cellCount - 1));

                            currentTile = neighborns.Where(c => c != null && !tiles.Contains(c)).ToList()[randomCellIndex];

                        }

                        currentTile.letter = DictionaryUtilities.RemoveDiacritics(letter.ToString()).Trim();
                        tiles.Add(currentTile);

                        i++;
                    }

                    //Opponent
                    i = 0;
                    foreach (char letter in opponentInitalWord.ToUpper())
                    {
                        if (i == 0)
                        {
                            currentTile = gameBoard.boardTiles.Where(c => !string.IsNullOrEmpty(c.playerInitial) && c.playerInitial != FirebaseInitializer.auth.CurrentUser.UserId).FirstOrDefault(); //map.CurrentPlayerTile;
                        }
                        else
                        {
                            var neighborns = GetNeighbors(currentTile, gameBoard.boardTiles);

                            var cellCount = neighborns.Where(c => c != null && !tiles.Contains(c)).Count();
                            int randomCellIndex = UnityEngine.Random.Range(0, (cellCount - 1));

                            currentTile = neighborns.Where(c => c != null && !tiles.Contains(c)).ToList()[randomCellIndex];
                        }

                        currentTile.letter = DictionaryUtilities.RemoveDiacritics(letter.ToString()).Trim();
                        tiles.Add(currentTile);

                        i++;
                    }



                    GameData.data.gameBoard = gameBoard;

                    string jsonGameBoard = Newtonsoft.Json.JsonConvert.SerializeObject(gameBoard);
                    FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/gameBoard").SetRawJsonValueAsync(jsonGameBoard).ContinueWithOnMainThread(task =>
                    {

                        var gamesRef = FirebaseInitializer.dbRef.GetReference("games");
                        gamesRef.ChildAdded -= HandleGameAdded;
                        //Se ha completado la carga en BBDD, podemos cargar la escena MultiplayerGame
                        multiplayerGameLoaded = true;
                    });


                }
                //Si es el player SLAVE, esperará a que se genere el tablero 
                else
                {
                    var gamesRef = FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/gameBoard");
                    gamesRef.ValueChanged += HandleGameBoardAdded;
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            throw;
        }

    }


    public List<BoardTile> GetNeighbors(BoardTile tile, List<BoardTile> gridTiles)
    {
        List<BoardTile> neighbors = new List<BoardTile>();

        neighbors.Add(gridTiles.Where(c => c.x == tile.x && c.y == (tile.y + 1f)).Any() ? gridTiles.Where(c => c.x == tile.x && c.y == (tile.y + 1f)).First() : null);
        neighbors.Add(gridTiles.Where(c => c.x == tile.x && c.y == (tile.y - 1f)).Any() ? gridTiles.Where(c => c.x == tile.x && c.y == (tile.y - 1f)).First() : null);
        neighbors.Add(gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y + 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y + 0.5f)).First() : null);
        neighbors.Add(gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y - 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x - 1f) && c.y == (tile.y - 0.5f)).First() : null);
        neighbors.Add(gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y + 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y + 0.5f)).First() : null);
        neighbors.Add(gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y - 0.5f)).Any() ? gridTiles.Where(c => c.x == (tile.x + 1f) && c.y == (tile.y - 0.5f)).First() : null);

        return neighbors;
    }

    void HandleGameBoardAdded(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if (!multiplayerGameLoaded)
        {
            var jsonGameBoard = args.Snapshot.GetRawJsonValue();

            try
            {
                var gameBoard = Newtonsoft.Json.JsonConvert.DeserializeObject<GameBoard>(jsonGameBoard);

                var gameId = args.Snapshot.Reference.Parent.Key;

                if (gameBoard != null && GameData.gameId == gameId)
                {
                    GameData.data.gameBoard = gameBoard;
                    //Cambiamos el status a GameBoardCompleted
                    FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/status").SetValueAsync((int)GameStatus.GameBoardCompleted).ContinueWithOnMainThread(task =>
                    {
                        //Eliminamos el trigger
                        var gamesRef = FirebaseInitializer.dbRef.GetReference($"games/{GameData.gameId}/gameBoard");
                        gamesRef.ValueChanged -= HandleGameBoardAdded;

                        //Se ha completado la carga en BBDD, podemos cargar la escena MultiplayerGame
                        multiplayerGameLoaded = true;
                    });

                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw;
            }

        }

    }


    private void GameEvents_OnFirebaseInitialize()
    {
        if (authFirebaseManager != null)
        {
            Firebase.Auth.FirebaseAuth auth = FirebaseInitializer.auth;


#if UNITY_EDITOR
        if (auth.CurrentUser == null || auth.CurrentUser.Email != "oarjones@gmail.com")
        {
            if (auth.CurrentUser != null)
            {
                auth.SignOut();
            }

            authFirebaseManager.OnSignInWithEmailAndPasswordAsync("oarjones@gmail.com", "Am1lcarbarca");
        }
#else
            
        if (auth.CurrentUser == null || auth.CurrentUser.Email != "manuelp@gmail.com")
        {
            if (auth.CurrentUser != null)
            {
                auth.SignOut();
            }

            authFirebaseManager.OnSignInWithEmailAndPasswordAsync("manuelp@gmail.com", "Am1lcarbarca");
        }
#endif

            if (auth.CurrentUser != null)
            {
                FirebaseInitializer.dbRef.RootReference.Child("users").Child(auth.CurrentUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        var currentUser = task.Result;

                        if (currentUser != null && currentUser.Value != null)
                        {
                            var currentUserJson = currentUser.GetRawJsonValue();
                            userData = JsonConvert.DeserializeObject<User>(currentUserJson);

                            if (userData != null)
                            {
                                //var currentLevelData = userData.level.LevelPhaseStatus.Where(c => c.Key == userData.level.CurrentLevel).FirstOrDefault();

                                //foreach (var levelPhase in currentLevelData.Value.OrderBy(c => c.Key))
                                //{
                                //    PlayerGameLevelPhase phaseNotCompleted = levelPhase.Value.Where(c => !c.Complete).OrderBy(c => c.FaseNumber).FirstOrDefault();

                                //    if (phaseNotCompleted != null)
                                //    {
                                //        gameMode = levelPhase.Key;

                                //        gameLevelPhase = GameConfiguration.GameLevels.Where(c => c.Level == userData.level.CurrentLevel).Select(c => c.LevelsMode).FirstOrDefault()
                                //            .Where(c => c.Mode == gameMode).Select(c => c.Fases).FirstOrDefault().Where(c => c.FaseNumber == phaseNotCompleted.FaseNumber).FirstOrDefault();

                                //        break;
                                //    }
                                //}


                                if (scoreBox != null)
                                {
                                    foreach (var textBox in scoreBox.GetComponentsInChildren<TextMeshProUGUI>())
                                    {
                                        if (textBox.name == "TextBottom")
                                        {
                                            textBox.text = userData.score.ToString();
                                            _currentGameScore = 0;
                                            _currentPlayerScore = userData.score;
                                        }
                                    }
                                }

                                if (userBox != null)
                                {
                                    foreach (var textBox in userBox.GetComponentsInChildren<TextMeshProUGUI>())
                                    {
                                        if (textBox.name == "TextTop")
                                        {
                                            textBox.text = userData.username;
                                        }

                                        if (textBox.name == "TextBottom")
                                        {
                                            textBox.text = $"lvl {userData.level}";
                                        }
                                    }
                                }

                                var levelIconImage = userBox.GetComponentInChildren<Image>();

                                if (levelIconImage != null)
                                {
                                    var sprite = string.Format("Art/UI/icons/{0}_icon", "grifo"/*Enum.GetName(typeof(gameLevel), userData.level.CurrentLevel).ToLower()*/);
                                    levelIconImage.sprite = Resources.Load(sprite, typeof(Sprite)) as Sprite;
                                }

                                if (gameType == GameType.Standalone)
                                {
                                    SetInitialWord();
                                }

                                else
                                {
                                    //InitializeMultiplayerGame(userData);

                                    //TODO: PopUp WaitRoom
                                    //if (PopUpLoading != null)
                                    //    PopUpLoading.SetActive(true);

                                    //Task.Run(async () => await addToGameWaitRoom(auth.CurrentUser)).GetAwaiter().GetResult();
                                    addToGameWaitRoom(FirebaseInitializer.auth.CurrentUser);


                                }

                            }
                        }
                    }
                    else
                    {
                        // Manejar error
                        Debug.LogError("Error al obtener datos de Firebase: " + task.Exception);
                    }
                });

            }
        }


    }

    private void GameEvents_OnResolveWord(bool isValid, string wordToValidate, GameActor actor, ref int wordPoints)
    {
        updateScoreAnimationFinalized = false;

        if (isValid)
        {
            if (actor == GameActor.Player)
            {

                PlayValidWordEffect(wordToValidate.Length);

                var complexity = DictionaryUtilities.GetWordComplexity(wordToValidate);
                wordPoints = (int)((complexity > 0 ? complexity : 0.2f) * 100);

                Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

                if (auth != null && auth.CurrentUser != null)
                {
                    UpdateScore(auth.CurrentUser.UserId, wordPoints);
                }

                scoreGlowParticles.Play();

                particleSys.transform.localPosition = sourcePlayer.localPosition;
                particleSys.Play();

                StartCoroutine(UpdateScoreAndAnimateStars(wordPoints, wordToValidate));
            }

            if (actor == GameActor.Opponent)
            {

                //PlayValidWordEffect(wordToValidate.Length);

                var complexity = DictionaryUtilities.GetWordComplexity(wordToValidate);
                wordPoints = (int)((complexity > 0 ? complexity : 0.2f) * 100);

                //Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

                //if (auth != null && auth.CurrentUser != null)
                //{
                //    UpdateScore(auth.CurrentUser.UserId, wordPoints);
                //}

                scoreGlowParticlesOpponent.Play();

                particleSysOpponent.transform.localPosition = sourceOpponent.localPosition;
                particleSysOpponent.Play();

                StartCoroutine(UpdateScoreAndAnimateStarsOpponent(wordPoints, wordToValidate));
            }


        }
        else
        {
            map.ToggleGameEvents(true);
        }
    }

    public void PlayValidWordEffect(int lettersCount)
    {

        if (lettersCount > 4)
        {
            var text = "MUY BIEN!!";
            if (lettersCount > 4 && lettersCount <= 6)
            {
                text = "GENIAL!!";
            }
            else if (lettersCount > 6 && lettersCount <= 8)
            {
                text = "FANTÁSTICO!!";
            }
            else if (lettersCount > 8 && lettersCount <= 10)
            {
                text = "INCREIBLE!!";
            }
            else if (lettersCount > 10)
            {
                text = "SUBLIME!!";
            }


            // Asegúrate de que tienes un prefab asignado
            if (efectoPalabraValida != null)
            {
                // Si es la primera vez, busca y asigna las referencias
                if (textUI == null || efecto == null)
                {
                    textUI = efectoPalabraValida.GetComponentInChildren<TMP_Text>(true); // true para incluir inactivos
                    efecto = efectoPalabraValida.GetComponentInChildren<ParticleSystem>(true);
                    palabraValida = efectoPalabraValida.transform.Find("PalabraValida");
                }

                efectoPalabraValida.SetActive(true);
                palabraValida.gameObject.SetActive(true);
                textUI.gameObject.SetActive(true);

                if (textUI != null && efecto != null)
                {
                    textUI.text = text; // Actualiza el texto

                    // Muestra el texto UI y juega el efecto de partículas
                    efecto.Play();

                    // Inicia la coroutine para esperar a que la animación de partículas termine
                    StartCoroutine(WaitForParticleEffectToEnd());
                }

            }
            else
            {
                Debug.LogError("No se ha asignado un prefab de ParticleSystem.");
            }
        }

        //TODO: Compribar si hay o ha alcanzado el record de letraas por palabra
    }

    private IEnumerator WaitForParticleEffectToEnd()
    {
        var fondoTexto = palabraValida.GetComponent<Image>();
        TMP_Text texto = efectoPalabraValida.GetComponentInChildren<TMP_Text>(true);
        ParticleSystem efecto = efectoPalabraValida.GetComponentInChildren<ParticleSystem>();

        // Configura las escalas inicial y final
        Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 endScale = new Vector3(1.75f, 1.75f, 1.75f);
        float duration = 1.0f; // Duración de la animación de escala en segundos
        float waitTime = 0.5f; // Tiempo de espera antes de ocultar el texto

        // Establece la visibilidad inicial (alfa) del texto
        Color startColor = texto.color;
        startColor.a = 0f; // Inicialmente invisible
        texto.color = startColor;

        // Establece la visibilidad inicial (alfa) del texto
        Color startBgColor = fondoTexto.color;
        startBgColor.a = 0f; // Inicialmente invisible
        fondoTexto.color = startBgColor;

        texto.gameObject.SetActive(true);

        float time = 0;
        while (time < duration + waitTime)
        {
            if (time <= duration)
            {
                // Animar la escala
                texto.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
                fondoTexto.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
                // Animar la visibilidad (alfa)
                float alpha = Mathf.Lerp(0f, 1f, time / duration);
                texto.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                fondoTexto.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            else if (time > duration)
            {
                // Comienza a ocultar el texto después del tiempo de animación
                float alpha = Mathf.Lerp(1f, 0f, (time - duration) / waitTime);
                texto.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                fondoTexto.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Asegúrate de que el texto y el efecto de palabra válida se oculten completamente al final
        texto.gameObject.SetActive(false);
        efectoPalabraValida.SetActive(false);
    }

    private void OnInvalidTileClick()
    {
        Debug.Log("Invalid click!");
    }

    private void OnWinGame(GameActor actor)
    {
        //TODO: Game Win animation

        //Actualizar nivel y fase del jugador


        ////Completamos fase/nivel
        //userData.level.LevelPhaseStatus.Where(c => c.Key == userData.level.CurrentLevel).FirstOrDefault().Value.Where(c => c.Key == gameMode).FirstOrDefault().Value.Where(c => c.FaseNumber == gameLevelPhase.FaseNumber).FirstOrDefault().Complete = true;

        ////Comprobamso que haya completado todas las fases del nivel y aumentamos el nivel
        //if (!userData.level.LevelPhaseStatus.Where(c => c.Key == userData.level.CurrentLevel).FirstOrDefault().Value.SelectMany(c => c.Value).Where(c => !c.Complete).Any())
        //{
        //    userData.level.CurrentLevel = userData.level.CurrentLevel + 1;

        //    //TODO: animación nivel superado
        //}
        //else
        //{
        //    //TODO: animación fase superado
        //    foreach (var particle in phaseCompletedConfetties)
        //    {
        //        particle.Play();
        //    }

        //}

        Firebase.Auth.FirebaseAuth auth = FirebaseInitializer.auth;
        if (auth.CurrentUser != null)
        {
            string jsonLevel = Newtonsoft.Json.JsonConvert.SerializeObject(userData.level);

            FirebaseInitializer.dbRef.RootReference.Child("users").Child(auth.CurrentUser.UserId).Child("level").SetRawJsonValueAsync(jsonLevel).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Nivel/Fase de jugador actualizado.");
                }
                else
                {
                    // Manejar error
                    Debug.LogError("Error al obtener datos de Firebase: " + task.Exception);
                }
            });


        }

        LoadLevelScene();
    }

    private void OnGameOver()
    {
        //TODO: Game Over animation
        LoadLevelScene();
    }

    //private void OnTimeOut()
    //{
    //    Debug.Log("Time out!");
    //    EndEpisode();
    //}

    //private void OnEndSelecteTilesTimer()
    //{
    //    Debug.Log("Tiempo de selección de celdas agotado!");
    //}







    private IEnumerator UpdateScoreAndAnimateStars(int scoreToAdd, string palabra)
    {
        //float _starsNumber = palabra.Length;
        var animduration = palabra.Length > 4 ? (palabra.Length / duration) : duration; // Duración total de la animación
        float timeElapsed = 0f;
        int scoreAtStart = _currentGameScore;
        int scoreTarget = _currentGameScore + scoreToAdd;

        while (timeElapsed < animduration)
        {
            var newGameScore = (int)Mathf.Lerp(scoreAtStart, scoreTarget, timeElapsed / animduration);
            UpdateScoreText(newGameScore);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        scoreGlowParticles.Stop();

        _currentGameScore = scoreTarget;
        FinalizeUpdateScoreText();

        map.ToggleGameEvents(true);

        updateScoreAnimationFinalized = true;

    }


    private IEnumerator UpdateScoreAndAnimateStarsOpponent(int scoreToAdd, string palabra)
    {
        //float _starsNumber = palabra.Length;
        var animduration = palabra.Length > 4 ? (palabra.Length / duration) : duration; // Duración total de la animación
        float timeElapsed = 0f;
        int scoreAtStart = _currentGameScoreOpponent;
        int scoreTarget = _currentGameScoreOpponent + scoreToAdd;

        while (timeElapsed < animduration)
        {
            var newGameScore = (int)Mathf.Lerp(scoreAtStart, scoreTarget, timeElapsed / animduration);
            UpdateScoreTextOpponent(newGameScore);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        scoreGlowParticlesOpponent.Stop();

        _currentGameScoreOpponent = scoreTarget;
        FinalizeUpdateScoreTextOpponent();

        //updateScoreAnimationFinalized = true;

    }



    private void FinalizeUpdateScoreText()
    {
        foreach (var textBox in scoreBox.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (textBox.name == "TextTop")
            {
                textBox.text = _currentGameScore.ToString();
            }

            if (textBox.name == "TextBottom")
            {
                textBox.text = _currentPlayerScore.ToString();
            }
        }
    }

    private void FinalizeUpdateScoreTextOpponent()
    {
        var textBox = scoreBoxOpponent.GetComponent<TextMeshProUGUI>();
        textBox.text = _currentGameScoreOpponent.ToString();
    }

    private void UpdateScoreText(int newScore)
    {
        foreach (var textBox in scoreBox.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (textBox.name == "TextTop")
            {
                textBox.text = newScore.ToString();
            }

            //if (textBox.name == "TextBottom")
            //{
            //    textBox.text = newScore.ToString();
            //}
        }
    }

    private void UpdateScoreTextOpponent(int newScore)
    {
        var textBox = scoreBoxOpponent.GetComponent<TextMeshProUGUI>();
        textBox.text = newScore.ToString();
    }


    private void UpdateScore(string userId, int points)
    {
        FirebaseInitializer.dbRef.GetReference($"users/{userId}/score").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                var scoreObj = task.Result;

                if (scoreObj != null && scoreObj.Value != null)
                {
                    var score = Convert.ToInt32(scoreObj.GetRawJsonValue());
                    score += points;

                    FirebaseInitializer.dbRef.GetReference($"users/{userId}/score").SetValueAsync(score).ContinueWithOnMainThread(task1 =>
                    {
                        userData.score = score;
                    });

                }
            }
            else
            {
                // Manejar error
                Debug.LogError("Error al obtener datos de Firebase: " + task.Exception);
            }
        });


        //var scoreObj = await FirebaseInitializer.dbRef.GetReference($"users/{userId}/score").GetValueAsync();

        //if (scoreObj != null && scoreObj.Value != null)
        //{
        //    var score = Convert.ToInt32(scoreObj.GetRawJsonValue());
        //    score += points;

        //    await FirebaseInitializer.dbRef.GetReference($"users/{userId}/score").SetValueAsync(score);
        //    userData.score = score;
        //}
    }

    public void RechargeScene()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadLevelScene()
    {
        loadLevelScene = true;

        //SceneManager.LoadScene("LevelMenu");
    }


}
