using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BatchBoard : MonoBehaviour
{
    public GameObject BatchBoardBase;
    public Vector3 InitBasePosition;
    public List<GameObject> BatchBaseObj = new List<GameObject>();
    public List<GameObject> BatchCardObj = new List<GameObject>();
    public int BatchRow = 8;
    public int BatchColumn = 16;
    private void Awake()
    {
        float MoveRight = 0.75f;
        float MoveDown = 0.75f;
        Vector3 TempPosition;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
        InitBasePosition.y -= MoveDown;
        for (int i = 0; i < 16; i++)
        {
            TempPosition = InitBasePosition + new Vector3(MoveRight * (i), 0, 0);
            GameObject Base = Instantiate(BatchBoardBase, TempPosition, Quaternion.identity, transform);
            BatchBaseObj.Add(Base);
            BatchCardObj.Add(null);
        }
    }

}
