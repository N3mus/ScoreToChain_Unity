using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ScoreUploader : MonoBehaviour
{
    private string apiUrl = "https://your-backend-url.com/postMatchResults";

    public void SubmitScore(string walletAddress, string matchUuid, float score)
    {
        StartCoroutine(PostScore(walletAddress, matchUuid, score));
    }

    private IEnumerator PostScore(string walletAddress, string matchUuid, float score)
    {
        // Convert the score to a JSON payload
        Dictionary<string, string> jsonData = new Dictionary<string, string>
        {
            { "address", walletAddress },
            { "uuid", matchUuid },
            { "amount", score.ToString() } // Ensure amount is stringified
        };
        
        string jsonPayload = JsonUtility.ToJson(jsonData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
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
