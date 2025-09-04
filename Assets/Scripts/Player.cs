using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Linq;

public class Player : NetworkBehaviour
{
    private NetworkVariable<int> TotalCardSetID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Order = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString128Bytes> PlayerNameNV
        = new NetworkVariable<FixedString128Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
          );
    [SerializeField] private TMP_Text UserName;
    [SerializeField] private GameObject CardSet;
    public int NextTurnIndex = 1;
    public GameObject CardObj;
    public bool MyTurn = false;
    private void Awake()
    {
        // 씬 전환 시에도 제거되지 않도록
        DontDestroyOnLoad(gameObject);
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)   //이름 설정해주기기
        {
            foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("PlayerInfo"))
            {
                Player player = playerObj.GetComponent<Player>();
                if (player.PlayerNameNV.Value == "")
                {
                    try
                    {
                        player.PlayerNameNV.Value = GameObject.Find("GameManager").GetComponent<GameManager>().PlayersNickname[player.OwnerClientId];
                        Debug.Log(player.PlayerNameNV.Value);
                    }
                    catch (RpcException e)
                    {
                        Debug.Log(e);
                    }
                }
            }
        }
        UserName.text = PlayerNameNV.Value.ToString();

        StartCoroutine(SetPlayerPlace());
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            DeletePlayerInfo(OwnerClientId);
        }
        foreach (SpawnPoint spawnPoint in GameObject.Find("SpawnPoints").GetComponentsInChildren<SpawnPoint>())
        {
            spawnPoint.UnSettingLocalPlayerInfo();
        }
        StartCoroutine(SetPlayerPlace());
    }

    //게임의 기능
    [Rpc(SendTo.NotOwner)]
    public void UpdateCardCountRpc(int UserCardCount, int clientID)
    {
        foreach (GameObject SpawnObj in GameObject.FindGameObjectsWithTag("Spawn"))
        {
            LocalPlayerInfo localPlayerInfo = SpawnObj.GetComponentInChildren<LocalPlayerInfo>();
            if (localPlayerInfo)
            {
                if (localPlayerInfo.UserOwnerClientID == clientID)
                {
                    localPlayerInfo.UserCardCount.text = UserCardCount.ToString();
                }
            }
        }
    }
    public void PassNextPlayer()
    {
        MyTurn = false;
        SwitchBtnInPlayer(false);
        PassNextPlayerRpc(NextTurnIndex);
    }
    [Rpc(SendTo.Everyone)]
    private void PassNextPlayerRpc(int nextTurnIndex)
    {
        //Debug.Log("Pass Signal");
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("PlayerInfo"))
        {
            if (playerObj.GetComponent<NetworkObject>().IsOwner)
            {
                if (playerObj.GetComponent<Player>().Order.Value == nextTurnIndex)
                {
                    //Debug.Log("I received");
                    Player Currentplayer = playerObj.GetComponent<Player>();
                    Currentplayer.MyTurn = true;
                    Currentplayer.SwitchBtnInPlayer(true);
                }
            }
        }
    }
    private void SwitchBtnInPlayer(bool type)
    {
        GameObject.Find("ButtonListener").GetComponent<CreateCard>().SwitchBtn(type);
    }

    [Rpc(SendTo.Everyone)]
    public void FixingCardRpc()
    {
        GameObject[] cardSetObjs = GameObject.FindGameObjectsWithTag("CardSet");
        foreach (GameObject cardsetObj in cardSetObjs)
        {
            CardSet cardSet = cardsetObj.GetComponent<CardSet>();
            cardSet.IsValid = true;
            cardSet.IsSyllable = true;
            if (cardSet.transform.childCount == 0)
            {
                Destroy(cardsetObj);
            }
            else
            {
                cardSet.FixingTheCard();
            }
        }
    }
    [ClientRpc]
    public void DeleteEmptyCardSetClientRpc()
    {
        GameObject[] FixedCards = GameObject.FindGameObjectsWithTag("FixedCard");

        if (FixedCards.Length != 0)
        {
            GameObject[] cardSetObjs = GameObject.FindGameObjectsWithTag("CardSet");
            foreach (GameObject cardSetObj in cardSetObjs)
            {
                CardSet cardSet = cardSetObj.GetComponent<CardSet>();
                if (cardSet.transform.childCount == 0 && cardSet.FixedCardObj.Count == 0)
                {
                    Destroy(cardSetObj);
                }
                else if (cardSet.FixedCardObj.Count != 0)
                {
                    cardSet.SetOriginParent(); // 이걸 모든 플레이어에 적용 change 이용해서
                }
            }
        }
    }
    [Rpc(SendTo.NotOwner)]
    public void DeleteMyCardFromOtherPlayerRpc()
    {
        BatchBoard batchBoard = GameObject.Find("BatchBoard").GetComponent<BatchBoard>();
        for (int j = 0; j < batchBoard.BatchCardObj.Count; j++)
        {
            GameObject cardObj = batchBoard.BatchCardObj[j];
            if (cardObj)
            {
                if (cardObj.tag == "Card")
                {
                    cardObj.transform.SetParent(null);
                    Destroy(cardObj);
                    batchBoard.BatchBaseObj[j].GetComponent<BoxCollider2D>().enabled = true;
                }
            }
        }
    }
    public void DeleteCard(Card card, Card.CardData cardData)
    {
        DestroyCardInOtherPlayerRpc(cardData);
        CardCenter cardCenter = card.transform.GetComponentInChildren<CardCenter>();
        GameObject lastBaseObj = cardCenter.LastBaseObj;
        if (lastBaseObj)
        {
            //card.transform.position = UnityEngine.Vector2.Lerp(card.transform.position, lastBaseObj.transform.position, smoothTime * Time.deltaTime);
            card.transform.position = lastBaseObj.transform.position;
            BoxCollider2D BaseCollider2D = lastBaseObj.GetComponent<BoxCollider2D>();
            BaseCollider2D.enabled = false;
        }
    }
    public void ChangeCard(Card card, Card.CardData cardData, CardCenter.CardCenterData centerData)
    {
        DestroyCardInAllPlayerRpc(cardData, centerData);
        UploadingCardSetServerRpc(cardData, centerData);
        Destroy(card.gameObject);
    }
    [Rpc(SendTo.NotOwner)]
    private void DestroyCardInOtherPlayerRpc(Card.CardData cardData)
    {
        //Debug.Log("PastBaseObjIndex: " + cardData.PastBaseObjIndex);
        if (cardData.PastBaseObjIndex != -1)
        {
            Destroy(GameObject.Find("BatchBoard").GetComponent<BatchBoard>().BatchCardObj[cardData.PastBaseObjIndex]);
        }
    }
    [Rpc(SendTo.Everyone)]
    private void DestroyCardInAllPlayerRpc(Card.CardData cardData, CardCenter.CardCenterData centerData)
    {
        if (cardData.PastBaseObjIndex != -1)
        {
            GameObject CardObj = GameObject.Find("BatchBoard").GetComponent<BatchBoard>().BatchCardObj[cardData.PastBaseObjIndex];
            if (CardObj)
            {
                Destroy(CardObj);
            }
        }
    }
    [ServerRpc]
    private void UploadingCardSetServerRpc(Card.CardData cardData, CardCenter.CardCenterData centerData)
    {
        //Debug.Log("is it server?" + IsServer);
        if (IsServer)
        {
            TotalCardSetID.Value++;
        }
        UpdateCardSetClientRpc(cardData, centerData);
    }
    [ClientRpc]
    private void UpdateCardSetClientRpc(Card.CardData cardData, CardCenter.CardCenterData centerData)
    {
        GameObject NewCard = SyncCardAndCardCenter(cardData, centerData);
        SetCardInBatchBoard(NewCard.transform);
    }
    private GameObject SyncCardAndCardCenter(Card.CardData cardData, CardCenter.CardCenterData centerData)
    {
        GameObject NewCard = Instantiate(CardObj);
        //NewCard.transform.position = cardData.CardPos;
        NewCard.GetComponent<Card>().CopyFromOtherCard(cardData);
        NewCard.GetComponentInChildren<CardCenter>().CopyFromOtherCardCenter(centerData, NewCard);

        //카드 이미지 삽입
        CreateCard createCard = GameObject.Find("ButtonListener").GetComponent<CreateCard>();
        if (NewCard.GetComponent<Card>().WordKind == "consonant")
        {
            int wordIndex = createCard.FindWordFactorIndex(NewCard.GetComponent<Card>().WordFactor, 0);
            Sprite cardImage = createCard.consonant[wordIndex];
            NewCard.GetComponent<SpriteRenderer>().sprite = cardImage;
        }
        else
        {
            string OriginWord = NewCard.GetComponent<CardRotate>().FindOriginWordFactor();
            int wordIndex = createCard.FindWordFactorIndex(OriginWord, 1);
            Sprite cardImage = createCard.vowel[wordIndex];
            NewCard.GetComponent<SpriteRenderer>().sprite = cardImage;
        }
        //뒤집기
        NewCard.transform.Rotate(new UnityEngine.Vector3(0, 0, -90f * (cardData.RotateCount % 4)));

        return NewCard;
    }
    public void SetCardInBatchBoard(Transform tr)
    {
        CardCenter cardCenter = tr.GetComponentInChildren<CardCenter>();
        GameObject lastBaseObj = cardCenter.LastBaseObj;
        if (lastBaseObj)
        {
            //tr.position = UnityEngine.Vector2.Lerp(tr.position, lastBaseObj.transform.position, smoothTime * Time.deltaTime);
            tr.position = lastBaseObj.transform.position;
            //tr.GetComponent<Card>().TheTempLastPosIndex = cardCenter.FindObjIndexInBatchBoard(lastBaseObj);
            BoxCollider2D BaseCollider2D = lastBaseObj.GetComponent<BoxCollider2D>();
            BaseCollider2D.enabled = false;

            if (lastBaseObj.name == "BatchBoardBase(Clone)")
            {
                List<GameObject> AroundCardObj = cardCenter.FindAroundCardObj();
                GameObject cardsetObj = Instantiate(CardSet, new UnityEngine.Vector3(0, 0, 0), UnityEngine.Quaternion.identity);
                if (AroundCardObj.Count == 0)
                {
                    cardsetObj.GetComponent<CardSet>().CardSetID = TotalCardSetID.Value;
                    tr.SetParent(cardsetObj.transform);
                }
                else
                {
                    foreach (GameObject cardObj in AroundCardObj)
                    {
                        if (cardObj.transform.parent != cardsetObj.transform)
                        {
                            cardObj.transform.parent.SetParent(cardsetObj.transform);
                            CardSet cardSet = cardObj.GetComponentInParent<CardSet>();
                            cardSet.HesitateParent();
                        }
                    }
                    tr.SetParent(cardsetObj.transform);
                }
            }
            else
            {
                //Debug.Log("no name is BatchBoardBase");
            }
        }
        else
        {
            //Debug.Log("no lastBaseObj");
        }
        //Debug.Log("is?" + tr.position);
    }

    //게임 상태 설정
    public void SetPlayerOrderId()
    {
        int PlayerCount = GameObject.Find("GameManager").GetComponent<GameManager>().PlayersNickname.Count;
        List<int> orderIndices = Enumerable.Range(0, PlayerCount).ToList();
        for (int i = 0; i < PlayerCount; i++)
        {
            int randomIndex = Random.Range(i, PlayerCount);

            int temp = orderIndices[i];
            orderIndices[i] = orderIndices[randomIndex];
            orderIndices[randomIndex] = temp;
        }
        int j = 0;
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("PlayerInfo"))
        {
            playerObj.GetComponent<Player>().Order.Value = orderIndices[j];
            //Debug.Log("Player: " + playerObj.GetComponent<Player>().PlayerNameNV.Value.ToString() + ", " + playerObj.GetComponent<Player>().Order.Value);
            j++;
        }
        StartGameRpc();
    }
    [Rpc(SendTo.Everyone)]
    public void StartGameRpc()
    {
        StartCoroutine(StartGame());
    }
    IEnumerator StartGame()
    {
        Debug.Log("Start!");
        //yield return new WaitForSeconds(1f);
        yield return null;

        GameObject.Find("Canvas").GetComponent<CanvasInGame>().TurnOnButton();
        CreateCard createCard = GameObject.Find("ButtonListener").GetComponent<CreateCard>();
        for (int i = 0; i < 5; i++)
        {
            createCard.GenerateConsonantCard(1);
        }
        for (int i = 0; i < 5; i++)
        {
            createCard.GenerateVowelCard(1);
        }

        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("PlayerInfo"))
        {
            if (playerObj.GetComponent<NetworkObject>().IsOwner)
            {

                if (playerObj.GetComponent<Player>().Order.Value == 0)
                {
                    Debug.Log("I'm first");
                    playerObj.GetComponent<Player>().MyTurn = true;
                }
                else
                {
                    Debug.Log("I'm not first, I'm " + playerObj.GetComponent<Player>().Order.Value);
                    SwitchBtnInPlayer(false);

                }
                playerObj.GetComponent<Player>().NextTurnIndex = (playerObj.GetComponent<Player>().Order.Value + 1) % GameObject.FindGameObjectsWithTag("PlayerInfo").Length;
                //Debug.Log("Next turn: " + playerObj.GetComponent<Player>().NextTurnIndex + " in " + GameObject.FindGameObjectsWithTag("PlayerInfo").Length);
                break;
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    public void FinishGameRpc()
    {
        GameObject finishGameUI = GameObject.Find("Canvas").GetComponent<CanvasInGame>().EndGameUI;
        finishGameUI.transform.gameObject.SetActive(true);
        finishGameUI.transform.GetChild(1).GetComponentInChildren<TMP_Text>().text = new string("The winner is\n" + PlayerNameNV.Value.ToString());
        if (IsOwner)
        {
            finishGameUI.GetComponent<EndingSound>().PlaySound(0);
            Debug.Log("you win");
        }
        else
        {
            Debug.Log("you lose");
            finishGameUI.GetComponent<EndingSound>().PlaySound(1);
        }
        if (IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
        //GameObject.Find("LobbyButtonListener").GetComponent<ButtonListener>().LeaveLobby();
        //Destroy(this.gameObject);
    }

    //플레이어 설정과 관련
    private void DeletePlayerInfo(ulong ClientID)
    {
        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.PlayersNickname.Remove(ClientID);
    }
    IEnumerator SetPlayerPlace()
    {
        yield return new WaitUntil(() => UserName.text != "");
        transform.position = new UnityEngine.Vector3(100, 100, 0);
        int i = 0;
        //Debug.Log(GameObject.FindGameObjectsWithTag("PlayerInfo").Length);
        foreach (GameObject PlayerObj in GameObject.FindGameObjectsWithTag("PlayerInfo"))
        {
            if (!PlayerObj.GetComponent<NetworkObject>().IsOwner)
            {
                //Debug.Log("i am not owner about this object " + OwnerClientId);
                Player player = PlayerObj.GetComponent<Player>();
                StartCoroutine(SetOtherPlayerObj(i, player.PlayerNameNV.Value.ToString(), (int)player.OwnerClientId));
                i++;
            }
            else
            {
                PlayerObj.GetComponent<SpriteRenderer>().color = Color.blue;
                Player player = PlayerObj.GetComponent<Player>();
                StartCoroutine(SetSelfPlayerObj(player.PlayerNameNV.Value.ToString(), (int)player.OwnerClientId));
            }
        }
    }
    IEnumerator SetOtherPlayerObj(int i, string username, int clientID)
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Spawn").Length > 0);
        GameObject.Find("SpawnPoints").GetComponentsInChildren<SpawnPoint>()[i].SettingLocalPlayerInfo(username, clientID);
    }
    IEnumerator SetSelfPlayerObj(string username, int clientID)
    {
        yield return new WaitUntil(() => GameObject.Find("OwnerPlayerSpawnPoint"));
        GameObject.Find("OwnerPlayerSpawnPoint").GetComponent<SpawnPoint>().SettingLocalPlayerInfo(username, clientID);
    }
}
