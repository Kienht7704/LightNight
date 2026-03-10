using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Màn hình Loading — Vietnam Heritage style.
/// Không cần gắn thủ công, LobbyUI sẽ tự gọi.
/// </summary>
public class LoadingScreenUI : MonoBehaviour
{
    private static readonly string[] Tips = {
        "Landmark 81 - tòa nhà cao nhất Việt Nam, 461m",
        "Nhà thờ Đức Bà - biểu tượng 135 năm lịch sử Sài Gòn",
        "Chợ Bến Thành - trái tim thương mại TP.HCM",
        "Bitexco Tower - tháp tài chính biểu tượng Sài Gòn",
        "Cầu Phú Mỹ - cầu dây văng lớn nhất TP.HCM",
        "Dinh Độc Lập - chứng nhân lịch sử thống nhất đất nước",
        "Bưu điện Trung tâm - kiến trúc Pháp hơn 130 năm tuổi",
    };

    private static readonly string[] Msgs = {
        "Đang khởi động động cơ tại Dinh Độc Lập...",
        "Nạp dữ liệu Landmark 81...",
        "Đang dọn đường qua Nhà thờ Đức Bà...",
        "Kiểm tra phanh trước Chợ Bến Thành...",
        "Đang đi qua hầm Thủ Thiêm...",
        "Chuẩn bị đường đua ven sông Sài Gòn...",
        "Sẵn sàng drift quanh Bitexco...",
        "Khởi động hệ thống đèn neon thành phố...",
    };

    private Canvas          _canvas;
    private GameObject      _root;
    private RectTransform   _card;
    private Slider          _bar;

    private TextMeshProUGUI _titleTxt;
    private TextMeshProUGUI _pctTxt;
    private TextMeshProUGUI _msgTxt;
    private TextMeshProUGUI _tipTxt;
    private TextMeshProUGUI _statusTxt;
    private TextMeshProUGUI _playerTxt;
    private TextMeshProUGUI _flagTxt;

    private float _prog;
    private float _tipTimer;
    private int   _tipIdx;
    private float _msgTimer;
    private int   _msgIdx;
    private float _flagTimer;

    private RectTransform[] _bgLines;

    // ============================================================
    //  PUBLIC API
    // ============================================================
    public void ShowLoadingForNetwork(bool isHost = false, string player = null)
    {
        Build();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        _playerTxt.text = "Tay đua: " + Resolve(player);
        _statusTxt.text = isHost ? "SERVER ĐANG CHẠY - CHỜ NGƯỜI CHƠI" : "ĐANG KẾT NỐI VỚI HOST";
        StartCoroutine(UITheme.SlideIn(_card, 0.42f, 0.05f, 300f));
    }

    // Hàm public mới để Controller khác có thể update thanh trượt
    public void SetProgress(float v)
    {
        if (_root != null && _root.activeSelf)
        {
            _prog = Mathf.Clamp01(v);
            UpdateBar(_prog);
        }
    }

    public void ShowConnecting(string player = null)
    {
        Build();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        _playerTxt.text = "Tay đua: " + Resolve(player);
        _statusTxt.text = "ĐANG KẾT NỐI VỚI HOST";
        StartCoroutine(UITheme.SlideIn(_card, 0.42f, 0.05f, 300f));
        StartCoroutine(ConnectLoop());
    }

    public void Hide()
    {
        if (_root) _root.SetActive(false);
    }

    // ============================================================
    //  BUILD
    // ============================================================
    private bool _built = false;

    private void Build()
    {
        if (_built) return;
        _built = true;

        _canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (!_canvas) _canvas = UITheme.CreateCanvas("LoadCanvas");

        // Nền
        RectTransform bg = UITheme.CreateFullScreen(_canvas.transform, "LoadRoot",
            UITheme.HeritageBlue);
        _root = bg.gameObject;

        // === Nền Sài Gòn về đêm ===
        UITheme.NightStars(bg, 20);
        UITheme.SaigonSkyline(bg);
        UITheme.CityFog(bg);

        // Speed lines (nhiều và nhanh)
        float[] yp = { 0.08f,0.18f,0.28f,0.40f,0.50f,0.60f,0.72f,0.82f,0.92f };
        _bgLines = new RectTransform[yp.Length];
        for (int i = 0; i < yp.Length; i++)
        {
            var ln = UITheme.Divider(bg, $"SL{i}",
                Random.Range(220f,550f), 1f,
                new Color(1f,0.84f,0f, i%2==0 ? 0.09f : 0.04f));
            ln.anchorMin = new Vector2(0, yp[i]);
            ln.anchorMax = new Vector2(0, yp[i]);
            ln.anchoredPosition = new Vector2(Random.Range(-400f,1400f),0);
            _bgLines[i] = ln;
        }

        // Dragon lines
        SimpleDragon(bg, 0.97f);
        SimpleDragon(bg, 0.03f);

        // ====== CARD ======
        _card = UITheme.CreateCard(bg, "LoadCard", 440f, 480f);
        UITheme.CreateHeaderBand(_card, "\u2605 ĐANG KHỞI ĐỘNG ĐƯỜNG ĐUA \u2605", 38f);

        // Content
        GameObject ct = new GameObject("Content");
        ct.transform.SetParent(_card, false);
        RectTransform ctr = ct.AddComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero; ctr.offsetMax = new Vector2(0,-38);
        UITheme.VLayout(ct, 8f, new RectOffset(36,36,14,12));

        // Flag  (h=48)
        _flagTxt = UITheme.MakeText(ct.transform, "Fl", "[>]", 36f, UITheme.RoyalGold);
        UITheme.PH(_flagTxt.gameObject, 48f);

        // Title  (h=56)
        _titleTxt = UITheme.MakeText(ct.transform, "Ti",
            "LIGHT NIGHT", 48f, UITheme.RoyalGold);
        _titleTxt.fontStyle = FontStyles.Bold;
        _titleTxt.characterSpacing = 4f;
        UITheme.PH(_titleTxt.gameObject, 56f);

        // Subtitle  (h=16)
        var sub = UITheme.MakeText(ct.transform, "Sub",
            "SÀI GÒN RACING", UITheme.TinySize, UITheme.TextGold);
        sub.characterSpacing = 3f;
        UITheme.PH(sub.gameObject, 16f);

        // Player name  (h=20)
        _playerTxt = UITheme.MakeText(ct.transform, "Pl",
            "Tay đua: username", UITheme.SmallSize, UITheme.ElectricCyan);
        UITheme.PH(_playerTxt.gameObject, 20f);

        // Divider  (h=10)
        var dv = UITheme.Divider(ct.transform, "Dv", 0, 1f,
            new Color32(255,215,0,80));
        dv.anchorMin = new Vector2(0,0.5f); dv.anchorMax = new Vector2(1,0.5f);
        UITheme.PH(dv.gameObject, 10f);

        // Msg row  (h=16)
        GameObject mr = new GameObject("MsgRow");
        mr.transform.SetParent(ct.transform, false);
        mr.AddComponent<RectTransform>();
        HorizontalLayoutGroup mhlg = mr.AddComponent<HorizontalLayoutGroup>();
        mhlg.childControlWidth = true; mhlg.childForceExpandWidth = true;
        mhlg.childControlHeight = false;
        UITheme.PH(mr, 16f);

        _msgTxt = UITheme.MakeText(mr.transform, "Mg",
            Msgs[0], UITheme.TinySize, UITheme.TextGold, TextAlignmentOptions.Left);
        _msgTxt.overflowMode = TextOverflowModes.Ellipsis;
        UITheme.PH(_msgTxt.gameObject, 16f);

        _pctTxt = UITheme.MakeText(mr.transform, "Pc",
            "0%", UITheme.TinySize, UITheme.RoyalGold, TextAlignmentOptions.Right);
        _pctTxt.fontStyle = FontStyles.Bold;
        UITheme.PH(_pctTxt.gameObject, 16f);

        // Progress bar  (h=8)
        _bar = UITheme.MakeProgressBar(ct.transform, "Bar", 0, 7f);
        UITheme.PH(_bar.gameObject, 8f);

        // Status  (h=20)
        _statusTxt = UITheme.MakeText(ct.transform, "St",
            "", UITheme.SmallSize, new Color32(0,245,255,180));
        _statusTxt.characterSpacing = 2f;
        UITheme.PH(_statusTxt.gameObject, 20f);

        // Tip box  (h=64)
        GameObject tb = new GameObject("TipBox");
        tb.transform.SetParent(ct.transform, false);
        tb.AddComponent<RectTransform>();
        Image tbI = tb.AddComponent<Image>(); tbI.color = UITheme.TagBg;
        tb.AddComponent<Outline>().effectColor = UITheme.TagBorder;
        UITheme.VLayout(tb, 4f, new RectOffset(16,16,10,10));
        UITheme.PH(tb, 60f);

        var tipLbl = UITheme.MakeText(tb.transform, "TL",
            "[!] BẠN CÓ BIẾT?", UITheme.TinySize, new Color32(255,150,150,150));
        tipLbl.alignment = TextAlignmentOptions.Left;
        UITheme.PH(tipLbl.gameObject, 14f);

        _tipIdx = Random.Range(0, Tips.Length);
        _tipTxt = UITheme.MakeText(tb.transform, "TT",
            Tips[_tipIdx], UITheme.SmallSize,
            new Color32(230,210,180,210));
        _tipTxt.alignment = TextAlignmentOptions.Left;
        _tipTxt.overflowMode = TextOverflowModes.Ellipsis;
        UITheme.PH(_tipTxt.gameObject, 36f);

        // Cancel  (h=36)
        Button can = UITheme.MakeButton(ct.transform, "CanBtn",
            "HỦY", new Color32(0,0,0,0), UITheme.TextGold, 0, 36f);
        can.GetComponent<Outline>().effectColor = new Color32(200,180,140,40);
        can.onClick.AddListener(OnCancel);
        UITheme.PH(can.gameObject, 36f);
    }

    // ============================================================
    //  COROUTINES
    // ============================================================
    // XÓA HÀM LoadAsync ĐỂ TRÁNH TRÙNG VỚI NetworkManager

    private IEnumerator ConnectLoop()
    {
        int dot = 0; float dt = 0;
        int mIdx = Random.Range(0, Msgs.Length); float mt = 0;
        while (_root && _root.activeSelf)
        {
            dt += Time.deltaTime;
            if (dt > 0.5f)
            {
                dt = 0; dot = (dot+1)%4;
                _statusTxt.text = "DANG KET NOI" + new string('.', dot);
            }
            _bar.value = Mathf.PingPong(Time.time * 0.22f, 0.65f);
            _pctTxt.text = "";
            mt += Time.deltaTime;
            if (mt > 1.8f) { mt=0; mIdx=(mIdx+1)%Msgs.Length; _msgTxt.text = Msgs[mIdx]; }
            _tipTimer += Time.deltaTime;
            if (_tipTimer > 2.5f) RotateTip();
            yield return null;
        }
    }

    // ============================================================
    //  UPDATE
    // ============================================================
    void Update()
    {
        if (!_root || !_root.activeSelf) return;

        // Flag swing
        if (_flagTxt)
        {
            _flagTimer += Time.deltaTime;
            _flagTxt.transform.localRotation =
                Quaternion.Euler(0, 0, Mathf.Sin(_flagTimer * 3f) * 9f);
        }

        // Speed lines
        if (_bgLines != null)
        {
            for (int i = 0; i < _bgLines.Length; i++)
            {
                if (!_bgLines[i]) continue;
                Vector2 p = _bgLines[i].anchoredPosition;
                p.x += (280f + i*35f) * Time.deltaTime;
                if (p.x > 2100f)
                {
                    p.x = Random.Range(-700f,-150f);
                    Image im = _bgLines[i].GetComponent<Image>();
                    if (im) im.color = new Color(1f,0.84f,0f, Random.Range(0.03f,0.10f));
                }
                _bgLines[i].anchoredPosition = p;
            }
        }

        // Tự chuyển thông điệp
        _msgTimer += Time.deltaTime;
        if (_msgTimer > 1.8f) { _msgTimer=0; _msgIdx=(_msgIdx+1)%Msgs.Length; if(_msgTxt) _msgTxt.text = Msgs[_msgIdx]; }

        // Tự đổi tip
        _tipTimer += Time.deltaTime;
        if (_tipTimer > 2.5f) RotateTip();
    }

    private void UpdateBar(float v)
    {
        _bar.value = v;
        _pctTxt.text = Mathf.RoundToInt(v * 100f) + "%";
    }

    private void RotateTip()
    {
        _tipTimer = 0;
        _tipIdx = (_tipIdx + 1) % Tips.Length;
        _tipTxt.text = Tips[_tipIdx];
    }

    private void OnCancel()
    {
        StopAllCoroutines();
        Hide();
        var lb = UnityEngine.Object.FindAnyObjectByType<LobbyUI>();
        if (lb) lb.gameObject.SetActive(true);
    }

    private string Resolve(string n)
    {
        if (string.IsNullOrWhiteSpace(n)) n = UITheme.DefaultUsername;
        return n;
    }

    private void SimpleDragon(Transform p, float yA)
    {
        var d = UITheme.Divider(p, "Dg", 0, 2f, UITheme.RoyalGold);
        d.anchorMin = new Vector2(0.1f, yA);
        d.anchorMax = new Vector2(0.9f, yA);
        d.sizeDelta = new Vector2(0, 2f);
    }
}
