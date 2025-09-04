using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerContainer : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> PlayerInfo = new List<TMP_Text>();
    private int activatedPlayerInfoCount = 0;
    public void TurnOnPlayerInfo(List<string> playernames)
    {
        try
        {
            int count = playernames.Count;
            if (activatedPlayerInfoCount != count)
            {
                TurnOffPlayerInfo();
                activatedPlayerInfoCount = count;
            }
            for (int i = 0; i < count; i++)
            {
                PlayerInfo[i].transform.parent.gameObject.SetActive(true);
                PlayerInfo[i].text = playernames[i];
            }
        }
        catch (MissingReferenceException e)
        {
            Debug.Log(e);
            Debug.Log("you are in game!");

            throw;
        }
    }
    private void TurnOffPlayerInfo()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerInfo[i].transform.parent.gameObject.SetActive(false);
        }
    }
}
