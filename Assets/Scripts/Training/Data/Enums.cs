using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{

    public enum GameActor
    {
        None = 0,
        Player = 1,
        Opponent
        
    }

    public enum CellMovement
    {
        None = 0,
        Top,
        Bottom,
        LeftUp,
        LeftDown,
        RightUp,
        RightDown
    }

    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }
    public enum GameTileState
    {
        Blocked = -1,
        Unselected = 0,
        Selected,
        FreezeTrapFromPlayer,
        FreezeTrapFromOpponent

    }

    public enum SoundTypes
    {
        SELECT_TILE,
        DESELECT_TILE,
        INVALID_WORD,
        VALID_WORD,
        GAME_MELODY
    }

    public enum GameType
    {
        Standalone = 1,
        Multiplayer
    }

    public enum GameMode
    {
        CatchLetter = 1, //Conseguir la celda objetivo en un tiempo límite. (En niveles superiores puede tene que alcanzar más de una celda objetivo)
        PointsChallenge = 2, //Conseguir un número de puntos dado en un tiempo límite. (En niveles superiores aumentará el número de puntos)
        NLetterWordChallenge = 3, //Conseguir una palabra con un número de letras dadas en un tiempo límite. (En niveles superiores aumentará el número de letras de la palabra)
        NLetterChallenge = 4, //Conseguir una letra un númerod e veces dado en un tiempo límite. (En niveles superiores aumentará el número de veces)
        HiddenWord = 5, //Completar la palabra dada oculta en el tablero en un tiempo límite
        Flash = 6, //Completar un número de palabras distintas dada en un tiempo límite
        Puzze = 7, //Se muestra una palabra con las letras ocultas. El jugador deberá completar la palabra, formando palabras que contengan las letras.
                  //Cuando se forme una palabra que contenga una letra de la palabra oculta, esta letra/s se develará en la palabra. Tendrá que hacerlo en un tiempo límite
                  //En niveles superiores se añadirán palabras más complejas
        VsAlgorithm // Supoera al algoritmo
    }

    

    public enum GameDifficulty
    {
        Timer = 1,
        WordMinLetter, //Número mínimo de latras por palabra
        LetterComplexity, //Complejidad de las letras del tablero
        //ToolsWaitTimeout, //Tiempo de espera entre uso de herramientas de ayuda
        //ToolsNumber, //Número de herramientas de ayuda
        PointsToWin, //Puntos necesarios para ganar el juego
        WordNumLetters, //Númerod de letras de la palabra
        TargetLetterComplexity, //Complejidad de la letra a conseguir
        NumLetters, //Número de letras a conseguir
        WordComplexity, //Complejidad de la palabra
        GameBoardSize, //Tamaño del tablero
        EmptyTiles, //Número de Celdas vacías
        TileChangeLetter, //Cambia la letra de cada celda cada n segundos
        ObjectiveTileMoves, //La letra objetivo se aleja del jugador,
        RepeatWord //Permite o no repetir palabras
    }

    

    

    public enum GameAidTool
    {
        ChangeCurrentLetter = 1,
        MoveToTile,
        PauseTimerSecond,
        locktile,
        FreezeTrap
    }

    public enum GameAidToolModeType
    {
        ByNumEquiped = 1,
        ByTime = 2,
        Mixed = 3
    }

    public enum GameStatus
    {
        Pending = 1,
        GameBoardCompleted = 2,
        PlayersReady = 3,
        Playing = 4,
        Finished = 5
    }

    public enum TileAction
    {
        None = 0,
        Select = 1,
        Unselect = 2,
        ChangeLetter = 3,
        UpdateLetter = 4,
        SetObjective = 5,
        FreezeTile = 6,
        FreezePlayer = 7
    }

    public enum PlayerAction
    {
        None = 0,
        TileUnselected = 1,
        TileSelected = 2,
        InvalidWord = 3,
        ValidWord = 4,
        ReplaceCurrentLetter = 5,
        MoveToTile = 6,
        FreezeTile = 7,
        FreezePlayer = 8,
        ClearTiles = 9
    }


    //public enum gameLevel
    //{
    //    //Hada
    //    //Sátiro
    //    //Unicornio
    //    Grifo = 1,
    //    Fenix,
    //    Dragón,
    //    Leviatán,
    //    Quimera,
    //    Hydra,
    //    Basilisco,
    //    //Centauro
    //    Tritón,
    //    //Minotauro
    //    Pegaso,
    //    //Sirena
    //    //Gárgola
    //    //Genio (Djinn)
    //    Kraken,
    //    Banshee,
    //    //Cíclope
    //    //Gigante
    //    //Quetzalcóatl (serpiente emplumada azteca)
    //    //Anubis (dios egipcio con cabeza de chacal)
    //    //Wendigo
    //    //Roc (pájaro gigante)
    //    //Yeti
    //    //Kappa (criatura acuática japonesa)
    //    Manticora,
    //    Valkiria,
    //    Naga
    //}

    public enum GameDivision
    {
        jasper = 1,
        citrine = 2,
        quartz = 3,
        jade = 4,
        lapislazuli = 5,
        amber = 6,
        agate = 7,
        peridot = 8,
        turquoise = 9,
        amethyst = 10,
        opal = 11,
        aquamarine = 12,
        spinel = 13,
        sapphire = 14,
        emerald = 15,
        ruby = 16,
        diamond = 17
    }


    public enum GameReult
    {
        unfinished = -1,
        lost = 0,
        won = 1
    }
}
