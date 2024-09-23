using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Net;
using System.Collections.Generic;

public class CustomEditorWindow : EditorWindow
{
    private string description = "";
    private bool isGenerating = false;

    private bool useLessThan15GB = false;

    private List<GameObject> generatedObjects = new List<GameObject>();

    [MenuItem("Window/Custom Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("TxtTo3D");
    }

    private void OnGUI()
    {
        GUILayout.Label("Describe the object to generate", EditorStyles.boldLabel);

        useLessThan15GB = EditorGUILayout.Toggle("Use with less than 15GB", useLessThan15GB);

        //description = EditorGUILayout.TextField("Description", description);
        GUILayout.Label("Description", EditorStyles.label);

        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;

        description = EditorGUILayout.TextArea(description, textAreaStyle, GUILayout.Height(100), GUILayout.ExpandWidth(true));

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.fontSize = 14;
        buttonStyle.fixedHeight = 40;

        if (isGenerating)
        {
            GUILayout.Space(20);
            GUILayout.Label("Your model is being generated...");
        }
        else
        {
            if (GUILayout.Button("Generate",buttonStyle))
            {
                Debug.Log("Send button pressed.");
                SendDescription(description);
            }
        }

    }

    private void SendDescription(string desc)
    {
        Debug.Log("Starting coroutine to send description.");
        isGenerating = true;
        EditorCoroutine.Start(SendRequest(desc));
    }

    private IEnumerator SendRequest(string desc)
    {
        Debug.Log($"Sending request with description: {desc}");
        string url = "http://ServerIP:5000/process";

        WWWForm form = new WWWForm();
        form.AddField("description", desc);
	WWWForm form = new WWWForm();
        form.AddField("description", desc);
        form.AddField("use_less_than_15GB", useLessThan15GB.ToString());

        UnityWebRequest www = UnityWebRequest.Post(url, form); 
        var response = www.SendWebRequest();
        while (!response.isDone) yield return null;

        Debug.Log($"UnityWebRequest result: {www.result}");
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Form upload complete!");
            string responseText = www.downloadHandler.text;
            Debug.Log("Response received: " + responseText);

            try
            {
                Debug.Log("Full response: " + responseText);

                // Parse the JSON response
                var jsonResponse = JsonUtility.FromJson<ResponseData>(responseText);
                if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.object_url))
                {
                    Debug.Log("Object URL found: " + jsonResponse.object_url);
                    Download3DObject(jsonResponse.object_url);
                }
                else
                {
                    Debug.LogError("Object URL or Texture URL not found in response.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing JSON response: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Error while sending request: " + www.error);
            Debug.LogError("Error details: " + www.downloadHandler);
            Debug.LogError("HTTP status code: " + www.responseCode);
            isGenerating = false;
        }

        isGenerating = false;
    }

    private void Download3DObject(string objectUrl)
    {
        Debug.Log("Starting coroutine to download 3D object: " + objectUrl);
        EditorCoroutine.Start(DownloadObjectCoroutine(objectUrl));
    }

    private IEnumerator DownloadObjectCoroutine(string objectUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(objectUrl);
        
        var response = www.SendWebRequest();
        while (!response.isDone) yield return null; //Waiting for the Models 

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("3D object complete.");
            string directoryPath = "Assets/DownloadedObjects/";
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //Guid for unique names
            string filePath = directoryPath + "mesh" + System.Guid.NewGuid() + ".glb";
            File.WriteAllBytes(filePath, www.downloadHandler.data);
            AssetDatabase.ImportAsset(filePath);
            GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);

            if(importedObject != null)
            { 
                GameObject instance = Instantiate(importedObject, Vector3.zero, Quaternion.identity);
                generatedObjects.Add(instance);

                Debug.Log("3D object downloaded and imported successfully!");
            }
            else
            {
                Debug.LogError("Failed to load the 3D object.");
            }
        }
        else
        {
            Debug.LogError("Error while downloading 3D object: " + www.error);
        }
    }
}

[System.Serializable]
public class ResponseData
{
    public string object_url;
}

public class EditorCoroutine
{
    private readonly IEnumerator routine;

    private EditorCoroutine(IEnumerator routine)
    {
        this.routine = routine;
    }

    public static EditorCoroutine Start(IEnumerator routine)
    {
        EditorCoroutine coroutine = new EditorCoroutine(routine);
        coroutine.Start();
        return coroutine;
    }

    private void Start()
    {
        EditorApplication.update += Update;
    }

    public void Stop()
    {
        EditorApplication.update -= Update;
    }

    private void Update()
    {
        if (!routine.MoveNext())
        {
            Stop();
        }
    }
}
