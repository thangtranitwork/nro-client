using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NETHTTPClient
{
    public static string baseUrl = "https://hoiucnro.com/server.txt";
    public static string finishRespone = "";
    public static void start()
    {
        Main.main.StartCoroutine(GetTextFromURL(baseUrl));
        
    }
    public static IEnumerator GetTextFromURL(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string content = request.downloadHandler.text;
                finishRespone = content;
                Debug.LogError("finishRespone: " + content);
                connect();
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    public static void connect()
    {
        ServerListScreen.getServerList(finishRespone);

    }
}