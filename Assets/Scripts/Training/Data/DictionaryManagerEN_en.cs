using Assets.Scripts.Data;
using Firebase.Database;
using Firebase;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Training.Data
{
    public class DictionaryManagerEN_en : MonoBehaviour, IDictionaryManager
    {

        //public FirebaseDatabase dbRef;
        //private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        //private const string GOOGLE_URL = "https://www.google.com/async/callback:5493?fc=EswBCowBQUNQd2pqVGdGMU5QVmVMZlZZWXhQWENBWXRySHhlWWo5RFMycmkwTWtXTjVseEx1N0dJWkFReDJsdDVEVmJ6MzMzQnJaa0pEdmc2eXA0MmRRYVFIRGhoRmt1VWk2ODhNMXVoVjZTRVhuaDRUT1FRaHE5UVU2X0ZKSWhDQWhLNkdseVFEam5IeURFdTYSF0YzTVBaT2pOSU1uaGtkVVB3OWVza0FRGiJBRl82TUFOMlFOMVhKMU5HNTFZUzVoTEY4cmY1ZENXU1R3&fcv=3&vet=12ahUKEwiotafvy9n9AhXJcKQEHcMrC0IQg4MCegQIDxAB..i&ei=F3MPZOjNIMnhkdUPw9eskAQ&yv=3&expnd=1&cs=0&async=term:{0},corpus:es,hhdr:true,hwdgt:true,wfp:true,ttl:,tsl:es,ptl:en,htror:false,_ck:xjs.s.4FMbONGMdrE.L.W.O,_fmt:prog,_id:fc_1";
        private const string GOOGLE_URL = "https://www.google.es/async/callback:5493?fc=EswBCowBQU51V2hjbjU2aEI3eEV5S2ZsU3RVLU1PUDV2MlFJNEF5VnF5YUZjQ1BhdjVpdWlTNTdxaTVWS0lqTC1hOUE1a3ZPYWVGNy1SMGhRS3RaYm1BVkhKNFRXaXZtaEhYY0p3NmpRTzdraWNrdEstd01VeXoyN1FGSFVvUmpsSC02RXFpcFVSQVV3YWdEd2oSF3hrV2RaZlBtTVBUNGtkVVBsLTJqd0FRGiJBUFhobTVhSF9RNlgydlBrc1Q2dHE1T1VKOEp2T0hoZkFn&fcv=3&vet=12ahUKEwjz4dLPsNCDAxV0fKQEHZf2CEgQg4MCegQIDhAB..i&ved=2ahUKEwjz4dLPsNCDAxV0fKQEHZf2CEgQu-gBegQIDhAG&bl=vE0u&s=web&opi=89978449&sca_esv=596880998&yv=3&oq={0}&gs_l=dictionary-widget.12..0.5002.10320.0.12618.12.12.0.0.0.0.118.1080.11j1.12.0....0....1.64.dictionary-widget..0.12.1079....0.uFVtWzYiB7Y&expnd=0&cs=0&async=term:{0},corpus:es,hhdr:false,hwdgt:true,wfp:true,ttl:,tsl:,ptl:,htror:false,_ck:xjs.s.fxJy9dPwulE.L.W.O,_k:xjs.s.es.7p7dTzOUeWU.O,_am:ABAAAAQIAAAAAAAAAAAAAAAgAAAAQIIQEA4B2AAB8Ms8AEACCAAggBAsCgHgAEgg4PPfEAAAAAAAABMgsABEBZASfgYBAEATUAXQDnwAAAAABPsBUQCBBwQAAMBADgJoKAQHEAQUQAAAAAB5APA8ADiIsAAAAAAAAAAAAAAggATBcED6URAAAQAAAAAAAAAAAABS0sTKwwBAAw,_csss:ACT90oHFcPuOqfxhW9BrKVi0J8PIVWRXzA,_fmt:prog,_id:fc_1";
        private const string DIC_REVERSO_URL = "https://diccionario.reverso.net/espanol-definiciones/{0}";

        //public void InitializeFirebase()
        //{
        //    Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        //        dependencyStatus = task.Result;
        //        if (dependencyStatus == Firebase.DependencyStatus.Available)
        //        {
        //            dbRef = FirebaseDatabase.DefaultInstance;
        //        }
        //        else
        //        {
        //            Debug.LogError("MW ERROR: Could not resolve all Firebase dependencies: " + dependencyStatus);
        //        }
        //    });
        //}

        public void MigrateWordsFromEnToEs()
        {
            throw new NotImplementedException();
        }


        public void SaveWordInDictionary(string word, Action<bool> callback)
        {
            word = DictionaryUtilities.RemoveDiacritics(word.Trim().ToLower());

            var wordData = new Word()
            {
                word = word,
                numberOfLetters = word.Length,
                complexityratio = (decimal)DictionaryUtilities.GetWordComplexity(word)
            };

            string jsonWordData = Newtonsoft.Json.JsonConvert.SerializeObject(wordData);

            FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.EN_en}/{word[0]}/{word}")
                .SetRawJsonValueAsync(jsonWordData).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        // La tarea ha fallado, manejar el error aquí
                        Debug.LogError("Error al guardar la palabra en Firebase: " + task.Exception);
                        callback(false);
                    }
                    else if (task.IsCompleted)
                    {
                        // La tarea se completó con éxito
                        Debug.Log("Palabra guardada con éxito en Firebase.");
                        callback(true);
                    }
                });
        }

        public void ValidateWordFirebase(string word, Action<bool> callback)
        {
            word = word.Trim().ToLower();
            var wordNodiacritics = DictionaryUtilities.RemoveDiacritics(word);

            if (string.IsNullOrEmpty(word) || word.Length < 3)
            {
                callback(false);
                return;
            }

            //Primero buscar en nuestro diccionario. Si no existe se buscará en diccionario externo
            FirebaseInitializer.dbRef.GetReference($"dictionaries/{LanguageCodes.ES_es}/{wordNodiacritics[0]}/{wordNodiacritics}")
               .GetValueAsync().ContinueWithOnMainThread(task =>
               {
                   if (task.IsCompleted && task.Result != null && task.Result.Value != null)
                   {
                       var wordInDictionaryJson = task.Result.GetRawJsonValue();
                       Word wordData = JsonConvert.DeserializeObject<Word>(wordInDictionaryJson);

                       if (wordData.complexityratio == 0)
                       {
                           SaveWordInDictionary(wordNodiacritics, (result) =>
                           {
                           });
                       }

                       callback(wordData != null && !string.IsNullOrEmpty(wordData.word));
                   }
                   else
                   {
                       // Manejar error
                       Debug.LogError("Error al obtener datos de Firebase: " + task.Exception);
                       callback(false);
                   }
               });
        }

        public IEnumerator ValidateWord(string word, System.Action<bool> resultCallback)
        {
            bool isValid = false;
            bool isValidationComplete = false;

            word = word.Trim().ToLower();
            var wordNodiacritics = DictionaryUtilities.RemoveDiacritics(word);

            if (string.IsNullOrEmpty(word) || word.Length < 3)
            {
                resultCallback(false);
                yield break;
            }


            ValidateWordFirebase(word, (result) =>
            {
                isValid = result;
                isValidationComplete = true;
            });

            yield return new WaitUntil(() => isValidationComplete);

            if (isValid)
            {
                resultCallback(true);
                yield break;
            }
            else
            {
                // Primera petición a Google
                using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(GOOGLE_URL, word)))
                {
                    SetupWebRequest(webRequest);
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        if (HandleWebRequestResultGoogle(webRequest, wordNodiacritics, word))
                        {                            
                            var isWordSaved = false;
                            SaveWordInDictionary(wordNodiacritics, (result) =>
                            {
                                isValid = result;
                                isWordSaved = true;
                            });

                            yield return new WaitUntil(() => isWordSaved);
                            resultCallback(isValid);
                            yield break;
                        }
                        else
                        {
                            resultCallback(false);
                            yield break;
                        }
                    }
                    else
                    {
                        // Log del error si la petición no fue exitosa
                        Debug.LogError($"Error en la petición web a google: {webRequest.error}");
                        resultCallback(false);
                    }
                }

                // Segunda petición a Diccionario reverso
                using (UnityWebRequest webRequest = UnityWebRequest.Get(string.Format(DIC_REVERSO_URL, word)))
                {
                    SetupWebRequest(webRequest);
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        if (HandleWebRequestResultDicReverso(webRequest, wordNodiacritics, word))
                        {
                            var isWordSaved = false;
                            SaveWordInDictionary(wordNodiacritics, (result) =>
                            {
                                isValid = result;
                                isWordSaved = true;
                            });

                            yield return new WaitUntil(() => isWordSaved);
                            resultCallback(isValid);
                            yield break;
                        }
                        else
                        {
                            resultCallback(false);
                            yield break;
                        }
                    }
                    else
                    {
                        // Log del error si la petición no fue exitosa
                        Debug.LogError($"Error en la petición web a diccionario reverso: {webRequest.error}");
                        resultCallback(false);
                        yield break;
                    }
                }
            }
            
        }

        private void SetupWebRequest(UnityWebRequest webRequest)
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Connection", "keep-alive");
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

        private bool HandleWebRequestResultDicReverso(UnityWebRequest webRequest, string wordNodiacritics, string word)
        {
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
                    var wordmatch = Regex.Match(html, "tu_regex_aquí");

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

        public int CheckWords(string currentLetters, string adyacentLetter)
        {
            throw new NotImplementedException();
        }

    }
}
