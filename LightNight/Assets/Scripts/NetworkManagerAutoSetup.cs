using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Gắn script này vào cùng GameObject với NetworkManager trong Scene Lobby.
/// Nó sẽ tự động DontDestroyOnLoad NetworkManager để không bao giờ bị mất khi chuyển scene.
/// </summary>
[RequireComponent(typeof(NetworkManager))]
public class NetworkManagerAutoSetup : MonoBehaviour
{
    private static NetworkManagerAutoSetup _instance;

    private void Awake()
    {
        // Singleton - chỉ tồn tại 1 cái duy nhất
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Tách khỏi parent (nếu có) trước khi DontDestroyOnLoad
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Tự động thêm UnityTransport nếu thiếu
        var transport = GetComponent<UnityTransport>();
        if (transport == null)
        {
            transport = gameObject.AddComponent<UnityTransport>();
            var nm = GetComponent<NetworkManager>();
            if (nm != null) nm.NetworkConfig.NetworkTransport = transport;
            Debug.Log("[NetworkManagerAutoSetup] Da tu dong them UnityTransport.");
        }

        Debug.Log("[NetworkManagerAutoSetup] NetworkManager se ton tai xuyen scene.");
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}
