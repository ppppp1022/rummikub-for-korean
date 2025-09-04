using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using Unity.Collections;

public class CardCenter : MonoBehaviour
{
    public bool TouchedBase;
    public GameObject LastBaseObj;    //이동 중 Card가 지나치는 Base중 가장 가까운 Base오브젝트

    private BatchBoard batchBoard;
    private CardBoard cardBoard;
    public int LastBatchPosIndex = -1;
    public int LastCardBoardPosIndex = -1;
    public struct CardCenterData : INetworkSerializable
    {
        public bool TouchedBase;
        public int LastBatchPosIndex;
        public int LastCardBoardPosIndex;
        public int LastBaseObjIndex;
        public FixedString128Bytes LastBaseObjName;
        public int PastBaseObjIndex;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TouchedBase);
            serializer.SerializeValue(ref LastBatchPosIndex);
            serializer.SerializeValue(ref LastCardBoardPosIndex);
            serializer.SerializeValue(ref LastBaseObjIndex);
            serializer.SerializeValue(ref LastBaseObjName);
            serializer.SerializeValue(ref PastBaseObjIndex);
        }
    }
    void Awake()
    {
        batchBoard = GameObject.Find("BatchBoard").GetComponent<BatchBoard>();
        cardBoard = GameObject.Find("CardBoard").GetComponent<CardBoard>();
    }
    public CardCenterData GetData()
    {
        //Debug.Log("get data");
        CardCenterData data = new CardCenterData();
        data.TouchedBase = TouchedBase;
        data.LastBatchPosIndex = LastBatchPosIndex;
        data.LastCardBoardPosIndex = LastCardBoardPosIndex;
        if (LastBaseObj.name == "BatchBoardBase(Clone)")
        {
            data.LastBaseObjIndex = FindObjIndexInBatchBoard(LastBaseObj);
            data.LastBaseObjName = "BatchBoardBase(Clone)";
        }
        else
        {
            data.LastBaseObjIndex = FindObjIndexInCardBoard(LastBaseObj);
            data.LastBaseObjName = "CardBoardBase(Clone)";
        }
        return data;
    }
    public void CopyFromOtherCardCenter(CardCenterData cardCenterData, GameObject CardObj)
    {
        TouchedBase = cardCenterData.TouchedBase;
        LastBatchPosIndex = cardCenterData.LastBatchPosIndex;
        LastCardBoardPosIndex = cardCenterData.LastCardBoardPosIndex;
        if (cardCenterData.LastBaseObjName.ToString() == "BatchBoardBase(Clone)")
        {
            LastBaseObj = batchBoard.BatchBaseObj[cardCenterData.LastBaseObjIndex];
            batchBoard.BatchCardObj[LastBatchPosIndex] = CardObj;
        }
        else
        {
            LastBaseObj = cardBoard.CardBaseObj[cardCenterData.LastBaseObjIndex];
            cardBoard.CardBoardCardObj[LastCardBoardPosIndex] = CardObj;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Base")
        {
            if (other.transform.name == "BatchBoardBase(Clone)")
            {
                if (GetComponentInParent<Card>().OwnPlayer.MyTurn)
                {
                    if (LastCardBoardPosIndex != -1)
                    {
                        cardBoard.CardBoardCardObj[LastCardBoardPosIndex] = null;
                        LastCardBoardPosIndex = -1;
                    }
                    int BatchIndex = FindObjIndexInBatchBoard(other.gameObject);
                    if (LastBatchPosIndex != -1)
                    {
                        batchBoard.BatchCardObj[LastBatchPosIndex] = null;
                        batchBoard.BatchCardObj[BatchIndex] = transform.parent.gameObject;
                        LastBatchPosIndex = BatchIndex;
                    }
                    else
                    {
                        batchBoard.BatchCardObj[BatchIndex] = transform.parent.gameObject;
                        LastBatchPosIndex = BatchIndex;
                    }
                    LastBaseObj = other.gameObject;
                }
            }
            else if (other.transform.name == "CardBoardBase(Clone)" && transform.parent.tag != "FixedCard")
            {
                if (LastBatchPosIndex != -1)
                {
                    batchBoard.BatchCardObj[LastBatchPosIndex] = null;
                    LastBatchPosIndex = -1;
                }
                int CardIndex = FindObjIndexInCardBoard(other.gameObject);
                if (LastCardBoardPosIndex != -1)
                {
                    cardBoard.CardBoardCardObj[LastCardBoardPosIndex] = null;
                    cardBoard.CardBoardCardObj[CardIndex] = transform.parent.gameObject;
                    LastCardBoardPosIndex = CardIndex;
                }
                else
                {
                    cardBoard.CardBoardCardObj[CardIndex] = transform.parent.gameObject;
                    LastCardBoardPosIndex = CardIndex;
                }
                LastBaseObj = other.gameObject;
            }
            else
            {
                //Debug.Log(GetComponentInParent<Card>().TheTempLastPosIndex);
                if (GetComponentInParent<Card>().OwnPlayer.MyTurn)
                {
                    LastBaseObj = batchBoard.BatchBaseObj[GetComponentInParent<Card>().TheTempLastPosIndex];
                }
            }
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.transform.tag == "Base")
        {
            //Debug.Log("trigger base");
            if (other.transform.name == "BatchBoardBase(Clone)")
            {
                int BatchIndex = FindObjIndexInBatchBoard(other.gameObject);
                List<int> indices = FindAroundBatchBase(BatchIndex);
                foreach (int index in indices)
                {
                    SpriteRenderer sprite = batchBoard.BatchBaseObj[index].transform.GetComponent<SpriteRenderer>();
                    sprite.color = new Vector4(1, 0, 0, 0.5f);
                }
            }
            else
            {
                int CardIndex = FindObjIndexInCardBoard(other.gameObject);
                List<int> indices = FindAroundCardBase(CardIndex);
                foreach (int index in indices)
                {
                    SpriteRenderer sprite = cardBoard.CardBaseObj[index].transform.GetComponent<SpriteRenderer>();
                    sprite.color = new Vector4(1, 0, 0, 0.5f);
                }
            }
            SpriteRenderer spriteRenderer = other.transform.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Vector4(1, 0, 0, 1);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.tag == "Base")
        {
            if (other.transform.name == "BatchBoardBase(Clone)")
            {
                int BatchIndex = FindObjIndexInBatchBoard(other.gameObject);
                List<int> indices = FindAroundBatchBase(BatchIndex);
                foreach (int index in indices)
                {
                    SpriteRenderer sprite = batchBoard.BatchBaseObj[index].transform.GetComponent<SpriteRenderer>();
                    sprite.color = new Vector4(0, 0, 0, 0);
                }
            }
            else
            {
                int CardIndex = FindObjIndexInCardBoard(other.gameObject);
                List<int> indices = FindAroundCardBase(CardIndex);
                foreach (int index in indices)
                {
                    SpriteRenderer sprite = cardBoard.CardBaseObj[index].transform.GetComponent<SpriteRenderer>();
                    sprite.color = new Vector4(0, 0, 0, 0);
                }
            }
            SpriteRenderer spriteRenderer = other.transform.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Vector4(0, 0, 0, 0);
        }
    }
    public List<GameObject> FindAroundCardObj()
    {
        List<GameObject> AroundCardObj = new List<GameObject>();
        int ObjIndexInBatchBoard = FindCardObjIndexInBatchBoard(transform.parent.gameObject);
        List<int> AroundBaseObj = FindAroundBatchBase(ObjIndexInBatchBoard);
        foreach (int BaseIndex in AroundBaseObj)
        {
            if (batchBoard.BatchCardObj[BaseIndex])
            {
                AroundCardObj.Add(batchBoard.BatchCardObj[BaseIndex]);
            }
        }
        //Debug.Log("Lenght: " + AroundCardObj.Count);
        return AroundCardObj;
    }
    private List<int> FindAroundBatchBase(int BatchIndex)
    {
        List<int> AroundBaseIndex = new List<int>();
        int remainder = BatchIndex % batchBoard.BatchColumn;
        List<int> Around = new List<int>
        {
            BatchIndex - batchBoard.BatchColumn,
            BatchIndex - 1,
            BatchIndex + 1,
            BatchIndex + batchBoard.BatchColumn
        };

        if (Around[0] >= 0)
        {
            AroundBaseIndex.Add(Around[0]);
        }
        if (Around[3] <= batchBoard.BatchColumn * batchBoard.BatchRow - 1)
        {
            AroundBaseIndex.Add(Around[3]);
        }
        if (remainder != 0)
        {
            AroundBaseIndex.Add(Around[1]);
        }
        if (remainder != batchBoard.BatchColumn - 1)
        {
            AroundBaseIndex.Add(Around[2]);
        }
        return AroundBaseIndex;
    }
    private List<int> FindAroundCardBase(int CardIndex)
    {
        List<int> AroundBaseIndex = new List<int>();
        int remainder = CardIndex % cardBoard.CardColumn;
        List<int> Around = new List<int>();
        Around.Add(CardIndex - cardBoard.CardColumn);
        Around.Add(CardIndex - 1);
        Around.Add(CardIndex + 1);
        Around.Add(CardIndex + cardBoard.CardColumn);

        if (Around[0] >= 0)
        {
            AroundBaseIndex.Add(Around[0]);
        }
        if (Around[3] <= cardBoard.CardColumn * cardBoard.CardRow - 1)
        {
            AroundBaseIndex.Add(Around[3]);
        }
        if (remainder != 0 && Around[1] != -1)
        {
            AroundBaseIndex.Add(Around[1]);
        }
        if (remainder != cardBoard.CardColumn - 1 && Around[2] != cardBoard.CardColumn * cardBoard.CardRow)
        {
            AroundBaseIndex.Add(Around[2]);
        }
        return AroundBaseIndex;
    }
    public int FindObjIndexInBatchBoard(GameObject gameObject)
    {
        for (int i = 0; i < batchBoard.BatchBaseObj.Count; i++)
        {
            if (batchBoard.BatchBaseObj[i] == gameObject)
            {
                return i;
            }
        }
        return -1;
    }
    private int FindCardObjIndexInBatchBoard(GameObject gameObject)
    {
        for (int i = 0; i < batchBoard.BatchCardObj.Count; i++)
        {
            if (batchBoard.BatchCardObj[i] == gameObject)
            {
                return i;
            }
        }
        return -1;
    }
    public int FindObjIndexInCardBoard(GameObject gameObject)
    {
        for (int i = 0; i < cardBoard.CardBaseObj.Count; i++)
        {
            if (cardBoard.CardBaseObj[i] == gameObject)
            {
                return i;
            }
        }
        return -1;
    }

}
