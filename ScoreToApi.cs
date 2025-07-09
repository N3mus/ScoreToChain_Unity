using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// A simple data model for a score entry
[System.Serializable]
public class ScoreEntry
{
    public string wallet;
    public long score;
    public Dictionary<string, int> meta;

    public override string ToString()
    {
        return $"{wallet} → {score} pts; meta={JsonConvert.SerializeObject(meta)}";
    }
}

// Manages an in-memory queue and local log of pending scores
public class ScoreQueue : MonoBehaviour
{
    private List<ScoreEntry> _pending = new List<ScoreEntry>();
    private string _logPath;

    void Awake()
    {
        _logPath = Path.Combine(Application.persistentDataPath, "scores.log");
    }

    public void Enqueue(ScoreEntry entry)
    {
        _pending.Add(entry);
        var line = $"{System.DateTime.UtcNow:o} {entry}\n";
        File.AppendAllText(_logPath, line);
    }

    public int PendingCount => _pending.Count;

    public List<ScoreEntry> DequeueAll()
    {
        var batch = new List<ScoreEntry>(_pending);
        _pending.Clear();
        return batch;
    }
}

// Handles sending batches of scores to the backend API
public class ScoreUploader : MonoBehaviour
{
    [SerializeField]
    private string studioApiUrl = "https://studio-backend.com/postMatchResults";

    public void SubmitScores(
        List<string> walletAddresses,
        List<long> scores,
        List<Dictionary<string, int>> additionalDataList = null
    )
    {
        if (walletAddresses == null || scores == null ||
            walletAddresses.Count != scores.Count ||
            (additionalDataList != null && additionalDataList.Count != walletAddresses.Count))
        {
            Debug.LogError("Invalid parameters for SubmitScores");
            return;
        }

        // Build metadata arrays
        var keysArray = new List<List<string>>();
        var valuesArray = new List<List<int>>();
        if (additionalDataList != null)
        {
            foreach (var data in additionalDataList)
            {
                keysArray.Add(new List<string>(data.Keys));
                valuesArray.Add(new List<int>(data.Values));
            }
        }

        var payload = new Dictionary<string, object>
        {
            { "wallets", walletAddresses },
            { "scores", scores },
            { "keys", keysArray },
            { "values", valuesArray }
        };

        StartCoroutine(PostPayload(payload));
    }

    private IEnumerator PostPayload(Dictionary<string, object> payload)
    {
        string jsonPayload = JsonConvert.SerializeObject(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (var request = new UnityWebRequest(studioApiUrl, "POST"))
        {
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Batch scores submitted: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Batch submission failed: {request.error}");
                Debug.LogError($"⚠️ Server response: {request.downloadHandler.text}");
            }
        }
    }
}

// Orchestrates queuing and flushing scores on triggers
[RequireComponent(typeof(ScoreUploader))]
public class ScoreManager : MonoBehaviour
{
    private ScoreQueue queue;
    private ScoreUploader uploader;
    private const int BATCH_SIZE = 10;

    void Awake()
    {
        queue = gameObject.AddComponent<ScoreQueue>();
        uploader = GetComponent<ScoreUploader>();
    }

    void Start()
    {
        StartCoroutine(PeriodicFlush());
    }

    /// <summary>
    /// Record a new score entry
    /// </summary>
    public void RecordScore(string wallet, long score, Dictionary<string,int> meta = null)
    {
        var entry = new ScoreEntry
        {
            wallet = wallet,
            score  = score,
            meta   = meta ?? new Dictionary<string,int>()
        };
        queue.Enqueue(entry);

        if (queue.PendingCount >= BATCH_SIZE)
        {
            FlushNow();
        }
    }

    private IEnumerator PeriodicFlush()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
            if (queue.PendingCount > 0)
                FlushNow();
        }
    }

    void OnApplicationQuit()
    {
        FlushNow();
    }

    private void FlushNow()
    {
        var batch = queue.DequeueAll();
        if (batch.Count == 0) return;

        var wallets = batch.Select(e => e.wallet).ToList();
        var scores  = batch.Select(e => e.score).ToList();
        var metas   = batch.Select(e => e.meta).ToList();

        uploader.SubmitScores(wallets, scores, metas);
        Debug.Log($"→ Flushing {batch.Count} scores to API");
    }
}
