using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Màn hình Menu (Pause) trong lúc đua, dùng code UITheme hoàn toàn để đồng bộ thẩm mỹ.
/// Cách dùng: Kéo vào GameScene, nhấn Tạm Dừng.
/// </summary>
public class InGameMenuUI : MonoBehaviour
{
    private Canvas _canvas;
    private GameObject _root;
    private RectTransform _card;

    private bool _isPaused = false;

    private void Start()
    {
        BuildMenu();
        _root.SetActive(false);
    }

    private void Update()
    {
        // Nhấn Esc để toggle menu (hoặc tuỳ chỉnh phím của bạn)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void BuildMenu()
    {
        _canvas = UITheme.CreateCanvas("InGameMenuCanvas");
        // Ensure that the pause menu is deeply on top of HUD
        _canvas.sortingOrder = 999; 

        RectTransform bg = UITheme.CreateFullScreen(_canvas.transform, "MenuBg", new Color(0, 0, 0, 0.7f));
        _root = bg.gameObject;

        // Card Menu: Cao 480, Rộng 420
        _card = UITheme.CreateCard(bg, "MenuCard", 420f, 480f);
        UITheme.CreateHeaderBand(_card, "* TAM DUNG TRO CHOI *", 38f);

        GameObject ct = new GameObject("Content");
        ct.transform.SetParent(_card, false);
        RectTransform ctr = ct.AddComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero; ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero; ctr.offsetMax = new Vector2(0, -38);
        
        UITheme.VLayout(ct, 16f, new RectOffset(32, 32, 28, 28));

        // Title
        var title = UITheme.MakeText(ct.transform, "Title", "TAM DUNG", UITheme.HeadingSize, UITheme.ElectricCyan);
        title.fontStyle = FontStyles.Bold;
        UITheme.PH(title.gameObject, 50f);

        // Divider
        var dv = UITheme.Divider(ct.transform, "Dv", 0, 1f, new Color32(255, 215, 0, 80));
        dv.anchorMin = new Vector2(0, 0.5f); dv.anchorMax = new Vector2(1, 0.5f);
        UITheme.PH(dv.gameObject, 15f);

        // Chỗ trống nhỏ
        var sp1 = UITheme.MakeText(ct.transform, "S", "", 1f, Color.clear);
        UITheme.PH(sp1.gameObject, 10f);

        // Button Continue
        Button btnContinue = UITheme.MakeButton(ct.transform, "ContinueBtn", "TIEP TUC", UITheme.VietnamRed, UITheme.RoyalGold, 0, 55f);
        btnContinue.onClick.AddListener(ToggleMenu);
        UITheme.PH(btnContinue.gameObject, 55f);

        // Chỗ trống nhỏ
        var sp2 = UITheme.MakeText(ct.transform, "S2", "", 1f, Color.clear);
        UITheme.PH(sp2.gameObject, 5f);

        // Button Settings
        Button btnSettings = UITheme.MakeButton(ct.transform, "SettingsBtn", "CAI DAT", new Color32(0, 245, 255, 28), UITheme.ElectricCyan, 0, 55f);
        btnSettings.GetComponent<Outline>().effectColor = new Color32(0, 245, 255, 80);
        UITheme.PH(btnSettings.gameObject, 55f);

        // Chỗ trống nhỏ
        var sp3 = UITheme.MakeText(ct.transform, "S3", "", 1f, Color.clear);
        UITheme.PH(sp3.gameObject, 5f);

        // Button Exit
        Button btnExit = UITheme.MakeButton(ct.transform, "ExitBtn", "THOAT GAME", new Color32(0, 0, 0, 0), UITheme.TextGold, 0, 55f);
        btnExit.GetComponent<Outline>().effectColor = new Color32(200, 180, 140, 40);
        btnExit.onClick.AddListener(ExitGame);
        UITheme.PH(btnExit.gameObject, 55f);
    }

    public void ToggleMenu()
    {
        _isPaused = !_isPaused;
        _root.SetActive(_isPaused);
        
        // Dừng thời gian khi bật menu
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_isPaused)
        {
            _card.gameObject.SetActive(true);
            StartCoroutine(UITheme.SlideIn(_card, 0.25f, 0f, 150f));     
        }
    }

    private void ExitGame()
    {
        Time.timeScale = 1f; // Phục hồi Time Scale trước khi load
        
        // Ngắt kết nối Network/Lobby
        var mgr = FindAnyObjectByType<LobbyManager>();
        if (mgr != null) mgr.Disconnect();
        
        // Load lại màn hình Lobby (Giả sử build index 0 là Menu)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); 
    }
}
