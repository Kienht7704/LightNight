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
        "Landmark 81 - toa nha cao nhat Viet Nam, 461m",
        "Nha tho Duc Ba - bieu tuong 135 nam lich su Sai Gon",
        "Cho Ben Thanh - trai tim thuong mai TP.HCM",
        "Bitexco Tower - thap tai chinh bieu tuong Sai Gon",
        "Cau Phu My - cau day vang lon nhat TP.HCM",
        "Dinh Doc Lap - chung nhan lich su thong nhat dat nuoc",
        "Buu dien Trung tam - kien truc Phap hon 130 nam tuoi",
    };

    private static readonly string[] Msgs = {
        "Dang khoi dong dong co tai Dinh Doc Lap...",
        "Nap du lieu Landmark 81...",
        "Dang don duong qua Nha tho Duc Ba...",
        "Kiem tra phanh truoc Cho Ben Thanh...",
        "Dang di qua ham Thu Thiem...",
        "Chuan bi duong dua ven song Sai Gon...",
        "San sang drift quanh Bitexco...",
        "Khoi dong he thong den neon thanh pho...",
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
    public void LoadScene(string scene, bool isHost = false, string player = null)
    {
        Build();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        _playerTxt.text = "Tay dua: " + Resolve(player);
        _statusTxt.text = isHost ? "SERVER DANG CHAY - CHO NGUOI CHOI" : "DANG KET NOI VAN HOST";
        StartCoroutine(UITheme.SlideIn(_card, 0.42f, 0.05f, 300f));
        StartCoroutine(LoadAsync(scene));
    }

    public void ShowConnecting(string player = null)
    {
        Build();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
        _playerTxt.text = "Tay dua: " + Resolve(player);
        _statusTxt.text = "DANG KET NOI VOI HOST";
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

        _canvas = FindAnyObjectByType<Canvas>();
        if (!_canvas) _canvas = UITheme.CreateCanvas("LoadCanvas");

        // Nền
        RectTransform bg = UITheme.CreateFullScreen(_canvas.transform, "LoadRoot",
            UITheme.HeritageBlue);
        _root = bg.gameObject;

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
        _card = UITheme.CreateCard(bg, "LoadCard", 500f, 530f);
        UITheme.CreateHeaderBand(_card, "* DANG KHOI DONG DUONG DUA *", 38f);

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
            "SAI GON RACING", UITheme.TinySize, UITheme.TextGold);
        sub.characterSpacing = 3f;
        UITheme.PH(sub.gameObject, 16f);

        // Player name  (h=20)
        _playerTxt = UITheme.MakeText(ct.transform, "Pl",
            "Tay dua: username", UITheme.SmallSize, UITheme.ElectricCyan);
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
            "[!] BAN CO BIET?", UITheme.TinySize, new Color32(255,150,150,150));
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
            "HUY", new Color32(0,0,0,0), UITheme.TextGold, 0, 36f);
        can.GetComponent<Outline>().effectColor = new Color32(200,180,140,40);
        can.onClick.AddListener(OnCancel);
        UITheme.PH(can.gameObject, 36f);
    }

    // ============================================================
    //  COROUTINES
    // ============================================================
    private IEnumerator LoadAsync(string scene)
    {
        _prog = 0f;
        float elapsed = 0f;
        float minTime = 2.5f;
        int mIdx = 0; float mTimer = 0;

        AsyncOperation op = SceneManager.LoadSceneAsync(scene);
        if (op == null)
        {
            _tipTxt.text = "LOI: Khong tim thay scene '" + scene + "'!";
            _statusTxt.text = "[!] LOI LOAD";
            yield break;
        }
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            elapsed += Time.deltaTime;
            float real = Mathf.Clamp01(op.progress / 0.9f);
            float target = Mathf.Max(real, elapsed / minTime);
            _prog = Mathf.MoveTowards(_prog, Mathf.Clamp01(target), Time.deltaTime * 0.55f);

            UpdateBar(_prog);

            mTimer += Time.deltaTime;
            if (mTimer > 1.8f) { mTimer=0; mIdx=(mIdx+1)%Msgs.Length; _msgTxt.text = Msgs[mIdx]; }

            if (real >= 1f && elapsed >= minTime && _prog >= 0.98f)
            {
                UpdateBar(1f);
                yield return new WaitForSeconds(0.4f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }

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
    }

    private void UpdateBar(float v)
    {
        _bar.value = v;
        _pctTxt.text = Mathf.RoundToInt(v * 100f) + "%";
        _tipTimer += Time.deltaTime;
        if (_tipTimer > 2.5f) RotateTip();
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
        var lb = FindAnyObjectByType<LobbyUI>();
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
