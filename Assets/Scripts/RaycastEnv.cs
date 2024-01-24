using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastEnv : MonoBehaviour
{
    Arduino arduino;
    
    // servos
    GameObject bottomServo;
    GameObject topServo;

    public bool incompatibleBottom = false;
    public bool incompatibleTop = true;

    // Update is called once per frame
    void Update()
    {

        if (bottomServo == null)
            bottomServo = GameObject.Find("BottomServoRot");
        if (topServo == null)
            topServo = GameObject.Find("TopServoRot");
        if (arduino == null)
            arduino = GameObject.Find("Controller").GetComponent<Arduino>();

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.green);

        GameObject activeDemo;
        
        // Find a triggered state-based demo
        GameObject triggeredStateDemo = FindStateDemos();

        // Find the closest location-based demo
        GameObject closestDemo = FindClosestDemoROI(transform.TransformDirection(Vector3.forward) * 1000);

        // if the state-based demo isn't null, prioritize it over location-based
        if (triggeredStateDemo != null)
            activeDemo = triggeredStateDemo;
        else
            activeDemo = closestDemo;

        // if there are no demos, do nothing
        if (activeDemo != null)
        {
            //if (activeDemo.GetComponent<DemoManager>().onlyScreen)
                UnTriggerDemos(activeDemo);
            MoveProjectors(activeDemo);
        }
    }

    // un-trigger all of the other demos
    private void UnTriggerDemos(GameObject currentDemo)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Demo");
        foreach (GameObject go in gos)
        {
            DemoManager dManager = go.GetComponent<DemoManager>();
            if (go.Equals(currentDemo))
            {
                dManager.triggered = true;
            }

            else
            {
                dManager.triggered = false;
            }
        }
    }

    // Move the virtual servo models to look at the specified demo
    public void MoveProjectors(GameObject demo)
    {
        // move the bottom servo
        Vector3 prevBottom = bottomServo.transform.localEulerAngles;
        Debug.DrawRay(bottomServo.transform.position, demo.transform.position * 1000, Color.red);
        bottomServo.transform.LookAt(demo.transform, Vector3.up);
        float sideRot = -19.584f;
        bottomServo.transform.localRotation = Quaternion.Euler(0, bottomServo.transform.localEulerAngles.y - sideRot, 0);
        
        // mapping the Unity angles to Arduino angles
        float bottomRotation = arduino.neutralBottom - utils.Utils.ConvertEuler(bottomServo.transform.localEulerAngles.y);

        // verify that the bottom servo values are compatible with the servo range
        if (bottomRotation < arduino.minBottom)
        {
            print("Bottom rotation " + bottomRotation + " is less than min.");
            bottomRotation = arduino.minBottom;
            bottomServo.transform.localRotation = Quaternion.Euler(prevBottom);
            incompatibleBottom = true;
        }
        else if (bottomRotation > arduino.maxBottom)
        {
            print("Bottom rotation " + bottomRotation + " is greater than max.");
            bottomRotation = arduino.maxBottom;
            bottomServo.transform.localRotation = Quaternion.Euler(prevBottom);
            incompatibleBottom = true;
        }

        else
        {
            incompatibleBottom = false;
        }
        arduino.bottomServoAngle = (int)bottomRotation;


        // move the top servo
        Vector3 prevTop = topServo.transform.localEulerAngles;
        Debug.DrawRay(topServo.transform.position, demo.transform.position * 1000, Color.red);
        topServo.transform.LookAt(demo.transform, Vector3.up);
        float topRot = -17.785f;
        topServo.transform.localRotation = Quaternion.Euler(topServo.transform.localEulerAngles.x, topRot, 270f);
        
        // mapping the Unity angles to Arduino angles
        float topRotation = arduino.neutralTop + utils.Utils.ConvertEuler(topServo.transform.localEulerAngles.x);

        // verify that the top servo values are compatible with the servo range
        if (topRotation < arduino.minTop)
        {
            print("Top rotation " + topRotation + " is less than min.");
            topRotation = arduino.minTop;
            topServo.transform.localRotation = Quaternion.Euler(prevTop);
            incompatibleTop = true;
        }
        else if (topRotation > arduino.maxTop)
        {
            print("Top rotation " + topRotation + " is greater than max.");
            topRotation = arduino.maxTop;
            topServo.transform.localRotation = Quaternion.Euler(prevTop);
            incompatibleTop = true;
        }
        else
        {
            incompatibleTop = false;
        }

        // send the top and bottom servo values to the Arduino
        arduino.topServoAngle = (int)topRotation;
        
    }

    // Find state-based demos have been triggered
    public GameObject FindStateDemos()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Demo");
        List<GameObject> triggered = new List<GameObject>();

        foreach (GameObject go in gos)
        {
            DemoManager demoManager = go.GetComponent<DemoManager>();
            // only want to find state-based demos
            if (!demoManager.isStateBased)
                continue;

            // return the first one for now
            if (demoManager.triggered)
                triggered.Add(go);
        }

        // sort the triggered list so the first triggered demo isn't changing every frame
        if (triggered.Count > 0)
        {
            triggered.Sort(SortByName);
            return triggered[0];
        }

        // no state-based demos have been triggered
        return null;
    }

    private int SortByName(GameObject g1, GameObject g2)
    {
        return g2.name.CompareTo(g1.name);
    }


    // Find the closest demo in the environment
    public GameObject FindClosestDemoROI(Vector3 position)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Demo");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        foreach (GameObject go in gos)
        {
            // only want to find location-based demos
            if (go.GetComponent<DemoManager>().isStateBased)
                continue;

            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}
