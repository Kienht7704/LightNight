using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script đồng bộ dữ liệu Lobby (tên người chơi, trạng thái Ready).
/// CẦN LÀM: Tạo 1 Empty GameObject -> Đặt tên "LobbyNetworkSync" -> Kéo script này + NetworkObject vào.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class LobbyNetworkSync : NetworkBehaviour
{
    public static LobbyNetworkSync Instance { get; private set; }

    public NetworkList<LobbyPlayerState> LobbyPlayers;

    public event System.Action OnLobbyUpdated;

    private void Awake()
    {
        LobbyPlayers = new NetworkList<LobbyPlayerState>();
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        LobbyPlayers.OnListChanged += (changeEvent) => OnLobbyUpdated?.Invoke();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            // Host tự add chính mình
            AddPlayer(NetworkManager.ServerClientId, LobbyManager.Instance.LocalPlayerName);
        }
        else 
        {
            // Client xin join
            SubmitPlayerJoinServerRpc(LobbyManager.Instance.LocalPlayerName);
        }
        OnLobbyUpdated?.Invoke();
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < LobbyPlayers.Count; i++) {
            if (LobbyPlayers[i].ClientId == clientId) {
                LobbyPlayers.RemoveAt(i); 
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitPlayerJoinServerRpc(string playerName, ServerRpcParams rpcParams = default)
    {
        AddPlayer(rpcParams.Receive.SenderClientId, playerName);
    }

    private void AddPlayer(ulong clientId, string playerName)
    {
        LobbyPlayers.Add(new LobbyPlayerState {
            ClientId = clientId,
            PlayerName = playerName,
            IsReady = false
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        var id = rpcParams.Receive.SenderClientId;
        for (int i = 0; i < LobbyPlayers.Count; i++) {
            if (LobbyPlayers[i].ClientId == id) {
                var p = LobbyPlayers[i];
                p.IsReady = !p.IsReady;
                LobbyPlayers[i] = p; // kích hoạt event NetworkList
                break;
            }
        }
    }

    public bool AreAllReady() {
        if (LobbyPlayers.Count == 0) return false;
        foreach (var p in LobbyPlayers) {
            if (!p.IsReady) return false;
        }
        return true;
    }
}

public struct LobbyPlayerState : INetworkSerializable, System.IEquatable<LobbyPlayerState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public bool IsReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
    }

    public bool Equals(LobbyPlayerState other) => ClientId == other.ClientId && IsReady == other.IsReady && PlayerName.Equals(other.PlayerName);
}
