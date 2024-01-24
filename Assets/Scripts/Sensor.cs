using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    // hold info about the copper tape/FSR's current state
    public bool isActive = false; // whether there is pressure
    public double timestamp = 0; // time of interaction (for touch)
    public float rawValue = 0;

    public void Reset()
    {
        timestamp = 0;
        isActive = false;
    }
}
