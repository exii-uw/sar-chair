using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using utils;

public class Room : MonoBehaviour
{
    [System.Serializable]
    public enum Mode
    {
        Debug,
        Calibrate,
        Run,
        Refine
    }

    public Mode currentMode;
    int numChildren = 0;
    private float nextUpdate = 20f;
    private float period = 20f;

    public string roomName = "My Room";
    public string roomDirectory;
    public bool saveRoomOnQuit = false;


    // Room Generation Point Cloud Prefab
    public GameObject FilteredMeshPrefab;

    // Sounds for Calibration feedback
    AudioSource done;
    AudioSource starting;
    AudioSource[] audio;
    bool playStarting = false;
    private float nextWarning = 15f;
    private float warningPeriod = 15f;

    // Aruco
    GameObject camPositionAruco;
    public GameObject reference;
    Vector3 diffPosition;
    Vector3 diffRotation;

    private void OnApplicationQuit()
    {
        if (currentMode == Mode.Refine)
        {
            SaveRefinement("Assets/SavedRooms/" + roomName + "/refinement.csv");
        }
    }

    // Save the room refinement
    private void SaveRefinement(string path)
    {
        StreamWriter writer = new StreamWriter(path, false);
        string header = "tX,tY,tZ,rX,rY,rZ";
        string currTransform = diffPosition.x + "," + diffPosition.y + "," + diffPosition.z + "," +
            diffRotation.x + "," + diffRotation.y + "," + diffRotation.z;
        writer.WriteLine(header);
        writer.WriteLine(currTransform);
        writer.Close();
    }

    public void RefineRoom(string path)
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

            Vector3 dPos = new Vector3(transform[0], transform[1], transform[2]);
            Vector3 dRot = new Vector3(transform[3], transform[4], transform[5]);

            reference.transform.position += dPos;
            reference.transform.Rotate(dRot);

            print("Applied refinement from file " + path);
        }

        catch (FileNotFoundException e)
        {
            print("No refinement found!");
        }
    }

    void Start()
    {
        audio = gameObject.GetComponents<AudioSource>();
        done = audio[0];
        starting = audio[1];
        camPositionAruco = transform.Find("CamPositionAruco").gameObject;
        try { 
            reference = transform.Find("Reference").gameObject;
        } catch
        {
            print("No reference has been created as yet.");
        }
    }

    // Update is called once per frame
    void Update()
    {

        // if calibrating / creating a new room, automatically create new meshes
        if (currentMode == Mode.Calibrate)
        {
            // WARNING that a mesh is about to be created
            saveRoomOnQuit = true;
            if (Time.time > nextWarning)
            {
                nextWarning += warningPeriod;
                print("Warning message...");
                starting.Play();
            }

            // Mesh has actually been created
            else if (Time.time > nextUpdate)
            {
                nextUpdate += period;
                playStarting = true;
                if (numChildren == 0)
                    CreateReference();
                CreateNewMesh("PC_" + ++numChildren);
                done.Play();
                print("Done making a mesh!");
                
            }
        }

        // Debug mode - only create meshes on keypress
        else if (currentMode == Mode.Debug)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (numChildren == 0)
                    CreateReference();

                CreateNewMesh("PC_" + ++numChildren);
            }
        }

        // Refine mode - only calculate the difference
        else if (currentMode == Mode.Refine)
        {
            diffPosition = camPositionAruco.transform.position - reference.transform.position;
            diffRotation = camPositionAruco.transform.eulerAngles - reference.transform.eulerAngles;
        }
    }

    private void CreateReference()
    {
        reference = GameObject.CreatePrimitive(PrimitiveType.Cube);
        reference.name = "Reference";
        reference.transform.rotation = camPositionAruco.transform.rotation;
        reference.transform.position = camPositionAruco.transform.position;
        reference.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        reference.GetComponent<Renderer>().material.color = Color.red;
        //GameObject.Find("Room").transform.parent = reference.transform;
    }

    // Create a static mesh
    private void CreateNewMesh(string name)
    {
        GameObject pointCloud = GameObject.Instantiate(FilteredMeshPrefab);
        pointCloud.name = name;
        pointCloud.transform.parent = gameObject.transform;

        pointCloud.GetComponent<FilteredStaticPointCloud>().InitializeMesh();
    }
    
    // Delete all children
    public void ResetRoom()
    {
        numChildren = 0;
        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            if (!child.name.Equals("CamPositionAruco"))
                DestroyImmediate(child.gameObject);
        }
        DestroyImmediate(reference);
        print("Room Cleared!");
    }

    // Delete the latest child
    public void DeleteRecent()
    {
        print("Deleting recent...");
        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            print(child.name);
            if (child.name.Equals("PC_" + (numChildren.ToString())))
            {
                DestroyImmediate(child.gameObject);
                numChildren--;
                print("Deleted child");
            }

        }

        if (numChildren == 0)
            DestroyImmediate(reference);

    }
}
