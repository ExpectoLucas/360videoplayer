using UnityEngine;
using System.IO;

public class DebugToFile : MonoBehaviour
{
    private static DebugToFile instance = null;

    private string fileName;
    private StreamWriter writer;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);

        fileName = "log_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        string logPath = Path.Combine("C:/Git/Basic360VideoPlayer/Assets/logs", fileName); //change your local address
        writer = new StreamWriter(logPath, true);
        writer.AutoFlush = true;
        Debug.Log("Logging to file: " + logPath);
        
        Application.logMessageReceived += LogCallback;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= LogCallback;
        writer.Close();
        writer.Dispose();
    }

    private void LogCallback(string condition, string stackTrace, LogType type)
    {
        //only write "debug.log" type
        if (type == LogType.Log)
        {
            writer.WriteLine("[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + condition);
        }
        /*if (type == LogType.Exception)
        {
            writer.WriteLine(stackTrace);
        }*/
    }
}