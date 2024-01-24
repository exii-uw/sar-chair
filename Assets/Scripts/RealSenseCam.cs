using OrbSLAM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RealSenseCam : MonoBehaviour
{
    // Controller
    Controller controller;

    Thread camThread;
    bool threadRunning;

    // Live textures
    public Texture2D colorTexture;
    public Texture2D depthTexture;
    public Texture2D uvMapTexture;

    public byte[] colorRaw;
    public byte[] depthRaw;
    public byte[] uvRaw;
    public UInt16[] depthArrayUint16Raw;

    public Matrix4x4 camPose;
    private Vector3 m_cameraPosition;
    public Vector3 m_eulerAngles;

    public struct Intrinsics
    {
        public float fx;
        public float fy;
        public float fovX;
        public float fovY;
        public float ppx;
        public float ppy;
        public float[] coeffs;
    }

    public Intrinsics depthIntrinsics;
    public Intrinsics colorIntrinsics;

    // SLAM starting position
    Vector3 startPosition;
    Vector3 startRight;
    Vector3 startUp;
    Vector3 startForward;

    // One Euro Filter
    OneEuroFilter<Vector3> positionFilter;
    OneEuroFilter<Vector3> rightFilter;
    OneEuroFilter<Vector3> forwardFilter;
    OneEuroFilter<Vector3> upFilter;
    public bool useFilter = false;
    public float filterFrequency = 60.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;


    private void OnApplicationQuit()
    {
        //Arduino arduino = GetComponent<Arduino>();
        //arduino.ResetServoPosition();
        
        // shutdown the camera
        ShutdownCamera();

        // save the old camera position to a file
        if (controller.saveLastPosition)
            SaveCameraPosition("Assets/ChairPosition/lastPosition.csv");

    }

    void Start()
    {
        if (controller == null)
            controller = gameObject.GetComponent<Controller>();

        // load in the prev camera position
        if (controller.usePrevPosition)
            LoadCameraPosition("Assets/ChairPosition/lastPosition.csv");
        //else
        //{
        //    startPosition = gameObject.transform.position;
        //    //controller.transform.eulerAngles = new Vector3(0, 97.44f, 0);
        //    startForward = gameObject.transform.forward;
        //    startRight = gameObject.transform.right;
        //    startUp = gameObject.transform.up;

        //    print(gameObject.transform.position.ToString());
        //}


        // Initialize textures and intrinsics
        InitializeTextures();
        InitializeIntrinsics();

        // don't use the camera - quit after initialization
        if (!controller.useServer)
        {
            print("Not using the server...");
            return;
        }

        print("Starting RealSense Camera...");

        camThread = new Thread(StartSLAM);

        camThread.IsBackground = true;
        camThread.Start();
        threadRunning = true;

        // initialize the one euro filter for position and rotation
        positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
        rightFilter = new OneEuroFilter<Vector3>(filterFrequency);
        upFilter = new OneEuroFilter<Vector3>(filterFrequency);
        forwardFilter = new OneEuroFilter<Vector3>(filterFrequency);
    }
    private void InitializeTextures()
    {
        colorTexture = new Texture2D(SLAMInterfaceRealSense.C_IMAGE_WIDTH,
           SLAMInterfaceRealSense.C_IMAGE_HEIGHT, TextureFormat.RGB24, false);
        colorTexture.wrapMode = TextureWrapMode.Clamp;

        depthTexture = new Texture2D(SLAMInterfaceRealSense.D_IMAGE_WIDTH,
            SLAMInterfaceRealSense.D_IMAGE_HEIGHT, TextureFormat.R16, false);
        depthTexture.filterMode = FilterMode.Point;

        uvMapTexture = new Texture2D(SLAMInterfaceRealSense.D_IMAGE_WIDTH,
           SLAMInterfaceRealSense.D_IMAGE_HEIGHT, TextureFormat.RGFloat, false, true);
        uvMapTexture.wrapMode = TextureWrapMode.Clamp;
        uvMapTexture.filterMode = FilterMode.Point;

        int bufferSize = SLAMInterfaceRealSense.D_IMAGE_WIDTH * SLAMInterfaceRealSense.D_IMAGE_HEIGHT;
        depthArrayUint16Raw = new UInt16[bufferSize];
    }
    private void InitializeIntrinsics()
    {
        // Intrinsics (normal)
        if (!controller.usePi)
        {
            depthIntrinsics.fx = 385.715f;
            depthIntrinsics.fy = 385.715f;
            depthIntrinsics.fovX = 79.3531f;
            depthIntrinsics.fovY = 63.7756f;
            depthIntrinsics.ppx = 313.954f;
            depthIntrinsics.ppy = 244.336f;
            depthIntrinsics.coeffs = new float[5];

            colorIntrinsics.fx = 608.349f;
            colorIntrinsics.fy = 608.442f;
            colorIntrinsics.fovX = 55.4838f;
            colorIntrinsics.fovY = 43.0438f;
            colorIntrinsics.ppx = 327.377f;
            colorIntrinsics.ppy = 249.829f;
            colorIntrinsics.coeffs = new float[5];
        }

        else
        {

            // Intrinsics (small)
            colorIntrinsics.fovX = 69.7437f;
            colorIntrinsics.fovY = 43.0428f;
            colorIntrinsics.fx = 304.175f;
            colorIntrinsics.fy = 304.221f;
            colorIntrinsics.ppx = 215.689f;
            colorIntrinsics.ppy = 124.914f;
            colorIntrinsics.coeffs = new float[5];

            depthIntrinsics.fovX = 89.7393f;
            depthIntrinsics.fovY = 58.4909f;
            depthIntrinsics.fx = 241.072f;
            depthIntrinsics.fy = 241.072f;
            depthIntrinsics.ppx = 236.221f;
            depthIntrinsics.ppy = 137.71f;
            depthIntrinsics.coeffs = new float[5];
        }
    }

    private void StartSLAM()
    {
        string yamlFile;
        string address;
        if (controller.usePi)
        {
            yamlFile = "C:/Users/nvjoshi/SAR-Chair/ORB_SLAM_UNITY_SERVER/Vocabulary/RealSenseSmall.yaml";
            address = "192.168.1.166:5023";
        }
        else
        {
            yamlFile = "C:/Users/nvjoshi/SAR-Chair/ORB_SLAM_UNITY_SERVER/Vocabulary/RealSense.yaml";
            address = "127.0.0.1:8080";
        }
        
        
        SLAMInterfaceRealSense.StartSystem("C:/Users/nvjoshi/SAR-Chair/ORB_SLAM_UNITY_SERVER/Vocabulary/small_voc.yml",
        yamlFile, controller.showSLAMVisualization, address, controller.useArduino, controller.useServer, controller.useSLAM);
    }

    public void LoadImageData()
    {
        if (GameObject.Find("Room").GetComponent<Room>().currentMode != Room.Mode.Run)
        {

            // Get the raw color and depth data
            colorRaw = SLAMInterfaceRealSense.GetCameraColorRaw();
            depthRaw = SLAMInterfaceRealSense.GetCameraDepthRaw();
            uvRaw = SLAMInterfaceRealSense.GetCameraUVMap();

            // Load them into the textures
            colorTexture.LoadRawTextureData(colorRaw);
            colorTexture.Apply();

            depthTexture.LoadRawTextureData(depthRaw);
            depthTexture.Apply();

            uvMapTexture.LoadRawTextureData(uvRaw);
            uvMapTexture.Apply();

            // DEBUG: update the debug quads to show the images in the scene
            if (SceneManager.GetActiveScene().name.Equals("RealSenseTest"))
            {
                GameObject.Find("Colour").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", colorTexture);
                GameObject.Find("Depth").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", depthTexture);
                GameObject.Find("UV").GetComponent<MeshRenderer>().material.SetTexture("_MainTex", uvMapTexture);
            }

            // Convert raw depth to Uint16 
            int bufferSize = SLAMInterfaceRealSense.D_IMAGE_WIDTH * SLAMInterfaceRealSense.D_IMAGE_HEIGHT;
            Buffer.BlockCopy(depthRaw, 0, depthArrayUint16Raw, 0, bufferSize);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (SLAMInterfaceRealSense.CheckIfSystemIsInReadyState() && controller.useServer)
        {
            //LoadImageData();

            if (controller.useSLAM)
            {
                camPose = SLAMInterfaceRealSense.GetCameraPose();

                Vector3 right = camPose.GetColumn(0);
                Vector3 up = camPose.GetColumn(1);
                Vector3 forward = camPose.GetColumn(2);
                forward.y *= -1; // flip because ORB-SLAM flips the y axis
                Vector3 position = camPose.GetColumn(3);
                position.y *= -1; // flip because ORB-SLAM flips the y axis

                // if the position is 0, tracking was lost
                if (!position.Equals(Vector3.zero))
                {

                    // Update the camera pose - using this approach because setting the rotation does
                    // not work and I don't want to mess with Quaternions right now.

                    Vector3 noisyPosition;
                    Vector3 noisyRight;
                    Vector3 noisyUp;
                    Vector3 noisyForward;

                    // if using the tripod (images are not flipped)
                    if (controller.useTripod)
                    {
                        noisyRight = startRight + right;
                        noisyUp = startUp + up;
                        noisyForward = startForward + forward;
                        noisyPosition = startPosition + position;

                    }

                    // camera is on the mount and the position should be rotated
                    else
                    {
                        // rotate the position by -90 degrees
                        Quaternion rotation = Quaternion.Euler(0, 0, -90); // change to -90 when not using rotated tripod
                        noisyRight = startRight + (rotation * right);
                        noisyUp = startUp + (rotation * up);
                        noisyForward = startForward + (rotation * forward);
                        noisyPosition = startPosition + (rotation * position);
                    }

                    // apply filters
                    if (useFilter)
                    {
                        positionFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
                        rightFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
                        upFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
                        forwardFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);

                        gameObject.transform.position = positionFilter.Filter(noisyPosition);
                        gameObject.transform.right = rightFilter.Filter(noisyRight);
                        gameObject.transform.up = upFilter.Filter(noisyUp);
                        gameObject.transform.forward = forwardFilter.Filter(noisyForward);

                        //gameObject.transform.right = noisyRight;
                        //gameObject.transform.up = noisyUp;
                        //gameObject.transform.forward = noisyForward;
                    }

                    else
                    {
                        gameObject.transform.position = noisyPosition;
                        gameObject.transform.right = noisyRight;
                        gameObject.transform.up = noisyUp;
                        gameObject.transform.forward = noisyForward;
                    }

                    // random error causes the chair to move in odd angles/positions, lock those in place
                    Vector3 currPosition = gameObject.transform.position;
                    gameObject.transform.position = new Vector3(currPosition.x, 0, currPosition.z);

                    Vector3 currRotation = gameObject.transform.eulerAngles;
                    gameObject.transform.eulerAngles = new Vector3(0, currRotation.y, currRotation.z);
                    // need to rotate by 45 if at the workbench, not sure why
                    //gameObject.transform.eulerAngles = new Vector3(0, currRotation.y + 45, currRotation.z);



                    m_eulerAngles = gameObject.transform.rotation.eulerAngles;
                    m_cameraPosition = gameObject.transform.position;

                }
            }
        }
    }

    public Vector3 GetCameraPosition()
    {
        return m_cameraPosition;
    }

    public Vector3 GetCameraEulerAngles()
    {
        return m_eulerAngles;
    }

    // Stop the camera loop
    private void ShutdownCamera()
    {
        // not using the camera - quit
        if (!controller.useServer)
            return;

        camThread.Interrupt();
        if (threadRunning)
        {
            threadRunning = false;

            print("Shutting down the system...");
            SLAMInterfaceRealSense.StopSystem();

            camThread.Abort();
        }

    }

    // Save the camera's position
    private void SaveCameraPosition(string path)
    {
        StreamWriter writer = new StreamWriter(path, false);
        Vector3 currPosition = gameObject.transform.position;
        Vector3 currRight = gameObject.transform.right;
        Vector3 currUp = gameObject.transform.up;
        Vector3 currForward = gameObject.transform.forward;

        string header = "tX,tY,tZ,rX,rY,rZ,uX,uY,uZ,fX,fY,fZ";
        string currTransform = currPosition.x + "," + currPosition.y + "," + currPosition.z + "," +
            currRight.x + "," + currRight.y + "," + currRight.z + "," +
            currUp.x + "," + currUp.y + "," + currUp.z + "," +
            currForward.x + "," + currForward.y + "," + currForward.z;
        writer.WriteLine(header);
        writer.WriteLine(currTransform);
        writer.Close();
    }

    // Load the camera's position from a file
    private void LoadCameraPosition(string path)
    {
        try
        {
            StreamReader reader = new StreamReader(path);
            reader.ReadLine(); // read header
            string[] transformString = reader.ReadLine().Split(',');
            float[] transform = new float[transformString.Length];
            for (int i = 0; i < transformString.Length; i++)
            {
                transform[i] = float.Parse(transformString[i]);
            }

            startPosition = new Vector3(transform[0], transform[1], transform[2]);
            startRight = new Vector3(transform[3], transform[4], transform[5]);
            startUp = new Vector3(transform[6], transform[7], transform[8]);
            startForward = new Vector3(transform[9], transform[10], transform[11]);
          

            print("Loaded in a previous camera position from " + path);
        }
        
        catch (FileNotFoundException e)
        {
            startPosition = Vector3.zero;
            startRight = Vector3.zero;
            startUp = Vector3.zero;
            startForward = Vector3.zero;
            
            print("Could not load the camera's position from a file");
        }

        // set the camera position
        gameObject.transform.position = startPosition;
        gameObject.transform.right = startRight;
        gameObject.transform.up = startUp;
        gameObject.transform.forward = startForward;
    }
}
