using Assets.Scripts.Data;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class DictionaryInstaller : MonoBehaviour
{
    string userLang = LanguageCodes.ES_es;
    Language language = new SpanishLang();
    private void Start()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("LANG")))
            userLang = PlayerPrefs.GetString("LANG");

        if (!string.IsNullOrEmpty(userLang))
            language = Language.GetLanguages().Where(c => c.Code == userLang).FirstOrDefault();

        InstallDictionaries();
    }

    private void OnEnable()
    {
        GameEvents.OnFirebaseInitialize += GameEvents_OnFirebaseInitialize;
    }

    private void OnDestroy()
    {
        GameEvents.OnFirebaseInitialize -= GameEvents_OnFirebaseInitialize;
    }

    private void GameEvents_OnFirebaseInitialize()
    {
        ActualizarDiccionarios();
    }

    private void InstallDictionaries()
    {
        if (!PlayerPrefs.HasKey($"DictionariesInstalled{userLang}"))
        {
            // Obtener todos los archivos de diccionario de la carpeta de recursos
            object[] dictionaryObjects = Resources.LoadAll($"dictionary/{userLang}", typeof(TextAsset));

            var directoryPath = Application.persistentDataPath + $"/dictionary/{userLang}";
            
            if(!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            foreach (var obj in dictionaryObjects)
            {
                TextAsset dictionaryData = obj as TextAsset;
                if (dictionaryData != null)
                {
                    
                    var dictionaryPath = $"/dictionary/{userLang}/{dictionaryData.name}.txt";
                    string filePath = Application.persistentDataPath + dictionaryPath;

                    if (!File.Exists(filePath))
                    {
                        using(var sw = File.CreateText(filePath))
                        {
                            sw.Write(dictionaryData.text);
                            sw.Close();
                        }
                    }
                }
            }

            PlayerPrefs.SetInt($"DictionariesInstalled{userLang}", 1);
            PlayerPrefs.Save();

            if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Resources.UnloadUnusedAssets();
            }

        }
    }


    private void ActualizarDiccionarios()
    {
        // Obtener la fecha de la última actualización
        DateTime ultimaActualizacion = ObtenerFechaUltimaActualizacion();
        long ultimaActualizacionTimestamp = ((DateTimeOffset)ultimaActualizacion).ToUnixTimeSeconds();

        var letterCount = language.Abecedary.Select(c => c.Letter).Count();
        var iter = 0;
        foreach (var letra in language.Abecedary.Select(c => c.Letter))
        {
            FirebaseInitializer.dbRef.RootReference.Child("dictionaries").Child(userLang).Child(letra.ToString().ToLower())
                .OrderByChild("CreationDate").StartAt(ultimaActualizacionTimestamp).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Manejar el error...
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        foreach (DataSnapshot wordSnapshot in snapshot.Children)
                        {
                            var creationDateObj = wordSnapshot.Child("CreationDate").Value;
                            if (creationDateObj != null)
                            {
                                long creationDate = long.Parse(creationDateObj.ToString());
                                if (creationDate > ultimaActualizacionTimestamp)
                                {
                                    // Si la CreationDate es más reciente, actualizar el diccionario local
                                    string palabra = wordSnapshot.Child("NoDiacritics").Value.ToString();
                                    AñadirPalabraAlDiccionarioLocal(letra, palabra);
                                }
                            }
                        }
                    }

                    iter++;

                    if(iter == (letterCount-1))
                    {
                        GuardarFechaUltimaActualizacion(DateTime.UtcNow);
                    }

                });
        }
    }


    private DateTime ObtenerFechaUltimaActualizacion()
    {
        // Valor predeterminado es la fecha mínima si no se ha guardado una fecha antes
        string ultimaActualizacionStr = PlayerPrefs.GetString("UltimaActualizacion", new DateTime(2024, 1, 1).ToString("o"));
        return DateTime.Parse(ultimaActualizacionStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private void GuardarFechaUltimaActualizacion(DateTime fecha)
    {
        PlayerPrefs.SetString("UltimaActualizacion", fecha.ToString("o"));
        PlayerPrefs.Save();
    }

    private void AñadirPalabraAlDiccionarioLocal(char letra, string palabra)
    {

        var dictionaryPath = $"/dictionary/{userLang}/{letra.ToString().ToLower()}.txt";
        string filePath = Application.persistentDataPath + dictionaryPath;

        // Verifica si el archivo ya existe y añade la palabra
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.Write(" " + palabra);  // Añade la palabra seguida de un espacio
        }
    }

}
