using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetinLobby : MonoBehaviour
{
    /*
    get username, and ready to get in lobby
    */
    [SerializeField] private GameObject GetinLobbyBtn;
    [SerializeField] private string username;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (GetinLobbyBtn.activeInHierarchy)
            {
                GetInLobby();
            }
        }
    }
    public async void GetInLobby()
    {
        ButtonListener buttonListener = GetComponent<ButtonListener>();
        buttonListener.Playername = username;
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            SceneManager.LoadScene("Lobby");
            buttonListener.SetInitialSettings();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    public void EnterPlayerName(string name)
    {
        username = name;
        if (username != "")
        {
            GetinLobbyBtn.SetActive(true);
        }
        else
        {
            GetinLobbyBtn.SetActive(false);
        }
    }
}
