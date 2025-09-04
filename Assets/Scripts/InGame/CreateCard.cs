using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateCard : MonoBehaviour
{
    public Sprite[] consonant;
    private Player OwnPlayer;
    public Sprite[] vowel;
    public GameObject CardObj;
    public GameObject cardBoardObj;
    private CardBoard cardBoard;
    [SerializeField] private GameObject FinishBtn;
    [SerializeField] private GameObject RollBackBtn;
    public GameObject FinishUI;
    public bool CanPass = true;
    public int CheckAll = 0;
    private string[] Con = new string[] { "ㄱ", "ㄴ", "ㄷ", "ㄹ", "ㅁ", "ㅂ", "ㅅ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
    private string[] Vow = new string[] { "ㅏ", "ㅑ", "ㅣ" };
    void Start()
    {
        StartCoroutine(SetOwnPlayer());
        cardBoard = cardBoardObj.GetComponent<CardBoard>();

    }
    public void Pass()
    {
        RollBack();
        OwnPlayer.PassNextPlayer();
    }
    IEnumerator SetOwnPlayer()
    {
        yield return new WaitForSeconds(0.01f);
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("PlayerInfo");
        //Debug.Log(playerObjs.Count());
        foreach (GameObject playerObj in playerObjs)
        {
            if (playerObj.GetComponent<NetworkObject>().IsOwner)
            {
                OwnPlayer = playerObj.GetComponent<Player>();
                break;
            }
        }
        StartCoroutine(StartGame());
    }
    IEnumerator StartGame()
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "InGame");
        //yield return new WaitForSeconds(0.1f);
        try
        {
            if (OwnPlayer.IsHost)
            {
                OwnPlayer.SetPlayerOrderId();
            }
        }
        catch (NullReferenceException e)
        {
            Debug.Log(e);
        }
    }
    public void RollBack()
    {
        BatchBoard batchBoard = GameObject.Find("BatchBoard").GetComponent<BatchBoard>();

        OwnPlayer.DeleteEmptyCardSetClientRpc();
        OwnPlayer.DeleteMyCardFromOtherPlayerRpc();
        // Owner에게만 적용, 나머지는 카드를 삭제하는 방향으로
        for (int j = 0; j < batchBoard.BatchCardObj.Count; j++)
        {
            GameObject cardObj = batchBoard.BatchCardObj[j];
            if (cardObj)
            {
                if (cardObj.tag == "Card")
                {
                    cardObj.transform.SetParent(null);
                    for (int i = 0; i < cardBoard.CardRow * cardBoard.CardColumn; i++)
                    {
                        if (cardBoard.CardBoardCardObj[i] == null)
                        {
                            Vector3 BasePos = cardBoard.CardBaseObj[i].transform.position;
                            BoxCollider2D boxCollider2D = cardBoard.CardBaseObj[i].GetComponent<BoxCollider2D>();
                            boxCollider2D.enabled = false;
                            cardObj.transform.position = BasePos;
                            CardCenter cardCenter = cardObj.GetComponentInChildren<CardCenter>();
                            cardBoard.CardBoardCardObj[i] = cardObj.gameObject;
                            cardCenter.LastCardBoardPosIndex = cardCenter.FindObjIndexInCardBoard(cardBoard.CardBaseObj[i]);
                            cardCenter.LastBaseObj = cardBoard.CardBaseObj[i];
                            break;
                        }
                    }
                    batchBoard.BatchBaseObj[j].GetComponent<BoxCollider2D>().enabled = true;
                    batchBoard.BatchCardObj[j] = null;
                }
            }
        }
    }
    public void GenerateConsonantCard(int type = 0)
    {
        //Debug.Log("d");
        if (type == 0)
        {
            if (OwnPlayer.MyTurn)
            {
                CardBoard cardBoard = cardBoardObj.GetComponent<CardBoard>();
                for (int j = 0; j < cardBoard.CardRow * cardBoard.CardColumn; j++)
                {
                    if (cardBoard.CardBoardCardObj[j] == null)
                    {
                        Vector3 BasePos = cardBoard.CardBaseObj[j].transform.position;
                        BoxCollider2D boxCollider2D = cardBoard.CardBaseObj[j].GetComponent<BoxCollider2D>();
                        boxCollider2D.enabled = false;
                        int randomIndex = UnityEngine.Random.Range(0, consonant.Length);
                        Sprite consonantCard = consonant[randomIndex];
                        GameObject NewCard = Instantiate(CardObj, BasePos, Quaternion.identity);
                        Card card = NewCard.GetComponent<Card>();
                        card.WordKind = "consonant";
                        card.WordFactor = Con[randomIndex];
                        NewCard.GetComponent<SpriteRenderer>().sprite = consonantCard;

                        CardCenter cardCenter = card.GetComponentInChildren<CardCenter>();
                        cardBoard.CardBoardCardObj[j] = card.gameObject;
                        cardCenter.LastCardBoardPosIndex = cardCenter.FindObjIndexInCardBoard(cardBoard.CardBaseObj[j]);
                        cardCenter.LastBaseObj = cardBoard.CardBaseObj[j];
                        break;
                    }
                }
            }
        }
        else if (type == 1)
        {
            CardBoard cardBoard = cardBoardObj.GetComponent<CardBoard>();
            for (int j = 0; j < cardBoard.CardRow * cardBoard.CardColumn; j++)
            {
                if (cardBoard.CardBoardCardObj[j] == null)
                {
                    Vector3 BasePos = cardBoard.CardBaseObj[j].transform.position;
                    BoxCollider2D boxCollider2D = cardBoard.CardBaseObj[j].GetComponent<BoxCollider2D>();
                    boxCollider2D.enabled = false;
                    int randomIndex = UnityEngine.Random.Range(0, consonant.Length);
                    Sprite consonantCard = consonant[randomIndex];
                    GameObject NewCard = Instantiate(CardObj, BasePos, Quaternion.identity);
                    Card card = NewCard.GetComponent<Card>();
                    card.WordKind = "consonant";
                    card.WordFactor = Con[randomIndex];
                    NewCard.GetComponent<SpriteRenderer>().sprite = consonantCard;

                    CardCenter cardCenter = card.GetComponentInChildren<CardCenter>();
                    cardBoard.CardBoardCardObj[j] = card.gameObject;
                    cardCenter.LastCardBoardPosIndex = cardCenter.FindObjIndexInCardBoard(cardBoard.CardBaseObj[j]);
                    cardCenter.LastBaseObj = cardBoard.CardBaseObj[j];
                    break;
                }
            }
        }

    }
    public void GenerateVowelCard(int type)
    {
        if (type == 0)
        {
            if (OwnPlayer.MyTurn)
            {
                CardBoard cardBoard = cardBoardObj.GetComponent<CardBoard>();
                for (int j = 0; j < cardBoard.CardRow * cardBoard.CardColumn; j++)
                {
                    if (cardBoard.CardBoardCardObj[j] == null)
                    {
                        Vector3 BasePos = cardBoard.CardBaseObj[j].transform.position;
                        BoxCollider2D boxCollider2D = cardBoard.CardBaseObj[j].GetComponent<BoxCollider2D>();
                        boxCollider2D.enabled = false;

                        int randomIndex = UnityEngine.Random.Range(0, vowel.Length);
                        Sprite vowelCard = vowel[randomIndex];
                        GameObject NewCard = Instantiate(CardObj, BasePos, Quaternion.identity);
                        Card card = NewCard.GetComponent<Card>();
                        card.WordKind = "vowel";
                        card.WordFactor = Vow[randomIndex];
                        NewCard.GetComponent<SpriteRenderer>().sprite = vowelCard;

                        CardCenter cardCenter = card.GetComponentInChildren<CardCenter>();
                        cardBoard.CardBoardCardObj[j] = card.gameObject;
                        cardCenter.LastCardBoardPosIndex = cardCenter.FindObjIndexInCardBoard(cardBoard.CardBaseObj[j]);
                        cardCenter.LastBaseObj = cardBoard.CardBaseObj[j];
                        break;
                    }
                }
            }
        }
        else if (type == 1)
        {
            CardBoard cardBoard = cardBoardObj.GetComponent<CardBoard>();
            for (int j = 0; j < cardBoard.CardRow * cardBoard.CardColumn; j++)
            {
                if (cardBoard.CardBoardCardObj[j] == null)
                {
                    Vector3 BasePos = cardBoard.CardBaseObj[j].transform.position;
                    BoxCollider2D boxCollider2D = cardBoard.CardBaseObj[j].GetComponent<BoxCollider2D>();
                    boxCollider2D.enabled = false;

                    int randomIndex = UnityEngine.Random.Range(0, vowel.Length);
                    Sprite vowelCard = vowel[randomIndex];
                    GameObject NewCard = Instantiate(CardObj, BasePos, Quaternion.identity);
                    Card card = NewCard.GetComponent<Card>();
                    card.WordKind = "vowel";
                    card.WordFactor = Vow[randomIndex];
                    NewCard.GetComponent<SpriteRenderer>().sprite = vowelCard;

                    CardCenter cardCenter = card.GetComponentInChildren<CardCenter>();
                    cardBoard.CardBoardCardObj[j] = card.gameObject;
                    cardCenter.LastCardBoardPosIndex = cardCenter.FindObjIndexInCardBoard(cardBoard.CardBaseObj[j]);
                    cardCenter.LastBaseObj = cardBoard.CardBaseObj[j];
                    break;
                }
            }
        }
    }
    public int FindWordFactorIndex(string wordFactor, int wordKindType)
    {
        if (wordKindType == 0)
        {
            for (int i = 0; i < 14; i++)
            {
                if (wordFactor == Con[i])
                {
                    return i;
                }
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (wordFactor == Vow[i])
                {
                    return i;
                }
            }
        }
        return -1;
    }
    public void Finish()
    {
        StartCoroutine(FinsihMyTurn());
    }
    public void SwitchBtn(bool type)
    {
        if (type)
        {
            FinishBtn.SetActive(true);
            RollBackBtn.SetActive(true);
        }
        else
        {
            FinishBtn.SetActive(false);
            RollBackBtn.SetActive(false);
        }
    }
    public IEnumerator FinsihMyTurn()
    {
        GameObject[] cardSetObjs = GameObject.FindGameObjectsWithTag("CardSet");
        CheckAll = 0;
        foreach (GameObject cardsetObj in cardSetObjs)
        {
            CardSet cardSet = cardsetObj.GetComponent<CardSet>();
            if (cardsetObj.transform.childCount == 0)
            {

                if (cardSet.FixedCardObj.Count == 0)
                {
                    Destroy(cardsetObj);
                }
            }
            else
            {
                if (!cardSet.IsValid)
                {
                    cardSet.CheckRealWord();
                }
                else
                {
                    CheckAll++;
                }
            }
        }
        yield return new WaitUntil(() => CheckAll == GameObject.FindGameObjectsWithTag("CardSet").Length);

        CanPass = true;
        cardSetObjs = GameObject.FindGameObjectsWithTag("CardSet");
        foreach (GameObject cardsetObj in cardSetObjs)
        {
            CardSet cardSet = cardsetObj.GetComponent<CardSet>();
            if (!cardSet.IsValid)
            {
                GetComponent<ManageSoundInGameRule>().PlaySound(1);
                CanPass = false;
                break;
            }
        }
        if (CanPass)
        {
            int UserCardCount = GameObject.Find("CardBoard").GetComponent<CardBoard>().CardBoardCardObj.Count(g => g != null);
            LocalPlayerInfo localPlayerInfo = GameObject.Find("OwnerPlayerSpawnPoint").GetComponentInChildren<LocalPlayerInfo>();
            localPlayerInfo.UserCardCount.text = UserCardCount.ToString();
            OwnPlayer.UpdateCardCountRpc(UserCardCount, localPlayerInfo.UserOwnerClientID);
            if (UserCardCount == 0)
            {
                OwnPlayer.FinishGameRpc();
            }
            else
            {
                GetComponent<ManageSoundInGameRule>().PlaySound(0);
                OwnPlayer.FixingCardRpc();
                OwnPlayer.PassNextPlayer();
            }
        }
    }
}
