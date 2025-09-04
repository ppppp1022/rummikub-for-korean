using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InWaitingRoom : MonoBehaviour
{
    /*
    when player select create or quick join room, this script make player to get in waiting room
    */
    [SerializeField] private GameObject WaitingRoom;
    [SerializeField] private GameObject SelectMethodObj;
    [SerializeField] private GameObject loadingImg;
    [SerializeField] private GameObject InfoUI;

    public void WaitingPlayerinRoom()
    {
        loadingImg.GetComponent<Loading>().loadingTime = 1.8f;
        loadingImg.SetActive(true);
        WaitingRoom.SetActive(true);
        SelectMethodObj.SetActive(false);
        InfoUI.SetActive(false);

        GameObject.Find("LeaveLobby").GetComponent<Button>().onClick.AddListener(AgainSelectGameMode);
        GameObject.Find("LeaveLobby").GetComponent<Button>().onClick.AddListener(GameObject.Find("LobbyButtonListener").GetComponent<ButtonListener>().LeaveLobby);
    }
    public void AgainSelectGameMode()
    {
        InfoUI.SetActive(true);
        WaitingRoom.SetActive(false);
        SelectMethodObj.SetActive(true);
    }
    public void SetGameObject()
    {
        WaitingRoom = GameObject.Find("WaitingRoom");
        SelectMethodObj = GameObject.Find("SelectMethod");
        loadingImg = GameObject.Find("LoadingImg");
        InfoUI = GameObject.Find("InfoUI");
        WaitingRoom.SetActive(false);
    }
}
