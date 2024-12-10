using Assets.Scripts.Data;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Training.Data
{
    public class DictionaryManagerES_es : MonoBehaviour, IDictionaryManager
    {

        //public FirebaseDatabase dbRef;
        //private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        //private const string GOOGLE_URL = "https://www.google.com/async/callback:5493?fc=EswBCowBQUNQd2pqVGdGMU5QVmVMZlZZWXhQWENBWXRySHhlWWo5RFMycmkwTWtXTjVseEx1N0dJWkFReDJsdDVEVmJ6MzMzQnJaa0pEdmc2eXA0MmRRYVFIRGhoRmt1VWk2ODhNMXVoVjZTRVhuaDRUT1FRaHE5UVU2X0ZKSWhDQWhLNkdseVFEam5IeURFdTYSF0YzTVBaT2pOSU1uaGtkVVB3OWVza0FRGiJBRl82TUFOMlFOMVhKMU5HNTFZUzVoTEY4cmY1ZENXU1R3&fcv=3&vet=12ahUKEwiotafvy9n9AhXJcKQEHcMrC0IQg4MCegQIDxAB..i&ei=F3MPZOjNIMnhkdUPw9eskAQ&yv=3&expnd=1&cs=0&async=term:{0},corpus:es,hhdr:true,hwdgt:true,wfp:true,ttl:,tsl:es,ptl:en,htror:false,_ck:xjs.s.4FMbONGMdrE.L.W.O,_fmt:prog,_id:fc_1";
        private const string GOOGLE_URL = "https://www.google.es/async/callback:5493?fc=EswBCowBQU51V2hjbjU2aEI3eEV5S2ZsU3RVLU1PUDV2MlFJNEF5VnF5YUZjQ1BhdjVpdWlTNTdxaTVWS0lqTC1hOUE1a3ZPYWVGNy1SMGhRS3RaYm1BVkhKNFRXaXZtaEhYY0p3NmpRTzdraWNrdEstd01VeXoyN1FGSFVvUmpsSC02RXFpcFVSQVV3YWdEd2oSF3hrV2RaZlBtTVBUNGtkVVBsLTJqd0FRGiJBUFhobTVhSF9RNlgydlBrc1Q2dHE1T1VKOEp2T0hoZkFn&fcv=3&vet=12ahUKEwjz4dLPsNCDAxV0fKQEHZf2CEgQg4MCegQIDhAB..i&ved=2ahUKEwjz4dLPsNCDAxV0fKQEHZf2CEgQu-gBegQIDhAG&bl=vE0u&s=web&opi=89978449&sca_esv=596880998&yv=3&oq={0}&gs_l=dictionary-widget.12..0.5002.10320.0.12618.12.12.0.0.0.0.118.1080.11j1.12.0....0....1.64.dictionary-widget..0.12.1079....0.uFVtWzYiB7Y&expnd=0&cs=0&async=term:{0},corpus:es,hhdr:false,hwdgt:true,wfp:true,ttl:,tsl:,ptl:,htror:false,_ck:xjs.s.fxJy9dPwulE.L.W.O,_k:xjs.s.es.7p7dTzOUeWU.O,_am:ABAAAAQIAAAAAAAAAAAAAAAgAAAAQIIQEA4B2AAB8Ms8AEACCAAggBAsCgHgAEgg4PPfEAAAAAAAABMgsABEBZASfgYBAEATUAXQDnwAAAAABPsBUQCBBwQAAMBADgJoKAQHEAQUQAAAAAB5APA8ADiIsAAAAAAAAAAAAAAggATBcED6URAAAQAAAAAAAAAAAABS0sTKwwBAAw,_csss:ACT90oHFcPuOqfxhW9BrKVi0J8PIVWRXzA,_fmt:prog,_id:fc_1";
        private const string DIC_REVERSO_URL = "https://diccionario.reverso.net/espanol-definiciones/{0}";
        private const string RAE_URL = "https://dle.rae.es/{0}";
        private const string FBVVA = "https://serviciosdms.gnoss.com/diccionariomanuelseco/Buscador?text={0}&searchType=1&resultsPerPage=3&currentPage=1";

        //public void InitializeFirebase()
        //{
        //    Debug.Log("MW LOG: DictionaryManagerES_es InitializeFirebase BEGIN");

        //    Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        //        dependencyStatus = task.Result;
        //        if (dependencyStatus == Firebase.DependencyStatus.Available)
        //        {
        //            dbRef = FirebaseDatabase.DefaultInstance;
        //            Debug.Log("MW LOG: Firebase dbRef initialized: " + dbRef.ToString());
        //        }
        //        else
        //        {
        //            Debug.LogError("MW ERROR: Could not resolve all Firebase dependencies: " + dependencyStatus);
        //        }
        //    });
        //}

        public IEnumerator ValidateWordFbbva(string word, Action<bool> callback)
        {
            // Primera petición a Google
            using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(FBVVA, word)))
            {
                SetupWebRequest(webRequest);

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    if (HandleWebRequestResultFbbva(webRequest, word))
                    {
                        Debug.Log(string.Format("MW LOG: {0}", "Palabra encontrada en Google. Hacemos segunda comprobación..."));
                        callback(true);
                    }
                    else
                    {
                        Debug.Log(string.Format("MW LOG: {0}", "Palabra no encontrada en Google"));
                        callback(false);
                    }
                }
                else
                {
                    // Log del error si la petición no fue exitosa
                    Debug.Log(string.Format("MW ERROR: {0}{1}", "Error en la petición web a diccionario reverso: ", webRequest.error));
                    callback(false);
                }

            }
        }


        public void MigrateWordsFromEnToEs()
        {
            FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.EN_en}")
               .GetValueAsync().ContinueWithOnMainThread(task =>
               {
                   if (task.IsFaulted)
                   {
                       Debug.Log(string.Format("MW ERROR: {0}", task.Exception.Message));
                   }
                   else if (task.IsCompleted && task.Result != null && task.Result.Value != null)
                   {
                       var dict = task.Result.GetRawJsonValue();
                       Dictionary<string, Dictionary<string, Word>> dictData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Word>>>(dict);

                       foreach (var letter in dictData.Values)
                       {
                           foreach (var worddata in letter.Values)
                           {
                               string word = worddata.word;

                               FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.ES_es}/{word[0]}/{word}")
                               .GetValueAsync().ContinueWithOnMainThread(task =>
                               {
                                   if (task.IsFaulted)
                                   {
                                       Debug.Log(string.Format("MW ERROR: {0}", task.Exception.Message));
                                   }
                                   else if (task.IsCompleted && task.Result != null && task.Result.Value != null)
                                   {

                                   }
                                   else
                                   {
                                       var wordData = new Word()
                                       {
                                           word = word,
                                           numberOfLetters = word.Length,
                                           complexityratio = (decimal)DictionaryUtilities.GetWordComplexity(word)
                                       };

                                       string jsonWordData = Newtonsoft.Json.JsonConvert.SerializeObject(wordData);

                                       FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.ES_es}/{word[0]}/{word}")
                                           .SetRawJsonValueAsync(jsonWordData).ContinueWithOnMainThread(task =>
                                           {
                                               if (task.IsFaulted)
                                               {
                                                   // La tarea ha fallado, manejar el error aquí
                                                   Debug.Log(string.Format("MW ERROR: {0}", task.Exception.Message));
                                               }
                                               else if (task.IsCompleted)
                                               {
                                                   // La tarea se completó con éxito
                                                   Debug.Log(string.Format("MW LOG: {0}", "Palabra guardada en Firebase dictionary"));
                                               }
                                           });
                                   }
                               });
                           }
                       }
                   }

               });
        }

        public void SaveWordInFirebase(string word, Action<bool> callback)
        {
            Debug.Log(string.Format("MW LOG: {0}", "SaveWordInDictionary BEGIN"));

            var wordData = new Word()
            {
                word = word,
                numberOfLetters = word.Length,
                complexityratio = (decimal)DictionaryUtilities.GetWordComplexity(word)
            };

            string jsonWordData = Newtonsoft.Json.JsonConvert.SerializeObject(wordData);

            FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.ES_es}/{word[0]}/{word}")
                .SetRawJsonValueAsync(jsonWordData).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        // La tarea ha fallado, manejar el error aquí
                        Debug.Log(string.Format("MW ERROR: {0}", task.Exception.Message));
                        callback(false);
                    }
                    else if (task.IsCompleted)
                    {
                        // La tarea se completó con éxito
                        Debug.Log(string.Format("MW LOG: {0}", "Palabra guardada en Firebase dictionary"));
                        callback(true);
                    }
                });

            Debug.Log(string.Format("MW LOG: {0}", "SaveWordInDictionary END"));
        }


        public void SaveWordInDictionary(string word, Action<bool> callback)
        {
            Console.WriteLine("SaveWordInDictionary BEGIN");

            try
            {
                var wordNoDiacritics = DictionaryUtilities.RemoveDiacritics(word);

                var wordData = new WordInfo()
                {
                    NoDiacritics = word,
                    CreationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                //string firstLetter = wordNoDiacritics.Trim().Substring(0, 1).ToLower();
                //string filePath = $"Assets/dictionary/{LanguageCodes.ES_es}/{firstLetter}.txt";

                ////Dictionary<string, WordInfo> wordsDictionary;
                //if (System.IO.File.Exists(filePath))
                //{
                //    using (var sw = System.IO.File.AppendText(filePath))
                //    {
                //        sw.Write(" " + word);
                //        sw.Close();
                //    }
                //}

                //Console.WriteLine("Palabra guardada en el diccionario local.");

                string jsonWordData = Newtonsoft.Json.JsonConvert.SerializeObject(wordData);


                FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.ES_es}/{word[0]}/{word}")
                .SetRawJsonValueAsync(jsonWordData).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        // La tarea ha fallado, manejar el error aquí
                        Debug.Log(string.Format("MW ERROR: {0}", task.Exception.Message));
                        callback(false);
                    }
                    else if (task.IsCompleted)
                    {
                        // La tarea se completó con éxito
                        Debug.Log(string.Format("MW LOG: {0}", "Palabra guardada en Firebase dictionary"));
                        callback(true);
                    }
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la palabra: {ex.Message}");
                callback(false);
            }

            Console.WriteLine("SaveWordInDictionary END");
        }

        public void ValidateWordFirebase(string word, Action<bool> callback)
        {
            Debug.Log(string.Format("MW LOG: {0}", "ValidateWordFirebase BEGIN"));

            //Primero buscar en nuestro diccionario. Si no existe se buscará en diccionario externo
            FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.ES_es}/{word[0]}/{word}")
               .GetValueAsync().ContinueWithOnMainThread(task =>
               {
                   if (task.IsFaulted)
                   {
                       Debug.Log(string.Format("MW ERROR: {0}", task.Exception.Message));
                       //Debug.LogError("Error al obtener datos de Firebase: " + task.Exception);
                   }
                   else if (task.IsCompleted && task.Result != null && task.Result.Value != null)
                   {
                       var wordInDictionaryJson = task.Result.GetRawJsonValue();

                       Debug.Log(string.Format("MW LOG: {0}{1}", "Palabra encontrado en Firebase dictionary: ", wordInDictionaryJson));

                       Word wordData = JsonConvert.DeserializeObject<Word>(wordInDictionaryJson);

                       if (wordData.complexityratio == 0)
                       {
                           SaveWordInDictionary(word, (result) =>
                           {
                               Debug.Log(string.Format("MW LOG: {0}{1}", "Resultado de guardar palabra en Firebase dictionary: ", result));

                           });
                       }

                       callback(wordData != null && !string.IsNullOrEmpty(wordData.word));
                   }
                   else
                   {
                       Debug.Log(string.Format("MW LOG: {0}", "Palabra no encontrada en Firebase dictionary"));
                       callback(false);
                   }
               });

            Debug.Log(string.Format("MW LOG: {0}", "ValidateWordFirebase END"));
        }

        public void ValidateWordDictionary(string word, Action<bool> callback)
        {
            try
            {
                string firstLetter = word.Trim().Substring(0, 1).ToLower();
                // Asegúrate de que la ruta comience directamente con la ubicación dentro de la carpeta Resources y no incluyas la extensión del archivo.
                //string resourcePath = $"dictionary/es-ES/{firstLetter}";
                //TextAsset dictionaryFile = Resources.Load<TextAsset>(resourcePath);


                var dictionaryPath = $"/dictionary/es-ES/{firstLetter}.txt";
                string filePath = Application.persistentDataPath + dictionaryPath;

                //if (dictionaryFile != null)
                //{
                //    Debug.Log(string.Format("MW LOG: Exists filepath {0}", resourcePath));

                //    string words = dictionaryFile.text;

                //    if (Regex.IsMatch(words, $"\\b{word}\\b", RegexOptions.IgnoreCase))
                //    {
                //        callback(true);
                //        return;
                //    }
                //}

                if (System.IO.File.Exists(filePath))
                {
                    string words = System.IO.File.ReadAllText(filePath);
                    if (Regex.IsMatch(words, $"\\b{word}\\b", RegexOptions.IgnoreCase))
                    {
                        callback(true);
                        return;
                    }
                }                
                else
                {
                    Debug.LogError($"El archivo de diccionario {filePath} no se encontró en el contenedor persistente.");
                }


                // Si no se encuentra la palabra en el diccionario local, se considera inválida.
                Console.WriteLine("Palabra no encontrada en el diccionario local.");
                callback(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error al validar la palabra: {ex.Message}");
                callback(false);
            }

        }

        public IEnumerator ValidateWord(string word, System.Action<bool> resultCallback)
        {

            Debug.Log(string.Format("MW LOG: {0}", "validateWord BEGIN"));

            bool isValid = false;
            bool isValidationComplete = false;

            word = word.Trim().ToLower();
            var wordNodiacritics = DictionaryUtilities.RemoveDiacritics(word);

            if (string.IsNullOrEmpty(word) || word.Length < 3)
            {
                resultCallback(false);
                yield break;
            }


            ValidateWordDictionary(wordNodiacritics, (result) =>
            {
                isValid = result;
                isValidationComplete = true;
            });

            yield return new WaitUntil(() => isValidationComplete);

            if (isValid)
            {
                Debug.Log(string.Format("MW LOG: {0}", "Palabra válida."));

                resultCallback(true);
                yield break;
            }
            else
            {
                // Primera petición a Google
                using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(FBVVA, word)))
                {
                    SetupWebRequest(webRequest);

                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        if (HandleWebRequestResultFbbva(webRequest, word))
                        {
                            var isWordSaved = false;
                            SaveWordInDictionary(wordNodiacritics, (result) =>
                            {
                                isValid = result;
                                isWordSaved = true;
                            });

                            yield return new WaitUntil(() => isWordSaved);
                            resultCallback(true);
                            yield break;
                        }
                    }
                }

                resultCallback(false);
                yield break;
            }

        }

        private void SetupWebRequest(UnityWebRequest webRequest)
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            //webRequest.SetRequestHeader("Connection", "keep-alive");
            webRequest.SetRequestHeader("Accept", "*/*");
            webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
        }

        private bool HandleWebRequestResultGoogle(UnityWebRequest webRequest, string wordNodiacritics, string word)
        {
            string regexPalabra = "<span data-dobid=\"hdw\">(?<def1>.*?)<\\/span>(?<def2>.*?)<\\/div>";
            string html = "";
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(webRequest.url + ": Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.url + ": HTTP Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.Success:
                    html = webRequest.downloadHandler.text;

                    var wordMatches = Regex.Matches(html, regexPalabra, RegexOptions.Multiline);

                    if (wordMatches != null && wordMatches.Count > 0)
                    {
                        foreach (Match wordFounded in wordMatches)
                        {
                            string def1 = "";
                            string def2 = "";

                            if (wordFounded.Success)
                            {
                                def1 = wordFounded.Groups["def1"]?.Value.Replace("\r", "").Replace("\n", "").Trim().ToLower() ?? null;
                                def2 = wordFounded.Groups["def2"]?.Value.Replace("\r", "").Replace("\n", "").Trim().ToLower() ?? null;

                                if (!string.IsNullOrEmpty(def1))
                                {
                                    var matchWord = System.Text.RegularExpressions.Regex.Match(def1, @"^.*?(?<word>[\p{L}]+).*?$");

                                    if (matchWord.Success)
                                    {
                                        def1 = matchWord.Groups["word"]?.Value ?? "";

                                        if (!string.IsNullOrEmpty(def1) && (DictionaryUtilities.RemoveDiacritics(def1) == wordNodiacritics || def1 == word))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(def2))
                                {
                                    var matchWord = System.Text.RegularExpressions.Regex.Match(def2, @"^.*?(?<word>[\p{L}]+).*?$");

                                    if (matchWord.Success)
                                    {
                                        def2 = matchWord.Groups["word"]?.Value ?? "";

                                        if (!string.IsNullOrEmpty(def2) && (DictionaryUtilities.RemoveDiacritics(def2) == wordNodiacritics || def2 == word))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            return false;
        }


        private bool HandleWebRequestResultFbbva(UnityWebRequest webRequest, string word)
        {
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(webRequest.url + ": Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.url + ": HTTP Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.Success:
                    var res = webRequest.downloadHandler.text;

                    Fbbva resFbbva = Newtonsoft.Json.JsonConvert.DeserializeObject<Fbbva>(res) ?? new Fbbva();
                    return resFbbva.response != null && resFbbva.response.Count > 0 && resFbbva.response.Any(c => c.lema == word);
            }
            return false;
        }

        public class Metadata
        {
            public int totalResults { get; set; }
            public int resultsPerPage { get; set; }
            public int currentPage { get; set; }
            public int pages { get; set; }
        }

        public class Response
        {
            public string id { get; set; }
            public string lema { get; set; }
            public int score { get; set; }
        }

        public class Fbbva
        {
            public Metadata metadata { get; set; }
            public List<Response> response { get; set; }
        }

        private bool HandleWebRequestResultDicReverso(UnityWebRequest webRequest, string wordNodiacritics, string word)
        {
            string html = "";
            string regexPalabra = "<h2 class=.*?resh2.*?>(\n{0,}.*?\n{0,})</h2>";
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(webRequest.url + ": Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.url + ": HTTP Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.Success:
                    html = webRequest.downloadHandler.text;
                    var wordmatch = Regex.Match(html, regexPalabra);

                    if (wordmatch.Success)
                    {
                        foreach (Group rel in wordmatch.Groups)
                        {
                            var opcion = rel.Value.Replace("\r", "").Replace("\n", "").Trim().ToLower();
                            if (DictionaryUtilities.RemoveDiacritics(opcion) == wordNodiacritics || opcion == word)
                            {
                                return true;
                            }
                        }
                    }
                    break;
            }
            return false;
        }

        private bool HandleWebRequestResultRae(UnityWebRequest webRequest, string wordNodiacritics, string word)
        {
            string regexPalabra = "<div id=\"resultados\">\\s*<article .*>\\s*<header title=\"Definición de (?<def1>.*?)\".*>$";
            string html = "";
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(webRequest.url + ": Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.url + ": HTTP Error: " + webRequest.error);
                    return false;
                case UnityWebRequest.Result.Success:
                    html = webRequest.downloadHandler.text;

                    var wordMatches = Regex.Matches(html, regexPalabra, RegexOptions.Multiline);

                    if (wordMatches != null && wordMatches.Count > 0)
                    {
                        foreach (Match wordFounded in wordMatches)
                        {
                            string def1 = "";

                            if (wordFounded.Success)
                            {
                                def1 = wordFounded.Groups["def1"]?.Value.Replace("\r", "").Replace("\n", "").Trim().ToLower() ?? null;

                                if (!string.IsNullOrEmpty(def1))
                                {
                                    var splitWord = def1.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var splWors in splitWord)
                                    {
                                        var matchWord = System.Text.RegularExpressions.Regex.Match(splWors.Trim(), @"^.*?(?<word>[\p{L}]+).*?$");

                                        if (matchWord.Success)
                                        {
                                            def1 = matchWord.Groups["word"]?.Value ?? "";

                                            if (!string.IsNullOrEmpty(def1) && (DictionaryUtilities.RemoveDiacritics(def1) == wordNodiacritics || def1 == word))
                                            {
                                                return true;
                                            }
                                        }
                                    }


                                }

                            }
                        }
                    }
                    break;
            }
            return false;
        }



        private int HandleWebRequestCheckWords(UnityWebRequest webRequest)
        {
            string regexPalabra = @"<a href=""https:\/\/www\.qsignifica\.com\/[\wáéíóúñ]*"">([\wáéíóúñ]*)<\/a>";

            string html = "";
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(webRequest.url + ": Error: " + webRequest.error);
                    return 0;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(webRequest.url + ": HTTP Error: " + webRequest.error);
                    return 0;
                case UnityWebRequest.Result.Success:
                    html = webRequest.downloadHandler.text;

                    var wordMatches = Regex.Matches(html, regexPalabra, RegexOptions.Multiline);

                    if (wordMatches != null && wordMatches.Count > 0)
                    {
                        return wordMatches.Where(c => c.Success && c.Groups[1].Length > 2 && c.Groups[1].Length <= 12).Count();
                    }
                    break;
            }
            return 0;
        }

        public int CheckWords(string currentLetters, string adyacentLetter)
        {

            var url = string.Format("https://www.palabrascon.com/palabras-con.php?i={0}&d=38", (currentLetters + adyacentLetter));

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Configura los headers y realiza la petición
                SetupWebRequest(webRequest);
                webRequest.SendWebRequest();
                Thread.Sleep(500);
                var resCount = HandleWebRequestCheckWords(webRequest);
                return resCount;
            }

        }


        //https://www.datsi.fi.upm.es/~coes/interactivo/palabra.cgi?palabra=Working&idioma=english
        //https://www.datsi.fi.upm.es/~coes/interactivo/palabra.cgi?palabra=Gato&idioma=spanish

        //<body bgcolor="#FFF090"><h1>COES: Corrección Ortográfica Interactiva</h1>
        //<hr>
        //<p>La palabra "andarais" no existe en el diccionario.
        // </p><p> Alternativas: 
        //andabais, andaréis, andaríais, anidarais, anudarais, candarais, mandarais, nadarais, andar-r+rais
        //</p><p>
        //</p><hr>
        //<h5>
        //Salida obtenida usando la herramienta de libre distribución
        //ispell con el diccionario y las reglas para español de COES
        //</h5>
        //<h5>
        //<hr>
        //<p>¿Problemas?  E-mail a <i>
        // <a href="mailto:espanol-bugs@datsi.fi.upm.es">&lt;espanol-bugs@datsi.fi.upm.es&gt;</a></i>
        //</p><hr>
        //</h5>
        //</body>



        //<body bgcolor="#FFF090"><h1>COES: Corrección Ortográfica Interactiva</h1>
        //<hr>
        //<p>La palabra "calamar" es correcta 
        //</p><p>
        //</p><hr>
        //<h5>
        //Salida obtenida usando la herramienta de libre distribución
        //ispell con el diccionario y las reglas para español de COES
        //</h5>
        //<h5>
        //<hr>
        //<p>¿Problemas?  E-mail a <i>
        // <a href="mailto:espanol-bugs@datsi.fi.upm.es">&lt;espanol-bugs@datsi.fi.upm.es&gt;</a></i>
        //</p><hr>
        //</h5>

        //</body>

    }
}
