using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Gameplay
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;
    }

    public class LeaderboardService : MonoBehaviour
    {
        public string ServerBaseUrl = "http://localhost:3000";

        [System.Serializable]
        private class ScoreListWrapper
        {
            public LeaderboardEntry[] scores;
        }

        public void GetScores(Action<LeaderboardEntry[]> onSuccess, Action<string> onError)
        {
            StartCoroutine(GetScoresCoroutine(onSuccess, onError));
        }

        public void PostScore(string name, int score, Action<LeaderboardEntry[]> onSuccess, Action<string> onError)
        {
            StartCoroutine(PostScoreCoroutine(name, score, onSuccess, onError));
        }

        private IEnumerator GetScoresCoroutine(Action<LeaderboardEntry[]> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(ServerBaseUrl + "/scores"))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(req.result == UnityWebRequest.Result.ProtocolError
                        ? "HTTP " + req.responseCode + ": " + req.downloadHandler.text
                        : req.error);
                }
                else
                {
                    string wrapped = "{\"scores\":" + req.downloadHandler.text + "}";
                    onSuccess?.Invoke(JsonUtility.FromJson<ScoreListWrapper>(wrapped).scores);
                }
            }
        }

        private IEnumerator PostScoreCoroutine(string name, int score, Action<LeaderboardEntry[]> onSuccess, Action<string> onError)
        {
            string jsonBody = "{\"name\":\"" + name + "\",\"score\":" + score + "}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            using (UnityWebRequest req = new UnityWebRequest(ServerBaseUrl + "/scores", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(req.result == UnityWebRequest.Result.ProtocolError
                        ? "HTTP " + req.responseCode + ": " + req.downloadHandler.text
                        : req.error);
                }
                else
                {
                    string wrapped = "{\"scores\":" + req.downloadHandler.text + "}";
                    onSuccess?.Invoke(JsonUtility.FromJson<ScoreListWrapper>(wrapped).scores);
                }
            }
        }
    }
}
