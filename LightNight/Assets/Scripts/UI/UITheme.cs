using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Bảng màu "Vietnam Heritage" + Helpers tạo UI bằng code.
/// Fix: childControlHeight=false, richText=false, overflow=Truncate.
/// </summary>
public static class UITheme
{
    // ===== BẢNG MÀU =====
    public static readonly Color VietnamRed    = new Color32(200, 0, 10, 255);
    public static readonly Color RoyalGold     = new Color32(255, 215, 0, 255);
    public static readonly Color ElectricCyan  = new Color32(0, 245, 255, 255);
    public static readonly Color HeritageBlue  = new Color32(5, 10, 24, 255);
    public static readonly Color CardBg        = new Color32(8, 15, 35, 245);
    public static readonly Color LotusPink     = new Color32(229, 62, 122, 255);
    public static readonly Color TextWhite     = new Color32(232, 224, 208, 255);
    public static readonly Color TextGold      = new Color32(229, 190, 100, 200);
    public static readonly Color TextGray      = new Color32(150, 130, 100, 130);
    public static readonly Color GoldBorder    = new Color32(255, 215, 0, 64);
    public static readonly Color InputBg       = new Color32(0, 20, 40, 200);
    public static readonly Color TagBg         = new Color32(200, 0, 10, 38);
    public static readonly Color TagBorder     = new Color32(200, 0, 10, 76);

    // ===== FONT SIZES =====
    public const float TitleSize    = 68f;
    public const float HeadingSize  = 38f;
    public const float SubtitleSize = 22f;
    public const float ButtonSize   = 20f;
    public const float BodySize     = 16f;
    public const float SmallSize    = 13f;
    public const float TinySize     = 11f;

    // ===== USERNAME =====
    public const string DefaultUsername = "username";
    private const string UsernameKey    = "LN_PlayerName";

    public static string GetSavedUsername()
    {
        string s = PlayerPrefs.GetString(UsernameKey, "");
        return string.IsNullOrWhiteSpace(s) ? DefaultUsername : s;
    }

    public static void SaveUsername(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = DefaultUsername;
        PlayerPrefs.SetString(UsernameKey, name);
        PlayerPrefs.Save();
    }

    // ============================================================
    //  CANVAS
    // ============================================================
    public static Canvas CreateCanvas(string name = "GameCanvas")
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.65f; // Cân bằng giữa chiều rộng & cao, tối ưu cho Laptop 16:9/16:10
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    // ============================================================
    //  FULL SCREEN PANEL
    // ============================================================
    public static RectTransform CreateFullScreen(Transform parent, string name, Color? bg = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = bg ?? HeritageBlue;
        return r;
    }

    // ============================================================
    //  CARD (trung tâm, viền vàng)
    // ============================================================
    public static RectTransform CreateCard(Transform parent, string name,
        float w = 480f, float h = 600f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(w, h);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = CardBg;

        // Viền vàng trên
        Bar(go.transform, "TopLine",
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1f),
            new Vector2(0,2), Vector2.zero, RoyalGold);

        // 4 góc
        Corner(go.transform, "TL", new Vector2(0,1), new Vector2( 8,-8));
        Corner(go.transform, "TR", new Vector2(1,1), new Vector2(-8,-8));
        Corner(go.transform, "BL", new Vector2(0,0), new Vector2( 8, 8));
        Corner(go.transform, "BR", new Vector2(1,0), new Vector2(-8, 8));
        return r;
    }

 


    private static void Corner(Transform p, string id, Vector2 a, Vector2 off)
    {
        foreach ((float w, float h) in new[] { (16f,2f),(2f,16f) })
        {
            GameObject g = new GameObject($"C_{id}");
            g.transform.SetParent(p, false);
            RectTransform r = g.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = a;
            r.pivot = a;
            r.sizeDelta = new Vector2(w,h);
            r.anchoredPosition = off;
            g.AddComponent<Image>().color = RoyalGold;
        }
    }

    // ============================================================
    //  HEADER BAND (đỏ)
    // ============================================================
    public static void CreateHeaderBand(Transform parent, string text, float h = 38f)
    {
        GameObject go = new GameObject("HeaderBand");
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0,1);
        r.anchorMax = new Vector2(1,1);
        r.pivot     = new Vector2(0.5f,1f);
        r.sizeDelta = new Vector2(0,h);
        r.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = VietnamRed;

        TextMeshProUGUI t = MakeText(go.transform, "BT", text, SmallSize,
            new Color32(255,249,224,255));
        t.fontStyle = FontStyles.Bold;
        t.characterSpacing = 3f;
        t.richText = false;
        RectTransform tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero; tr.anchoredPosition = Vector2.zero;
    }

    // ============================================================
    //  TEXT
    // ============================================================
    public static TextMeshProUGUI MakeText(Transform parent, string name, string content,
        float size = BodySize, Color? color = null,
        TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        // Không set anchor ở đây — để VerticalLayoutGroup kiểm soát
        r.anchorMin = new Vector2(0,0.5f);
        r.anchorMax = new Vector2(1,0.5f);
        r.sizeDelta = new Vector2(0, size * 1.4f);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.color = color ?? TextWhite;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Truncate;
        t.richText = false;
        t.raycastTarget = false;
        return t;
    }

    // ============================================================
    //  BUTTON
    // ============================================================
    public static Button MakeButton(Transform parent, string name, string label,
        Color bg, Color textCol, float w = 380f, float h = 56f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(w, h);
        // Anchor sẽ do VerticalLayoutGroup quyết định
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);

        Image img = go.AddComponent<Image>();
        img.color = bg;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor    = bg;
        cb.highlightedColor = Color.Lerp(bg, Color.white, 0.22f);
        cb.pressedColor   = Color.Lerp(bg, Color.black, 0.15f);
        cb.fadeDuration   = 0.1f;
        btn.colors = cb;

        go.AddComponent<Outline>().effectColor = GoldBorder;

        TextMeshProUGUI t = MakeText(go.transform, "L", label, ButtonSize, textCol);
        t.fontStyle = FontStyles.Bold;
        t.characterSpacing = 2f;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform tr = t.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero; tr.anchoredPosition = Vector2.zero;

        return btn;
    }

    // ============================================================
    //  INPUT FIELD
    // ============================================================
    public static TMP_InputField MakeInputField(Transform parent, string name,
        string placeholder = "...", float w = 380f, float h = 50f,
        Color? borderColor = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(w, h);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        go.AddComponent<Image>().color = InputBg;

        Color bc = borderColor ?? ElectricCyan;
        Bar(go.transform, "BotLine",
            new Vector2(0,0), new Vector2(1,0), new Vector2(0.5f,0f),
            new Vector2(0,2), Vector2.zero, bc);

        // Text Area
        GameObject ta = new GameObject("TA");
        ta.transform.SetParent(go.transform, false);
        RectTransform tar = ta.AddComponent<RectTransform>();
        tar.anchorMin = Vector2.zero; tar.anchorMax = Vector2.one;
        tar.offsetMin = new Vector2(14,4); tar.offsetMax = new Vector2(-14,-4);
        ta.AddComponent<RectMask2D>();

        // Input text
        GameObject tg = new GameObject("T");
        tg.transform.SetParent(ta.transform, false);
        RectTransform tgr = tg.AddComponent<RectTransform>();
        tgr.anchorMin = Vector2.zero; tgr.anchorMax = Vector2.one;
        tgr.sizeDelta = Vector2.zero;
        TextMeshProUGUI tt = tg.AddComponent<TextMeshProUGUI>();
        tt.fontSize = SubtitleSize - 2f;
        tt.color = bc;
        tt.alignment = TextAlignmentOptions.MidlineLeft;
        tt.richText = false;
        tt.textWrappingMode = TextWrappingModes.NoWrap;

        // Placeholder
        GameObject pg = new GameObject("P");
        pg.transform.SetParent(ta.transform, false);
        RectTransform pgr = pg.AddComponent<RectTransform>();
        pgr.anchorMin = Vector2.zero; pgr.anchorMax = Vector2.one;
        pgr.sizeDelta = Vector2.zero;
        TextMeshProUGUI pt = pg.AddComponent<TextMeshProUGUI>();
        pt.text = placeholder;
        pt.fontSize = BodySize - 1f;
        pt.color = new Color(bc.r, bc.g, bc.b, 0.35f);
        pt.fontStyle = FontStyles.Italic;
        pt.alignment = TextAlignmentOptions.MidlineLeft;
        pt.richText = false;
        pt.textWrappingMode = TextWrappingModes.NoWrap;

        TMP_InputField field = go.AddComponent<TMP_InputField>();
        field.textViewport  = tar;
        field.textComponent = tt;
        field.placeholder   = pt;
        field.caretColor    = bc;
        field.selectionColor = new Color(bc.r, bc.g, bc.b, 0.25f);
        field.richText      = false;
        return field;
    }

    // ============================================================
    //  PROGRESS BAR
    // ============================================================
    public static Slider MakeProgressBar(Transform parent, string name,
        float w = 420f, float h = 7f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(w, h);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);

        Slider s = go.AddComponent<Slider>();
        s.minValue = 0f; s.maxValue = 1f; s.interactable = false;

        // BG
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgr = bg.AddComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one; bgr.sizeDelta = Vector2.zero;
        Image bgi = bg.AddComponent<Image>(); bgi.color = new Color32(255,255,255,15);

        // Fill area
        GameObject fa = new GameObject("FA");
        fa.transform.SetParent(go.transform, false);
        RectTransform far = fa.AddComponent<RectTransform>();
        far.anchorMin = Vector2.zero; far.anchorMax = Vector2.one;
        far.offsetMin = far.offsetMax = Vector2.zero;

        // Fill
        GameObject f = new GameObject("F");
        f.transform.SetParent(fa.transform, false);
        RectTransform fr = f.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(0,1); fr.sizeDelta = Vector2.zero;
        f.AddComponent<Image>().color = RoyalGold;

        s.fillRect = fr;
        return s;
    }

    // ============================================================
    //  LAYOUT GROUP (UpperCenter, childControlHeight=FALSE)
    // ============================================================
    /// <summary>
    /// Tạo VerticalLayoutGroup. childControlHeight = FALSE để SetLayoutSize hoạt động đúng.
    /// </summary>
    public static VerticalLayoutGroup VLayout(GameObject go, float spacing = 10f,
        RectOffset pad = null)
    {
        VerticalLayoutGroup v = go.AddComponent<VerticalLayoutGroup>();
        v.spacing = spacing;
        v.padding = pad ?? new RectOffset(32, 32, 16, 16);
        v.childAlignment      = TextAnchor.UpperCenter;
        v.childControlWidth   = true;
        v.childControlHeight  = false;   // QUAN TRỌNG: false để preferred height có hiệu lực
        v.childForceExpandWidth  = true;
        v.childForceExpandHeight = false;
        return v;
    }

    /// <summary>
    /// Gán preferred height (và width) cho một LayoutElement.
    /// </summary>
    public static void PH(GameObject go, float h, float w = -1f)
    {
        LayoutElement le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        if (h >= 0) le.preferredHeight = h;
        if (w >= 0) le.preferredWidth  = w;
    }

    // ============================================================
    //  ANIMATION
    // ============================================================
    public static IEnumerator SlideIn(RectTransform t, float dur = 0.4f,
        float delay = 0f, float dist = 280f)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        Vector2 end   = t.anchoredPosition;
        Vector2 start = end + new Vector2(-dist, 0);
        t.anchoredPosition = start;
        float e = 0;
        while (e < dur)
        {
            e += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(e / dur), 3f);
            t.anchoredPosition = Vector2.Lerp(start, end, p);
            yield return null;
        }
        t.anchoredPosition = end;
    }

    public static IEnumerator SlideOut(RectTransform t, float dur = 0.28f,
        float dist = 360f)
    {
        Vector2 start = t.anchoredPosition;
        Vector2 end   = start + new Vector2(dist, 0);
        float e = 0;
        while (e < dur)
        {
            e += Time.deltaTime;
            float p = Mathf.Clamp01(e / dur);
            t.anchoredPosition = Vector2.Lerp(start, end, p * p);
            yield return null;
        }
        t.gameObject.SetActive(false);
        t.anchoredPosition = start;
    }

    public static IEnumerator ScalePunch(Transform t, float s = 1.07f, float dur = 0.22f)
    {
        Vector3 orig = t.localScale;
        float e = 0;
        while (e < dur)
        {
            e += Time.deltaTime;
            t.localScale = orig * (1f + Mathf.Sin(e / dur * Mathf.PI) * (s - 1f));
            yield return null;
        }
        t.localScale = orig;
    }

    // ============================================================
    //  INTERNAL HELPERS
    // ============================================================
    private static void Bar(Transform p, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pivot,
        Vector2 size, Vector2 pos, Color col)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(p, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;
        go.AddComponent<Image>().color = col;
    }

    public static RectTransform Divider(Transform p, string name,
        float w = 0f, float h = 1f, Color? col = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(p, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(w, h);
        Image img = go.AddComponent<Image>();
        img.color = col ?? new Color32(255,215,0,80);
        img.raycastTarget = false;
        return r;
    }

    public static RectTransform Lotus(Transform p, float size, Color col, float alpha)
    {
        Color c = col; c.a = alpha;
        GameObject go = new GameObject("Lotus");
        go.transform.SetParent(p, false);
        RectTransform r = go.AddComponent<RectTransform>();
        r.sizeDelta = new Vector2(size, size);
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);

        float[] angles = { 0,45,90,135,180,225,270,315 };
        foreach (float a in angles)
        {
            GameObject petal = new GameObject("P");
            petal.transform.SetParent(go.transform, false);
            RectTransform pr = petal.AddComponent<RectTransform>();
            pr.sizeDelta = new Vector2(size*0.26f, size*0.58f);
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f,0.5f);
            pr.anchoredPosition = Vector2.zero;
            pr.localRotation = Quaternion.Euler(0,0,a);
            Color pc = c; pc.a *= 0.7f;
            petal.AddComponent<Image>().color = pc;
        }
        GameObject cen = new GameObject("C");
        cen.transform.SetParent(go.transform, false);
        RectTransform cr = cen.AddComponent<RectTransform>();
        cr.sizeDelta = new Vector2(size*0.22f, size*0.22f);
        cr.anchorMin = cr.anchorMax = new Vector2(0.5f,0.5f);
        cr.anchoredPosition = Vector2.zero;
        cen.AddComponent<Image>().color = c;

        return r;
    }

    /// <summary>
    /// Tạo dải skyline thành phố Sài Gòn (hình chữ nhật mô phỏng các tòa nhà) ở đáy màn hình.
    /// </summary>
    public static void SaigonSkyline(Transform parent, float baseY = 0f)
    {
        // Mảng chiều cao các "tòa nhà" - mô phỏng Bitexco, Landmark 81, nhà thờ Đức Bà...
        float[] heights = { 60,40,90,35,55,130,45,70,35,50,170,40,65,45,80,55,35,95,50,40,60,45,75,35,110 };
        float totalW = 1920f;
        float bw = totalW / heights.Length;

        for (int i = 0; i < heights.Length; i++)
        {
            GameObject b = new GameObject($"Bld{i}");
            b.transform.SetParent(parent, false);
            RectTransform r = b.AddComponent<RectTransform>();
            r.anchorMin = new Vector2((float)i / heights.Length, 0f);
            r.anchorMax = new Vector2((float)(i + 1) / heights.Length, 0f);
            r.pivot = new Vector2(0.5f, 0f);
            r.sizeDelta = new Vector2(-2f, heights[i] + baseY);
            r.anchoredPosition = Vector2.zero;
            Image img = b.AddComponent<Image>();
            // Sáng tối xen kẽ tạo cảm giác chiều sâu
            float v = (i % 3 == 0) ? 0.08f : (i % 3 == 1) ? 0.05f : 0.12f;
            img.color = new Color(v, v * 1.2f, v * 1.8f, 0.7f);
            img.raycastTarget = false;

            // Thêm "cửa sổ" sáng ngẫu nhiên
            int windows = (int)(heights[i] / 18f);
            for (int w = 0; w < windows; w++)
            {
                if (Random.value < 0.4f) continue; // Một số cửa sổ tối
                GameObject win = new GameObject("W");
                win.transform.SetParent(b.transform, false);
                RectTransform wr = win.AddComponent<RectTransform>();
                wr.anchorMin = wr.anchorMax = new Vector2(Random.Range(0.15f, 0.85f), (float)w / windows + 0.05f);
                wr.sizeDelta = new Vector2(Random.Range(3f, 6f), Random.Range(3f, 5f));
                Image wi = win.AddComponent<Image>();
                wi.color = new Color(1f, 0.9f, 0.5f, Random.Range(0.15f, 0.5f));
                wi.raycastTarget = false;
            }
        }

        // Bitexco "chóp" đặc biệt giữa màn hình
        GameObject spire = new GameObject("Spire");
        spire.transform.SetParent(parent, false);
        RectTransform sr = spire.AddComponent<RectTransform>();
        sr.anchorMin = sr.anchorMax = new Vector2(0.44f, 0f);
        sr.pivot = new Vector2(0.5f, 0f);
        sr.sizeDelta = new Vector2(8f, 200f);
        sr.anchoredPosition = Vector2.zero;
        Image si = spire.AddComponent<Image>();
        si.color = new Color(0.15f, 0.2f, 0.35f, 0.6f);
        si.raycastTarget = false;
    }

    /// <summary>
    /// Tạo nhiều ngôi sao nhấp nháy trên nền trời đêm.
    /// </summary>
    public static void NightStars(Transform parent, int count = 30)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject s = new GameObject($"Star{i}");
            s.transform.SetParent(parent, false);
            RectTransform r = s.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(Random.Range(0.02f, 0.98f), Random.Range(0.25f, 0.97f));
            float sz = Random.Range(1.5f, 4f);
            r.sizeDelta = new Vector2(sz, sz);
            Image img = s.AddComponent<Image>();
            img.color = new Color(1f, 1f, 0.9f, Random.Range(0.15f, 0.55f));
            img.raycastTarget = false;
        }
    }

    /// <summary>
    /// Tạo neon line chạy ngang (hiệu ứng đèn đô thị đêm).
    /// </summary>
    public static RectTransform NeonLine(Transform parent, float yAnchor, Color col, float alpha = 0.12f)
    {
        Color c = col; c.a = alpha;
        var ln = Divider(parent, "Neon", 0, 2f, c);
        ln.anchorMin = new Vector2(0f, yAnchor);
        ln.anchorMax = new Vector2(1f, yAnchor);
        ln.sizeDelta = new Vector2(0, 2f);
        return ln;
    }

    /// <summary>
    /// Tạo dải gradient mờ ở đáy (hiệu ứng sương mù thành phố).
    /// </summary>
    public static void CityFog(Transform parent)
    {
        GameObject fog = new GameObject("CityFog");
        fog.transform.SetParent(parent, false);
        RectTransform r = fog.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = new Vector2(1f, 0.18f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        Image img = fog.AddComponent<Image>();
        img.color = new Color(0.02f, 0.04f, 0.12f, 0.85f);
        img.raycastTarget = false;
    }
}
