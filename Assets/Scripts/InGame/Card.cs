using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Card : MonoBehaviour
{

    public int TheRealLastPosIndex = -1;
    public int TheTempLastPosIndex = -1;
    public Player OwnPlayer;
    public string WordKind; //consonant or vowel
    public string WordFactor;   //ㄱ,ㄴ,ㄷ...ㅏ, ㅑ, ㅣ...
    public int FixedCardSetID = -1;
    private CardRotate cardRotate;
    public int PastBaseObjIndex = -1;
    private Camera cam;
    private float smoothTime = 1000f;
    public struct CardData : INetworkSerializable
    {
        public int TheRealLastPosIndex;
        public int TheTempLastPosIndex;
        public string WordKind;
        public string WordFactor;
        public int WordFactorIndex;
        public int PastBaseObjIndex;
        public Vector3 CardPos;
        public int RotateCount;
        public string CardTag;
        public int CardSetID;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PastBaseObjIndex);
            serializer.SerializeValue(ref TheRealLastPosIndex);
            serializer.SerializeValue(ref TheTempLastPosIndex);
            serializer.SerializeValue(ref WordKind);
            serializer.SerializeValue(ref WordFactor);
            serializer.SerializeValue(ref CardPos);
            serializer.SerializeValue(ref RotateCount);
            serializer.SerializeValue(ref WordFactorIndex);
            serializer.SerializeValue(ref CardTag);
            serializer.SerializeValue(ref CardSetID);
        }
    }
    private void Awake()
    {
        cam = Camera.main;
        cardRotate = GetComponent<CardRotate>();
    }
    private void Start()
    {
        StartCoroutine(SetMyPlayer());
    }
    public void CopyFromOtherCard(CardData cardData)
    {
        TheRealLastPosIndex = cardData.TheRealLastPosIndex;
        TheTempLastPosIndex = cardData.TheTempLastPosIndex;
        PastBaseObjIndex = cardData.TheTempLastPosIndex;
        transform.tag = cardData.CardTag;
        WordKind = cardData.WordKind;
        WordFactor = cardData.WordFactor;
        cardRotate.WordFactorIndex = cardData.WordFactorIndex;
        cardRotate.RotateCount = cardData.RotateCount;
        if (transform.CompareTag("FixedCard"))
        {
            FixedCardSetID = cardData.CardSetID;
            foreach (GameObject cardSetObj in GameObject.FindGameObjectsWithTag("CardSet"))
            {
                CardSet cardSet = cardSetObj.GetComponent<CardSet>();
                if (cardSet.CardSetID == FixedCardSetID)
                {
                    cardSet.FixedCardObj.Add(gameObject);
                    //Debug.Log("Add Fixed Card Object");
                    return;
                }
            }
        }
    }
    IEnumerator SetMyPlayer()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("PlayerInfo").Count() > 0);
        GameObject[] playerSet = GameObject.FindGameObjectsWithTag("PlayerInfo");
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("PlayerInfo"))
        {
            if (playerObj.GetComponent<NetworkObject>().IsOwner)
            {
                OwnPlayer = playerObj.GetComponent<Player>();
                break;
            }
        }
    }
    Vector3 GetMousePos()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
    private void OnMouseDown()
    {
        GetComponent<AudioSource>().Play();
        if (OwnPlayer.MyTurn)
        {
            if (WordKind == "vowel")
            {
                cardRotate.enabled = true;
            }
            if (transform.parent)
            {
                CardSet cardSet = GetComponentInParent<CardSet>();
                cardSet.IsValid = false;
                cardSet.IsSyllable = false;
                transform.SetParent(null);
            }
            CardCenter cardCenter = GetComponentInChildren<CardCenter>();
            GameObject lastBaseObj = cardCenter.LastBaseObj;
            if (lastBaseObj)
            {
                BoxCollider2D BaseCollider2D = lastBaseObj.GetComponent<BoxCollider2D>();
                BaseCollider2D.enabled = true;
            }
            transform.position = GetMousePos();
        }
        else
        {
            if (!transform.parent)
            {
                if (WordKind == "vowel")
                {
                    cardRotate.enabled = true;
                }
                CardCenter cardCenter = GetComponentInChildren<CardCenter>();
                GameObject lastBaseObj = cardCenter.LastBaseObj;
                if (lastBaseObj)
                {
                    BoxCollider2D BaseCollider2D = lastBaseObj.GetComponent<BoxCollider2D>();
                    BaseCollider2D.enabled = true;
                }
                transform.position = GetMousePos();
            }
        }
    }
    private void OnMouseDrag()
    {
        if (OwnPlayer.MyTurn)
        {
            transform.position = GetMousePos();
        }
        else
        {
            if (PastBaseObjIndex == -1)
            {
                transform.position = GetMousePos();
            }
        }
    }
    private void OnMouseUp()
    {
        if (OwnPlayer.MyTurn)
        {
            TheTempLastPosIndex = GetComponentInChildren<CardCenter>().LastBatchPosIndex;
            if (WordKind == "vowel")
            {
                cardRotate.enabled = false;
            }
            StartCoroutine(SendingCardInfoToPlayer());
            //카드 개수 세기 
            int UserCardCount = GameObject.Find("CardBoard").GetComponent<CardBoard>().CardBoardCardObj.Count(g => g != null);
            LocalPlayerInfo localPlayerInfo = GameObject.Find("OwnerPlayerSpawnPoint").GetComponentInChildren<LocalPlayerInfo>();
            localPlayerInfo.UserCardCount.text = UserCardCount.ToString();
            OwnPlayer.UpdateCardCountRpc(UserCardCount, localPlayerInfo.UserOwnerClientID);
        }
        else
        {
            CardCenter cardCenter = GetComponentInChildren<CardCenter>();
            GameObject lastBaseObj = cardCenter.LastBaseObj;
            if (lastBaseObj)
            {
                transform.position = Vector2.Lerp(transform.position, lastBaseObj.transform.position, smoothTime * Time.deltaTime);
                BoxCollider2D BaseCollider2D = lastBaseObj.GetComponent<BoxCollider2D>();
                BaseCollider2D.enabled = false;
            }
        }
    }
    public IEnumerator SendingCardInfoToPlayer()
    {
        yield return new WaitForEndOfFrame();
        //yield return new WaitUntil(() => GetComponentInChildren<CardCenter>().triggerOut == true);
        CardData cardData = new CardData
        {
            TheRealLastPosIndex = TheRealLastPosIndex,
            TheTempLastPosIndex = TheTempLastPosIndex,
            WordKind = WordKind,
            CardTag = transform.tag,
            WordFactor = WordFactor,
            WordFactorIndex = cardRotate.WordFactorIndex,
            CardPos = transform.position,
            PastBaseObjIndex = PastBaseObjIndex,
            RotateCount = cardRotate.RotateCount,
            CardSetID = FixedCardSetID
        };
        CardCenter cardCenter = GetComponentInChildren<CardCenter>();
        CardCenter.CardCenterData centerData = cardCenter.GetData();

        if (TheTempLastPosIndex == -1)
        {
            OwnPlayer.DeleteCard(this, cardData);
            PastBaseObjIndex = TheTempLastPosIndex;
        }
        else
        {
            OwnPlayer.ChangeCard(this, cardData, centerData);
        }
    }
}
