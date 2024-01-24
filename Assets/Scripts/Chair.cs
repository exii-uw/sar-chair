using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using utils;

public class Chair : MonoBehaviour
{
    RealSenseCam camLoop;
    Arduino arduinoLoop;
    Controller controller;

    // Chair GameObjects
    public GameObject back;
    public GameObject seat;
    public GameObject lArmRest;
    public GameObject rArmRest;

    // Relevant Mount GameObjects
    public GameObject topServoRotation;
    public GameObject bottomServoRotation;

    // FSR threshold
    const float FSR_THRESHOLD = 300;

    // Touch threshold
    const float TOUCH_THRESHOLD = 1;

    // get the starting back tilt
    float baseTilt = 0;
    public float tilt = -90;


    // acceleration calculations
    float nextUpdate = 0;
    float period = 0.01f;
    Vector3 prevPosition;
    public float acceleration;
    private float prevAcceleration;
    public readonly float accelerationMax = 0.02f; // only move the projector if below the max

    // Update is called once per frame
    void Update()
    {
        if (controller == null)
            controller = GameObject.Find("Controller").GetComponent<Controller>();
        if (camLoop == null)
            camLoop = GameObject.Find("Controller").GetComponent<RealSenseCam>();
        if (prevPosition == null)
            prevPosition = gameObject.transform.position;
        

        // Calibrating - don't need to access the chair's visualization
        if (GameObject.Find("Room").GetComponent<Room>().currentMode != Room.Mode.Calibrate)
        {
            if (arduinoLoop == null)
            {
                arduinoLoop = GameObject.Find("Controller").GetComponent<Arduino>();
                
                //while (!arduinoLoop.CheckServoState());
            }

            if (back == null)
                back = GameObject.Find("Back");

            if (seat == null)
                seat = GameObject.Find("Seat");

            if (lArmRest == null)
                lArmRest = GameObject.Find("LeftArm");

            if (rArmRest == null)
                rArmRest = GameObject.Find("RightArm");

            if (topServoRotation == null)
                topServoRotation = GameObject.Find("TopServoRot");

            if (bottomServoRotation == null)
                bottomServoRotation = GameObject.Find("BottomServoRot");


            // Update the visualization
            if (controller.useArduino)
            {
                UpdateFSRs();
                UpdateTouchInput(); // TODO: fix touch input detection
                UpdateBackTilt();
                //arduinoLoop.CheckServoState();
            }

        }

        arduinoLoop.CheckServoState();
        //CheckAcceleration();

    }

    // Check the chair's acceleration every second - if the acceleration is less than max, move
    private void CheckAcceleration()
    {
        // calculate the acceleration every second
        if (Time.time > nextUpdate)
        {
            nextUpdate += period;
            Vector3 currPosition = gameObject.transform.position;
            Vector3 diff = currPosition - prevPosition;
            acceleration = diff.magnitude / period;
            prevPosition = currPosition;

            //arduinoLoop.CheckServoState();

            if (acceleration < accelerationMax && prevAcceleration >= accelerationMax)
            {
                //print(acceleration);
                if (controller.useArduino)
                    arduinoLoop.CheckServoState();
            }

            prevAcceleration = acceleration;
        }
    }

    // Update the virtual servo model using the Arduino values
    private void UpdateServoPosition()
    {
        print("Updating the servo pos...");
        if (arduinoLoop == null) return;
        float topServoAngle = arduinoLoop.topServoAngle;
        float bottomServoAngle = arduinoLoop.bottomServoAngle;

        // rotate the top and bottom servos
        topServoRotation.transform.localRotation = Quaternion.Euler(topServoAngle, -17.785f, 270f);
        bottomServoRotation.transform.localRotation = Quaternion.Euler(0, bottomServoAngle, 0);
    }

    // Update the chair's position based on the camera's pos
    private void UpdateObjectTransform()
    {
        if (camLoop == null) return;
        transform.eulerAngles = camLoop.GetCameraEulerAngles();
        transform.position = camLoop.GetCameraPosition(); ;
    }

    // Update the back tilt based on the IMU values
    private void UpdateBackTilt()
    {
        if (arduinoLoop == null) return;
        float rawTilt = arduinoLoop.GetBackTilt();
        //print(rawTilt);
        if (rawTilt < 150) return; // haven't received the data from the IMU as yet

        // starting angle (~172) doesn't match the unity start angles (-90)
        if (baseTilt == 0)
        {
            baseTilt = rawTilt;
            return;
        }

        float tiltDiff = baseTilt - rawTilt;
        tilt = -90 - tiltDiff;
        back.transform.localEulerAngles = new Vector3(tilt, 0, 0);
    }

    // Visualize the FSRs
    private void UpdateFSRs()
    {
        if (arduinoLoop == null) return;

        float[] lBackFSR = arduinoLoop.GetLowerBackValues();
        ChangeAllObjectColour(lBackFSR, back, "LowerBackFSRs", FSR_THRESHOLD);

        float[] uBackFSR = arduinoLoop.GetUpperBackValues();
        ChangeAllObjectColour(uBackFSR, back, "UpperBackFSRs", FSR_THRESHOLD);

        float[] bottomFSR = arduinoLoop.GetSeatValues();
        ChangeAllObjectColour(bottomFSR, seat, "SeatFSRs", FSR_THRESHOLD);

        float[] lArmFSR = { arduinoLoop.GetLeftValue() };
        ChangeAllObjectColour(lArmFSR, lArmRest, "FSR", FSR_THRESHOLD);

        float[] rArmFSR = { arduinoLoop.GetRightValue() };
        ChangeAllObjectColour(rArmFSR, rArmRest, "FSR", FSR_THRESHOLD);
    }

    // Visualize the touch input values
    private void UpdateTouchInput()
    {
        if (arduinoLoop == null) return;
        float[] lTouchInput = arduinoLoop.GetLeftTouch();
        ChangeAllObjectColour(lTouchInput, lArmRest, "TouchInput", TOUCH_THRESHOLD);

        float[] rTouchInput = arduinoLoop.GetRightTouch();
        ChangeAllObjectColour(rTouchInput, rArmRest, "TouchInput", TOUCH_THRESHOLD);
    }

    // Change the colour of many objects
    private static void ChangeAllObjectColour(float[] dataValues, GameObject parent, string parentGameObjectName, float threshold)
    {
        Transform chairPart = parent.transform.Find(parentGameObjectName);
        for (int i = 0; i < dataValues.Length; i++)
        {
            float rawValue = dataValues[i];

            GameObject currObject = chairPart.Find((i + 1).ToString()).gameObject;
            Sensor sensor = currObject.GetComponent<Sensor>();
            sensor.rawValue = rawValue;

            if (rawValue >= threshold)
            {
                double currTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                Utils.ToggleState(true, currObject);
                sensor.isActive = true;
                sensor.timestamp = currTime;
            }

            else
            {
                Utils.ToggleState(false, currObject);
                sensor.isActive = false;
            }
        }
    }


}
