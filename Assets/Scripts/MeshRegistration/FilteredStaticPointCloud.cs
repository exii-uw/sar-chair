using OrbSLAM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using utils;

namespace FiltedPointCloudCallbacks
{
    public delegate void SnapShotCallback();
}

class FilteredStaticPointCloud : MonoBehaviour
{
    

    [Range(1, 100)]
    public int SampleFrameAmount = 50;

    [Range(0, 1)]
    public float StandardDeviationTolerance = 0.3f;

    [Range(0, 1)]
    public float DepthContinuityTolerance = 0.1f;

    Vector3[] vertices;


    //int width = SLAMInterfaceRealSense.IMAGE_WIDTH;
    //int height = SLAMInterfaceRealSense.IMAGE_HEIGHT;

    RealSenseCam camLoop;
    RealSenseCam.Intrinsics depthIntrinsics;
    Mesh mesh;
    UInt16[] depthArrayUint16Raw;

    int SamplesObtained = 0;
    float[] depthMean; // mean meters
    float[] depthSum;
    float[] depthSumSqr;
    float[] depthVariance; // variance


    Texture2D colorTex;
    Texture2D uvMap;

    byte[] colorRaw;
    byte[] uvRaw;

    // Late Update
    AutoResetEvent processMesh = new AutoResetEvent(false);
    AutoResetEvent collectMeshSamples = new AutoResetEvent(false);


    // Callback for snapshot completion
    FiltedPointCloudCallbacks.SnapShotCallback m_snapShotCallback;

    Controller controller;
    // Start is called before the first frame update
    void Start()
    { }

    // Update is called once per frame
    void Update()
    {
        if (controller == null)
            controller = GameObject.Find("Controller").GetComponent<Controller>();

        // Collect 
        if (collectMeshSamples.WaitOne(0))
        {
            if (camLoop == null)
            {
                camLoop = GameObject.Find("Controller").GetComponent<RealSenseCam>();
                depthIntrinsics = camLoop.depthIntrinsics;

                // Convert raw depth to Uint16 
                int bufferSize = SLAMInterfaceRealSense.D_IMAGE_WIDTH * SLAMInterfaceRealSense.D_IMAGE_HEIGHT;
                depthArrayUint16Raw = new UInt16[bufferSize];
                depthMean = new float[bufferSize];
                depthSum = new float[bufferSize];
                depthSumSqr = new float[bufferSize];
                depthVariance = new float[bufferSize];

                Array.Clear(depthArrayUint16Raw, 0, bufferSize);
                Array.Clear(depthMean, 0, bufferSize);
                Array.Clear(depthSum, 0, bufferSize);
                Array.Clear(depthSumSqr, 0, bufferSize);
                Array.Clear(depthVariance, 0, bufferSize);

                Buffer.BlockCopy(camLoop.depthArrayUint16Raw, 0, depthArrayUint16Raw, 0, bufferSize);
            }

            if (SamplesObtained < SampleFrameAmount)
            {
                Debug.Log("Sample Obtained: " + SamplesObtained.ToString());

                // instead of getting images every frame, only get it when needed
                camLoop.LoadImageData();

                for (int i = 0; i < camLoop.depthArrayUint16Raw.Length; ++i)
                {
                    float val = camLoop.depthArrayUint16Raw[i] / 1000.0f;
                    depthSum[i] += val;
                    depthSumSqr[i] += (val * val);
                }


                SamplesObtained++;
                collectMeshSamples.Set();
            }
            else
            {
                processMesh.Set();
            }
        }


        if (processMesh.WaitOne(0))
        {
            // Calculate mean and variance
            for (int i = 0; i < depthMean.Length; ++i)
            {
                depthMean[i] = (depthSum[i] / SampleFrameAmount);
                depthVariance[i] = ((depthSumSqr[i] / (float) SampleFrameAmount) - Mathf.Pow(depthSum[i] / (float) SampleFrameAmount, 2));
            }


            // Custom shader
            gameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/DynamicMesh"));
            gameObject.GetComponent<MeshCollider>().enabled = true;

            // Update game object based on current position of camera
            var euler = camLoop.GetCameraEulerAngles();
            var pos = camLoop.GetCameraPosition();

            gameObject.transform.position = pos;
            gameObject.transform.eulerAngles = euler;

            CopyDataFromCamera();
            GenerateMesh();

            if (m_snapShotCallback != null)
                m_snapShotCallback();

            m_snapShotCallback = null;
        }

    }

    public void InitializeMesh(FiltedPointCloudCallbacks.SnapShotCallback callback = null)
    {
        m_snapShotCallback = callback;
        collectMeshSamples.Set();
    }


    // Make the mesh static
    private void CopyDataFromCamera()
    {
        // Copying the textures
        colorTex = new Texture2D(SLAMInterfaceRealSense.C_IMAGE_WIDTH, SLAMInterfaceRealSense.C_IMAGE_HEIGHT, TextureFormat.RGB24, false);
        colorTex.wrapMode = TextureWrapMode.Clamp;

        uvMap = new Texture2D(SLAMInterfaceRealSense.D_IMAGE_WIDTH, SLAMInterfaceRealSense.D_IMAGE_HEIGHT, TextureFormat.RGFloat, false, true);
        uvMap.wrapMode = TextureWrapMode.Clamp;
        uvMap.filterMode = FilterMode.Point;

        uvRaw = new byte[camLoop.uvRaw.Length];
        colorRaw = new byte[camLoop.colorRaw.Length];



        Array.Copy(uvRaw, 0, camLoop.uvRaw, 0, camLoop.uvRaw.Length);
        Array.Copy(colorRaw, 0, camLoop.colorRaw, 0, camLoop.colorRaw.Length);

        Graphics.CopyTexture(camLoop.colorTexture, colorTex);
        Graphics.CopyTexture(camLoop.uvMapTexture, uvMap);

        gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", colorTex);
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", uvMap);

    }

    // Generate a single mesh
    private void GenerateMesh()
    {
        int width = SLAMInterfaceRealSense.D_IMAGE_WIDTH;
        int height = SLAMInterfaceRealSense.D_IMAGE_HEIGHT;

        mesh = new Mesh();
        mesh.name = "Procedural Mesh";
        mesh.MarkDynamic();
        mesh.indexFormat = IndexFormat.UInt32;
        vertices = new Vector3[width * height];

        int depthPointsThrownAway = 0;
        int index = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int depthIndex = i * width + j;
                float depth = depthArrayUint16Raw[depthIndex] / 1000.0f; // Real Sense Units are in mm
                float mean = depthMean[depthIndex];
                float variance = depthVariance[depthIndex];

                if (Math.Sqrt(variance) > StandardDeviationTolerance)
                {
                    vertices[index] = Vector3.zero;
                    depthPointsThrownAway++;
                }
                else
                {
                    Vector3 vertex = DepthToCamera(j, i, mean);
                    if (controller.useSLAM)
                    {
                        vertex.y = -vertex.y;
                    }

                    vertices[index] = vertex;
                }

                index++;
            }
        }
        Debug.Log("Depth Exceeding Std Tol: " + depthPointsThrownAway.ToString());

        // Edit indices
        var indices = new int[vertices.Length * 6 - 6 * width];
        
        index = 0;
        for (int i = 0; i < (vertices.Length - width - 1) / 1; i += 1)
        {

            if (i % width - 1 == 0) continue;

            var v1 = i;
            var v2 = i + width;
            var v3 = i + width + 1;
            var v4 = i + 1;

            var vec1 = vertices[v1];
            var vec2 = vertices[v2];
            var vec3 = vertices[v3];
            var vec4 = vertices[v4];

            if (vec1 == Vector3.zero ||
                vec2 == Vector3.zero ||
                vec3 == Vector3.zero ||
                vec4 == Vector3.zero)
            {
                continue;
            }


            var d1 = vertices[v1].magnitude;
            var d2 = vertices[v2].magnitude;
            var d3 = vertices[v3].magnitude;
            var d4 = vertices[v4].magnitude;


            if (Math.Abs(d1 - d2) > DepthContinuityTolerance ||
                Math.Abs(d1 - d3) > DepthContinuityTolerance ||
                Math.Abs(d1 - d4) > DepthContinuityTolerance ||
                Math.Abs(d2 - d3) > DepthContinuityTolerance ||
                Math.Abs(d2 - d4) > DepthContinuityTolerance ||
                Math.Abs(d3 - d4) > DepthContinuityTolerance)
            {
                continue;
            }

            indices[index++] = v1;
            indices[index++] = v3;
            indices[index++] = v2;
            indices[index++] = v1;
            indices[index++] = v4;
            indices[index++] = v3;
        }

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;

        mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
        mesh.UploadMeshData(false);
        mesh.RecalculateBounds();
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;


        // rotate by -90 if on the mount
        if (!controller.useTripod)
        {
            gameObject.transform.Rotate(0, 0, -90f);
        }
    }

    // Depth to cam space
    private Vector3 DepthToCamera(int xIndex, int yIndex, float depth)
    {
        float x = (xIndex - depthIntrinsics.ppx) / depthIntrinsics.fx;
        float y = (yIndex - depthIntrinsics.ppy) / depthIntrinsics.fy;

        float r2 = x * x + y * y;
        float f = 1 + depthIntrinsics.coeffs[0] * r2 + depthIntrinsics.coeffs[1] * r2 * r2 + depthIntrinsics.coeffs[4] * r2 * r2 * r2;
        float ux = x * f + 2 * depthIntrinsics.coeffs[2] * x * y + depthIntrinsics.coeffs[3] * (r2 + 2 * x * x);
        float uy = y * f + 2 * depthIntrinsics.coeffs[3] * x * y + depthIntrinsics.coeffs[2] * (r2 + 2 * y * y);

        x = ux;
        y = uy;

        return new Vector3(x * depth, y * depth, depth);
    }



}
