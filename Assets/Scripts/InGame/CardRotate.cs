using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class CardRotate : MonoBehaviour
{
    private string[][] Vow =
    {
        new string[] {"ㅏ","ㅜ","ㅓ","ㅗ"},
        new string[] {"ㅣ","ㅡ","ㅣ","ㅡ"},
        new string[] {"ㅑ","ㅠ","ㅕ","ㅛ"}
    };
    public int WordFactorIndex;
    public int RotateCount = 0;
    private void Start()
    {
        Card card = GetComponent<Card>();
        FindArrayIndex(card.WordFactor);
        this.enabled = false;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Transform transform = GetComponent<Transform>();
            Card card = GetComponent<Card>();
            transform.Rotate(new Vector3(0, 0, -90f));
            switch (RotateCount % 4)
            {
                case 0:
                    RotateCount++;
                    card.WordFactor = Vow[WordFactorIndex][RotateCount % 4];
                    break;
                case 1:
                    RotateCount++;
                    card.WordFactor = Vow[WordFactorIndex][RotateCount % 4];
                    break;
                case 2:
                    RotateCount++;
                    card.WordFactor = Vow[WordFactorIndex][RotateCount % 4];
                    break;
                case 3:
                    RotateCount++;
                    card.WordFactor = Vow[WordFactorIndex][RotateCount % 4];
                    break;
                default:
                    break;
            }
        }
    }
    private void FindArrayIndex(string wordFactor)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Vow[i][0] == wordFactor)
            {
                WordFactorIndex = i;
                break;
            }
        }
    }
    public string MetchingWord(int rotateCount, int wordFactorIndex)
    {
        //알맞게 글자 뒤집기
        string MetchedWord = "";
        switch (rotateCount % 4)
        {
            case 0:
                MetchedWord = Vow[wordFactorIndex][RotateCount % 4];
                break;
            case 1:
                MetchedWord = Vow[wordFactorIndex][RotateCount % 4];
                break;
            case 2:
                MetchedWord = Vow[wordFactorIndex][RotateCount % 4];
                break;
            case 3:
                MetchedWord = Vow[wordFactorIndex][RotateCount % 4];
                break;
            default:
                break;
        }
        return MetchedWord;
    }
    public string FindOriginWordFactor()
    {
        return Vow[WordFactorIndex][0];
    }
}
