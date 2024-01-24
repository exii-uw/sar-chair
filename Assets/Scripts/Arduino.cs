using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OrbSLAM;
using System;
using System.Threading;

public class Arduino : MonoBehaviour
{
    Controller controller;

    // Sliders to control the servo angle from the inspector
    [Range(0, 130)] // 70 is neutral, 0 for ceiling, 130 for floor
    public int topServoAngle = 70;
    public readonly int neutralTop = 70;
    public readonly int minTop = 0;
    public readonly int maxTop = 130;

    [Range(0, 180)] // 130 is neutral, 0 for backward, 180 toward the seat
    public int bottomServoAngle = 130;
    public readonly int neutralBottom = 130;
    public readonly int minBottom = 0;
    public readonly int maxBottom = 180;

    public const int NUM_VALUES = 45; // 45 values are read from the Arduino
    private float[] sensorData; // incoming raw data from the Arduino

    private int prevTopServoAngle = -1;
    private int prevBottomServoAngle = -1;


    float nextUpdate = 0;
    float period = 0.01f;


    bool initialized = false;
    public enum Servo
    {
        Top = 1,
        Bottom  = 2
    }

    // Start is called before the first frame update
    void Start()
    {
        if (controller == null)
            controller = gameObject.GetComponent<Controller>();
        sensorData = new float[NUM_VALUES];
        //ResetServoPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (SLAMInterfaceRealSense.CheckIfSystemIsInReadyState() && controller.useArduino)
        {
            // Initialize the servo values in the editor
            if (!initialized)
            {
                //ResetServoPosition();
                initialized = true;
            }


            if (Time.time > nextUpdate)
            {
                nextUpdate += period;


                string rawData = SLAMInterfaceRealSense.GetSensorData();
                if (rawData.Length == 0)
                    return;

                // Process the data
                //print(rawData);
                ProcessRawData(rawData);
                //print("Top: " + GetTopServo() + " Bottom: " + GetBottomServo());



                //CheckServoState();
            }


            //CheckServoState();


        }

    }

    private void CheckTopServo()
    {
        //print("TOP: " + prevTopServoAngle + ", " + topServoAngle);
        // Check if a servo should be moved
        if (prevTopServoAngle != topServoAngle)
        {
            //if (Mathf.Abs(prevTopServoAngle - topServoAngle) > 5)
            //{
                prevTopServoAngle = topServoAngle;
                // only move the servos if the Arduino is being used
                if (controller.useArduino)
                    MoveServo((int)Servo.Top, topServoAngle);
                else
                    print("Updated top servo angle.");
            //}
        }
    }

    private void CheckBottomServo()
    {
        //print("BOTTOM: " + prevBottomServoAngle + ", " + bottomServoAngle);
        if (prevBottomServoAngle != bottomServoAngle)
        {
            //if (Mathf.Abs(prevBottomServoAngle - bottomServoAngle) > 5)
            //{
                prevBottomServoAngle = bottomServoAngle;
                // only move the servos if the Arduino is being used
                if (controller.useArduino)
                    MoveServo((int)Servo.Bottom, bottomServoAngle);
                else
                    print("Updated bottom servo angle.");
            //}
        }
    }

    // Check and see if the servos should be moved
    public bool CheckServoState()
    {
        if (initialized)
        {
            //Thread topServoThread = new Thread(CheckTopServo);
            //topServoThread.Start();
            //Thread bottomServoThread = new Thread(CheckBottomServo);
            //bottomServoThread.Start();

            //topServoThread.Join();
            //bottomServoThread.Join();

            //print("Checking servos...");
            CheckBottomServo();
            CheckTopServo();



            return true;
        }

        return false;
        
    }

    // Convert the string of raw values to a float array
    private void ProcessRawData(string rawData)
    {
        string[] splitString = rawData.Split(',');
        if (splitString.Length < NUM_VALUES) return;
        for (int i = 0; i < NUM_VALUES; i++)
        {
            string currString = splitString[i];
            float sensorValue = float.Parse(currString);
            sensorData[i] = sensorValue;
        }
    }

    // Get the seat FSR values
    public float[] GetSeatValues()
    {
        return GetSubArray(sensorData, 0, 5);
    }

    // Get the lower back FSR values
    public float[] GetLowerBackValues()
    {
        float[] output = GetSubArray(sensorData, 5, 5);
        output[1] = sensorData[15]; // 6th value is a dummy value
        return output;
    }

    // Get the upper back FSR values
    public float[] GetUpperBackValues()
    {
        return GetSubArray(sensorData, 10, 5);
    }

    // Get the touch sensor values (left)
    public float[] GetLeftTouch()
    {
        return GetSubArray(sensorData, 16, 12);
    }

    // Get the touch sensor values (right)
    public float[] GetRightTouch()
    {
        return GetSubArray(sensorData, 28, 12);
    }

    // FSR values for the armrest (left) VERIFY
    public float GetLeftValue()
    {
        return sensorData[41];
    }

    // FSR values for the armrest (right) VERIFY
    public float GetRightValue()
    {
        return sensorData[40];
    }

    // Top servo value VERIFY
    public float GetTopServo()
    {
        return sensorData[42];
    }

    // Bottom servo value VERIFY
    public float GetBottomServo()
    {
        return sensorData[43];
    }

    // Back tilt
    public float GetBackTilt()
    {
        return sensorData[44];
    }


    // Helper to create a subarray
    private float[] GetSubArray(float[] inArray, int inIndex, int outSize)
    {
        float[] outArray = new float[outSize];
        Array.Copy(inArray, inIndex, outArray, 0, outArray.Length);
        return outArray;
    }

    // Move a servo
    public void MoveServo(int servo, int angle)
    {
        SLAMInterfaceRealSense.SetServoAngle(servo, angle);
    }

    public void ResetServoPosition()
    {
        if (controller.useArduino)
        {
            MoveServo((int)Servo.Bottom, neutralBottom);
            MoveServo((int)Servo.Top, neutralTop);
        }
    }

}
