using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    // Cached references
    public InputField emailInputField;
    public InputField passwordInputField;
    public Player player;

    public delegate void LoginAction();
    public static event LoginAction OnLogin;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        if (!string.IsNullOrEmpty(player.Id))
        {
            OnLogin?.Invoke();
        }
    }

    public void OnLoginButtonClicked()
    {
        StartCoroutine(TryLogin());
    }

    private IEnumerator TryLogin()
    {
        yield return Helper.InitializeToken(emailInputField.text, passwordInputField.text);
        yield return Helper.GetPlayerInfo();
        yield return LoginChat();
        if (!string.IsNullOrEmpty(player.Id))
        {
            if (OnLogin != null)
            {
                OnLogin();
            }
        }
        else
        {
            throw new Exception("Login: Error " + player.Name);
        }

    }

    private IEnumerator LoginChat()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Chat/UserIsLogued", "POST");

        ChatModel cm = new ChatModel();
        cm.Id = player.Id;
        cm.LastMesage = "";

        string jsonData = JsonUtility.ToJson(cm);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("OnClickRegisterButton: Error > " + httpClient.error);
        }

        httpClient.Dispose();
    }

}
