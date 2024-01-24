using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using utils;

public class SaveMesh : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        bool save = gameObject.GetComponent<Room>().saveRoomOnQuit;
        string name = gameObject.GetComponent<Room>().roomName;

        if (save)
            SaveRoom(name);
    }

    // Save an entire room
    public void SaveRoom(string roomName)
    {

        GameObject room = GameObject.Find("Room");
        double saveTime = Utils.GetTimestamp();
        Directory.CreateDirectory("Assets/SavedRooms/" + roomName);
        print(roomName);
        GameObject reference = GameObject.Find("Reference");

        Transform[] segmentChildren = Utils.FindObjectsByName("PC_", room.transform);
        MeshMetaData.RoomMetaData metaData = new MeshMetaData.RoomMetaData(roomName, saveTime, segmentChildren.Length,
            reference.transform.eulerAngles, reference.transform.position);

        AssetDatabase.StartAssetEditing();
        for (int i = 0; i < segmentChildren.Length; i++)
        {
            GameObject roomSegment = segmentChildren[i].gameObject;
            StartCoroutine(SaveSegment(roomName, roomSegment, metaData, i));
            print("Done " + roomSegment.name);
        }
        AssetDatabase.StopAssetEditing();

        // Write JSON to file
        string jsonData = JsonUtility.ToJson(metaData);
        string jsonPath = "Assets/SavedRooms/" + roomName + "/" + roomName + ".json";
        File.WriteAllText(jsonPath, jsonData);

        //EditorUtility.ClearProgressBar();
    }

    // Save a single chunk of the room
    public IEnumerator SaveSegment(string roomName, GameObject roomSegment, MeshMetaData.RoomMetaData metaData, int i)
    {
        Directory.CreateDirectory("Assets/SavedRooms/" + roomName);
        string basePath = "Assets/SavedRooms/" + roomName + "/" + roomSegment.name;
        Transform transform = roomSegment.transform;


        MeshMetaData.Segment segment = new MeshMetaData.Segment(roomSegment.name,
            transform.eulerAngles, transform.position, basePath);

        print("Saving " + roomSegment.name);
        SaveTextureFile(roomSegment, segment.colorPath);
        SaveTextureFile(roomSegment, segment.uvPath);
        SaveMeshFile(roomSegment, segment.meshPath);
        metaData.segments[i] = segment;

        yield return null;
    }

    // Saving the mesh
    private void SaveMeshFile(GameObject roomSegment, string modelPath)
    {
        Mesh mesh = roomSegment.GetComponent<MeshFilter>().mesh;
        mesh.name = roomSegment.name;
        AssetDatabase.CreateAsset(mesh, modelPath);
    }

    //Saving the material
    private void SaveMaterialFile(GameObject roomSegment, string matPath)
    {
        Material material = roomSegment.GetComponent<MeshRenderer>().sharedMaterial;
        AssetDatabase.CreateAsset(material, matPath);
    }

    // Save the textures
    private void SaveTextureFile(GameObject roomSegment, string path)
    {
        Material mat = roomSegment.GetComponent<MeshRenderer>().sharedMaterial;
        
        if (path.Contains("color"))
            Utils.SaveImage((Texture2D)mat.GetTexture("_MainTex"), path);
        else
            Utils.SaveImage((Texture2D)mat.GetTexture("_UVMap"), path);
    }
}
