using Assets.Scripts.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameEvents 
{
    //public delegate void TileMultiPlayerSelected(MultiplayerTile hex);
    //public static event TileMultiPlayerSelected OnTileMultiPlayerSelected;

    //public static void OnTileMultiPlayerSelectedMethod(MultiplayerTile hex)
    //{
    //    if (OnTileMultiPlayerSelected != null)
    //    {
    //        OnTileMultiPlayerSelected(hex);
    //    }
    //}


    public delegate void OnFirebaseInitializeEvent();
    public static event OnFirebaseInitializeEvent OnFirebaseInitialize;
    public static void FirebaseInitialize()
    {
        if (OnFirebaseInitialize != null)
        {
            OnFirebaseInitialize();
        }
    }


    public delegate void TileSelected(Hex hex, GameActor actor);
    public static event TileSelected OnTileSelected;

    public static void OnTileSelectedMethod(Hex hex, GameActor actor)
    {
        if (OnTileSelected != null)
        {
            OnTileSelected(hex, actor);
        }
    }


    /********************************************************/
    public delegate void ValidateWord(GameActor actor);
    public static event ValidateWord OnValidateWord;

    public static void FireValidateWord(GameActor actor)
    {
        if (OnValidateWord != null)
        {
            OnValidateWord(actor);
        }
    }


    ///********************************************************/
    //public delegate void OnBeginPan();
    //public static event OnBeginPan OnBeginPanEvent;

    //public static void OnBeginPanMethod()
    //{
    //    if (OnBeginPanEvent != null)
    //    {
    //        OnBeginPanEvent();
    //    }
    //}


    ///********************************************************/
    //public delegate void OnEndPan();
    //public static event OnEndPan OnEndPanEvent;

    //public static void OnEndPanMethod()
    //{
    //    if (OnEndPanEvent != null)
    //    {
    //        OnEndPanEvent();
    //    }
    //}


    /********************************************************/
    public delegate void ReubicateObjects(GameActor actor, float? newScale = null);
    public static event ReubicateObjects OnReubicateObjects;

    public static void OnReubicateObjectsMethod(GameActor actor, float? newScale = null)
    {
        if (OnReubicateObjects != null)
        {
            OnReubicateObjects(actor, newScale);
        }
    }

    /********************************************************/
    public delegate void OnDeselectedTile(Hex hex, GameActor actor);
    public static event OnDeselectedTile OnDeselectedTileEvent;

    public static void OnDeselectedTileMethod(Hex hex, GameActor actor)
    {
        if (OnDeselectedTileEvent != null)
        {
            OnDeselectedTileEvent(hex, actor);
        }
    }


    //public delegate void OnClearLastSelectedMultiplayer(MultiplayerTile hex);
    //public static event OnClearLastSelectedMultiplayer OnClearLastSelectedMultiplayerEvent;

    //public static void OnClearLastSelectedMultiplayerMethod(MultiplayerTile hex)
    //{
    //    if (OnClearLastSelectedMultiplayerEvent != null)
    //    {
    //        OnClearLastSelectedMultiplayerEvent(hex);
    //    }
    //}


    /********************************************************/
    public delegate void OnEndTimerCountdown();
    public static event OnEndTimerCountdown OnEndTimerCountdownEvent;

    public static void OnEndTimerCountdownMethod()
    {
        if (OnEndTimerCountdownEvent != null)
        {
            OnEndTimerCountdownEvent();
        }
    }


    /********************************************************/
    public delegate void OnCreateUser(string userid, string username, string email);
    public static event OnCreateUser OnCreateUserEvent;

    public static void OnCreateUserMethod(string userid, string username, string email)
    {
        if (OnCreateUserEvent != null)
        {
            OnCreateUserEvent(userid, username, email);
        }
    }


    /********************************************************/
    public delegate void OnSignIn(string userid, string email);
    public static event OnSignIn OnSignInEvent;

    public static void OnSignInMethod(string userid, string email)
    {
        if (OnSignInEvent != null)
        {
            OnSignInEvent(userid, email);
        }
    }



    /********************************************************/
    public delegate void OnInvalidWordAnimFinishedEvent(bool finished);
    public static event OnInvalidWordAnimFinishedEvent OnInvalidWordAnimFinished;

    public static void InvalidWordAnimFinished(bool finished)
    {
        if (OnInvalidWordAnimFinished != null)
        {
            OnInvalidWordAnimFinished(finished);
        }
    }


    /********************************************************/
    public delegate void OnValidWordAnimFinishedEvent(bool finished);
    public static event OnValidWordAnimFinishedEvent OnValidWordAnimFinished;

    public static void ValidWordAnimFinished(bool finished)
    {
        if (OnValidWordAnimFinished != null)
        {
            OnValidWordAnimFinished(finished);
        }
    }


    public delegate void OnChageLetterEvent(char letter, GameActor actor);
    public static event OnChageLetterEvent OnChageLetter;
    public static void FireChageLetter(char letter, GameActor actor)
    {
        if (OnChageLetter != null)
        {
            OnChageLetter(letter, actor);
        }
    }


    public delegate void OnUpdateCellLetterEvent(Hex cell);
    public static event OnUpdateCellLetterEvent OnUpdateCellLetter;
    public static void FireUpdateCellLetter(Hex cell)
    {
        if (OnUpdateCellLetter != null)
        {
            OnUpdateCellLetter(cell);
        }
    }


    public delegate void OnLifeDeleteEvent();
    public static event OnLifeDeleteEvent OnLifeDelete;
    public static void FireLifeDeleteEvent()
    {
        if (OnLifeDelete != null)
        {
            OnLifeDelete();
        }
    }


    public delegate void OnRuneRewriteLetterEvent();
    public static event OnRuneRewriteLetterEvent OnRuneRewriteLetter;
    public static void FireRuneRewriteLetterEvent()
    {
        if (OnRuneRewriteLetter != null)
        {
            OnRuneRewriteLetter();
        }
    }



    public delegate void OnFreezeTrapEvent();
    public static event OnFreezeTrapEvent OnFreezeTrap;
    public static void FireFreezeTrapEvent()
    {
        if (OnFreezeTrap != null)
        {
            OnFreezeTrap();
        }
    }


    public delegate void OnMoveToTileEvent();
    public static event OnMoveToTileEvent OnMoveToTile;
    public static void FireMoveToTileEvent()
    {
        if (OnMoveToTile != null)
        {
            OnMoveToTile();
        }
    }


    public delegate void OnMoveToTileFinalizedEvent(GameActor actor);
    public static event OnMoveToTileFinalizedEvent OnMoveToTileFinalized;
    public static void FireMoveToTileFinalizedEvent(GameActor actor)
    {
        if (OnMoveToTileFinalized != null)
        {
            OnMoveToTileFinalized(actor);
        }
    }


    public delegate void OnSetTileFreezeTrapEvent(GameActor actor);
    public static event OnSetTileFreezeTrapEvent OnSetTileFreezeTrap;
    public static void FireSetTileFreezeTrapEvent(GameActor actor)
    {
        if (OnSetTileFreezeTrap != null)
        {
            OnSetTileFreezeTrap(actor);
        }
    }


    public delegate void OnResolveWordEvent(bool isValid, string wordToValidate, GameActor actor, ref int wordPoints);
    public static event OnResolveWordEvent OnResolveWord;
    public static void FireResolveWordEvent(bool isValid, string wordToValidate, GameActor actor, ref int wordPoints)
    {
        if (OnResolveWord != null)
        {
            OnResolveWord(isValid, wordToValidate, actor, ref wordPoints);
        }
    }


    public delegate void OnWinGameEvent(GameActor actor);
    public static event OnWinGameEvent OnWinGame;
    public static void FireWinGameEvent(GameActor actor)
    {
        if (OnWinGame != null)
        {
            OnWinGame(actor);
        }
    }


    public delegate void OnGameOverEvent();
    public static event OnGameOverEvent OnGameOver;
    public static void FireGameOverEvent()
    {
        if (OnGameOver != null)
        {
            OnGameOver();
        }
    }


    public delegate void OnEndSelecteTilesTimerEvent(GameActor actor);
    public static event OnEndSelecteTilesTimerEvent OnEndSelecteTilesTimer;
    public static void FireEndSelecteTilesTimer(GameActor actor)
    {
        if (OnEndSelecteTilesTimer != null)
        {
            OnEndSelecteTilesTimer(actor);
        }
    }


    public delegate void OnInvalidTileClickEvent();
    public static event OnInvalidTileClickEvent OnInvalidTileClick;
    public static void FireInvalidTileClick()
    {
        if (OnInvalidTileClick != null)
        {
            OnInvalidTileClick();
        }
    }

    //

    public delegate void OnGameLoadedEvent();
    public static event OnGameLoadedEvent OnGameLoaded;
    public static void FireGameLoaded()
    {
        if (OnGameLoaded != null)
        {
            OnGameLoaded();
        }
    }


    public delegate void OnOpponentLoadedEvent();
    public static event OnOpponentLoadedEvent OnOpponentLoaded;
    public static void FireOpponentLoaded()
    {
        if (OnOpponentLoaded != null)
        {
            OnOpponentLoaded();
        }
    }


}
