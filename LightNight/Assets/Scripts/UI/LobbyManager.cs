using System;
using System.Threading.Tasks;
using UnityEngine;

// Các Package cần thiết (đã tự động tiêm vào manifest)
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Pro Lobby Manager (Steam Style).
/// Dùng Unity Relay + Auth để tạo Room Code (không cần gõ IP).
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public Action<string> OnError;
    public Action<string> OnHostStarted;
    public Action         OnClientStarted;
    
    public string LocalPlayerName { get; set; } = "Player";
    public string CurrentRoomCode { get; private set; } = "----";

    private async void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null); // Giải quyết lỗi DontDestroyOnLoad only works for root
        DontDestroyOnLoad(gameObject);

        // Khởi tạo hệ thống Unity Gaming Services
        try {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("[LobbyManager] Services Initialized. User ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e) {
            Debug.LogError("Khong the khoi tao Unity Services: " + e.Message);
        }
    }

    // ============================================================
    //  HOST
    // ============================================================
    public async void HostGame(string playerName, int maxPlayers = 4)
    {
        LocalPlayerName = playerName;
        try {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // Xin Unity cấp phát server cục bộ qua Relay
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            CurrentRoomCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            if (NetworkManager.Singleton == null) {
                OnError?.Invoke("Chưa có NetworkManager trong Scene!");
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null) {
                Debug.Log("[LobbyManager] Tu dong fix loi thieu UnityTransport...");
                transport = NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;
            }

            transport.SetHostRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData
            );

            // Bắt đầu Host
            NetworkManager.Singleton.StartHost();
            OnHostStarted?.Invoke(CurrentRoomCode);
        }
        catch (Exception e) {
            OnError?.Invoke("Host loi: " + e.Message);
        }
    }

    // ============================================================
    //  JOIN VỚI ROOM CODE
    // ============================================================
    public async void JoinGame(string roomCode, string playerName)
    {
        LocalPlayerName = playerName;
        // Loại bỏ TẤT CẢ ký tự ẩn (kể cả zero-width space của TMP), dấu cách, chỉ giữ lại chữ cái và số.
        CurrentRoomCode = System.Text.RegularExpressions.Regex.Replace(roomCode.ToUpper(), @"[^A-Z0-9]", "");
        try {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // Gọi Relay chuyển Code thành dữ liệu kết nối
            var alloc = await RelayService.Instance.JoinAllocationAsync(CurrentRoomCode);

            if (NetworkManager.Singleton == null) {
                OnError?.Invoke("Chưa có NetworkManager trong Scene!");
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null) {
                Debug.Log("[LobbyManager] Tu dong fix loi thieu UnityTransport...");
                transport = NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = transport;
            }

            transport.SetClientRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.HostConnectionData
            );

            // Bắt đầu Client
            NetworkManager.Singleton.StartClient();
            OnClientStarted?.Invoke();
        }
        catch (RelayServiceException re) {
            if (re.Reason == RelayExceptionReason.InvalidRequest) {
                OnError?.Invoke("Sai dinh dang! Code Relay phai co 6 ky tu.");
            } else {
                OnError?.Invoke("Khong the ket noi: Kiem tra lai Code!");
            }
            Debug.LogError($"[Relay Error] {re.Reason}: {re.Message}");
        }
        catch (Exception e) {
            OnError?.Invoke("Loi he thong khi ket noi!");
            Debug.LogError(e.Message);
        }
    }

    // ============================================================
    //  DISCONNECT
    // ============================================================
    public void Disconnect()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        CurrentRoomCode = "----";
    }
}
