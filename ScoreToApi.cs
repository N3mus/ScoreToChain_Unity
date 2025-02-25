using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json; // Using JSON.NET for proper dictionary serialization

public class ScoreUploader : MonoBehaviour
{
    private string studioApiUrl = "https://studio-backend.com/postMatchResults"; // Update with the studio's backend API URL

    public void SubmitScore(string walletAddress, float score, Dictionary<string, int> additionalData)
    {
        // Prepare match data
        Dictionary<string, object> matchData = new Dictionary<string, object>
        {
            { "address", walletAddress },
            { "amount", score.ToString() }, // Ensure score is sent as a string to match API format
            { "keys", new List<string>(additionalData.Keys) }, // Extract keys
            { "values", new List<int>(additionalData.Values) }  // Extract values
        };

        StartCoroutine(PostScore(matchData));
    }

    private IEnumerator PostScore(Dictionary<string, object> matchData)
    {
        // Convert dictionary to JSON properly
        string jsonPayload = JsonConvert.SerializeObject(matchData);
        byte[] postData = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest webRequest = new UnityWebRequest(studioApiUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(postData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Score successfully submitted: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("❌ Failed to submit score: " + webRequest.error);
                Debug.LogError("⚠️ Server response: " + webRequest.downloadHandler.text);
            }
        }
    }
}
