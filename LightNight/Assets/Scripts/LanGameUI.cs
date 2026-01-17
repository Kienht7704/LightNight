using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanGameUI : MonoBehaviour
{
    public TMP_InputField IpInput; 
    public GameObject UiPanel;

    void Start()
    {
        IpInput.text = "127.0.0.1";
    }

    public void OnHostClick()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", 7777);
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("NetworkTest", LoadSceneMode.Single);
        UiPanel.SetActive(false); 
    }

    public void OnJoinClick()
    {
        string ip = IpInput.text;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, 7777);
        NetworkManager.Singleton.StartClient();
        UiPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
