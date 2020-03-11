using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    private float spawnRate = 1.0f;
    private int score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI playerNameText;
    public GameObject titleScreen;
    public GameObject gameOverScreen;
    public bool isGameActive;

    public Player player;

    public InputField textToSend;
    public Text chatText;


    void Start()
    {
        player = FindObjectOfType<Player>();
        playerNameText.text = player.Name + "(" + (DateTime.Now - player.BirthDay).Days / 365 + ")";
        scoreText.text = "Score: " + score.ToString();
    }


    public void StartGame(int difficulty)
    {
        isGameActive = true;
        score = 0;
        UpdateScore(0);
        titleScreen.gameObject.SetActive(false);
        spawnRate /= difficulty;
        StartCoroutine(SpawnTarget());
    }

    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            int randomIndex = UnityEngine.Random.Range(0, 4);
            Instantiate(targets[randomIndex]);
        }
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        gameOverScreen.gameObject.SetActive(true);
        isGameActive = false;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickSendButton()
    {
        StartCoroutine(SendTextToChat());
        textToSend.text = "";
        StartCoroutine(ReadChat());
    }

    private IEnumerator SendTextToChat()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Chat/PostNewMesage", "POST");

        ChatModel cm = new ChatModel();
        cm.Id = player.Id;
        cm.LastMesage = textToSend.text;

        string jsonData = JsonUtility.ToJson(cm);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);

        httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("OnClickSendButton: Error > " + httpClient.error);
        }

        httpClient.Dispose();
    }

    private IEnumerator ReadChat()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Chat/GetMesagesIfLogued", "GET");

        httpClient.downloadHandler = new DownloadHandlerBuffer();

        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        httpClient.SetRequestHeader("Accept", "application/json");        

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("Helper > GetPlayerInfo: " + httpClient.error);
        }
        else
        {
            List<ChatModel> cm = JsonUtility.FromJson<List<ChatModel>>(httpClient.downloadHandler.text);
            //chatText.text = chatText.text + "/n" + cm.Id.Substring(0, 3) + "> " + cm.LastMesage;
            foreach(ChatModel c in cm)
            {
                chatText.text = chatText.text + "/n" + c.Id.Substring(0, 3) + "> " + c.LastMesage;
            }
        }

        httpClient.Dispose();
    }
    
}
