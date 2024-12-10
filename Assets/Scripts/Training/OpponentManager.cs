using Assets.Scripts.Data;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class OpponentManager : MonoBehaviour
{
    public Map map;

    string userLang = LanguageCodes.ES_es;
    private string dictionaryContent;
    //string searchDictPath_3_6 = "dictionary/{0}/search_dict_3_6";
    //string searchDictPath_7_8 = "dictionary/{0}/search_dict_7_8";
    //string searchDictPath_9_10 = "dictionary/{0}/search_dict_9_10";
    //string searchDictPath_11_12 = "dictionary/{0}/search_dict_11_12";
    //string searchDictPath_13 = "dictionary/{0}/search_dict_13";


    private void OnEnable()
    {
        GameEvents.OnOpponentLoaded += GameEvents_OnOpponentLoaded;
        GameEvents.OnResolveWord += GameEvents_OnResolveWord;
        GameEvents.OnUpdateCellLetter += GameEvents_OnUpdateCellLetter;
    }



    private void OnDestroy()
    {
        GameEvents.OnOpponentLoaded -= GameEvents_OnOpponentLoaded;
        GameEvents.OnResolveWord -= GameEvents_OnResolveWord;
        GameEvents.OnUpdateCellLetter -= GameEvents_OnUpdateCellLetter;
    }

    void Start()
    {
        if (string.IsNullOrEmpty(dictionaryContent))
        {
            

            var searchDictPath = string.Format("search_dict_{0}_{1}", 3, 6);

            var dictionaryPath = $"/dictionary/{userLang}/{searchDictPath}.txt";
            string filePath = Application.persistentDataPath + dictionaryPath;

            if(System.IO.File.Exists(filePath))
            {
                dictionaryContent = System.IO.File.ReadAllText(filePath);
            }
            else
            {
                Debug.LogError($"El archivo de diccionario {string.Format("dictionary/{0}/{1}", userLang, searchDictPath)} no se encontró en el contenedor persistente.");
            }

            //TextAsset dictionaryFile = Resources.Load<TextAsset>(string.Format("dictionary/{0}/{1}", userLang, searchDictPath));

            //if (dictionaryFile != null)
            //{
            //    dictionaryContent = dictionaryFile.text;
            //}
            //else
            //{
            //    Debug.LogError($"El archivo de diccionario {string.Format("dictionary/{0}/{1}", userLang, searchDictPath)} no se encontró en Resources.");
            //}
        }
    }


    private void GetDictionary(byte minletter, byte? maxletter)
    {
        var searchDictPath = maxletter.HasValue ? string.Format("search_dict_{0}_{1}", minletter, maxletter) : string.Format("search_dict_{0}", minletter);

        TextAsset dictionaryFile = Resources.Load<TextAsset>(string.Format("dictionary/{0}/{1}", userLang, searchDictPath));

        if (dictionaryFile != null)
        {
            dictionaryContent = dictionaryFile.text;
        }
        else
        {
            Debug.LogError($"El archivo de diccionario {string.Format("dictionary/{0}/{1}", userLang, searchDictPath)} no se encontró en Resources.");
        }
    }

    private WordCombination CurrentWord = default(WordCombination);
    WordCombination playerPossibleWord = default(WordCombination);
    //private Dictionary<int, List<WordCombination>> combinations = null;
    private bool isSearchingWord = false;
    private bool isSearchingPlayerWord = false;
    private bool isValidatingWord = false;
    private bool isResetingWord = false;
    private bool OponentLoaded = false;
    private bool hasPendingWordToValidate = false;

    private float setTrapsTimer = 0;

    private void Update()
    {
        if (map.gameType == GameType.Standalone && map.gameMode == GameMode.VsAlgorithm && OponentLoaded)
        {
            setTrapsTimer += Time.deltaTime;

            if (map.OpponentIsFrozen)
            {
                map.FreezeGamePlayer(GameActor.Opponent);
                return;
            }

            if(playerPossibleWord != null && map.FreezTrapOpponentButton != null && map.FreezTrapOpponentButton.NumberOfItemsEquiped > 0)
            {
                var cell = playerPossibleWord.Cells.Where(c => !map.selectedTilesPlayer.Contains(c)).FirstOrDefault();

                if(cell != null)
                {
                    cell.SetFreeztrapOnTile(GameActor.Opponent);
                    map.FreezTrapOpponentButton.OnUseTool(' ', GameActor.Opponent);
                    playerPossibleWord = default(WordCombination);
                }

            }

            //Si existe palabra a validar y no ha habido cambios de letras en el tablero que hayan modificado alguna de la palabra.
            if (CurrentWord != null && !isValidatingWord && !isSearchingWord && !isSearchingPlayerWord && !isResetingWord && CheckNoChanges(CurrentWord))
            {
                //Si no hay que hacer cambios de letras, o hay que hacerlos pero se tiene herramientas
                if (!CurrentWord.Changes.Any() || (CurrentWord.Changes.Any() && CanChangeLetters(CurrentWord)))
                {
                    hasPendingWordToValidate = true;
                    isValidatingWord = true;

                    //Enviamos a validar
                    StartCoroutine(ValidateWord((res) =>
                    {
                        CurrentWord = null;
                        isValidatingWord = false;

                    }));

                }
            }
            else if (map.selectedTilesOpponent.Count > 1)
            {
                if (!isSearchingWord && !isValidatingWord && !isSearchingPlayerWord && !hasPendingWordToValidate && !isResetingWord)
                {
                    isResetingWord = true;

                    //Enviamos a validar
                    StartCoroutine(map.ResetOponentWord((res) =>
                    {
                        CurrentWord = null;
                        isResetingWord = false;

                    }));
                }

            }
            else
            {
                //Si no se etá buscando una palabra
                if (!isSearchingWord && !isSearchingPlayerWord && !isValidatingWord && !isResetingWord)
                {
                    isSearchingWord = true;

                    Task.Run(async () =>
                    {
                        await Task.Delay(20000);
                        return await ChooseBestWordCombinationAsync(map.CurrentOpponentTile);

                    }).
                    ContinueWithOnMainThread((task) =>
                    {

                        if (task.IsCompletedSuccessfully)
                        {
                            var wordCombinations = task.Result;
                            var objectiveCell = map.GetObjectiveTile();

                            if (wordCombinations != null || wordCombinations.Count > 0)
                            {
                                // Intentar encontrar una combinación que contenga la celda objetivo.
                                var combinationWithObjective = wordCombinations.FirstOrDefault(wc => wc.Cells.Contains(objectiveCell));
                                if (combinationWithObjective != null)
                                {
                                    CurrentWord = combinationWithObjective;
                                }

                                

                                // Si no se encuentra ninguna con la celda objetivo, elegir la que tiene la última celda más cercana a la celda objetivo.
                                if (CurrentWord == null)
                                {
                                    WordCombination closestCombination = null;
                                    float closestDistance = float.MaxValue;

                                    if (wordCombinations.Count == 1)
                                    {
                                        closestCombination = wordCombinations[0];
                                    }
                                    else
                                    {
                                        foreach (var combination in wordCombinations)
                                        {
                                            var lastCell = combination.Cells.LastOrDefault();
                                            if (lastCell != null)
                                            {
                                                float distance = 0f; ;

                                                try
                                                {
                                                    distance = Vector3.Distance(lastCell.GetComponent<RectTransform>().position, objectiveCell.GetComponent<RectTransform>().position);
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.LogError(e);
                                                }

                                                if (distance < closestDistance)
                                                {
                                                    closestDistance = distance;
                                                    closestCombination = combination;
                                                }
                                            }
                                        }
                                    }



                                    CurrentWord = closestCombination;
                                }
                            }

                        }


                        isSearchingWord = false;

                    });

                }
            }

            //Buscar posibles palabras del jugador
            if(setTrapsTimer > 20)
            {
                playerPossibleWord = default(WordCombination);

                //Comprobar el estado del jugador para poner trampas
                var currentPlayerTile = map.CurrentPlayerTile;
                var objectiveCell = map.GetObjectiveTile();
                var distance = Vector3.Distance(currentPlayerTile.GetComponent<RectTransform>().position, objectiveCell.GetComponent<RectTransform>().position);

                if(distance < 1.5f)
                {
                    if(!map.selectedTilesPlayer.Contains(objectiveCell))
                    {
                        isSearchingPlayerWord = true;

                        var currentCombination = string.Concat(map.selectedTilesPlayer.Where(c => c != currentPlayerTile).Select(d => d.GetLetter()));
                                                

                        Task.Run(async () =>
                        {
                            return await ChooseBestWordCombinationAsync(currentPlayerTile, currentCombination: currentCombination);

                        }).
                        ContinueWithOnMainThread((task) =>
                        {

                            if (task.IsCompletedSuccessfully)
                            {
                                var wordCombinations = task.Result;
                                var objectiveCell = map.GetObjectiveTile();

                                if (wordCombinations != null || wordCombinations.Count > 0)
                                {
                                    
                                    // Intentar encontrar una combinación que contenga la celda objetivo.
                                    var combinationWithObjective = wordCombinations.FirstOrDefault(wc => wc.Cells.Contains(objectiveCell));
                                    if (combinationWithObjective != null)
                                    {
                                        playerPossibleWord = combinationWithObjective;
                                    }


                                    // Si no se encuentra ninguna con la celda objetivo, elegir la que tiene la última celda más cercana a la celda objetivo.
                                    if (playerPossibleWord == null)
                                    {
                                        WordCombination closestCombination = null;
                                        float closestDistance = float.MaxValue;

                                        if (wordCombinations.Count == 1)
                                        {
                                            closestCombination = wordCombinations[0];
                                        }
                                        else
                                        {
                                            foreach (var combination in wordCombinations)
                                            {
                                                var lastCell = combination.Cells.LastOrDefault();
                                                if (lastCell != null)
                                                {
                                                    float distance = 0f; ;

                                                    try
                                                    {
                                                        distance = Vector3.Distance(lastCell.GetComponent<RectTransform>().position, objectiveCell.GetComponent<RectTransform>().position);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Debug.LogError(e);
                                                    }

                                                    if (distance < closestDistance)
                                                    {
                                                        closestDistance = distance;
                                                        closestCombination = combination;
                                                    }
                                                }
                                            }
                                        }

                                        playerPossibleWord = closestCombination;
                                    }
                                }

                            }

                            isSearchingPlayerWord = false;

                        });
                    }                    

                }


                setTrapsTimer = 0;
            }
        }
    }

    /// <summary>
    /// Comprueba si se dispone de las herramientas necesarias para el cambio de letras
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private bool CanChangeLetters(WordCombination word)
    {
        if (word.Changes.Count <= map.opponentChangeLetterButton.NumberOfItemsEquiped)
        {
            return true;
        }
        else if (word.Changes.Count == 1 && !map.opponentChangeLetterButton.isCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Comprueba si las celdas de la palabra seleccionada mantienen las letras en el tablero
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private bool CheckNoChanges(WordCombination word)
    {
        var coinciden = true;
        int index = 0;
        foreach (var cell in word.Cells)
        {
            if (!word.Changes.ContainsKey(index))
            {
                if (word.Letters[index] != cell.GetLetter())
                {
                    coinciden = false;
                    break;
                }
            }

            index++;
        }

        //No coinciden, se resetea la búsqueda
        if (!coinciden)
            CurrentWord = null;

        return coinciden;
    }

    //private IEnumerator ResetWord(Action<bool> callback)
    //{

    //}

    private IEnumerator ValidateWord(Action<bool> callback)
    {
        yield return new WaitForSeconds(5);

        var res = true;
        var wordComb = CurrentWord;

        int index = 0;

        //Recorre letras
        foreach (var cell in wordComb.Cells)
        {
            if (map.OpponentIsFrozen)
            {
                isValidatingWord = false;
                map.FreezeGamePlayer(GameActor.Opponent);
                yield break;
            }

            //if(map.selectedTilesOpponent.Contains(cell))
            //{
            //    index++;
            //    continue;
            //}


            bool selected = map.selectedTilesOpponent.Contains(cell);

            //La primera celda ya está seleccionada
            if (index > 0)
            {
                //Si es la última letra de la palabra y tiene cambios, buscaremos si ya existe la letra para no usar la herramienta equipada
                if (index == (wordComb.Cells.Count - 1) && wordComb.Changes.ContainsKey(index))
                {
                    if (cell.name != map.GetObjectiveTile().name)
                    {
                        if (wordComb.Changes.Where(c => c.Key == index).Any())
                        {
                            var hex = map.CurrentOpponentTile.neighbors.ToList().Where(c => c != null && c.GetLetter() == wordComb.Changes.Where(c => c.Key == index).First().Value).FirstOrDefault();

                            if (hex != null && hex.name != cell.name)
                            {
                                if (!selected)
                                {
                                    SelectTile(hex);
                                    selected = true;
                                }

                                //eliminar cambio realizado
                                wordComb.Changes.Remove(index);
                            }
                        }
                    }
                }

                if (!selected)
                {
                    SelectTile(cell);
                    selected = true;
                }
            }

            //Si existe cambio de letra y coincide con el índicw
            if (wordComb.Changes.Any() && wordComb.Changes.ContainsKey(index))
            {
                //Si tiene herramientas para poder cambiar letra
                if (map.opponentChangeLetterButton.NumberOfItemsEquiped > 0 || !map.opponentChangeLetterButton.isCooldown)
                {
                    var newLetter = wordComb.Changes.Where(c => c.Key == index).First().Value;
                    map.GameEvents_OnFireChageLetter(newLetter, GameActor.Opponent);
                    //map.opponentChangeLetterButton.OnUseTool(newLetter, GameActor.Opponent);

                    //eliminar cambio realizado
                    wordComb.Changes.Remove(index);

                    if (CurrentWord != null)
                        CurrentWord.Changes.Remove(index);

                    yield return new WaitForSeconds(1);
                }
                else
                {
                    //No puede realizar el cambio, se vuelve al inicio y se espera a que recarge la herramienta
                    map.LimpiarYContinuarJuegoOpponent(updateLetter: false);
                    res = false;
                    break;
                }
            }

            index++;
            Debug.LogFormat("Esperamos por la siguiente letra ({0})...", index);
            yield return new WaitForSeconds(1.8f);
        }

        if (map.OpponentIsFrozen)
        {
            isValidatingWord = false;
            map.FreezeGamePlayer(GameActor.Opponent);
            yield break;
        }

        //Se han seleccionado y cambiado letras, se pasa a validar palabra.
        if (res)
            ValidateWord();        

        callback(res);
    }

    private void ValidateWord()
    {
        map.OnValidateWord(GameActor.Opponent);
    }

    private void SelectTile(Hex nextCell)
    {
        map.OnTileSelected(nextCell, GameActor.Opponent);
    }

    private void GameEvents_OnOpponentLoaded()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("LANG")))
            userLang = PlayerPrefs.GetString("LANG");

        //GenerateCombinationsTarget(map.GetCurrentOpponentTile(), "", new List<Hex>(), ref combinations, 0, 3, 6);

        OponentLoaded = true;

    }

    private void GameEvents_OnResolveWord(bool isValid, string wordToValidate, GameActor actor, ref int wordPoints)
    {
        if (actor == GameActor.Opponent)
        {
            hasPendingWordToValidate = false;
        }
    }


    private void GameEvents_OnUpdateCellLetter(Hex cell)
    {
    }


    ///// <summary>
    ///// Genera combinaciones de celdas
    ///// </summary>
    ///// <param name="cell"></param>
    ///// <param name="currentCombination"></param>
    ///// <param name="currentPath"></param>
    ///// <param name="combinations"></param>
    ///// <param name="depth"></param>
    ///// <param name="minDepth"></param>
    ///// <param name="maxDepth"></param>
    ///// <param name="maxCombinationsPerLevel"></param>
    ///// <param name="limitStartingDepth"></param>
    //private void GenerateCombinations(Hex cell, string currentCombination, List<Hex> currentPath, ref Dictionary<int, List<WordCombination>> combinations,
    //    int depth, int minDepth, int maxDepth, int maxCombinationsPerLevel = 300, int limitStartingDepth = 6)
    //{
    //    if (depth >= maxDepth || cell == null || currentPath.Any(c => c.TileName() == cell.TileName())) return;

    //    currentCombination += cell.GetLetter();
    //    currentPath.Add(cell);

    //    if (currentCombination.Length >= minDepth)
    //    {
    //        if (!combinations.ContainsKey(currentCombination.Length))
    //            combinations.Add(currentCombination.Length, new List<WordCombination>());

    //        if (!combinations[currentCombination.Length].Any(c => c.Letters == currentCombination))
    //            combinations[currentCombination.Length].Add(new WordCombination(currentCombination, new List<Hex>(currentPath)));

    //        if (combinations.ContainsKey(currentCombination.Length) && combinations[currentCombination.Length].Count == 500)
    //            Debug.Log("500 Combinaciones: " + currentCombination.Length.ToString());

    //        // Aplicar el límite de combinaciones solo si se cumple el nivel de profundidad desde el cual se debe comenzar a limitar
    //        // y si se ha alcanzado el número máximo de combinaciones permitido para ese nivel de longitud de combinación.
    //        if (currentCombination.Length >= limitStartingDepth && combinations[currentCombination.Length].Count >= maxCombinationsPerLevel)
    //        {
    //            return; // Se detiene la generación de más combinaciones para este nivel si se alcanza el límite.
    //        }
    //    }

    //    foreach (var adjacentCell in cell.neighbors.ToList().Where(c => c != null && c.tileState == GameTileState.Unselected).OrderBy(c => Guid.NewGuid()))
    //    {
    //        if (adjacentCell != null && !currentPath.Any(c => c.TileName() == adjacentCell.TileName()))
    //        {
    //            GenerateCombinations(adjacentCell, currentCombination, new List<Hex>(currentPath), ref combinations, depth + 1, minDepth, maxDepth, maxCombinationsPerLevel, limitStartingDepth);
    //        }
    //    }
    //}



    /// <summary>
    /// Genera combinaciones de celdas cuando se trata de atrapar la celda objetivo
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="currentCombination"></param>
    /// <param name="currentPath"></param>
    /// <param name="combinations"></param>
    /// <param name="depth"></param>
    /// <param name="minDepth"></param>
    /// <param name="maxDepth"></param>
    /// <param name="maxCombinationsPerLevel"></param>
    /// <param name="limitStartingDepth"></param>
    private void GenerateCombinationsTarget(Hex cell, string currentCombination, List<Hex> currentPath, ref Dictionary<int, List<WordCombination>> combinations,
        int depth, int minDepth, int maxDepth, int maxCombinationsPerLevel = 300, int limitStartingDepth = 6)
    {
        if (depth >= maxDepth || cell == null || currentPath.Any(c => c.TileName() == cell.TileName())) return;

        currentCombination += cell.GetLetter();
        currentPath.Add(cell);

        if (currentCombination.Length >= minDepth)
        {
            if (!combinations.ContainsKey(currentCombination.Length))
                combinations.Add(currentCombination.Length, new List<WordCombination>());

            if (!combinations[currentCombination.Length].Any(c => c.Letters == currentCombination))
                combinations[currentCombination.Length].Add(new WordCombination(currentCombination, new List<Hex>(currentPath)));

            if (combinations.ContainsKey(currentCombination.Length) && combinations[currentCombination.Length].Count == 500)
                Debug.Log("500 Combinaciones: " + currentCombination.Length.ToString());

            // Aplicar el límite de combinaciones solo si se cumple el nivel de profundidad desde el cual se debe comenzar a limitar
            // y si se ha alcanzado el número máximo de combinaciones permitido para ese nivel de longitud de combinación.
            if (currentCombination.Length >= limitStartingDepth && combinations[currentCombination.Length].Count >= maxCombinationsPerLevel)
            {
                return; // Se detiene la generación de más combinaciones para este nivel si se alcanza el límite.
            }
        }


        try
        {
            foreach (var adjacentCell in cell.neighbors.ToList().Where(c => c != null && (c.tileState == GameTileState.Unselected || c.tileState == GameTileState.FreezeTrapFromPlayer)).OrderBy(c => c.DistanceToObjective()).Take(2))
            {
                if (adjacentCell != null && !currentPath.Any(c => c.TileName() == adjacentCell.TileName()))
                {
                    GenerateCombinationsTarget(adjacentCell, currentCombination, new List<Hex>(currentPath), ref combinations, depth + 1, minDepth, maxDepth, maxCombinationsPerLevel, limitStartingDepth);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }


    public async Task<List<WordCombination>> ChooseBestWordCombinationAsync(Hex initialCell, short minDepth = 3, short maxDepth = 6, string currentCombination = "")
    {
        List<WordCombination> validWordCombinations = new List<WordCombination>();


        var combinations = new Dictionary<int, List<WordCombination>>();

        await Task.Run(() =>
        {
            GenerateCombinationsTarget(initialCell, currentCombination, new List<Hex>(), ref combinations, 0, minDepth, maxDepth);
        });


        var wordCombinations = await Task.Run(() =>
        {
            var validWordCombinations = new List<WordCombination>();

            // Aplana el diccionario para obtener una lista de todas las WordCombination junto con su clave original.
            var allCombinations = combinations
                .SelectMany(kvp => kvp.Value.Select(wordComb => new { Key = kvp.Key, WordComb = wordComb }))
                .ToList();

            // Mezcla la lista de todas las combinaciones.
            var randomizedCombinations = allCombinations.OrderBy(x => Guid.NewGuid()).ToList();

            // Selecciona aleatoriamente 50 combinaciones o todas si hay menos de 50.
            var selectedCombinations = randomizedCombinations.Take(50).ToList();

            // Primero se buscan posibles palabras de combinaciones iguales
            foreach (var selectedCombination in selectedCombinations)
            {
                if (Regex.IsMatch(dictionaryContent, $"\\b{selectedCombination.WordComb.Letters}\\b", RegexOptions.IgnoreCase))
                {
                    if (!map.OpponentWords.Contains(selectedCombination.WordComb.Letters.ToUpper()))
                    {
                        validWordCombinations.Add(selectedCombination.WordComb);

                        if (validWordCombinations.Count >= 3)
                            break;
                    }
                }
            }

            // Si no se han enccontrado palabras de las combinaciones iguales, buscamos con comodines cambiando alguna letra
            if (validWordCombinations.Count == 0)
            {
                foreach (var selectedCombination in selectedCombinations)
                {
                    List<(int index, char newLetter)> changes = new List<(int index, char newLetter)>();

                    //var success = FindWordsWithWildcard(selectedCombination.WordComb.Letters, ref changes);

                    var combination = selectedCombination.WordComb.Letters;

                    var success = false;
                    for (int i = 0; i < combination.Length; i++)
                    {
                        string pattern = combination.Substring(0, i) + "." + combination.Substring(i + 1);

                        Match match = Regex.Match(dictionaryContent, ("\\b" + pattern + "\\b"), RegexOptions.IgnoreCase);
                        if (match.Success && match.Value[i] != ' ')
                        {
                            char newLetter = match.Value.ToUpper()[i];

                            string wordChanged = new string(selectedCombination.WordComb.Letters.Select((c, index) => index == i ? newLetter : c).ToArray());
                            if (!map.OpponentWords.Contains(wordChanged.ToUpper()))
                            {
                                changes.Add((i, newLetter));
                                success = true;
                                break;
                            }
                            else
                            {
                                success = false;
                                break;
                            }

                        }
                    }


                    if (success)
                    {
                        foreach (var change in changes)
                        {
                            selectedCombination.WordComb.Changes.Add(change.index, change.newLetter);
                        }

                        validWordCombinations.Add(selectedCombination.WordComb);

                        if (validWordCombinations.Count >= 3)
                            break;
                    }
                }
            }

            // Elimina las combinaciones seleccionadas del diccionario original.
            foreach (var selectedCombination in selectedCombinations)
            {
                combinations[selectedCombination.Key].Remove(selectedCombination.WordComb);

                // Si después de eliminar la combinación, la lista en esa clave queda vacía, elimina también la clave.
                if (!combinations[selectedCombination.Key].Any())
                {
                    combinations.Remove(selectedCombination.Key);
                }
            }

            return validWordCombinations;
        });

        return wordCombinations;



    }


}




