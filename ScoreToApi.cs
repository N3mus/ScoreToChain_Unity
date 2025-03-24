using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json; // JSON.NET for proper serialization

public class ScoreUploader : MonoBehaviour
{
    private string studioApiUrl = "https://studio-backend.com/postMatchResults"; // ✅ Replace with your real backend URL

    /// <summary>
    /// Submits a score (can be negative) along with additional key-value match data.
    /// </summary>
    /// <param name="walletAddress">User wallet address</param>
    /// <param name="score">Score as float (can be negative)</param>
    /// <param name="additionalData">Dictionary with match keys and values</param>
    public void SubmitScore(string walletAddress, float score, Dictionary<string, int> additionalData)
    {
        // Convert additionalData to separate lists
        List<string> keys = new List<string>(additionalData.Keys);
        List<int> values = new List<int>(additionalData.Values);

        // Ensure we send the score as a string so it gets properly parsed as BigInt (int256) on backend
        Dictionary<string, object> matchData = new Dictionary<string, object>
        {
            { "address", walletAddress },
            { "amount", score.ToString() }, // 👈 Important: keep as string for BigInt parsing
            { "keys", keys },
            { "values", values }
        };

        StartCoroutine(PostScore(matchData));
    }

    private IEnumerator PostScore(Dictionary<string, object> matchData)
    {
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
