using OrbSLAM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    // Address for the server
    private string address = "127.0.0.1:8080"; // localhost

    //public string address = "192.168.1.166:5023"; // using the Linksys router/Pi

    // whether the normal tripod orientation should be used (DEBUG)
    public bool useTripod = false;

    // Whether the server should be used
    public bool useServer = false;

    // whether the Pi should be used
    public bool usePi = false;

    // Whether the Arduino should be used
    public bool useArduino = false;

    // Whether SLAM should be used
    public bool useSLAM = false;

    // Whether the SLAM visualization should be used
    public bool showSLAMVisualization = true;

    // Whether the previous SLAM positions should be used
    public bool usePrevPosition = true;

    public bool saveLastPosition = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!useServer)
        {
            useSLAM = false;
            useArduino = false;
        }

    }

}
