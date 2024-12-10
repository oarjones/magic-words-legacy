using Assets.Scripts.Data;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    public UserFirebaseManager userManager;

    private void OnEnable()
    {
        GameEvents.OnFirebaseInitialize += GameEvents_OnFirebaseInitialize; //Al inicializarse Firebase
    }

    private void OnDestroy()
    {
        GameEvents.OnFirebaseInitialize -= GameEvents_OnFirebaseInitialize; //Al inicializarse Firebase
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GameEvents_OnFirebaseInitialize()
    {
        
    }




    public void LoadGame()
    {
        PlayerPrefs.SetInt("GameType", (int)GameType.Standalone);
        SceneManager.LoadScene("Game");
    }


    public void LoadGameMultiplayer()
    {
        PlayerPrefs.SetInt("GameType", (int)GameType.Multiplayer);
        SceneManager.LoadScene("Game");
    }
}
