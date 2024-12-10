using Assets.Scripts.Data;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.SceneManagement;

[Serializable]
public class User
{
    [SerializeField]
    public string username;

    [SerializeField]
    public string email;

    [SerializeField]
    public string langCode;

    [SerializeField]
    public int level;

    [SerializeField]
    public int score;

    [SerializeField]
    public int coins;

    [SerializeField]
    public List<UserDevice> devices;

    [SerializeField]
    public GameDivision division;

    [SerializeField]
    public DateTime creationDate;

    public User()
    {
        devices = new List<UserDevice>();
    }

    public User(string username, string email, int level, string langCode = LanguageCodes.ES_es, int score = 0, int coins = 0, GameDivision division = GameDivision.jasper)
    {
        this.username = username;
        this.email = email;
        this.langCode = langCode;
        this.level = level;
        this.division = division;
        this.score = score;
        this.devices = new List<UserDevice>();
        this.coins = coins;

    }
}

[Serializable]
public class UserDevice
{
    [SerializeField]
    public string deviceUniqueIdentifier;
    [SerializeField]
    public string deviceName;
    [SerializeField]
    public string deviceModel;
}

public class UserFirebaseManager : MonoBehaviour
{

    private void OnEnable()
    {
        GameEvents.OnCreateUserEvent += GameEvents_OnCreateUserEvent;
        GameEvents.OnSignInEvent += GameEvents_OnSignInEvent;

    }



    private void OnDisable()
    {
        GameEvents.OnCreateUserEvent -= GameEvents_OnCreateUserEvent;
        GameEvents.OnSignInEvent -= GameEvents_OnSignInEvent;
    }


    private void GameEvents_OnCreateUserEvent(string userid, string username, string email)
    {
        writeNewUser(userid, username, email);
    }


    private void GameEvents_OnSignInEvent(string userid, string email)
    {
        logSignInUser(userid, email);
    }


    //private FirebaseDatabase dbRef;
    //private static DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Start()
    {
        //Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        //    dependencyStatus = task.Result;
        //    if (dependencyStatus == Firebase.DependencyStatus.Available)
        //    {
        //        InitializeFirebase();
        //    }
        //    else
        //    {
        //        Debug.LogError(
        //          "Could not resolve all Firebase dependencies: " + dependencyStatus);
        //    }
        //});
    }

    // Handle initialization of the necessary firebase modules:
    //protected void InitializeFirebase()
    //{
    //    dbRef = FirebaseDatabase.DefaultInstance;
    //}


    public void writeNewUser(string userId, string name, string email, int level = 1, string langCode = LanguageCodes.ES_es, int score = 0)
    {
        try
        {
            if (FirebaseInitializer.dbRef != null)
            {
                //var userLEvel = new PlayerGameLevel();
                //userLEvel.LevelPhaseStatus = new Dictionary<gameLevel, Dictionary<GameMode, List<PlayerGameLevelPhase>>>();

                //foreach (var gameLevel in GameConfiguration.GameLevels)
                //{
                //    userLEvel.LevelPhaseStatus.Add(gameLevel.Level, new Dictionary<GameMode, List<PlayerGameLevelPhase>>());

                //    foreach(var levelMode in gameLevel.LevelsMode)
                //    {
                //        userLEvel.LevelPhaseStatus[gameLevel.Level].Add(levelMode.Mode, 
                //            levelMode.Fases.Select(c => new PlayerGameLevelPhase() { FaseNumber = c.FaseNumber, Complete = false}).ToList());
                        
                //    }
                //}

                User user = new User(name, email, level, langCode);
                user.devices = new List<UserDevice>();
                var userdevice = new UserDevice()
                {
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    deviceName = SystemInfo.deviceName,
                    deviceModel = SystemInfo.deviceModel
                };

                user.devices.Add(userdevice);
                string json = JsonUtility.ToJson(user);
                FirebaseInitializer.dbRef.RootReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread((task) =>
                {
                    if (task.IsFaulted)
                    {
                        // La tarea ha fallado, manejar el error aquí
                        Debug.LogError("Error al guardar el usuario " + task.Exception);
                        //callback(false);
                    }
                    else if (task.IsCompleted)
                    {
                        // La tarea se completó con éxito
                        Debug.Log("Usuario guardada con éxito en Firebase.");

                        UserStatistics userStatistics = new UserStatistics();
                        string jsonStatistics = JsonUtility.ToJson(userStatistics);
                        FirebaseInitializer.dbRef.RootReference.Child("statistics").Child(userId).SetRawJsonValueAsync(jsonStatistics).ContinueWithOnMainThread((task) =>
                        {
                            if (task.IsFaulted)
                            {
                                // La tarea ha fallado, manejar el error aquí
                                Debug.LogError("Error al guardar las estadísticas de usuario " + task.Exception);
                                //callback(false);
                            }
                            else if (task.IsCompleted)
                            {
                                // La tarea se completó con éxito
                                Debug.Log("Estadísticas de usuario guardadas con éxito en Firebase.");
                                //callback(true);
                            }
                        });

                        //callback(true);
                    }
                });


            }
            else
            {
                throw new Exception("No se ha inicializado FirebaseDatabase dbRef!");
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void updateUser(string userId, string name, string email, int level = 1, string langCode = LanguageCodes.ES_es, int score = 0)
    {
        try
        {
            if (FirebaseInitializer.dbRef != null)
            {
                //var userLEvel = new PlayerGameLevel();
                //userLEvel.CurrentLevel = gameLevel.Grifo;
                //userLEvel.LevelPhaseStatus = new Dictionary<gameLevel, Dictionary<GameMode, List<PlayerGameLevelPhase>>>();

                //foreach (var gameLevel in GameConfiguration.GameLevels)
                //{
                //    userLEvel.LevelPhaseStatus.Add(gameLevel.Level, new Dictionary<GameMode, List<PlayerGameLevelPhase>>());

                //    foreach (var levelMode in gameLevel.LevelsMode)
                //    {
                //        userLEvel.LevelPhaseStatus[gameLevel.Level].Add(levelMode.Mode,
                //            levelMode.Fases.Select(c => new PlayerGameLevelPhase() { FaseNumber = c.FaseNumber, Complete = false }).ToList());

                //    }
                //}

                User user = new User(name, email, level, langCode);
                user.devices = new List<UserDevice>();
                var userdevice = new UserDevice()
                {
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    deviceName = SystemInfo.deviceName,
                    deviceModel = SystemInfo.deviceModel
                };

                user.devices.Add(userdevice);
                string json = JsonUtility.ToJson(user);
                FirebaseInitializer.dbRef.RootReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread((task) =>
                {
                    if (task.IsFaulted)
                    {
                        // La tarea ha fallado, manejar el error aquí
                        Debug.LogError("Error al guardar el usuario " + task.Exception);
                        //callback(false);
                    }
                    else if (task.IsCompleted)
                    {
                        // La tarea se completó con éxito
                        Debug.Log("Usuario guardada con éxito en Firebase.");

                        UserStatistics userStatistics = new UserStatistics();
                        string jsonStatistics = JsonUtility.ToJson(userStatistics);
                        FirebaseInitializer.dbRef.RootReference.Child("statistics").Child(userId).SetRawJsonValueAsync(jsonStatistics).ContinueWithOnMainThread((task) =>
                        {
                            if (task.IsFaulted)
                            {
                                // La tarea ha fallado, manejar el error aquí
                                Debug.LogError("Error al guardar las estadísticas de usuario " + task.Exception);
                                //callback(false);
                            }
                            else if (task.IsCompleted)
                            {
                                // La tarea se completó con éxito
                                Debug.Log("Estadísticas de usuario guardadas con éxito en Firebase.");
                                //callback(true);
                            }
                        });

                        //callback(true);
                    }
                });


            }
            else
            {
                throw new Exception("No se ha inicializado FirebaseDatabase dbRef!");
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    //public void updateUsers()
    //{
    //    if (FirebaseInitializer.dbRef != null)
    //    {
    //        FirebaseInitializer.dbRef.RootReference.Child("users").GetValueAsync().ContinueWithOnMainThread((task) =>
    //        {
    //            if (task.IsFaulted)
    //            {
    //                // La tarea ha fallado, manejar el error aquí
    //                Debug.LogError("Error al guardar las estadísticas de usuario " + task.Exception);
    //            }
    //            else if (task.IsCompleted)
    //            {
    //                DataSnapshot snapshot = task.Result;

    //                // Iterar sobre todos los usuarios y actualizar el nivel
    //                foreach (DataSnapshot userSnapshot in snapshot.Children)
    //                {
    //                    // Obtén la clave del usuario actual
    //                    string userKey = userSnapshot.Key;

    //                    //var userLEvel = new PlayerGameLevel();
    //                    //userLEvel.CurrentLevel = gameLevel.Grifo;
    //                    //userLEvel.LevelPhaseStatus = new Dictionary<gameLevel, Dictionary<GameMode, List<PlayerGameLevelPhase>>>();

    //                    //foreach (var gameLevel in GameConfiguration.GameLevels)
    //                    //{
    //                    //    userLEvel.LevelPhaseStatus.Add(gameLevel.Level, new Dictionary<GameMode, List<PlayerGameLevelPhase>>());

    //                    //    foreach (var levelMode in gameLevel.LevelsMode)
    //                    //    {
    //                    //        userLEvel.LevelPhaseStatus[gameLevel.Level].Add(levelMode.Mode,
    //                    //            levelMode.Fases.Select(c => new PlayerGameLevelPhase() { FaseNumber = c.FaseNumber, Complete = false }).ToList());

    //                    //    }
    //                    //}

    //                    // Actualiza el nivel del usuario en Firebase
    //                    // Obtener la fecha actual en formato string
    //                    string creationDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"); // Formato ISO 8601
    //                    //var userLevelJson = Newtonsoft.Json.JsonConvert.SerializeObject(userLEvel);

    //                    FirebaseInitializer.dbRef.RootReference.Child("users").Child(userKey).Child("level").SetRawJsonValueAsync(userLevelJson);

    //                    // Añade la fecha de creación al usuario
    //                    //FirebaseInitializer.dbRef.RootReference.Child("users").Child(userKey).Child("creationDate").SetValueAsync(creationDate);
    //                }

    //                // La tarea se completó con éxito
    //                Debug.Log("Estadísticas de usuario guardadas con éxito en Firebase.");
    //            }
    //        });
    //    }
    //}

    private void logSignInUser(string userid, string email)
    {
        Debug.Log($"New login:{userid} - {email}");
        //SceneManager.LoadScene("GameTypeSelection");

        //if (!MultiplayerGameManager.ModoTestMultiplayer)
        //    SceneManager.LoadScene("GameTypeSelection");
        //else
        //    SceneManager.LoadScene("MultiplayerGame");

    }


}
