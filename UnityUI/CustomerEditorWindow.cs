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

    private List<GameObject> generatedObjects = new List<GameObject>();

    [MenuItem("Window/Custom Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<CustomEditorWindow>("TxtTo3D");
    }

    private void OnGUI()
    {
        GUILayout.Label("Describe the Object you want to generate", EditorStyles.boldLabel);
        description = EditorGUILayout.TextField("Description", description);

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
        string url = "http://YourServerIPAdress:Port/process";

        WWWForm form = new WWWForm();
        form.AddField("description", desc);

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
                if (jsonResponse != null && !string.IsNullOrEmpty(jsonResponse.object_url) && !string.IsNullOrEmpty(jsonResponse.texture_url))
                {
                    Debug.Log("Object URL found: " + jsonResponse.object_url);
                    Debug.Log("Texture URL found: " + jsonResponse.texture_url);
                    Download3DObject(jsonResponse.object_url, jsonResponse.texture_url);
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

    private void Download3DObject(string objectUrl,string textureUrl)
    {
        Debug.Log("Starting coroutine to download 3D object and texture from: " + objectUrl + " & " + textureUrl);
        EditorCoroutine.Start(DownloadObjectCoroutine(objectUrl,textureUrl));
    }

    private IEnumerator DownloadObjectCoroutine(string objectUrl,string textureUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(objectUrl);
        var response = www.SendWebRequest();
        while (!response.isDone) yield return null; //Waiting for the Models 

        UnityWebRequest wwwTex = UnityWebRequestTexture.GetTexture(textureUrl);
        var secResponse = wwwTex.SendWebRequest();
        while (!secResponse.isDone) yield return null;

        if (www.result == UnityWebRequest.Result.Success && wwwTex.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("3D object and texture download complete.");
            string directoryPath = "Assets/DownloadedObjects/";
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //Guid for unique names
            string filePath = directoryPath + "3DModel_" + System.Guid.NewGuid() + ".obj";
            File.WriteAllBytes(filePath, www.downloadHandler.data);
            AssetDatabase.ImportAsset(filePath);
            GameObject importedObject = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);

            string textureFilePath = directoryPath + "texture_" + System.Guid.NewGuid() + ".png";
            File.WriteAllBytes(textureFilePath, wwwTex.downloadHandler.data);
            AssetDatabase.ImportAsset(textureFilePath);
            Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFilePath);

            if(importedObject != null && importedTexture != null)
            { 
                GameObject instance = Instantiate(importedObject, Vector3.zero, Quaternion.identity);
                generatedObjects.Add(instance);
                Renderer renderer = instance.GetComponentInChildren<Renderer>();
                
                if(renderer != null)
                {
                    Material newMaterial = new Material(Shader.Find("Standard"));
                    newMaterial.mainTexture = importedTexture;
                    renderer.sharedMaterial = newMaterial;
                    Debug.Log("Texture applied to the 3D object.");
                }
                else
                {
                        Debug.LogError("Failed to apply texture: Renderer component not found.");
                }
                Debug.Log("3D object downloaded and imported successfully!");
            }
            else
            {
                Debug.LogError("Failed to load the 3D object.");
            }
        }
        else
        {
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Error while downloading 3D object: " + www.error);

            if (wwwTex.result != UnityWebRequest.Result.Success)
                Debug.LogError("Error while downloading texture: " + wwwTex.error);
        }
    }
}

[System.Serializable]
public class ResponseData
{
    public string object_url;
    public string texture_url;
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
