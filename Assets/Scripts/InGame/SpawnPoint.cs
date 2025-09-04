using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject CircleChild;
    public bool IsHere = false;
    public void SettingLocalPlayerInfo(string username, int ID)
    {
        CircleChild.SetActive(true);
        CircleChild.GetComponent<LocalPlayerInfo>().UserOwnerClientID = ID;
        CircleChild.GetComponent<LocalPlayerInfo>().Username.text = username;
        CircleChild.GetComponent<LocalPlayerInfo>().UserCardCount.text = 16.ToString();
    }
    public void UnSettingLocalPlayerInfo()
    {
        CircleChild.GetComponent<LocalPlayerInfo>().UserOwnerClientID = -1;
        CircleChild.GetComponent<LocalPlayerInfo>().Username.text = "";
        CircleChild.GetComponent<LocalPlayerInfo>().UserCardCount.text = 16.ToString();
        CircleChild.SetActive(false);
    }
}
