using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ScoreUploader : MonoBehaviour
{
    private string studioApiUrl = "https://studio-backend.com/postMatchResults"; // Update with the studio's backend API URL

    public void SubmitScore(string walletAddress, string gameid, float score)
    {
        StartCoroutine(PostScore(walletAddress, gameid, score));
    }

    private IEnumerator PostScore(string walletAddress, string gameid, float score)
    {
        // Prepare JSON payload
        Dictionary<string, string> jsonData = new Dictionary<string, string>
        {
            { "address", walletAddress },
            { "gameid", gameid },
            { "amount", score.ToString() }
        };

        string jsonPayload = JsonUtility.ToJson(jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(studioApiUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(postData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Score successfully submitted: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Failed to submit score: " + webRequest.error);
            }
        }
    }
}
