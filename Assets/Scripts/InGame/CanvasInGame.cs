using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasInGame : MonoBehaviour
{
    [SerializeField] private GameObject ConsonantBtn;
    [SerializeField] private GameObject VowelBtn;
    [SerializeField] private GameObject FinishBtn;
    [SerializeField] private GameObject RollBackBtn;
    [SerializeField] public GameObject EndGameUI;
    public void TurnOnButton()
    {
        ConsonantBtn.SetActive(true);
        VowelBtn.SetActive(true);
        ConsonantBtn.SetActive(true);
        FinishBtn.SetActive(true);
        RollBackBtn.SetActive(true);
        //StartBtn.SetActive(false);
        //WaitBackGroundObj.SetActive(false);
    }
    public void EndofGame()
    {
        EndGameUI.SetActive(true);
    }
}
