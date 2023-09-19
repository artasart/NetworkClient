
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class ColliderExtractor : EditorWindow
{
    [MenuItem("Tools/Extract Colliders")]
    public static void ExtractColliders()
    {
        List<ColliderData> colliderDataList = new List<ColliderData>();

        List<BoxCollider> allColliders = new List<BoxCollider>(FindObjectsOfType<BoxCollider>());

        colliderDataList.Clear();

        foreach (var collider in allColliders)
        {
            ColliderData colliderData = new ColliderData();

            colliderData.SizeX = collider.size.x.ToString("F2");
            colliderData.SizeY = collider.size.y.ToString("F2");
            colliderData.SizeZ = collider.size.z.ToString("F2");

            colliderData.PositionX = collider.transform.position.x.ToString("F2");
            colliderData.PositionY = collider.transform.position.y.ToString("F2");
            colliderData.PositionZ = collider.transform.position.z.ToString("F2");

            colliderData.RotationX = collider.transform.eulerAngles.x.ToString("F2");
            colliderData.RotationY = collider.transform.eulerAngles.y.ToString("F2");
            colliderData.RotationZ = collider.transform.eulerAngles.z.ToString("F2");

            colliderDataList.Add(colliderData);
        }

        ColliderDataListWrapper wrapper = new ColliderDataListWrapper();
        wrapper.colliderDataList = colliderDataList;

        string jsonData = JsonUtility.ToJson(wrapper);

        DebugManager.ClearLog(jsonData);

        Beautify(jsonData);
    }

    private static void Beautify(string _jsonString)
    {
        JObject jsonObject = JObject.Parse(_jsonString);

        string prettyJson = jsonObject.ToString(Formatting.Indented);

        Debug.Log(prettyJson);
    }
}

[System.Serializable]
public class ColliderData
{
    public string SizeX;
    public string SizeY;
    public string SizeZ;

    public string PositionX;
    public string PositionY;
    public string PositionZ;

    public string RotationX;
    public string RotationY;
    public string RotationZ;
}

[System.Serializable]
public class ColliderDataListWrapper
{
    public List<ColliderData> colliderDataList;
}