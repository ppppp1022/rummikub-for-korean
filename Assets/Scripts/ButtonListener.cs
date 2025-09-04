using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonListener : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartBeatTimer;
    private float lobbyUpdateTimer;
    public string Playername;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ChangeGameMode changeGameMode;
    [SerializeField] private GameObject createLobby;
    [SerializeField] private GameObject quickJoin;
    private PlayerContainer playerContainter;
    [SerializeField] private InWaitingRoom inWaitingRoom;
    private GameObject StartBtn;

    //private FixedString128Bytes UserName = "";
    int m_MaxConnections = 4;
    public bool changedMode = false;
    private string joinCode = "";
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }
    public void SetInitialSettings()
    {
        StartCoroutine(IESetInitialSettings());
    }
    private IEnumerator IESetInitialSettings()
    {
        joinCode = "";
        changedMode = false;
        hostLobby = null;
        joinedLobby = null;
        playerContainter = null;
        StartBtn = null;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Lobby");

        GameObject.Find("Nickname").GetComponent<TMP_Text>().text += Playername;

        createLobby = GameObject.Find("CreateLobby");
        quickJoin = GameObject.Find("QuickJoin");
        createLobby.GetComponent<Button>().onClick.AddListener(CreateLobby);
        quickJoin.GetComponent<Button>().onClick.AddListener(QuickJoinLobby);

        playerContainter = GameObject.Find("PlayerContainer").GetComponent<PlayerContainer>();
        StartBtn = GameObject.Find("StartGame");
        StartBtn.GetComponent<Button>().onClick.AddListener(gameManager.StartGame);

        inWaitingRoom.SetGameObject();
        playerContainter.gameObject.SetActive(false);
        StartBtn.SetActive(false);

    }
    private async void HandleLobbyHeartBeat()   //create lobby에서 startcoroutine을 이용햐서 15초마다 불러올 수 있음.
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartBeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            if (joinedLobby.Data["GameMode"].Value != "Start")
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer < 0f)
                {
                    float lobbyUpdateTimerMax = 1.5f;
                    lobbyUpdateTimer = lobbyUpdateTimerMax;
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;
                    playerContainter.TurnOnPlayerInfo(GetPlayers());
                }
            }
            else
            {
                changedMode = true;
            }
        }
    }
    public async void CreateLobby()
    {
        try
        {
            string LobbyName = "LobbyName";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>{
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "NotStart")},
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Member, "")}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;

            Debug.Log("Created lobby: " + lobby.Name + ", " + lobby.MaxPlayers + ", " + lobby.Id + ", " + lobby.LobbyCode);
            inWaitingRoom.WaitingPlayerinRoom();
            playerContainter.gameObject.SetActive(true);
            playerContainter.TurnOnPlayerInfo(GetPlayers());
            StartCoroutine(ConnectRelay_Host());
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void DeleteMyLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
            hostLobby = null;
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>{
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GE),
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "free", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>{
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + ", " + lobby.MaxPlayers + ", " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void JoinLobbyByLobbyCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(),
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Joied with lobby code " + lobbyCode);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer(),
            };
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            Debug.Log("find any lobby: " + joinedLobby.Id);
            StartCoroutine(ConnectRelay_Client());
        }
        catch (LobbyServiceException e)
        {
            inWaitingRoom.AgainSelectGameMode();
            Debug.Log(e);
        }
    }
    public async void LeaveLobby()
    {
        try
        {
            if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId && joinedLobby.Players.Count > 1)
            {
                Debug.Log("host is out");
                MigrateLobbyHost();
            }
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            hostLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void LeaveLobby(string playerId)
    {
        try
        {
            if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId && joinedLobby.Players.Count > 1)
            {
                Debug.Log("host is out");
                MigrateLobbyHost();
            }
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            joinedLobby = null;
            hostLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void KickPlayer()
    {
        try
        {
            string targetPlayerID = joinedLobby.Players[1].Id;
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, targetPlayerID);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void MigrateLobbyHost()
    {
        try
        {
            string target = joinedLobby.Players[1].Id;
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                HostId = target
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private List<string> GetPlayers()
    {
        return GetPlayers(joinedLobby);
    }
    private List<string> GetPlayers(Lobby lobby)
    {
        List<string> playernames = new List<string>();

        //Debug.Log("Players in Lobby: " + lobby.Id + ", " + lobby.Data["GameMode"].Value);
        foreach (Unity.Services.Lobbies.Models.Player player in lobby.Players)
        {
            //Debug.Log(player.Id + ", " + player.Data["PlayerName"].Value);
            playernames.Add(player.Data["PlayerName"].Value);
        }
        return playernames;
    }
    public async void UpdateGameMode(string code = "", string gameMode = "NotStart")
    {
        if (joinCode != "")
        {
            code = joinCode;
        }
        try
        {
            Debug.Log("Let's start Game!");
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>{
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)},
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Member, code)}
                }
            });
            joinedLobby = hostLobby;
            playerContainter.gameObject.SetActive(true);
            playerContainter.TurnOnPlayerInfo(GetPlayers());
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        return new Unity.Services.Lobbies.Models.Player()
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Playername)}
            }
        };
    }
    public static async Task<(RelayServerData, string)> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;
        string createJoinCode;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            createJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        return (new RelayServerData(allocation, "dtls"), createJoinCode);
    }
    IEnumerator ConnectRelay_Host()
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        inWaitingRoom.WaitingPlayerinRoom();

        var (relayServerData, joinCode) = serverRelayUtilityTask.Result;
        UpdateGameMode(joinCode, "NotStart");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        gameManager.SetHost(Playername);
        StartBtn.SetActive(true);
        changeGameMode.enabled = true;
        yield return null;
    }
    public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client: {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }
    IEnumerator ConnectRelay_Client()
    {
        while (joinCode != "")
        {
            Debug.Log("get join code");
            joinCode = joinedLobby.Data["JoinCode"].Value;
            yield return null;
        }
        joinCode = joinedLobby.Data["JoinCode"].Value;
        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }
        inWaitingRoom.WaitingPlayerinRoom();
        
        var relayServerData = clientRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        playerContainter.gameObject.SetActive(true);
        playerContainter.TurnOnPlayerInfo(GetPlayers());
        gameManager.SetClient(Playername);
        changeGameMode.enabled = true;
        yield return null;
    }
    private async void UpdatePlayerName()
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Playername)}
                    }
                }
            );

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
