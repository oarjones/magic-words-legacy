using Assets.Scripts.Data;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{

    public static FirebaseAuth auth;
    public static FirebaseDatabase dbRef;
    public static Firebase.Functions.FirebaseFunctions functions;
    private static DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    public static bool FirebaseInitialized = false;
    public static Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth = new Dictionary<string, Firebase.Auth.FirebaseUser>();
    private static bool fetchingToken = false;

    

    protected void InitializeFirebase()
    {





    }

    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
            auth.IdTokenChanged -= IdTokenChanged;
        }
    }

    private void Awake()
    {
        LoadMain();
    }

    private void OnEnable()
    {
        
    }

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadMain()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                try
                {
                    dbRef = FirebaseDatabase.DefaultInstance;

#if UNITY_EDITOR
                    dbRef.SetPersistenceEnabled(false);
#endif


                    auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                    functions = Firebase.Functions.FirebaseFunctions.DefaultInstance;

                    GameEvents.FirebaseInitialize();

                    auth.StateChanged += AuthStateChanged;
                    auth.IdTokenChanged += IdTokenChanged;
                    AuthStateChanged(null, null);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }

                FirebaseInitialized = true;
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });


        var userLang = PlayerPrefs.GetString("LANG");
        int userLevel = PlayerPrefs.GetInt("LEVEL");

        if (string.IsNullOrEmpty(userLang))
        {
            PlayerPrefs.SetString("LANG", LanguageCodes.ES_es);
            userLang = LanguageCodes.ES_es;
        }

        //Language.SetUserLanguage(userLang);



        if (!(userLevel > 0))
        {
            PlayerPrefs.SetInt("LEVEL", 1);
        }


    }





    // Track state changes of the auth object.
    static void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        //Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (auth != null) userByAuth.TryGetValue(auth.App.Name, out user);
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            userByAuth[auth.App.Name] = user;
            if (signedIn)
            {
                Debug.Log("AuthStateChanged Signed in " + user.UserId);
            }
        }
    }

    // Track ID token changes.
    static void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == FirebaseInitializer.auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
              task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }
}
