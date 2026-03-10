using UnityEngine;

using TMPro;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance;

    [Header("Giao diện UI")]
    public TextMeshProUGUI timerText; // Kéo TxtTimer vào đây

    private float currentTime = 0f;
    private bool isTimerRunning = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Vừa vào game thì hiển thị 00:00:00
        UpdateTimerDisplay();
    }

    void Update()
    {
        // Chỉ cộng thời gian khi biến này bằng true
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime; // Thời gian thực trôi qua
            UpdateTimerDisplay();
        }
    }

    // --- CÁC HÀM CÔNG CỤ ĐỂ GỌI TỪ NƠI KHÁC ---

    // 1. Lệnh bắt đầu đếm
    public void StartTimer()
    {
        isTimerRunning = true;
    }

    // 2. Lệnh dừng đếm
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // 3. Lệnh reset về 0
    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimerDisplay();
    }

    // Hàm nội bộ để định dạng chữ số cho đẹp (Phút : Giây : MiliGiây)
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        int milliseconds = Mathf.FloorToInt((currentTime * 100) % 100); // Lấy 2 số thập phân

        // Hiển thị định dạng MM:SS:ms (VD: 01:25:40)
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}
