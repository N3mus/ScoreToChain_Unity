using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ScoreUploader : MonoBehaviour
{
    private string studioApiUrl = "https://studio-backend.com/postMatchResults"; // Update with the studio's backend API URL

    public void SubmitScore(Dictionary<string, object> scoreData)
    {
        StartCoroutine(PostScore(scoreData));
    }

    private IEnumerator PostScore(Dictionary<string, object> scoreData)
    {
        // Convert dictionary to JSON
        string jsonPayload = JsonUtility.ToJson(new Wrapper(scoreData));
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

    // Wrapper class to allow serialization of dictionaries
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
