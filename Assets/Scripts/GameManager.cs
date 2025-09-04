using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Dictionary<ulong, FixedString128Bytes> PlayersNickname = new Dictionary<ulong, FixedString128Bytes>();
    public Dictionary<int, string> PlayersOrder = new Dictionary<int, string>();
    [SerializeField]private ButtonListener buttonListener;
    public bool GameStatus = false;
    public IEnumerator ResetValue()
    {
        yield return new WaitUntil(()=> SceneManager.GetActiveScene().name=="Lobby");
        
        PlayersNickname = new Dictionary<ulong, FixedString128Bytes>();
        PlayersOrder = new Dictionary<int, string>();
        GameStatus = false;
    }
    public void SetHost(FixedString128Bytes Nickname)
    {
        string nicknameStr = Nickname.ToString();
        byte[] nicknameBytes = Encoding.UTF8.GetBytes(nicknameStr);
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = nicknameBytes;
        NetworkManager.Singleton.StartHost();
    }
    public void SetClient(FixedString128Bytes Nickname)
    {
        string nicknameStr = Nickname.ToString();
        byte[] nicknameBytes = Encoding.UTF8.GetBytes(nicknameStr);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = nicknameBytes;
        NetworkManager.Singleton.StartClient();
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;

        string nickname = Encoding.UTF8.GetString(connectionData);

        Debug.Log("connectionData(user nickname): " + nickname);
        PlayersNickname[clientId] = new FixedString128Bytes(nickname);

        // Your approval logic determines the following values
        if (!GameStatus)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
        }

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;
        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        response.Reason = "the room is max";
        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }
    public void StartGame(){
        buttonListener.UpdateGameMode(gameMode:"Start");
    }
    private void Awake()
    {
        // 씬 전환 시에도 제거되지 않도록
        DontDestroyOnLoad(gameObject);
    }
}
