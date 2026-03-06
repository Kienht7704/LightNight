using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Base")]
    public GameObject[] itemTemplates;

    [Header("Spawn location")]
    public Transform[] spawnAnchors;

    [Header("Cooldown and max item spawn")]
    public float spawnInterval = 60f;
    public int maxItemsOnMap = 5;

    private float _timer;

    // Đổi tên thành SpawnNode để thể hiện nó có thể là bất cứ vị trí nào
    private class SpawnNode
    {
        public Transform anchor;
        public GameObject currentItem;
    }

    private List<SpawnNode> _spawnNodes = new List<SpawnNode>();

    void Start()
    {
        foreach (Transform t in spawnAnchors)
        {
            if (t != null)
            {
                _spawnNodes.Add(new SpawnNode { anchor = t, currentItem = null });
            }
        }
        _timer = spawnInterval;
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            TrySpawnItem();
            _timer = spawnInterval;
        }
    }

    void TrySpawnItem()
    {
        if (itemTemplates.Length == 0 || spawnAnchors.Length == 0) return;

        int activeItemCount = 0;
        List<SpawnNode> freeAnchors = new List<SpawnNode>();

        foreach (var node in _spawnNodes)
        {
            if (node.currentItem != null) activeItemCount++;
            else freeAnchors.Add(node);
        }

        if (activeItemCount < maxItemsOnMap && freeAnchors.Count > 0)
        {
            SpawnNode chosenAnchor = freeAnchors[Random.Range(0, freeAnchors.Count)];
            GameObject chosenTemplate = itemTemplates[Random.Range(0, itemTemplates.Length)];

            GameObject newItem = Instantiate(chosenTemplate, chosenAnchor.anchor.position, chosenAnchor.anchor.rotation);
            newItem.SetActive(true);

            // Khóa mốc này lại
            chosenAnchor.currentItem = newItem;

            Debug.Log($"Đã spawn {newItem.name} tại mốc {chosenAnchor.anchor.name}");
        }
    }
    // Hàm này giúp bạn NHÌN THẤY các mốc vô hình (Empty Object) trong cửa sổ Scene
    void OnDrawGizmos()
    {
        if (spawnAnchors == null) return;

        // Cài đặt màu xanh lá cây hơi trong suốt
        Gizmos.color = new Color(0, 1, 0, 0.5f);

        foreach (Transform t in spawnAnchors)
        {
            if (t != null)
            {
                // Vẽ một hình cầu ảo tại vị trí mốc để dễ căn chỉnh
                Gizmos.DrawWireSphere(t.position, 1f);
            }
        }
    }
}