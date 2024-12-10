using Assets.Scripts.Data;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AuthFirebaseManager : MonoBehaviour
{
    

    //string userId;

    //public Firebase.FirebaseApp app;

    //protected Firebase.Auth.FirebaseAuth auth;

    //protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth = new Dictionary<string, Firebase.Auth.FirebaseUser>();

    //private bool fetchingToken = false;

    

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
    //    auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    //    auth.StateChanged += AuthStateChanged;
    //    auth.IdTokenChanged += IdTokenChanged;
        

    //    AuthStateChanged(this, null);
    //}


    void OnDestroy()
    {
        //if (FirebaseInitializer.auth != null)
        //{
        //    FirebaseInitializer.auth.StateChanged -= AuthStateChanged;
        //    FirebaseInitializer.auth.IdTokenChanged -= IdTokenChanged;
        //}
    }



    //// Track state changes of the auth object.
    //void AuthStateChanged(object sender, System.EventArgs eventArgs)
    //{
    //    Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
    //    Firebase.Auth.FirebaseUser user = null;
    //    if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
    //    if (senderAuth == FirebaseInitializer.auth && senderAuth.CurrentUser != user)
    //    {
    //        bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
    //        if (!signedIn && user != null)
    //        {
    //            Debug.Log("Signed out " + user.UserId);
    //        }
    //        user = senderAuth.CurrentUser;
    //        userByAuth[senderAuth.App.Name] = user;
    //        if (signedIn)
    //        {
    //            Debug.Log("AuthStateChanged Signed in " + user.UserId);
    //        }
    //    }
    //}

    //// Track ID token changes.
    //void IdTokenChanged(object sender, System.EventArgs eventArgs)
    //{
    //    Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
    //    if (senderAuth == FirebaseInitializer.auth && senderAuth.CurrentUser != null && !fetchingToken)
    //    {
    //        senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
    //          task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
    //    }
    //}

    
    // Create a user with the email and password.


    public void OnCreateUserWithEmailAsync(string email, string password, string username)
    {

        Debug.Log(String.Format("Attempting to create user {0}...", email));

        FirebaseInitializer.auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWithOnMainThread((task) => {
              
                  var user = task.Result;
                  GameEvents.OnCreateUserMethod(user.User.UserId, username, email);
              
              return task;
          }).Unwrap();
    }


    // SignIn a user with the email and password.
    public void OnSignInWithEmailAndPasswordAsync(string email, string password)
    {
        Debug.Log(String.Format("Attempting to signIn user {0}...", email));
        

        FirebaseInitializer.auth.SignInWithEmailAndPasswordAsync(email, password)
          .ContinueWithOnMainThread((task) => {
              
                  var user = task.Result;
                  GameEvents.OnSignInMethod(user.User.UserId, email);
              
              return task;
          }).Unwrap();
    }



}
