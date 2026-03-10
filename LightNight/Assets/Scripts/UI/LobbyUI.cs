using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Màn hình Lobby Pro: Tạo phòng (Room Code), Danh sách người chơi, Sẵn sàng.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("Tên Scene Đua để Load")]
    public string raceSceneName = "Prototype 1";

    [Header("Camera Rotate (tùy chọn)")]
    public Transform backgroundCar;
    public float orbitSpeed    = 4f;
    public float orbitDistance = 8f;
    public float orbitHeight   = 3f;

    // Các cụm UI
    private Canvas        _canvas;
    private GameObject    _lobbyRoot;
    private GameObject    _joinRoot;
    private GameObject    _roomRoot;

    // Card chính
    private RectTransform _lobbyCard;
    private RectTransform _joinCard;
    private RectTransform _roomCard;

    // Nhập liệu
    private TMP_InputField  _usernameInput;
    private TMP_InputField  _codeInput;

    // Text & Trạng thái
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _statusText;
    private TextMeshProUGUI _joinStatus;
    
    // UI Phòng chờ
    private TextMeshProUGUI _roomCodeText;
    private GameObject      _playerListContainer;
    private Button          _readyBtn;
    private Button          _startBtn;

    // Hệ thống
    private LoadingScreenUI _loader;
    private string          _resolvedName;
    private float           _orbitAngle;
    private Camera          _cam;

    // Animation
    private float _glitchTimer;
    private float _nextGlitch = 4f;
    private bool  _glitching;
    private bool  _isCountingDown = false;

    // ============================================================
    //  LIFECYCLE
    // ============================================================
    private void Start()
    {
        _cam    = Camera.main;
        _canvas = UITheme.CreateCanvas("LobbyCanvas");

        // Nhận kết nối thành công từ LobbyManager để mở giao diện phòng chờ
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnError         += ShowError;
            LobbyManager.Instance.OnHostStarted   += OnEnterRoomHost;
            LobbyManager.Instance.OnClientStarted += OnEnterRoomClient;
        }

        BuildLobby();
        BuildJoin();
        BuildRoom();
        
        ShowLobby();
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnError         -= ShowError;
            LobbyManager.Instance.OnHostStarted   -= OnEnterRoomHost;
            LobbyManager.Instance.OnClientStarted -= OnEnterRoomClient;
        }
    }

    private void Update()
    {
        GlitchTick();
        OrbitTick();

        // Kéo UI Lobby Network Sync và Host Start Status
        if (_roomRoot != null && _roomRoot.activeSelf && !_isCountingDown)
        {
            if (LobbyNetworkSync.Instance != null && Unity.Netcode.NetworkManager.Singleton != null)
            {
                // Xử lý nút Ready (của bản thân)
                if (_readyBtn != null)
                {
                    ulong localId = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
                    bool amIReady = false;
                    foreach (var p in LobbyNetworkSync.Instance.LobbyPlayers)
                    {
                        if (p.ClientId == localId) { amIReady = p.IsReady; break; }
                    }
                    _readyBtn.GetComponentInChildren<TextMeshProUGUI>().text = amIReady ? "HỦY SẴN SÀNG" : "SẴN SÀNG";
                }

                // Xử lý nút Start (của Host)
                if (_startBtn != null)
                {
                    // Chỉ hiện nút Bắt đầu khi là Host (Server) và Mọi người đã sẵn sàng
                    bool isHost = Unity.Netcode.NetworkManager.Singleton.IsServer;
                    _startBtn.gameObject.SetActive(isHost);
                    
                    bool allReady = LobbyNetworkSync.Instance.AreAllReady();
                    _startBtn.interactable = allReady;
                    _startBtn.GetComponentInChildren<TextMeshProUGUI>().text = 
                        allReady ? "BẮT ĐẦU GAME" : "CHỜ ĐỒNG ĐỘI...";
                }
            }
        }
    }

    // ============================================================
    //  1. GIAO DIỆN LOBBY CHÍNH
    // ============================================================
    private void BuildLobby()
    {
        RectTransform bg = UITheme.CreateFullScreen(_canvas.transform, "LobbyRoot", UITheme.HeritageBlue);
        _lobbyRoot = bg.gameObject;

        // === Nền Sài Gòn về đêm ===
        UITheme.NightStars(bg, 35);
        UITheme.SaigonSkyline(bg);
        UITheme.CityFog(bg);
        UITheme.NeonLine(bg, 0.15f, UITheme.ElectricCyan, 0.08f);
        UITheme.NeonLine(bg, 0.12f, UITheme.VietnamRed, 0.06f);
        PlaceLotus(bg, new Vector2(0.05f, 0.90f), 75f, UITheme.VietnamRed, 0.14f);
        PlaceLotus(bg, new Vector2(0.91f, 0.18f), 85f, UITheme.VietnamRed, 0.11f);
        DragonLine(bg, 0.97f); DragonLine(bg, 0.03f);

        _lobbyCard = UITheme.CreateCard(bg, "LobbyCard", 500f, 560f);
        UITheme.CreateHeaderBand(_lobbyCard, "* CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM *", 38f);

        GameObject ct = new GameObject("C"); ct.transform.SetParent(_lobbyCard, false);
        RectTransform ctr = ct.AddComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero; ctr.offsetMax = new Vector2(0, -38);
        UITheme.VLayout(ct, 12f, new RectOffset(32, 32, 20, 16));

        var pre = UITheme.MakeText(ct.transform, "P", "- SÀI GÒN RACING -", UITheme.TinySize, UITheme.TextGold);
        pre.characterSpacing = 4f; UITheme.PH(pre.gameObject, 16f);

        _titleText = UITheme.MakeText(ct.transform, "T", "LIGHT NIGHT", UITheme.TitleSize, UITheme.RoyalGold);
        _titleText.fontStyle = FontStyles.Bold; _titleText.lineSpacing = 0f; 
        UITheme.PH(_titleText.gameObject, 140f);

        var sub = UITheme.MakeText(ct.transform, "S", "Đường đua qua các biểu tượng TP.HCM", UITheme.SmallSize, UITheme.TextGold);
        UITheme.PH(sub.gameObject, 18f);

        var dv = UITheme.Divider(ct.transform, "D", 0, 1f, new Color32(255, 215, 0, 80));
        UITheme.PH(dv.gameObject, 10f);

        var nl = UITheme.MakeText(ct.transform, "L", "TÊN NGƯỜI CHƠI", UITheme.TinySize, UITheme.TextGold, TextAlignmentOptions.Left);
        nl.characterSpacing = 2f; UITheme.PH(nl.gameObject, 15f);

        _usernameInput = UITheme.MakeInputField(ct.transform, "I", "Tay đua...", 0, 50f, UITheme.RoyalGold);
        _usernameInput.text = UITheme.GetSavedUsername();
        UITheme.PH(_usernameInput.gameObject, 50f);

        _statusText = UITheme.MakeText(ct.transform, "St", "", UITheme.TinySize, UITheme.TextGray);
        UITheme.PH(_statusText.gameObject, 16f);

        Button hBtn = UITheme.MakeButton(ct.transform, "HBtn", "TẠO PHÒNG (HOST)", UITheme.VietnamRed, UITheme.RoyalGold, 0, 58f);
        hBtn.onClick.AddListener(OnHostClick); UITheme.PH(hBtn.gameObject, 58f);

        Button jBtn = UITheme.MakeButton(ct.transform, "JBtn", "NHẬP MÃ PHÒNG", new Color32(0, 245, 255, 28), UITheme.ElectricCyan, 0, 52f);
        jBtn.GetComponent<Outline>().effectColor = new Color32(0, 245, 255, 80);
        jBtn.onClick.AddListener(OnJoinClick); UITheme.PH(jBtn.gameObject, 52f);
    }

    // ============================================================
    //  2. GIAO DIỆN NHẬP CODE PRO
    // ============================================================
    private void BuildJoin()
    {
        RectTransform bg = UITheme.CreateFullScreen(_canvas.transform, "JoinRoot", UITheme.HeritageBlue);
        _joinRoot = bg.gameObject;

        // === Nền Sài Gòn về đêm ===
        UITheme.NightStars(bg, 25);
        UITheme.SaigonSkyline(bg);
        UITheme.CityFog(bg);
        UITheme.NeonLine(bg, 0.14f, UITheme.ElectricCyan, 0.10f);
        PlaceLotus(bg, new Vector2(0.85f, 0.85f), 55f, UITheme.RoyalGold, 0.09f);
        DragonLine(bg, 0.97f); DragonLine(bg, 0.03f);

        _joinCard = UITheme.CreateCard(bg, "JoinCard", 480f, 420f);
        UITheme.CreateHeaderBand(_joinCard, "* TÌM PHÒNG ĐUA *", 38f);

        GameObject ct = new GameObject("C"); ct.transform.SetParent(_joinCard, false);
        RectTransform ctr = ct.AddComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero; ctr.offsetMax = new Vector2(0, -38);
        UITheme.VLayout(ct, 16f, new RectOffset(32, 32, 24, 20));

        var t = UITheme.MakeText(ct.transform, "T", "NHẬP MÃ PHÒNG", UITheme.HeadingSize, UITheme.ElectricCyan);
        t.fontStyle = FontStyles.Bold; UITheme.PH(t.gameObject, 60f);

        var lbl = UITheme.MakeText(ct.transform, "L", "MÃ PHÒNG (VÍ DỤ: X4K2)", UITheme.TinySize, new Color32(0, 245, 255, 150), TextAlignmentOptions.Left);
        UITheme.PH(lbl.gameObject, 14f);

        _codeInput = UITheme.MakeInputField(ct.transform, "C", "Mã 6 ký tự...", 0, 50f, UITheme.ElectricCyan);
        UITheme.PH(_codeInput.gameObject, 55f);

        _joinStatus = UITheme.MakeText(ct.transform, "JS", "", UITheme.TinySize, UITheme.VietnamRed);
        UITheme.PH(_joinStatus.gameObject, 16f);

        Button con = UITheme.MakeButton(ct.transform, "CB", "XÁC NHẬN", new Color32(0, 245, 255, 28), UITheme.ElectricCyan, 0, 50f);
        con.GetComponent<Outline>().effectColor = new Color32(0, 245, 255, 80);
        con.onClick.AddListener(OnConnectClick); UITheme.PH(con.gameObject, 50f);

        Button bk = UITheme.MakeButton(ct.transform, "BB", "QUAY LẠI", new Color32(0, 0, 0, 0), UITheme.TextGold, 0, 40f);
        bk.GetComponent<Outline>().effectColor = new Color32(200, 180, 140, 40);
        bk.onClick.AddListener(ShowLobby); UITheme.PH(bk.gameObject, 40f);
    }

    // ============================================================
    //  3. GIAO DIỆN ROOM "PLAYER LIST" LOBBY
    // ============================================================
    private void BuildRoom()
    {
        RectTransform bg = UITheme.CreateFullScreen(_canvas.transform, "RoomRoot", UITheme.HeritageBlue);
        _roomRoot = bg.gameObject;

        // === Nền Sài Gòn về đêm ===
        UITheme.NightStars(bg, 20);
        UITheme.SaigonSkyline(bg);
        UITheme.CityFog(bg);
        UITheme.NeonLine(bg, 0.16f, UITheme.RoyalGold, 0.08f);
        PlaceLotus(bg, new Vector2(0.1f, 0.8f), 80f, UITheme.LotusPink, 0.1f);
        DragonLine(bg, 0.97f); DragonLine(bg, 0.03f);

        _roomCard = UITheme.CreateCard(bg, "RoomCard", 520f, 620f);
        UITheme.CreateHeaderBand(_roomCard, "* PHÒNG CHỜ ĐUA *", 38f);

        GameObject ct = new GameObject("C"); ct.transform.SetParent(_roomCard, false);
        RectTransform ctr = ct.AddComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero; ctr.offsetMax = new Vector2(0, -38);
        UITheme.VLayout(ct, 12f, new RectOffset(32, 32, 20, 16));

        _roomCodeText = UITheme.MakeText(ct.transform, "RC", "MÃ PHÒNG: ĐANG TẢI...", UITheme.SubtitleSize, UITheme.ElectricCyan);
        _roomCodeText.fontStyle = FontStyles.Bold; _roomCodeText.characterSpacing = 3f;
        UITheme.PH(_roomCodeText.gameObject, 30f);

        var dv = UITheme.Divider(ct.transform, "D", 0, 1f, new Color32(255, 215, 0, 80));
        UITheme.PH(dv.gameObject, 10f);

        var listTitle = UITheme.MakeText(ct.transform, "LT", "DANH SÁCH NGƯỜI CHƠI", UITheme.SmallSize, UITheme.TextGold, TextAlignmentOptions.Left);
        UITheme.PH(listTitle.gameObject, 20f);

        // Danh sách
        _playerListContainer = new GameObject("List"); _playerListContainer.transform.SetParent(ct.transform, false);
        UITheme.VLayout(_playerListContainer, 8f, new RectOffset(10, 10, 10, 10));
        // Cho vùng này 170 pixel
        UITheme.PH(_playerListContainer, 170f);

        // Khoảng trống đẩy các nút xuống dưới
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(ct.transform, false);
        UITheme.PH(spacer, 30f);

        GameObject btnRow = new GameObject("BtnRow");
        btnRow.transform.SetParent(ct.transform, false);
        HorizontalLayoutGroup hl = btnRow.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 16f;
        hl.childControlWidth = true; hl.childForceExpandWidth = true;
        hl.childControlHeight = false; hl.childForceExpandHeight = false;
        UITheme.PH(btnRow, 50f);

        _readyBtn = UITheme.MakeButton(btnRow.transform, "ReadyBtn", "SẴN SÀNG", new Color32(0, 245, 255, 28), UITheme.ElectricCyan, 0, 50f);
        _readyBtn.GetComponent<Outline>().effectColor = new Color32(0, 245, 255, 80);
        _readyBtn.onClick.AddListener(() => {
            if (LobbyNetworkSync.Instance != null)
                LobbyNetworkSync.Instance.ToggleReadyRpc();
        });
        UITheme.PH(_readyBtn.gameObject, 50f);

        _startBtn = UITheme.MakeButton(btnRow.transform, "StartBtn", "BẮT ĐẦU GAME", UITheme.VietnamRed, UITheme.RoyalGold, 0, 50f);
        _startBtn.onClick.AddListener(() => {
            if (LobbyNetworkSync.Instance != null && Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                LobbyNetworkSync.Instance.StartCountdown();
            }
        });
        UITheme.PH(_startBtn.gameObject, 50f);

        // Nút RỜI PHÒNG
        Button bk = UITheme.MakeButton(ct.transform, "Bk", "RỜI PHÒNG", new Color32(0, 0, 0, 0), UITheme.TextGray, 0, 35f);
        bk.GetComponent<Outline>().effectColor = new Color32(100, 100, 100, 40);
        bk.onClick.AddListener(() => { LobbyManager.Instance.Disconnect(); ShowLobby(); });
        UITheme.PH(bk.gameObject, 35f);
    }

    private void RebuildPlayerList()
    {
        if (_playerListContainer == null) return;
        // Chờ đồng bộ dữ liệu Lobby
        if (LobbyNetworkSync.Instance == null) return;

        // Xóa con cũ
        foreach (Transform child in _playerListContainer.transform) 
            Destroy(child.gameObject);

        var pList = LobbyNetworkSync.Instance.LobbyPlayers;
        foreach (var p in pList)
        {
            GameObject row = new GameObject("Row"); row.transform.SetParent(_playerListContainer.transform, false);
            UITheme.PH(row, 36f);
            Image bgRow = row.AddComponent<Image>();
            bgRow.color = new Color32(20, 30, 50, 200);

            // Container Text ngang
            var lg = row.AddComponent<HorizontalLayoutGroup>();
            lg.padding = new RectOffset(15, 15, 0, 0); lg.childAlignment = TextAnchor.MiddleLeft;

            // Name
            var nameT = UITheme.MakeText(row.transform, "N", p.PlayerName.ToString(), UITheme.BodySize, UITheme.TextWhite, TextAlignmentOptions.Left);
            nameT.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            // Status (Ready)
            Color statC = p.IsReady ? UITheme.ElectricCyan : UITheme.TextGray;
            var statT = UITheme.MakeText(row.transform, "S", p.IsReady ? "< READY >" : "WAITING", UITheme.SmallSize, statC, TextAlignmentOptions.Right);
            statT.fontStyle = FontStyles.Bold;
        }
    }

    // ============================================================
    //  ĐIỀU KHIỂN LOGIC
    // ============================================================
    private void OnEnterRoomHost(string code) { 
        _roomCodeText.text = $"CODE: {code}"; 
        ShowRoomSetup();
        SetupNetworkLoadingEvents();
        LobbyNetworkSync.AutoSpawn();
    }
    
    private void OnEnterRoomClient() { 
        _roomCodeText.text = $"CODE: {LobbyManager.Instance.CurrentRoomCode}"; 
        ShowRoomSetup(); 
        SetupNetworkLoadingEvents();
    }

    private void SetupNetworkLoadingEvents()
    {
        // Khi Host load scene, Client cũng tự load theo nhờ NetworkSceneManager. 
        // Chúng ta cần hiển thị LoadingScreenUI khi quá trình này bắt đầu
        if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.SceneManager != null)
        {
            Unity.Netcode.NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnNetworkSceneEvent; // tránh đăng ký 2 lần
            Unity.Netcode.NetworkManager.Singleton.SceneManager.OnSceneEvent += OnNetworkSceneEvent;
        }
    }

    private void OnNetworkSceneEvent(Unity.Netcode.SceneEvent sceneEvent)
    {
        // Khi scene bắt đầu load
        if (sceneEvent.SceneEventType == Unity.Netcode.SceneEventType.Load)
        {
            if (_loader == null) _loader = new GameObject("LoaderUI").AddComponent<LoadingScreenUI>();
            bool isHost = Unity.Netcode.NetworkManager.Singleton.IsServer;
            
            if (!_isCountingDown)
            {
                _loader.ShowLoadingForNetwork(isHost, LobbyManager.Instance.LocalPlayerName);
            }
            
            if (_roomRoot) _roomRoot.SetActive(false);
            if (_lobbyRoot) _lobbyRoot.SetActive(false);
            if (_joinRoot) _joinRoot.SetActive(false);
        }
        
        // Cập nhật tiến độ
        if (sceneEvent.SceneEventType == Unity.Netcode.SceneEventType.LoadEventCompleted || sceneEvent.SceneEventType == Unity.Netcode.SceneEventType.LoadComplete)
        {
            if (_loader != null) _loader.SetProgress(1f);
        }
    }

    private void OnHostClick()
    {
        _resolvedName = ResolveUsername();
        if (LobbyManager.Instance == null) { ShowError("Thiếu LobbyManager!"); return; }

        SetStatus("Dang len he thong...", UITheme.ElectricCyan);
        StartCoroutine(UITheme.SlideOut(_lobbyCard, 0.28f, 360f));
        StartCoroutine(After(0.32f, () => LobbyManager.Instance.HostGame(_resolvedName)));
    }

    private void OnJoinClick()
    {
        _resolvedName = ResolveUsername();
        StartCoroutine(UITheme.SlideOut(_lobbyCard, 0.28f, 360f));
        StartCoroutine(After(0.32f, ShowJoin));
    }

    private void OnConnectClick()
    {
        string c = (_codeInput?.text ?? "").Trim();
        if (c.Length != 6) { SetJoinStatus("CODE khong hop le! (Phai dung 6 ky tu)", UITheme.VietnamRed); return; }
        
        if (LobbyManager.Instance == null) {
            SetJoinStatus("Thiếu LobbyManager!", UITheme.VietnamRed);
            return;
        }

        SetJoinStatus("Dang ket noi qua Relay...", UITheme.ElectricCyan);
        StartCoroutine(UITheme.SlideOut(_joinCard, 0.28f, 360f));
        StartCoroutine(After(0.32f, () => LobbyManager.Instance.JoinGame(c, _resolvedName ?? UITheme.DefaultUsername)));
    }

    // ============================================================
    //  QUẢN LÝ HIỂN THỊ CÁC ROOT MÀN HÌNH
    // ============================================================
    private void ShowLobby()
    {
        _joinRoot.SetActive(false); _roomRoot.SetActive(false);
        _lobbyRoot.SetActive(true); _lobbyCard.gameObject.SetActive(true);
        _isCountingDown = false;
        SetStatus("", UITheme.TextGray);
        StartCoroutine(UITheme.SlideIn(_lobbyCard, 0.38f, 0.05f, 280f));

        if (LobbyNetworkSync.Instance != null)
        {
            LobbyNetworkSync.Instance.OnLobbyUpdated -= RebuildPlayerList;
            LobbyNetworkSync.Instance.OnCountdownStarted -= HandleCountdown;
        }
    }

    private void ShowJoin()
    {
        _lobbyRoot.SetActive(false); _roomRoot.SetActive(false);
        _joinRoot.SetActive(true); _joinCard.gameObject.SetActive(true);
        SetJoinStatus("", UITheme.ElectricCyan);
        StartCoroutine(UITheme.SlideIn(_joinCard, 0.38f, 0.05f, 280f));
    }

    public void ShowRoomSetup() 
    {
        // Hiện lobby room (Player List & Ready state)
        _lobbyRoot.SetActive(false); _joinRoot.SetActive(false);
        _roomRoot.SetActive(true); _roomCard.gameObject.SetActive(true);
        _isCountingDown = false;
        StartCoroutine(UITheme.SlideIn(_roomCard, 0.38f, 0.05f, 280f));

        if (LobbyNetworkSync.Instance != null) {
            LobbyNetworkSync.Instance.OnLobbyUpdated -= RebuildPlayerList;
            LobbyNetworkSync.Instance.OnLobbyUpdated += RebuildPlayerList;
            LobbyNetworkSync.Instance.OnCountdownStarted -= HandleCountdown;
            LobbyNetworkSync.Instance.OnCountdownStarted += HandleCountdown;
            RebuildPlayerList();
        } else {
            StartCoroutine(WaitForSyncAndBuildList());
        }
    }

    private IEnumerator WaitForSyncAndBuildList()
    {
        yield return new WaitUntil(() => LobbyNetworkSync.Instance != null);
        LobbyNetworkSync.Instance.OnLobbyUpdated -= RebuildPlayerList;
        LobbyNetworkSync.Instance.OnLobbyUpdated += RebuildPlayerList;
        LobbyNetworkSync.Instance.OnCountdownStarted -= HandleCountdown;
        LobbyNetworkSync.Instance.OnCountdownStarted += HandleCountdown;
        RebuildPlayerList();
    }

    private void HandleCountdown()
    {
        if (!_roomRoot.activeSelf) return;
        _isCountingDown = true;
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        if (_startBtn) _startBtn.interactable = false;
        if (_readyBtn) _readyBtn.interactable = false;

        if (_loader == null) _loader = new GameObject("LoaderUI").AddComponent<LoadingScreenUI>();
        bool isHost = Unity.Netcode.NetworkManager.Singleton.IsServer;
        
        _loader.ShowLoadingForNetwork(isHost, LobbyManager.Instance.LocalPlayerName);
        if (_roomRoot) _roomRoot.SetActive(false);
        if (_lobbyRoot) _lobbyRoot.SetActive(false);
        if (_joinRoot) _joinRoot.SetActive(false);

        float elapsed = 0f;
        while (elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            _loader.SetProgress(elapsed / 5f);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        if (isHost)
        {
            Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene(raceSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void ShowError(string msg)
    {
        if (_roomRoot.activeSelf) {
            LobbyManager.Instance.Disconnect();
            ShowLobby();
        }
        else if (_lobbyRoot.activeSelf) {
            _lobbyCard.gameObject.SetActive(true); StartCoroutine(UITheme.SlideIn(_lobbyCard)); SetStatus("LOI: " + msg, UITheme.VietnamRed);
        } else {
            _joinCard.gameObject.SetActive(true); StartCoroutine(UITheme.SlideIn(_joinCard)); SetJoinStatus("LOI: " + msg, UITheme.VietnamRed);
        }
    }



    // ============================================================
    //  UTILITIES
    // ============================================================
    private void GlitchTick()
    {
        if (!_titleText) return;
        _glitchTimer += Time.deltaTime;
        if (_glitching) {
            if (_glitchTimer > 0.13f) { _glitching = false; _titleText.color = UITheme.RoyalGold; _glitchTimer = 0f; _nextGlitch  = Random.Range(3.5f, 7f); }
        } else if (_glitchTimer > _nextGlitch) {
            _glitching = true; _glitchTimer = 0f; _titleText.color = Color.HSVToRGB(Random.value, 0.85f, 1f);
            StartCoroutine(UITheme.ScalePunch(_titleText.transform));
        }
    }

    private void OrbitTick()
    {
        if (!_cam || !backgroundCar) return;
        _orbitAngle += orbitSpeed * Time.deltaTime; float rad = _orbitAngle * Mathf.Deg2Rad; Vector3 cp = backgroundCar.position;
        _cam.transform.position = Vector3.Lerp(_cam.transform.position, new Vector3(cp.x + Mathf.Cos(rad) * orbitDistance, cp.y + orbitHeight, cp.z + Mathf.Sin(rad) * orbitDistance), Time.deltaTime * 3f);
        _cam.transform.LookAt(cp + Vector3.up * 1.2f);
    }

    private string ResolveUsername() { string n = (_usernameInput?.text ?? "").Trim(); if (string.IsNullOrEmpty(n)) n = UITheme.DefaultUsername; UITheme.SaveUsername(n); return n; }
    private void SetStatus(string msg, Color col) { if (_statusText) { _statusText.text = msg; _statusText.color = col; } }
    private void SetJoinStatus(string msg, Color col) { if (_joinStatus) { _joinStatus.text = msg; _joinStatus.color = col; } }
    private void PlaceLotus(Transform p, Vector2 a, float s, Color c, float al) { var l = UITheme.Lotus(p, s, c, al); l.anchorMin = l.anchorMax = a; l.anchoredPosition = Vector2.zero; }
    private void DragonLine(Transform p, float yA) { var ln = UITheme.Divider(p, "Dragon", 0, 2f); ln.anchorMin = new Vector2(0.1f, yA); ln.anchorMax = new Vector2(0.9f, yA); ln.sizeDelta = new Vector2(0, 2f); }
    private IEnumerator After(float t, System.Action action) { yield return new WaitForSeconds(t); action?.Invoke(); }
}
