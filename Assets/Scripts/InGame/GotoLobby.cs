using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GotoLobby : MonoBehaviour
{
    public void LeaveGame()
    {
        Debug.Log("Leave");
        ButtonListener buttonListener = GameObject.Find("LobbyButtonListener").GetComponent<ButtonListener>();
        buttonListener.SetInitialSettings();
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        StartCoroutine(gameManager.ResetValue());
        SceneManager.LoadScene("Lobby");
    }
}
