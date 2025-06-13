using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Requests
{
    private string baseUrl = "http://localhost:8000";

    // Funció per fer una petició POST amb un JSON
    public IEnumerator PostRequest(string endpoint, string json, System.Action<string> callback)
    {

        string url = baseUrl + endpoint;

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(request.downloadHandler.text);
            }
            else
            {
                callback?.Invoke($"ERROR: {request.responseCode} - {request.downloadHandler.text}");
            }
        }
    }

    // Funció per fer una petició GET
    public IEnumerator GetRequest(string endpoint, System.Action<string> callback)
    {
        string url = baseUrl + endpoint;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(request.downloadHandler.text);
            }
            else
            {
                callback?.Invoke($"ERROR: {request.responseCode} - {request.downloadHandler.text}");
            }
        }
    }
}
