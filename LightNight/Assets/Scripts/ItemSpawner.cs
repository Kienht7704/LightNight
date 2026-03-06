using UnityEngine;
using System.Collections.Generic;

public class ItemSpawnManager : MonoBehaviour
{
    [Header("Cấu hình Item")]
    [Tooltip("Danh sách các Prefab item bạn muốn spawn")]
    public GameObject[] itemPrefabs;

    [Header("Cấu hình Spawn")]
    public float spawnInterval = 60f; // Thời gian hồi 60 giây
    public int maxItemsOnMap = 5;    
    public Transform[] spawnPoints;  // Các vị trí có thể xuất hiện item

    // Danh sách để theo dõi các item đang tồn tại trên map
    private List<GameObject> _activeItems = new List<GameObject>();
    private float _nextSpawnTime;

    void Start()
    {
        // Lần spawn đầu tiên sẽ diễn ra sau 60s kể từ khi bắt đầu
        _nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        // 1. Kiểm tra thời gian hồi
        if (Time.time >= _nextSpawnTime)
        {
            // 2. Kiểm tra số lượng tối đa
            // Trước khi kiểm tra, hãy dọn dẹp danh sách (loại bỏ các item đã bị người chơi nhặt/Destroy)
            CleanActiveItemsList();

            if (_activeItems.Count < maxItemsOnMap)
            {
                SpawnRandomItem();
            }

            // Thiết lập mốc thời gian tiếp theo (ngay cả khi không spawn được do đầy, bộ đếm vẫn chạy)
            _nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnRandomItem()
    {
        if (itemPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Chọn ngẫu nhiên 1 item trong danh sách
        GameObject randomPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        // Chọn ngẫu nhiên 1 vị trí trong danh sách spawn points
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Tạo Item
        GameObject newItem = Instantiate(randomPrefab, randomPoint.position, randomPoint.rotation);

        // Thêm vào danh sách quản lý
        _activeItems.Add(newItem);

        Debug.Log($"Đã spawn: {newItem.name}. Số lượng hiện tại: {_activeItems.Count}/{maxItemsOnMap}");
    }

    // Hàm này giúp dọn dẹp các tham chiếu "null" khi Item đã bị Destroy trong lúc chơi
    void CleanActiveItemsList()
    {
        for (int i = _activeItems.Count - 1; i >= 0; i--)
        {
            if (_activeItems[i] == null)
            {
                _activeItems.RemoveAt(i);
            }
        }
    }

    // (Tùy chọn) Hiển thị thời gian còn lại ra màn hình Console để debug
    void OnGUI()
    {
        float timeLeft = _nextSpawnTime - Time.time;
        if (timeLeft > 0)
        {
            GUILayout.Label($"Tiếp tục spawn sau: {timeLeft:F1}s | Hiện tại: {_activeItems.Count}/{maxItemsOnMap}");
        }
    }
}