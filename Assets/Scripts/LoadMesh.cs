using OrbSLAM;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using utils;

public class LoadMesh : MonoBehaviour
{

    MeshMetaData.RoomMetaData roomMetaData;

    private string roomDir;

    // Start is called before the first frame update
    public void LoadSelectedMesh()
    {
        string[] filePaths = Directory.GetFiles(roomDir, "*.json");
        string jsonPath = filePaths[0];
        roomMetaData = JsonUtility.FromJson<MeshMetaData.RoomMetaData>(File.ReadAllText(jsonPath));
        gameObject.GetComponent<Room>().roomName = roomMetaData.roomName;

        // Load the reference point (Aruco)
        LoadReference(roomMetaData.arucoPosition, roomMetaData.arucoEuler);
        
        // Load in individual meshes
        AssetDatabase.StartAssetEditing();
        for (int i = 0; i < roomMetaData.numSegments; i++)
        {
            MeshMetaData.Segment segment = roomMetaData.segments[i];
            GameObject roomSegment = new GameObject(segment.name);
            LoadSingleMesh(roomSegment, segment);
            EditorUtility.SetDirty(roomSegment);
        }
        AssetDatabase.StopAssetEditing();
        //EditorUtility.ClearProgressBar();
    }

    private void LoadReference(Vector3 position, Vector3 eulerAngles)
    {
        GameObject reference = GameObject.CreatePrimitive(PrimitiveType.Cube);
        reference.name = "Reference";
        reference.transform.eulerAngles = eulerAngles;
        reference.transform.position = position;
        reference.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        reference.GetComponent<Renderer>().material.color = Color.red;
        reference.transform.parent = gameObject.transform;
    }

    private void LoadSingleMesh(GameObject roomSegment, MeshMetaData.Segment segment)
    {     
        InitializeComponents(roomSegment);
        LoadMeshAsset(roomSegment, segment.meshPath);
        LoadMaterial(roomSegment);
        LoadTexture(roomSegment, TextureFormat.RGFloat, segment.uvPath, SLAMInterfaceRealSense.D_IMAGE_WIDTH, SLAMInterfaceRealSense.D_IMAGE_HEIGHT);
        LoadTexture(roomSegment, TextureFormat.RGB24, segment.colorPath, SLAMInterfaceRealSense.C_IMAGE_WIDTH, SLAMInterfaceRealSense.C_IMAGE_HEIGHT);
        roomSegment.AddComponent<MeshCollider>();
        roomSegment.GetComponent<Rigidbody>().isKinematic = true;
        roomSegment.transform.position = segment.position;
        roomSegment.transform.eulerAngles = segment.eulerAngles;
        roomSegment.transform.parent = GameObject.Find("Reference").transform;
        //roomSegment.transform.parent = gameObject.transform;
        //yield return null;
    }

    // Load uv and color textures
    private void LoadTexture(GameObject roomSegment, TextureFormat format, string texPath, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, format, false, true);
        byte[] tex = File.ReadAllBytes(texPath);
        texture.LoadImage(tex);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        if (texPath.Contains("color"))
            roomSegment.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", texture);
        else
            roomSegment.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", texture);
    }

    // Set the mesh
    private void LoadMeshAsset(GameObject roomSegment, string meshFile)
    {
        Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshFile, typeof(Mesh));
        MeshFilter meshFilter = roomSegment.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
    }

    // Set the material
    private void LoadMaterial(GameObject roomSegment)
    {
        MeshRenderer meshRenderer = roomSegment.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Custom/DynamicMesh"));
        meshRenderer.sharedMaterial = mat;
    }

    // Adding the right components to each pc
    private void InitializeComponents(GameObject roomSegment)
    {
        roomSegment.AddComponent<MeshFilter>();
        roomSegment.AddComponent<MeshRenderer>();

        roomSegment.AddComponent<Rigidbody>();
        roomSegment.GetComponent<Rigidbody>().useGravity = false;
        roomSegment.GetComponent<Rigidbody>().isKinematic = false;
    }

    // Set the room diirectory
    public void SetRoomDir(string dir)
    {
        roomDir = dir;
    }
}
