using Assets.Scripts.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameBoardMultiplayer
{ 

    //public static Hex hexPrefab;
    public static float xOffset = 0.750f;
    public static float yOffset = 0.850f;
    public static float initialXpos = 0f;
    public static float initialYpos = 0f;
    private static float _cellScaleFactorY;
    public static Language lang;

    static float CalculateScaleFactorY(float screenWidth, float screenHeight)
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

    public static GameBoard GenerateBoard(float mapSize, GameMode gameMode, string playerId1, string playerId2, float hexSacalex, float hexScaley, Language language)
    {
        lang = language;
        lang.Initialize(lang.Code, 0.5f);

        var gameBoard = new GameBoard();
        gameBoard.boardTiles = new List<BoardTile>();

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        //_cellScaleFactorY = CalculateScaleFactorY(screenWidth, screenHeight);
        float yOffset1 = 0f;

        yOffset1 = yOffset /* _cellScaleFactorY*/;


        float xPos = initialXpos;
        float yPos = initialYpos;

        var scaleX = hexSacalex; // hexPrefab.GetComponent<Hex>().transform.localScale.x;
        var scaleY = hexScaley; // hexPrefab.GetComponent<Hex>().transform.localScale.y;

        float currentXOffset = 0.0f;
        float currentYOffset = 0.0f;

        //Modificamos escala
        if (scaleX != 0)
            currentXOffset = xOffset - (xOffset * (1f - scaleX));

        if (scaleY != 0)
            currentYOffset = yOffset1 - (yOffset1 * (1f - scaleY));

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
                BoardTile tile = GenerateBoardTile(posVector, 0, 0, 0f, 0f, tileIndex, isObjectiveTile: false, isEmptyLevel: isEmptyLevel);
                gameBoard.boardTiles.Add(tile);
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
                    bool isCurrentUserTile = level == (mapSize - 1) && tileNum == 0;
                    bool isCurrentOpponentTile = level == (mapSize - 1) && tileNum == ((levelTilesNumber / 3));

                    string playerInitial = isCurrentUserTile ? playerId1 : (isCurrentOpponentTile ? playerId2 : null);

                    //Instanciamos celda
                    BoardTile tile = GenerateBoardTile(posVector, level, tileNum, xMultipler, yMultipler, tileIndex, 
                        isEmptyLevel: isEmptyLevel, playerInitial: playerInitial);
                    

                    if (level == (mapSize - 1) && tileNum == 12)
                    {
                        tile.isObjectiveTile = true;
                    }

                    gameBoard.boardTiles.Add(tile);
                    tileIndex++;

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


        return gameBoard;

    }

    private static BoardTile GenerateBoardTile(Vector3 posVector, int level, int tileNum, float x, float y, int index, bool isObjectiveTile = false,
        bool isEmptyLevel = false, string playerInitial = null)
    {
        //var userLang = PlayerPrefs.GetString("LANG");
        //lang = Language.GetLanguages().Where(c => c.Code == userLang).FirstOrDefault();

        BoardTile tile = new BoardTile();
        tile.posVector = new PosVector() { x = posVector.x, y = posVector.y, z = posVector.z };
        tile.name = $"Hex_{level}_{tileNum}";
        tile.level = level;
        tile.tileNumber = tileNum;
        tile.x = x;
        tile.y = y;
        tile.isObjectiveTile = isObjectiveTile;
        tile.playerInitial = playerInitial;
        tile.playerOccupied = playerInitial;
        tile.letter = lang.GetRandomLetter().ToString();
        tile.action = (int)TileAction.None;
        tile.index = index;
        tile.tileState = isEmptyLevel ? GameTileState.Blocked : GameTileState.Unselected;
        //tile.IsCurrentPlayerTile = isCurrentUserTile;
        //tile.IsCurrentOpponentTile = isCurrentOpponentTile;
        //tile.actor = isCurrentUserTile ? GameActor.Player : (isCurrentOpponentTile ? GameActor.Opponent : GameActor.None);

        return tile;
    }


}
