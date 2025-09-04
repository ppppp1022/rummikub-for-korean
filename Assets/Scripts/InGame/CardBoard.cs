using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBoard : MonoBehaviour
{
    public GameObject CardBoardBase;
    private Vector3 InitBasePosition = new Vector3(-4.5f, -2.5f, 0);
    public List<GameObject> CardBaseObj = new List<GameObject>();
    public List<GameObject> CardBoardCardObj = new List<GameObject>();
    public int CardRow = 3;
    public int CardColumn = 13;

    private void Awake()
    {
        float MoveRight = 0.75f;
        float MoveDown = 0.75f;
        Vector3 TempPosition;
        for (int i = 0; i < 13; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i % 13), 0, 0);
            GameObject Base = Instantiate(CardBoardBase, TempPosition, Quaternion.identity, transform);
            CardBaseObj.Add(Base);
            CardBoardCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 13; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i % 13), 0, 0);
            GameObject Base = Instantiate(CardBoardBase, TempPosition, Quaternion.identity, transform);
            CardBaseObj.Add(Base);
            CardBoardCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 13; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i % 13), 0, 0);
            GameObject Base = Instantiate(CardBoardBase, TempPosition, Quaternion.identity, transform);
            CardBaseObj.Add(Base);
            CardBoardCardObj.Add(null);
        }
    }
}
