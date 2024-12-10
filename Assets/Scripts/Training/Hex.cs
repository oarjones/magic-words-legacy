using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Assets.Scripts.Data;
using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// Clase para manejar las celdas adyacentes a una celda
/// </summary>
public struct NeighborsTiles
{
    public Hex neighbor_TOP;
    public Hex neighbor_BOTTOM;
    public Hex neighbor_LEFTUP;
    public Hex neighbor_LEFTDOWN;
    public Hex neighbor_RIGHTUP;
    public Hex neighbor_RIGHTDOWN;

    public System.Collections.Generic.List<Hex> ToList()
    {
        return new System.Collections.Generic.List<Hex> { neighbor_TOP, neighbor_BOTTOM, neighbor_LEFTUP, neighbor_LEFTDOWN, neighbor_RIGHTUP, neighbor_RIGHTDOWN };
    }
}

public class Hex : MonoBehaviour
{

    public string TileName()
    {
        return string.Format("{0}_{1}_{2}", level, x, y);
    }

    public override int GetHashCode()
    {
        return Convert.ToInt32(tileNumber + x + y);
    }

    public override string ToString()
    {
        return string.Format("{0}_{1}_{2}", level, x, y);
    }
    public override bool Equals(object obj)
    {
        if (obj is null or not Hex)
            return false;
        else
            return (level == ((Hex)obj).level) && (x == ((Hex)obj).x) && (y == ((Hex)obj).y);
    }


    private Map map;

    /// <summary>
    /// Nivel de la celda en el tablero
    /// </summary>
    [HideInInspector]
    public int level;

    /// <summary>
    /// Número de celda
    /// </summary>
    [HideInInspector]
    public int tileNumber;

    [HideInInspector]
    public float x;

    [HideInInspector]
    public float y;

    public AudioSource AudioSource;

    /// <summary>
    /// Acceso al objeto text que contiene la letra
    /// </summary>
    //public TMP_Text textContainer;

    /// <summary>
    /// Celdas adyacentes
    /// </summary>
    [HideInInspector]
    public NeighborsTiles neighbors;

    public SpriteRenderer _displayedImage;
    private char _letter = ' ';
    //private bool _selectedTile = false;

    /// <summary>
    /// Indica si el temporizador está activado
    /// </summary>
    public bool _timerEnabled = false;
    public float timeToChange = 0f;

    ///// <summary>
    ///// Tiempo pendiente para el cambio de letra
    ///// </summary>
    private float _timeRemainingLetterChange = 0f;

    /// <summary>
    /// Estado del la celda: -1--> blocked, 0 --> unselected, 1 --> selected
    /// </summary>
    public GameTileState tileState = GameTileState.Unselected;

    /// <summary>
    /// Indica si es la celda actual donde se encuentra el jugador
    /// </summary>
    public bool IsCurrentPlayerTile = false;

    /// <summary>
    /// Indica si es la celda actual donde se encuentra el oponente
    /// </summary>
    public bool IsCurrentOpponentTile = false;

    /// <summary>
    /// Indica si es la celda objetivo
    /// </summary>
    public bool IsObjectiveTile = false;
    //public bool IsEmptyTile = false;


    //private bool IsAnimateToBillBoard = false;
    //private LetraPalabra BillBoardLetter;
    public float _speedBillboardAnim = 15.0f;
    public AnimationCurve ac;

    //private Hex AnimatedHex;
    //private readonly string _animLetter;
    //private bool DestroyAnimatedHex;

    private readonly bool IsAnimatedHex = false;
    public Vector3 _shakeAnimTilestartPos;
    public Vector3 hexlocalScale;

    public ParticleSystem ObjectiveHaloParticles;
    public ParticleSystem ChangeEffectParticles;
    public ParticleSystem SelectedHaloParticles;
    public ParticleSystem FrozenTrapParticles;

    private Color SelectedHaloSourceColor;

    public Vector3 position;

    public GameActor actor = GameActor.None;


    public GameObject letraobject;
    public GameObject effectObject;


    public TextMeshPro textoLetra;
    public TextMeshPro effectTime;


    public bool isFrozen;
    public float frozenTimeRemaining;
    public int index;

    public string GetPlayerOccupied()
    {
        // Si el tile está seleccionado por un jugador, devolver su ID
        if (this.IsCurrentPlayerTile)
        {
            return FirebaseInitializer.auth?.CurrentUser?.UserId;
        }
        // Si está seleccionado por el oponente
        if (this.IsCurrentOpponentTile)
        {
            // Aquí deberías devolver el ID del oponente
            // Podrías obtenerlo de GameManager.GameData.playersInfo
            return Map._opponent.Key;
        }
        return null;
    }

    public float DistanceToObjective()
    {
        if (map.ObjectiveTile == null)
            return float.MaxValue; // Devuelve un valor alto si no se puede calcular la distancia

        if (this.Equals(map.ObjectiveTile))
            return 0f;

        // Usa Vector3.Distance para calcular la distancia real
        return Vector3.Distance(this.position, map.ObjectiveTile.position);
    }


    public void ToggleObjectiveTile(bool isObjective)
    {
        IsObjectiveTile = isObjective;
        var spriterend = GetComponent<SpriteRenderer>();

        if (!IsObjectiveTile)
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_normal", typeof(Sprite)) as Sprite;
            ObjectiveHaloParticles.Stop();
            ObjectiveHaloParticles.gameObject.SetActive(false);

        }
        else
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_target", typeof(Sprite)) as Sprite;
            ObjectiveHaloParticles.gameObject.SetActive(true);
            ObjectiveHaloParticles.Play();

        }

        //spriterend.color = isObjective ? new Color(0.556f, 0.004f, 1f, 1f) : Color.white;
        //spriterend.sprite = Resources.Load("Art/UI/cell_bg_target", typeof(Sprite)) as Sprite;
    }

    private float GetTimeRemainingLetterChange()
    {
        //return UnityEngine.Random.Range(250f, 600f);

        var mintime = timeToChange -= timeToChange * 0.85f;
        var maxtime = timeToChange += timeToChange * 0.85f;

        return UnityEngine.Random.Range(mintime, maxtime);
    }


    public void ToggleTimer(bool enabled)
    {
        _timerEnabled = enabled;
    }



    public void UpdateLetter(char letter)
    {
        ChangeEffectParticles.gameObject.SetActive(true);
        ChangeEffectParticles.Play();

        _letter = letter;

        if (textoLetra != null)
        {
            textoLetra.text = letter.ToString();
        }
    }


    /// <summary>
    /// Establece la letra de una celda
    /// </summary>
    /// <param name="letter"></param>
    public void SetLetter(bool effect, char? letter = null)
    {
        var updatedLetter = false;

        if (effect)
        {
            ChangeEffectParticles.gameObject.SetActive(true);
            ChangeEffectParticles.Play();
        }

        //Cambio de letra aleatorio en celdas vacías
        if ((this.tileState == GameTileState.Unselected) && letter.HasValue)
        {
            if (_timerEnabled)
                _timerEnabled = false;

            _letter = letter.Value;

            if (textoLetra != null)
            {
                textoLetra.text = letter.ToString(); //Resources.Load(string.Format("Art/UI/Abecedary/{0}_nor", letter.ToString().ToLower()), typeof(Sprite)) as Sprite;
                updatedLetter = true;
            }

            if (_timerEnabled)
            {
                _timeRemainingLetterChange = GetTimeRemainingLetterChange();
                _timerEnabled = true;
            }
        }
        else if (this.IsCurrentPlayerTile && letter.HasValue) //Cambio de letra por herramienta runerewrite
        {
            _letter = letter.Value;

            if (textoLetra != null)
            {
                textoLetra.text = letter.ToString();
                updatedLetter = true;
            }
        }
        else if (this.IsCurrentOpponentTile && letter.HasValue) //Cambio de letra por herramienta runerewrite
        {
            _letter = letter.Value;

            if (textoLetra != null)
            {
                textoLetra.text = letter.ToString();
                updatedLetter = true;
            }
        }

        if (updatedLetter)
            GameEvents.FireUpdateCellLetter(this);

    }

    public char GetLetter()
    {
        return _letter;
    }

    public LangLetter GetLangLetter(string userLang)
    {
        return Language.GetLangAbecedary(userLang).Where(c => c.Letter == _letter).First();
    }

    /// <summary>
    /// Indica si la celda es vecina de otra por su nombre
    /// </summary>
    /// <param name="tileName"></param>
    /// <returns></returns>
    public bool IsNeighbour(string tileName)
    {

        return (neighbors.neighbor_TOP != null && neighbors.neighbor_TOP.name == tileName) || (neighbors.neighbor_BOTTOM != null && neighbors.neighbor_BOTTOM.name == tileName) ||
            (neighbors.neighbor_LEFTUP != null && neighbors.neighbor_LEFTUP.name == tileName) || (neighbors.neighbor_LEFTDOWN != null && neighbors.neighbor_LEFTDOWN.name == tileName) ||
            (neighbors.neighbor_RIGHTUP != null && neighbors.neighbor_RIGHTUP.name == tileName) || (neighbors.neighbor_RIGHTDOWN != null && neighbors.neighbor_RIGHTDOWN.name == tileName);
    }




    /// <summary>
    /// Indica si la celda es vecina de otra por su nombre
    /// </summary>
    /// <param name="tileName"></param>
    /// <returns></returns>
    public CellMovement GetCellMovement(string tileName)
    {
        if (neighbors.neighbor_TOP != null && neighbors.neighbor_TOP.name == tileName)
        {
            return CellMovement.Top;
        }
        else if (neighbors.neighbor_BOTTOM != null && neighbors.neighbor_BOTTOM.name == tileName)
        {
            return CellMovement.Bottom;
        }
        else if (neighbors.neighbor_LEFTUP != null && neighbors.neighbor_LEFTUP.name == tileName)
        {
            return CellMovement.LeftUp;
        }
        else if (neighbors.neighbor_LEFTDOWN != null && neighbors.neighbor_LEFTDOWN.name == tileName)
        {
            return CellMovement.LeftDown;
        }
        else if (neighbors.neighbor_RIGHTUP != null && neighbors.neighbor_RIGHTUP.name == tileName)
        {
            return CellMovement.RightUp;
        }
        else if (neighbors.neighbor_RIGHTDOWN != null && neighbors.neighbor_RIGHTDOWN.name == tileName)
        {
            return CellMovement.RightDown;
        }
        else { return CellMovement.None; }
    }



    //internal void ToggleHighLighted(bool hightlighted)
    //{
    //	this.GetComponent<RectTransform>().localScale = hightlighted ? new Vector3(this.GetComponent<RectTransform>().localScale.x * 1.1f, this.GetComponent<RectTransform>().localScale.y * 1.1f) :
    //		new Vector3(this.GetComponent<RectTransform>().localScale.x * 0.9f, this.GetComponent<RectTransform>().localScale.y * 0.9f);
    //	this.HighLighted = hightlighted;
    //}

    private void Awake()
    {
        _displayedImage = GetComponent<SpriteRenderer>();
        this.position = this.GetComponent<RectTransform>().position;
        SelectedHaloSourceColor = SelectedHaloParticles.main.startColor.color;
    }

    private void OnEnable()
    {
        if (letraobject != null)
            textoLetra = letraobject.GetComponent<TextMeshPro>();

        if (effectObject != null)
            effectTime = effectObject.GetComponent<TextMeshPro>();


    }

    void Start()
    {
        map = this.GetComponentInParent<Map>();


        if (!IsCurrentPlayerTile && !IsCurrentOpponentTile && !IsObjectiveTile && !IsAnimatedHex && this.tileState != GameTileState.Blocked)
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_normal", typeof(Sprite)) as Sprite;

        if (this.tileState == GameTileState.Blocked)
            _displayedImage.sprite = Resources.Load("Art/Training/empty_cell_bg", typeof(Sprite)) as Sprite;

        GetComponent<SpriteRenderer>().sortingOrder = -2;






        // Establecer el sorting order base para el TextMeshPro
        //var textMesh = GetComponentInChildren<TextMeshPro>();
        if (textoLetra != null)
        {
            textoLetra.sortingOrder = 20; // Un poco más alto que el tile para que aparezca encima
        }

    }

    public void InvalidTile()
    {
        _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_invalid", typeof(Sprite)) as Sprite;
    }

    /// <summary>
    /// Resetea un aletra
    /// </summary>
    public void ClearTile(bool updateLetter)
    {
        actor = GameActor.None;
        tileState = GameTileState.Unselected;
        // Aumentar la escala de la celda
        transform.localScale = hexlocalScale;
        GetComponent<SpriteRenderer>().sortingOrder = -2;

        //var textMesh = GetComponentInChildren<TextMeshPro>();
        if (textoLetra != null)
        {
            textoLetra.sortingOrder = 20; // Un poco más alto que el tile para que aparezca encima
        }

        if (!IsObjectiveTile)
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_normal", typeof(Sprite)) as Sprite;
            ObjectiveHaloParticles.Stop();
            ObjectiveHaloParticles.gameObject.SetActive(false);
            IsCurrentPlayerTile = false;
        }
        else
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_target", typeof(Sprite)) as Sprite;
            ObjectiveHaloParticles.gameObject.SetActive(true);
            ObjectiveHaloParticles.Play();
            IsCurrentPlayerTile = false;
        }

        if (updateLetter)
        {
            _letter = map.lang.GetRandomLetter();
        }

        if (updateLetter && textoLetra != null)
        {
            //textContainer.sprite = Resources.Load(string.Format("Art/UI/Abecedary/{0}_nor", _letter.ToString().ToLower()), typeof(Sprite)) as Sprite;
            textoLetra.text = _letter.ToString();
            GameEvents.FireUpdateCellLetter(this);
        }
    }

    public void ResetScale(float? scaleFactor = null)
    {
        transform.localScale = hexlocalScale * (scaleFactor.HasValue ? scaleFactor.Value : 1f);
        GetComponent<SpriteRenderer>().sortingOrder = -2;

        //var textMesh = GetComponentInChildren<TextMeshPro>();
        if (textoLetra != null)
        {
            textoLetra.sortingOrder = 20; // Un poco más alto que el tile para que aparezca encima
        }
    }




    /// <summary>
    /// Seleccionar celda
    /// </summary>
    public void SelectTile(float scaleFactor, GameActor actor)
    {
        this.actor = actor;

        if (this.tileState == GameTileState.FreezeTrapFromOpponent)
        {
            if (!map.PlayerIsFrozen)
            {
                if (map.gameType == GameType.Standalone)
                {

                    FrozenTrapParticles.Play();
                    //TODO: Lanzar efecto de sonido
                    _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_freeze", typeof(Sprite)) as Sprite;
                    map.FreezeGamePlayer(actor);
                    isFrozen = false;
                }
                else
                {
                    map.FreezePlayer(this);
                }
                return;
            }
        }
        else if (this.tileState == GameTileState.FreezeTrapFromPlayer)
        {
            if (!map.OpponentIsFrozen)
            {
                FrozenTrapParticles.Play();
                //TODO: Lanzar efecto de sonido
                _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_freeze", typeof(Sprite)) as Sprite;
                map.FreezeGamePlayer(actor);
                isFrozen = false;
                return;
            }
        }


        if (actor == GameActor.Player)
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_selected", typeof(Sprite)) as Sprite;
            IsCurrentPlayerTile = true;
        }

        if (actor == GameActor.Opponent)
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_opp_selected", typeof(Sprite)) as Sprite;
            IsCurrentOpponentTile = true;
        }



        // Aumentar la escala de la celda
        transform.localScale = transform.localScale * scaleFactor;
        GetComponent<SpriteRenderer>().sortingOrder = -1;

        if (textoLetra != null)
        {
            //textContainer.sprite = Resources.Load(string.Format("Art/UI/Abecedary/{0}_sel", _letter.ToString().ToLower()), typeof(Sprite)) as Sprite; //letter.ToString();
            textoLetra.text = _letter.ToString();
            //textContainer.color = new Color(70f, 70f, 70f);
        }

        //var textMesh = GetComponentInChildren<TextMeshPro>();
        if (textoLetra != null)
        {
            textoLetra.sortingOrder = 20; // Un poco más alto que el tile para que aparezca encima
        }

        tileState = GameTileState.Selected;
    }

    private bool AllowAction()
    {
        if (!Map.GameBoardEventsEnabled) return false;

        if (this.name == map.InitialPlayerTile.name && this.name == map.CurrentPlayerTile.name)
        {
            return true;
        }
        else if (this.tileState == GameTileState.Unselected && this.name != map.CurrentPlayerTile.name && this.IsNeighbour(map.CurrentPlayerTile.name))
        {
            return true;
        }
        else if (this.tileState == GameTileState.Selected && this.name == map.CurrentPlayerTile.name)
        {
            return true;
        }
        else { return false; }
    }

    /*
    public void Select()
    {
        Debug.Log("Hex Select");

        //CameraHandler cameraHandler = FindObjectOfType<CameraHandler>();
        //if (cameraHandler.isCameraBeingManipulated)
        //    return;

        if (!Map.GameBoardEventsEnabled) return;

        if (this.name == Map.InitialPlayerTile.name || this.tileState == GameTileState.Blocked)
        {
            GameEvents.FireInvalidTileClick();
            return;
        }

        if (this.tileState == GameTileState.Unselected && this.name != Map.CurrentPlayerTile.name && this.IsNeighbour(Map.CurrentPlayerTile.name))
        {
            GameEvents.OnTileSelectedMethod(this);
            this.tileState = GameTileState.Selected;
        }
        else if (this.tileState == GameTileState.Selected && this.name == Map.CurrentPlayerTile.name)
        {
            GameEvents.OnDeselectedTileMethod(this);
            this.tileState = GameTileState.Unselected;
        }

    }
    */

    private void OnMouseEnter()
    {
        //CameraHandler cameraHandler = FindObjectOfType<CameraHandler>();
        //if (cameraHandler.isCameraBeingManipulated)
        //    return;

        PanAndZoom.EnableEvents = !AllowAction();


        if (!Map.GameBoardEventsEnabled) return;



        //      if (Map.CanSelect())
        //{
        //          Map.SetAllowPaning(IsEmptyTile || (this.name != Map.CurrentUserTile.name && !this.IsNeighbour(Map.CurrentUserTile.name)));
        //      }



        //if (Map.CanSelect())
        //{
        //	GameEvents.OnTileSelectedMethod(this);
        //}
    }

    private void OnMouseUp()
    {
        if (!Map.GameBoardEventsEnabled) return;
    }

    private void Update()
    {
        // Comprobar si el usuario ha tocado la pantalla o ha hecho clic con el ratón
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 inputPos;

            // Comprobar si estamos en un dispositivo móvil
            if (Input.touchCount > 0)
            {
                inputPos = Input.GetTouch(0).position;
            }
            else
            {
                // Esto es para uso con el ratón
                inputPos = Input.mousePosition;
            }

            // Convertir la posición del toque/pulsación a una posición en el mundo
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            Hex hex = null;
            // Comprobar si hemos golpeado un objeto
            if (hit.collider != null && (hex = hit.collider.gameObject.GetComponent<Hex>()) == this)
            {
                if (hex != null)
                {
                    if (!Map.GameBoardEventsEnabled) return;


                    //Si no se está aplicacndo la trampa de hielo, ni el cambio a otra celda
                    if (!map.isSettingFreezeTrap && !map.isMovingToTile)
                    {

                        if (this.name == map.InitialPlayerTile.name || this.tileState == GameTileState.Blocked)
                        {
                            GameEvents.FireInvalidTileClick();
                            return;
                        }

                        if (this.tileState == GameTileState.Unselected && this.name != map.CurrentPlayerTile.name && this.IsNeighbour(map.CurrentPlayerTile.name))
                        {
                            GameEvents.OnTileSelectedMethod(this, GameActor.Player);
                            this.tileState = GameTileState.Selected;
                        }
                        else if (this.tileState == GameTileState.Selected && this.name == map.CurrentPlayerTile.name)
                        {
                            GameEvents.OnDeselectedTileMethod(this, GameActor.Player);
                            this.tileState = GameTileState.Unselected;
                        }
                        else if (this.tileState == GameTileState.FreezeTrapFromOpponent && this.name != map.CurrentPlayerTile.name)
                        {
                            GameEvents.OnTileSelectedMethod(this, GameActor.Player);
                        }

                    }
                    else if(map.isSettingFreezeTrap)
                    {
                        if (this.tileState == GameTileState.Unselected)
                        {
                            SetFreeztrapOnTile(GameActor.Player);
                        }
                    }
                    else if (map.isMovingToTile)
                    {
                        //Solo permitirá cambiar de celda desde la celda inicial
                        if (this.tileState == GameTileState.Unselected && map.CurrentPlayerTile == map.InitialPlayerTile)
                        {
                            map.MoveToTile(GameActor.Player, this);
                        }
                    }
                }
            }
        }

        if (_timerEnabled && !IsCurrentPlayerTile && !IsCurrentOpponentTile && !IsObjectiveTile && (this.tileState == GameTileState.Unselected))
        {
            if (_timeRemainingLetterChange > 0)
            {
                _timeRemainingLetterChange -= Time.deltaTime;
            }
            else
            {
                var newLetter = map.lang.GetRandomLetter();
                SetLetter(true, newLetter);
            }
        }

        if (this.tileState == GameTileState.Selected)
        {
            if (actor == GameActor.Player && map.DeselectTilesByTimePlayer)
            {
                if (map.idleTimerPlayer > 8f && map.idleTimerPlayer < 12f)
                {
                    var mainModule = SelectedHaloParticles.main;
                    mainModule.startColor = new Color(1f, 0.5f, 0f);

                    if (!SelectedHaloParticles.isPlaying)
                    {
                        SelectedHaloParticles.gameObject.SetActive(true);
                        SelectedHaloParticles.Play();
                    }

                }
                else if (map.idleTimerPlayer > 12f && map.idleTimerPlayer < 20f)
                {
                    var mainModule = SelectedHaloParticles.main;
                    mainModule.startColor = new Color(1f, 0f, 0f);

                    if (!SelectedHaloParticles.isPlaying)
                    {
                        SelectedHaloParticles.gameObject.SetActive(true);
                        SelectedHaloParticles.Play();
                    }
                }
                else
                {
                    var mainModule = SelectedHaloParticles.main;
                    mainModule.startColor = SelectedHaloSourceColor;

                    if (SelectedHaloParticles.isPlaying)
                    {
                        SelectedHaloParticles.Stop();
                        SelectedHaloParticles.gameObject.SetActive(false);
                    }
                }
            }
            else if (actor == GameActor.Player && !map.DeselectTilesByTimePlayer && SelectedHaloParticles.isPlaying)
            {
                SelectedHaloParticles.Stop();
                SelectedHaloParticles.gameObject.SetActive(false);
            }

            if (actor == GameActor.Opponent && map.DeselectTilesByTimeOpponent)
            {
                if (map.idleTimerOpponent > 8f && map.idleTimerOpponent < 12f)
                {
                    var mainModule = SelectedHaloParticles.main;
                    mainModule.startColor = new Color(1f, 0.5f, 0f);

                    if (!SelectedHaloParticles.isPlaying)
                    {
                        SelectedHaloParticles.gameObject.SetActive(true);
                        SelectedHaloParticles.Play();
                    }
                }
                else if (map.idleTimerOpponent > 12f && map.idleTimerOpponent < 20f)
                {
                    var mainModule = SelectedHaloParticles.main;
                    mainModule.startColor = new Color(1f, 0f, 0f);

                    if (!SelectedHaloParticles.isPlaying)
                    {
                        SelectedHaloParticles.gameObject.SetActive(true);
                        SelectedHaloParticles.Play();
                    }
                }
                else
                {
                    var mainModule = SelectedHaloParticles.main;
                    mainModule.startColor = SelectedHaloSourceColor;

                    if (SelectedHaloParticles.isPlaying)
                    {
                        SelectedHaloParticles.Stop();
                        SelectedHaloParticles.gameObject.SetActive(false);
                    }
                }
            }
            else if (actor == GameActor.Opponent && !map.DeselectTilesByTimeOpponent && SelectedHaloParticles.isPlaying)
            {
                SelectedHaloParticles.Stop();
                SelectedHaloParticles.gameObject.SetActive(false);
            }



        }
        else if (SelectedHaloParticles.isPlaying)
        {
            SelectedHaloParticles.Stop();
            SelectedHaloParticles.gameObject.SetActive(false);
        }


        if (isFrozen)
        {
            if (frozenTimeRemaining > 0)
            {
                frozenTimeRemaining -= Time.deltaTime;
                effectTime.text = Mathf.Ceil(frozenTimeRemaining).ToString();
            }
            else
            {
                ResetCellWithFreezeTrap();
            }
        }
        else
        {
            if (frozenTimeRemaining != 60 || this.tileState == GameTileState.FreezeTrapFromOpponent || this.tileState == GameTileState.FreezeTrapFromPlayer)
            {
                ResetCellWithFreezeTrap();
            }
        }
    }

    private void ResetCellWithFreezeTrap()
    {
        if (isFrozen)
        {
            if (this.tileState == GameTileState.FreezeTrapFromPlayer)
            {
                if (map.selectedTilesOpponent.Contains(this))
                {
                    _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_opp_selected", typeof(Sprite)) as Sprite;
                }
                else
                {
                    ClearTile(false);
                }
            }
            else if (this.tileState == GameTileState.FreezeTrapFromOpponent)
            {
                if (map.selectedTilesPlayer.Contains(this))
                {
                    _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_selected", typeof(Sprite)) as Sprite;
                }
                else
                {
                    ClearTile(false);
                }
            }
        }


        frozenTimeRemaining = 60;
        isFrozen = false;
        //this.tileState = GameTileState.Unselected;
        effectTime.gameObject.SetActive(false);
    }

    public void SetFreeztrapOnTile(GameActor actor, float time = 60)
    {
        if (map.gameType == GameType.Standalone)
        {
            this.tileState = actor == GameActor.Player ? GameTileState.FreezeTrapFromPlayer : GameTileState.FreezeTrapFromOpponent;

            if (actor == GameActor.Player)
            {
                _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_freeze", typeof(Sprite)) as Sprite;
                if (effectTime != null)
                {
                    effectTime.text = ((short)time).ToString();
                    isFrozen = true;
                    frozenTimeRemaining = time;
                    effectTime.gameObject.SetActive(true);
                }
            }

            map.isSettingFreezeTrap = false;
            GameEvents.FireSetTileFreezeTrapEvent(actor);
        }
        else
        {
            if(actor == GameActor.Player)
            {
                map.SetFreeztrapOnTilePlayer(this, time);
            }
        }
    }


    


    internal void UnFreeze(GameActor actor)
    {
        this.tileState = GameTileState.Selected;

        if (actor == GameActor.Player)
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_selected", typeof(Sprite)) as Sprite;
        }

        if (actor == GameActor.Opponent)
        {
            _displayedImage.sprite = Resources.Load("Art/UI/cell_bg_opp_selected", typeof(Sprite)) as Sprite;
        }
    }
}
