using Assets.Scripts.Data;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class OnlineManager : MonoBehaviour
{

    private Map map;

    public void InitializeMultiplayerManager(Map map)
    {
        this.map = map;

        //Registramos el evento que se lanzará cuando haya alguna actualización edl oponente
        FirebaseInitializer.dbRef
        .GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{Map._opponent.Key}/actions")
        .ChildAdded += OpponentUpdateAction_ChildAdded;

    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    private void OnDestroy()
    {
        FirebaseInitializer.dbRef
        .GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{Map._opponent.Key}/actions")
        .ChildAdded -= OpponentUpdateAction_ChildAdded;
    }


    /// <summary>
    /// Método que envía las acciones del jugador para que actualice el tablero el oponente
    /// </summary>
    /// <param name="updateAction">objeto update</param>
    /// <param name="celdas">celdas del tablero afectadas</param>
    public void SendGameUpdateAction(GameUpdateAction updateAction, List<Hex> celdas, string word = null)
    {
        string jsonUpdateAction = Newtonsoft.Json.JsonConvert.SerializeObject(updateAction);

        var updateActionId = FirebaseInitializer.dbRef
            .GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/actions").Push().Key;


        var actionRef = FirebaseInitializer.dbRef.GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/actions/{updateActionId}");

        // Helper method to wait for opponent update
        async Task WaitForOpponentUpdate()
        {
            bool opponentUpdated = false;
            while (!opponentUpdated)
            {
                var snapshot = await actionRef.GetValueAsync();
                if (snapshot.Exists && snapshot.Child("OponnentUpdated").Value != null)
                {
                    opponentUpdated = (bool)snapshot.Child("OponnentUpdated").Value;
                }
                await Task.Delay(75); // Wait for 500ms before checking again
            }

            await actionRef.RemoveValueAsync();
        }

        async Task SetActionAndWaitForUpdate(string json)
        {
            await actionRef.SetRawJsonValueAsync(json);
            await WaitForOpponentUpdate();
        }

        switch (Enum.Parse<PlayerAction>(updateAction.action.ToString()))
        {
            case PlayerAction.TileSelected:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction).ContinueWithOnMainThread(task =>
                    {
                        map.SelectPlayerTile(celdas[0]);
                    });
                }
                break;

            //case PlayerAction.TileSelected:
            //    {

            //        FirebaseInitializer.dbRef.GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/actions/{updateActionId}")
            //        .SetRawJsonValueAsync(jsonUpdateAction)
            //        .ContinueWithOnMainThread(task =>
            //        {
            //            var actionRef = FirebaseInitializer.dbRef.GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/actions/{updateActionId}");

            //            map.SelectPlayerTile(celdas[0]);
            //        });

            //    }
            //    break;
            case PlayerAction.TileUnselected:
                {

                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.DeselectPlayerTile(celdas[0]);

                    });
                }
                break;
            case PlayerAction.ValidWord:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.PalabraValida(GameActor.Player, word, clearTiles: false);

                    });
                }
                break;
            case PlayerAction.InvalidWord:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.PalabraInvalida(GameActor.Player);

                    });
                }
                break;
            case PlayerAction.ReplaceCurrentLetter:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.ChangeLetterPlayer(updateAction.tiles[0].letter[0]);

                    });
                }
                break;
            case PlayerAction.MoveToTile:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.MoveToTilePlayer(celdas[0]);

                    });
                }
                break;
            case PlayerAction.FreezeTile:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.UpdateFreeztrapOnTilePlayer(celdas[0], updateAction.tiles[0].actionTime);

                    });
                }
                break;

            case PlayerAction.FreezePlayer:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction)
                    .ContinueWithOnMainThread(task =>
                    {
                        map.UpdateFreezePlayer(celdas[0]);

                    });
                }
                break;

            case PlayerAction.ClearTiles:
                {
                    SetActionAndWaitForUpdate(jsonUpdateAction).ContinueWithOnMainThread(task =>
                    {
                        map.ResetTilesPlayer(resetTimer: updateAction.resetTimer);
                    });
                }
                break;
        }
    }




    /// <summary>
    /// Evento que se lanza cuando hay una actualización nueva paa aplicar en el oponente
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void OpponentUpdateAction_ChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            return;
        }

        //var game = args.Snapshot;
        var actionJson = args.Snapshot.GetRawJsonValue();

        try
        {
            GameUpdateAction opponentAction = Newtonsoft.Json.JsonConvert.DeserializeObject<GameUpdateAction>(actionJson);
            Hex gridTile = default;

            if (opponentAction != null)
            {

                switch (Enum.Parse<PlayerAction>(opponentAction.action.ToString()))
                {
                    case PlayerAction.TileUnselected:
                        map.DeselectOpponentTile(map.gridTiles.Where(c => c.index == opponentAction.tiles[0].index).FirstOrDefault());
                        break;
                    case PlayerAction.TileSelected:
                        map.SelectOpponentTile(map.gridTiles.Where(c => c.index == opponentAction.tiles[0].index).FirstOrDefault());
                        break;
                    case PlayerAction.InvalidWord:
                        map.PalabraInvalida(GameActor.Opponent);
                        break;
                    case PlayerAction.ValidWord:
                        int wordPoints = 0;

                        foreach (var tile in opponentAction.tiles)
                        {
                            gridTile = map.gridTiles.Where(c => c.index == tile.index).FirstOrDefault();
                            if (gridTile != null)
                            {
                                gridTile.UpdateLetter(tile.letter[0]);
                            }
                        }

                        GameEvents.FireResolveWordEvent(true, opponentAction.word, GameActor.Opponent, ref wordPoints);
                        PalabraValidaOponente(opponentAction.word);
                        break;
                    case PlayerAction.ReplaceCurrentLetter:
                        gridTile = map.gridTiles.Where(c => c.index == opponentAction.tiles[0].index).FirstOrDefault();
                        gridTile.UpdateLetter(opponentAction.tiles[0].letter[0]);
                        map.ChangeLetterOpponent(opponentAction.tiles[0].letter[0]);
                        break;

                    case PlayerAction.MoveToTile:
                        gridTile = map.gridTiles.Where(c => c.index == opponentAction.tiles[1].index).FirstOrDefault();
                        map.MoveToTileOpponent(gridTile);
                        break;

                    case PlayerAction.FreezeTile:
                        gridTile = map.gridTiles.Where(c => c.index == opponentAction.tiles[0].index).FirstOrDefault();                        
                        map.UpdateFreeztrapOnTileOpponent(gridTile, opponentAction.tiles[0].actionTime);
                        break;

                    case PlayerAction.FreezePlayer:
                        gridTile = map.gridTiles.Where(c => c.index == opponentAction.tiles[0].index).FirstOrDefault();
                        map.FreezeGamePlayer(GameActor.Opponent);
                        break;

                    case PlayerAction.ClearTiles:
                        //foreach(var tile in opponentAction.tiles)
                        //{
                        //    map.DeselectOpponentTile(map.gridTiles.Where(c => c.index == tile.index).FirstOrDefault());
                        //}
                        map.ResetOpponentTiles(opponentAction.resetTimer);

                        break;
                }



                //Eliminamos las referencias de la actualización.
                await FirebaseInitializer.dbRef.
                GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{Map._opponent.Key}/actions/{args.Snapshot.Key}").UpdateChildrenAsync(
                    new Dictionary<string, object>() { { "OponnentUpdated", true } }
                    );

                //await FirebaseInitializer.dbRef.
                //GetReference($"games/{GameManager.GameData.gameId}/playersInfo/{FirebaseInitializer.auth.CurrentUser.UserId}/opponentActions/{args.Snapshot.Key}").RemoveValueAsync();


            }
        }
        catch (Exception e)
        {
            throw;
        }
    }



    private void PalabraValidaOponente(string word)
    {
        var lastTile = map.selectedTilesOpponent[map.selectedTilesOpponent.Count - 1];
        map.CurrentOpponentTile.IsCurrentOpponentTile = false;
        PlayerManager.CurrentOpponentTile = map.CurrentOpponentTile = lastTile;
        PlayerManager.InitialOpponentTile = map.InitialOpponentTile = lastTile;

        map.OpponentWords.Add(word.ToUpper());

        //Limpiamos las celdas seleccionadas
        foreach (var tile in map.selectedTilesOpponent)
        {
            tile.ClearTile(updateLetter: false);
        }

        ///Nueva lista de celdas seleccinada, añadimos la actual para comenzar la nueva búsqueda
        map.selectedTilesOpponent = new List<Hex>
        {
            map.CurrentOpponentTile
        };

        PlayerManager.SelectedTilesOpponent = map.selectedTilesOpponent;

        map.CurrentOpponentTile.SelectTile(map.selectedTileScaleFactor, GameActor.Opponent);
        map.SetPlayerIconPosition(map.CurrentOpponentTile.transform, GameActor.Opponent);

        //Limpiamos el cartel de la palabra y añadimos la actual
        map.CleanCartelOpponent();
        map.AddLetraCartelOpponent(map.CurrentOpponentTile);

        map.DrawArrowsBetweenSelectedTiles(GameActor.Opponent);

        map.idleTimerOpponent = 0f;



    }


}
