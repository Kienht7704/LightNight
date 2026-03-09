using UnityEngine;
using System.IO; // Để thao tác Đọc/Ghi File
using System;    // Để dùng thư viện sinh mã UUID (Guid)


[System.Serializable]
public class UserData
{
    public string userId;
    public string userName;
}

public class UserManager : MonoBehaviour
{
    [Header("Thông tin người chơi")]
    public string currentUserId;
    public string currentUserName;

    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/UserData.json";

        LoadOrGenerateUserId();
    }

    public void LoadOrGenerateUserId()
    {
        if (File.Exists(saveFilePath))
        {
            string jsonContent = File.ReadAllText(saveFilePath);

            UserData data = JsonUtility.FromJson<UserData>(jsonContent);

            if (data != null )
            {
                if(!string.IsNullOrEmpty(data.userId))
                    currentUserId = data.userId;
                if(!string.IsNullOrWhiteSpace(data.userName))
                    currentUserName = data.userName;
            }
            else
            {
                GenerateAndSaveNewId();
            }

        }
        else
        {
            GenerateAndSaveNewId();
        }
    }

    public void SaveCurrentUserName()
    {
        UserData updatedData = new UserData();
        updatedData.userId = currentUserId;
        updatedData.userName = currentUserName;
        string jsonToSave = JsonUtility.ToJson(updatedData, true);
        File.WriteAllText(saveFilePath, jsonToSave);
    }

    private void GenerateAndSaveNewId()
    {
        currentUserId = Guid.NewGuid().ToString();
        currentUserName = "UserName";
        UserData newData = new UserData();
        newData.userId = currentUserId;
        newData.userName = currentUserName;

        string jsonToSave = JsonUtility.ToJson(newData, true);
        File.WriteAllText(saveFilePath, jsonToSave);
    }
}