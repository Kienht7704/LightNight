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

    [HideInInspector]
    public NetworkList<LobbyPlayerState> LobbyPlayers;

    public event System.Action OnLobbyUpdated;
    public event System.Action OnCountdownStarted;

    private void Awake()
    {
        LobbyPlayers = new NetworkList<LobbyPlayerState>();
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Tự động tạo và Spawn đối tượng đồng bộ Lobby khi Host mở phòng.
    /// Không cần kéo thả Prefab, không cần đăng ký trong DefaultNetworkPrefabs.
    /// </summary>
    public static void AutoSpawn()
    {
        if (Instance != null) return;
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // Tạo trực tiếp GameObject mới. Không cần prefab!
            GameObject go = new GameObject("LobbyNetworkSync_Runtime");
            go.AddComponent<NetworkObject>();
            go.AddComponent<LobbyNetworkSync>();
            go.GetComponent<NetworkObject>().Spawn();
            Debug.Log("[LobbyNetworkSync] Tu dong Spawn thanh cong!");
        }
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
            SubmitPlayerJoinRpc(LobbyManager.Instance.LocalPlayerName);
        }
        OnLobbyUpdated?.Invoke();
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        if (Instance == this) Instance = null;
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitPlayerJoinRpc(string playerName, RpcParams rpcParams = default)
    {
        AddPlayer(rpcParams.Receive.SenderClientId, playerName);
    }

    private void AddPlayer(ulong clientId, string playerName)
    {
        LobbyPlayers.Add(new LobbyPlayerState {
            ClientId = clientId,
            PlayerName = playerName.Length > 28 ? playerName.Substring(0, 28) : playerName,
            IsReady = false
        });
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleReadyRpc(RpcParams rpcParams = default)
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

    public void StartCountdown()
    {
        if (IsServer) StartCountdownRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void StartCountdownRpc()
    {
        OnCountdownStarted?.Invoke();
    }
}
public struct LobbyPlayerState : INetworkSerializable, System.IEquatable<LobbyPlayerState>
{
    public ulong ClientId;
    public Unity.Collections.FixedString32Bytes PlayerName;
    public bool IsReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
    }

    public bool Equals(LobbyPlayerState other)
    {
        return ClientId == other.ClientId && IsReady == other.IsReady && PlayerName == other.PlayerName;
    }
}
