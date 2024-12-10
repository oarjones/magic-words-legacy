/**
 * Copyright 2018 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
'use strict';

const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp(functions.config().firebase);

const database = admin.database();

class Vector3 {
    constructor(x, y, z) {
        this.x = x || Number.parseFloat(0.0);
        this.y = y || Number.parseFloat(0.0);
        this.z = z || Number.parseFloat(0.0);
    }
}

class BoardTile {
    constructor(posVector, level, tileNum, x, y, isObjectiveTile, isCurrentUserTile, isEmptyLevel) {
        this.posVector = posVector || new Vector3(0, 0, 0);
        this.level = x || Number.parseInt(0);
        this.tileNum = x || Number.parseInt(0);
        this.x = x || Number.parseFloat(0.0);
        this.y = x || Number.parseFloat(0.0);
        this.isObjectiveTile = x || false;
        this.isCurrentUserTile = x || false;
        this.isEmptyLevel = x || false;
    }
}


exports.generateBoard = functions.https.onCall((data) => {

    var boardTiles = new Array();

    if (Number.isNaN(Number.parseFloat(data.initialXpos)) || Number.isNaN(Number.parseFloat(data.initialYpos)) ||
        Number.isNaN(Number.parseFloat(data.scaleX)) || Number.isNaN(Number.parseFloat(data.scaleY)) ||
        Number.isNaN(Number.parseFloat(data.xOffset)) || Number.isNaN(Number.parseFloat(data.yOffset))) {
        throw new functions.https.HttpsError('invalid-argument', 'The argument types are not valid!');
    }

    if (Number.isNaN(Number.parseInt(data.depthLevels)) || Number.isNaN(Number.parseInt(data.depthEmptyLevels))) {
        throw new functions.https.HttpsError('invalid-argument', 'The argument types are not valid!');
    }

    var xPos = Number.parseFloat(data.initialXpos);
    var yPos = Number.parseFloat(data.initialYpos);

    var xOffset = Number.parseFloat(data.xOffset);
    var yOffset = Number.parseFloat(data.yOffset);

    var scaleX = Number.parseFloat(data.scaleX);
    var scaleY = Number.parseFloat(data.scaleY);

    var depthLevels = Number.parseInt(data.depthLevels);
    var depthEmptyLevels = Number.parseInt(data.depthEmptyLevels);

    var currentXOffset = Number.parseFloat(0.0);
    var currentYOffset = Number.parseFloat(0.0);

    //Modificamos escala
    if (scaleX != 0) {
        currentXOffset = xOffset - (xOffset * (Number.parseFloat(1.0) - scaleX));
    }

    if (scaleY != 0) {
        currentYOffset = yOffset - (yOffset * (Number.parseFloat(1.0) - scaleY));
    }

    //Por cada nivel (núermo de niveles + número de niveles vacíos)
    for (var level = 0; level < (depthLevels + depthEmptyLevels); level++) {
        //Es un nivel vacío
        var isEmptyLevel = Boolean(level >= depthLevels);

        //El nivel 0 solo contendrá una celda
        if (level == 0) {
            xPos = Number.parseFloat(0.0);
            yPos = Number.parseFloat(0.0);

            var posVector = new Vector3(xPos, yPos, Number.parseFloat(0.0));
            //InstatiateHexTile(posVector, 0, 0, Number.parseFloat(0.0), Number.parseFloat(0.0), isObjectiveTile: true, isEmptyLevel: isEmptyLevel);
            var tile = new BoardTile(posVector, Number.parseInt(0), Number.parseInt(0), Number.parseFloat(0.0), Number.parseFloat(0.0), true, false, isEmptyLevel);
            boardTiles.push(tile);
        }
        else {
            //El número de celdas es el nivel por 6. Esto trazará un diseñio en forma de panel
            var levelTilesNumber = level * Number.parseInt(6);

            //Incrementos para la ubicación de las celdas
            var yIncrement = Number.parseFloat(0.5);
            var xIncrement = Number.parseFloat(1.0);

            var xMultipler = Number.parseFloat(0.0);
            xMultipler = level * (xMultipler + xIncrement);

            var yMultipler = Number.parseFloat(0.0);
            yMultipler = level * (yMultipler + yIncrement);

            var negativeXtiles = Number.parseInt(0);
            var positiveXtiles = Number.parseInt(0);

            //Se instanciará cada celda comenzando por la derecaha y siguiendo la dirección contraria de las agujas del reloj
            for (var tileNum = 0; tileNum < levelTilesNumber; tileNum++) {
                xPos = (xMultipler * currentXOffset) * Number.parseFloat(1.030);
                yPos = (yMultipler * currentYOffset) * Number.parseFloat(1.030);

                var posVector = new Vector3(xPos, yPos, Number.parseFloat(0.0));

                //Marcamos la celda como actual, siempre se posivionará en el último nivel arriba -90 grados (x = 0, y = level)
                var isCurrentUserTile = Boolean(level == (depthLevels - 1) && xMultipler == 0 && yMultipler == level);

                //Instanciamos celda
                var tile = new BoardTile(posVector, level, tileNum, xMultipler, yMultipler, false, isCurrentUserTile, isEmptyLevel);
                boardTiles.push(tile);

                //Calculamos los valore de xMultipler y yMultipler para calcular la posición de la próxima celda
                if (tileNum < level) {
                    xMultipler = xMultipler - xIncrement;
                    yMultipler = yMultipler + yIncrement;
                }
                else if (tileNum == level) {
                    xMultipler = xMultipler - xIncrement;
                    yMultipler = yMultipler - yIncrement;
                }
                else {
                    if (xMultipler < 0) {
                        if (xMultipler == -(level) && negativeXtiles == 0) {
                            negativeXtiles++;
                        }

                        if (negativeXtiles == 1) {
                            yIncrement = 1;
                        }

                        if (negativeXtiles == 0) {
                            xMultipler = xMultipler - xIncrement;
                            yMultipler = yMultipler - yIncrement;
                        }
                        else if (negativeXtiles > 0 && negativeXtiles < (level + 1)) {
                            negativeXtiles++;
                            yMultipler = yMultipler - yIncrement;
                        }
                        else {
                            yIncrement = Number.parseFloat(0.5);
                            xMultipler = xMultipler + xIncrement;
                            yMultipler = yMultipler - yIncrement;
                        }
                    }
                    else {
                        if (xMultipler == (level) && positiveXtiles == 0) {
                            positiveXtiles++;
                        }

                        if (positiveXtiles == 1) {
                            yIncrement = 1;
                        }

                        if (positiveXtiles == 0) {
                            xMultipler = xMultipler + xIncrement;
                            yMultipler = yMultipler + yIncrement;
                        }
                        else if (positiveXtiles > 0 && positiveXtiles < (level + 1)) {
                            positiveXtiles++;
                            yMultipler = yMultipler + yIncrement;
                        }
                        else {
                            yIncrement = Number.parseFloat(0.5);
                            xMultipler = xMultipler - xIncrement;
                            yMultipler = yMultipler + yIncrement;
                        }

                    }
                }

            }
        }

    }

    //returning result.
    return JSON.stringify({
        gameId: data.gameId,
        boardTiles: boardTiles
    });
});


//Ejemplo de función OnCall
//exports.gameUpdateAction = functions.https.onCall((data, context) => {

//	var success = true;

//	console.log('Init gameUpdateAction!');
//	console.log('Param:' + data.param);

//	var jsonData = JSON.parse(data.param);
//	console.log('Tiles count:' + jsonData.tiles.length);

//	try {
//		jsonData.tiles.forEach(tile => {
//			const tileUpdate = database.ref(`games/${jsonData.gameId}/gameBoard/boardTiles/${tile.index}`);
//			tileUpdate.transaction((current_value) => {

//				console.log(`tileToUpdate: ${current_value.name}`);

//				current_value.action = tile.action;
//				current_value.playerOccupied = tile.playerOccupied;

//				return current_value;
//			}, function (error, committed, snapshot) {

//				if (error) {
//					console.log("INFO: Transaction failed abnormally!", error);
//					success = false;
//				} else if (!committed) {
//					console.log("INFO: We aborted the transaction..");
//					success = false;
//				} else {
//					console.log("Transaction committed!");
//				}

//				console.log("New Value: ", snapshot.val());
//			});

//		});

//		return { res: success };
//	}
//	catch (error) {
//		throw error;
//	}


//});



exports.joinPlayers = functions.region('europe-west1').database.ref('/gameWaitRoom/{userId}')
    .onCreate((snapshot, context) => {

        const userId = context.params.userId;
        var firstPlayer = snapshot.val();
        var secondPlayer = null;
		var pruebas1= 25;

        console.log('Player1: ' + userId);

        //TODO:add level array for (original.level - 2) to (original.level + 2)

        database.ref('gameWaitRoom').orderByChild('createdAt').once('value').then(snapshot => {
            // Search for a player who is not yet matched in the queue
            snapshot.forEach(child => {

                //console.log('Player1: ' + userId + '-' + firstPlayer.langCode + ' --> ' + child.key + '(' + child.val().key + ')' + + '-' + child.val().langCode);

                if (secondPlayer == null && child.key != userId && child.val().langCode == firstPlayer.langCode) {
                    //TODO: Check if is in level array
                    secondPlayer = child;
                }
            });


            //If not exists secondPlayer
            if (secondPlayer == null) {
                console.log('Ups! De momento no hay contrincante..');
            } else {

                console.log('Se ha encontrado un contrincante --> ' + secondPlayer.val().userName);

                //New game object
                const newGame = {
                    status: 1,
                    type: 2,
                    langCode: firstPlayer.langCode,
                    createdAt: Date.now(),
                    playersInfo:
                    {
                        [userId]: {
                            userName: firstPlayer.userName,
                            level: firstPlayer.level,
                            master: true
                        }
                        ,

                        [secondPlayer.key]: {
                            userName: secondPlayer.val().userName,
                            level: secondPlayer.val().level,
                            master: false
                        }
                    }
                    ,
                    gameBoard: {}
                };

                //New gameId
                var gameId = database.ref("games").push().getKey();

                //Insert newGame
                return database.ref("games").child(gameId).set(newGame)
                    .then(function (game) {
                        console.log('Game created!. Deleting gameWaitRoom users...');

                        database.ref("gameWaitRoom").child(userId).remove();
                        database.ref("gameWaitRoom").child(secondPlayer.key).remove();
                    })
                    .catch(function (error) {
                        console.error("Error creating game : ", error);
                    });
            }

        });

        return false;


    });


exports.checkPlayersReady = functions.region('europe-west1').database.ref('/games/{gameId}/playersInfo/{userId}/gameBoardLoaded')
    .onCreate((snapshot, context) => {

        const gameId = context.params.gameId;
        const userId = context.params.userId;

        var playersReady = true;

        database.ref('/games/' + gameId + '/playersInfo').once('value').then(snapshot => {
            snapshot.forEach(child => {
                if (!child.val().gameBoardLoaded) {
                    playersReady = child.val().gameBoardLoaded;
                }
            });


            //If two players have loaded the map
            if (!playersReady) {
                console.log('Los jugadores no están preparados..');
            } else {

                console.log('Players ready --> ' + playersReady);


                //Update game status
                return database.ref("games").child(gameId).update({ status: 3 })
                    .then(function (game) {
                        console.log('Players are ready!');

                        //database.ref("gameWaitRoom").child(userId).remove();
                        //database.ref("gameWaitRoom").child(secondPlayer.key).remove();
                    })
                    .catch(function (error) {
                        console.error("Error updateing game status : ", error);
                    });
            }

        });

        return false;


    });




//exports.playerUpdate = functions.region('europe-west1').database.ref('/games/{gameId}/playersInfo/{userId}/actions/{actionId}')
//    .onCreate((snapshot, context) => {

//        const gameId = context.params.gameId;
//        const userId = context.params.userId;
//        const actionId = context.params.actionId;
//        var playerAction = snapshot.val();


//        switch (playerAction.action) {

            
//            case 1://TileUnselected            
//            case 2://TileSelected 
//            case 3://InvalidWord
//            case 4://ValidWord
//                playerAction.tiles.forEach(tile => {

//                    const tileUpdateRef = database.ref(`games/${gameId}/gameBoard/boardTiles/${tile.index}`);

//                    if (playerAction.action != 2 || tileUpdateRef.playerOccupied == null) {

//                        tileUpdateRef.transaction(function (oldTile) {
//                            // Check if the result is NOT NULL:
//                            if (oldTile != null) {

//                                if (tile.playerOccupied != null) {

//                                    return {
//                                        action: tile.tileAction,
//                                        index: oldTile.index,
//                                        isObjectiveTile: oldTile.isObjectiveTile,
//                                        letter: oldTile.letter,
//                                        level: oldTile.level,
//                                        name: oldTile.name,
//                                        posVector: oldTile.posVector,
//                                        tileNumber: oldTile.tileNumber,
//                                        x: oldTile.x,
//                                        y: oldTile.y,
//                                        playerOccupied: tile.playerOccupied
//                                    };
//                                } else {
//                                    return {
//                                        action: tile.tileAction,
//                                        index: oldTile.index,
//                                        isObjectiveTile: oldTile.isObjectiveTile,
//                                        letter: oldTile.letter,
//                                        level: oldTile.level,
//                                        name: oldTile.name,
//                                        posVector: oldTile.posVector,
//                                        tileNumber: oldTile.tileNumber,
//                                        x: oldTile.x,
//                                        y: oldTile.y
//                                    };
//                                }
//                            } else {
//                                // Return a value that is totally different 
//                                // from what is saved on the server at this address:
//                                return null;
//                            }
//                        }, function (error, committed, snapshot) {
//                            if (error) {
//                                console.log("INFO: tileUpdate failed abnormally!", error);
//                            } else if (!committed) {
//                                console.log("INFO: We aborted the transaction (tileUpdate)..");
//                            } else {
//                                console.log(`Transaction (player action type ${tile.tileAction}: index ${tile.index}) committed!`);

//                                //Generamos el registro para que el oponente actualice el tablero
//                                var opponentActionRef = database.ref(`games/${gameId}/playersInfo/${playerAction.oponnentId}/opponentActions`).push();
//                                var opponentAction = {
//                                    actionId: actionId
//                                };

//                                opponentActionRef.set(opponentAction);
//                            }
//                        }, true);

//                    }
//                });
//                break;            
//            case 5://ReplaceCurrentLetter
//                playerAction.tiles.forEach(tile => {

//                    const tileUpdateRef = database.ref(`games/${gameId}/gameBoard/boardTiles/${tile.index}`);

//                    tileUpdateRef.transaction(function (oldTile) {
//                            // Check if the result is NOT NULL:
//                            if (oldTile != null) {

//                                if (tile.playerOccupied != null) {

//                                    return {
//                                        action: tile.tileAction,
//                                        index: oldTile.index,
//                                        isObjectiveTile: oldTile.isObjectiveTile,
//                                        letter: tile.letter,
//                                        level: oldTile.level,
//                                        name: oldTile.name,
//                                        posVector: oldTile.posVector,
//                                        tileNumber: oldTile.tileNumber,
//                                        x: oldTile.x,
//                                        y: oldTile.y,
//                                        playerOccupied: tile.playerOccupied
//                                    };
//                                } else {
//                                    return {
//                                        action: tile.tileAction,
//                                        index: oldTile.index,
//                                        isObjectiveTile: oldTile.isObjectiveTile,
//                                        letter: tile.letter,
//                                        level: oldTile.level,
//                                        name: oldTile.name,
//                                        posVector: oldTile.posVector,
//                                        tileNumber: oldTile.tileNumber,
//                                        x: oldTile.x,
//                                        y: oldTile.y
//                                    };
//                                }
//                            } else {
//                                // Return a value that is totally different 
//                                // from what is saved on the server at this address:
//                                return null;
//                            }
//                        }, function (error, committed, snapshot) {
//                            if (error) {
//                                console.log("INFO: tileUpdate failed abnormally!", error);
//                            } else if (!committed) {
//                                console.log("INFO: We aborted the transaction (tileUpdate)..");
//                            } else {
//                                console.log(`Transaction (player action type ${tile.tileAction}: index ${tile.index}) committed!`);

//                                //Generamos el registro para que el oponente actualice el tablero
//                                var opponentActionRef = database.ref(`games/${gameId}/playersInfo/${playerAction.oponnentId}/opponentActions`).push();
//                                var opponentAction = {
//                                    actionId: actionId
//                                };

//                                opponentActionRef.set(opponentAction);
//                            }
//                        }, true);

                    
//                });
//                break;

//        }



//        return true;


//    });




