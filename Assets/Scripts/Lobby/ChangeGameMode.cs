using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeGameMode : MonoBehaviour
{
    [SerializeField]private ButtonListener buttonListener;
    void Update()
    {
        if(buttonListener.changedMode)
        {
            SceneManager.LoadScene("InGame");
            this.GetComponent<ChangeGameMode>().enabled = false;
        }
    }
}
