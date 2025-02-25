using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ScoreUploader : MonoBehaviour
{
    private string studioApiUrl = "https://studio-backend.com/postMatchResults"; // Update with the studio's backend API URL

    public void SubmitScore(Dictionary<string, object> matchData)
    {
        StartCoroutine(PostScore(matchData));
    }

    private IEnumerator PostScore(Dictionary<string, object> matchData)
    {
        // Convert dictionary to JSON
        string jsonPayload = JsonUtility.ToJson(new Wrapper(matchData));
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

    // Wrapper class for Unity JSON serialization
    [System.Serializable]
    private class Wrapper
    {
        public Dictionary<string, object> data;

        public Wrapper(Dictionary<string, object> dictionary)
        {
            this.data = dictionary;
        }
    }
}
