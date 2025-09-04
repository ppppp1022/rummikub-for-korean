using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingInGame : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] float lerpSpeed = 0.01f;
    [SerializeField] Color targetColor;
    public float loadingTime = 1f;
    void Update()
    {
        if (loadingTime > 0)
        {
            /*
            image.color = Color.Lerp(image.color, targetColor, Time.deltaTime*lerpSpeed);
            loadingTime-=Time.deltaTime;
            */
            loadingTime -= Time.deltaTime;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
