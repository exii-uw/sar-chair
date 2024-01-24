using OrbSLAM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTAruco : MonoBehaviour
{

    // One Euro Filter
    OneEuroFilter<Vector3> positionFilter;
    OneEuroFilter<Vector3> rightFilter;
    OneEuroFilter<Vector3> forwardFilter;
    OneEuroFilter<Vector3> upFilter;
    public bool useFilter = true;
    public float filterFrequency = 60.0f;
    public float filterMinCutoff = 1.0f;
    public float filterBeta = 0.0f;
    public float filterDcutoff = 1.0f;

    GameObject reference;
    Vector3 diffPosition;
    Vector3 diffRotation;

    void Start()
    {
        // initialize the one euro filter for position and rotation
        positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
        rightFilter = new OneEuroFilter<Vector3>(filterFrequency);
        upFilter = new OneEuroFilter<Vector3>(filterFrequency);
        forwardFilter = new OneEuroFilter<Vector3>(filterFrequency);
    }

    // Update is called once per frame
    void Update()
    {
        //if (SLAMInterfaceRealSense.CheckIfSystemIsInReadyState())
        //{
        //    // DEBUG Aruco
        //    Matrix4x4 camPose = SLAMInterfaceRealSense.GetArucoPose();
        //    Vector3 right = camPose.GetColumn(0);
        //    Vector3 up = camPose.GetColumn(1);
        //    Vector3 forward = camPose.GetColumn(2);
        //    Vector3 position = camPose.GetRow(3);

        //    forward.x *= -1;

        //    if (useFilter)
        //    {
        //        positionFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        //        rightFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        //        upFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);
        //        forwardFilter.UpdateParams(filterFrequency, filterMinCutoff, filterBeta, filterDcutoff);

        //        //gameObject.transform.position = positionFilter.Filter(position);
        //        gameObject.transform.right = rightFilter.Filter(right);
        //        gameObject.transform.up = upFilter.Filter(up);
        //        gameObject.transform.forward = forwardFilter.Filter(forward);
        //    }
        //    else
        //    {
        //        gameObject.transform.right = right;
        //        gameObject.transform.up = up;
        //        gameObject.transform.forward = forward;
        //        //gameObject.transform.position = position;
        //    }
        //    gameObject.transform.position = position;
        //    //gameObject.GetComponent<Renderer>().material.color = Color.white;
            
            
        //    // // To adjust the position
        //    //if (reference != null)
        //    //{
        //    //    //Collider[] hitColliders = Physics.OverlapBox(reference.transform.position, reference.transform.localScale / 16);
        //    //    //int i = 0;
        //    //    ////Check when there is a new collider coming into contact with the box
        //    //    //while (i < hitColliders.Length)
        //    //    //{
        //    //    //    //Output all of the collider names
        //    //    //    if (hitColliders[i].name.Equals("Reference"))
        //    //    //    {
        //    //    //        i++;
        //    //    //        continue;
        //    //    //    }
        //    //    //    hitColliders[i].GetComponent<Renderer>().material.color = Color.green;
        //    //    //    Debug.Log("Hit : " + hitColliders[i].name + i);
        //    //    //    //Increase the number of Colliders in the array
        //    //    //    i++;
        //    //    //}

        //    //    diffPosition = gameObject.transform.position - reference.transform.position;
        //    //    diffRotation = gameObject.transform.eulerAngles - reference.transform.eulerAngles;
        //    //}
        //}

        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    if (GameObject.Find("Reference") == null)
        //    {
        //        reference = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        reference.name = "Reference";
        //        reference.transform.rotation = gameObject.transform.rotation;
        //        reference.transform.position = gameObject.transform.position;
        //        reference.transform.localScale = gameObject.transform.localScale;
        //        reference.GetComponent<Renderer>().material.color = Color.red;
        //        GameObject.Find("Room").transform.parent = reference.transform;
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    reference.transform.position += diffPosition;
        //    reference.transform.Rotate(diffRotation);
        //}
    }
}
