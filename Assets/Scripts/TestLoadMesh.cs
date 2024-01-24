using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class TestLoadMesh : MonoBehaviour
{
    int width = 640;
    int height = 480;

    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (mesh != null)
        //    return;

        //gameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/DynamicMesh"));

        //GameObject saved = GameObject.Find("Saved");
        //Mesh savedMesh = saved.GetComponent<MeshFilter>().mesh;
        //Vector3[] vertices = savedMesh.vertices;

        //mesh = new Mesh();
        //mesh.name = "Procedural Mesh - Loaded";

        //mesh.indexFormat = IndexFormat.UInt32;

        //mesh.vertices = vertices;
        //mesh.uv = savedMesh.uv;

        //mesh.SetIndices(savedMesh.GetIndices(0), MeshTopology.Triangles, 0, false);
        //mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        //gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        Texture2D uvMap = new Texture2D(640,
           480, TextureFormat.RGFloat, false, true);
        byte[] uv = File.ReadAllBytes("Assets/SavedRooms/1581643410/PC_1_uv.png");
        uvMap.LoadImage(uv);
        print(uvMap.width);
        gameObject.GetComponent<MeshRenderer>().material.SetTexture("_UVMap", uvMap);

        Texture2D cMap = new Texture2D(width, height, TextureFormat.RGB24, false);
        byte[] c = File.ReadAllBytes("Assets/SavedRooms/1581643410/PC_1_color.png");
        cMap.LoadImage(c);
        gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", cMap);
        //gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture()
    }
}
