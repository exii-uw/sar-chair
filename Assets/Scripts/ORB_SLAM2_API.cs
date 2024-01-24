using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Unity.Collections;
using UnityEngine;

namespace OrbSLAM
{
    public class SLAMInterfaceKinect
    {

        [DllImport("ORB_SLAM_DLL")]
        private static extern void InitializeSLAMKinect(string vocab, string cam, bool showUI);

        [DllImport("ORB_SLAM_DLL")]
        private static extern void InitializeKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void StopKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void ShutdownKinect();
        
        [DllImport("ORB_SLAM_DLL")]
        private static extern void RunKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void RunSLAMKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void ResetSLAMKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void StopSLAMKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void ShutdownSLAMKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern bool MapChangedKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static extern int GetSLAMTrackingStateKinect();

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetCamPoseKinect([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out float[] camPose, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetCamPointsKinect([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out float[] camPoints, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetDepthToCamTableKinect([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out float[] table, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetColorPointsKinect ([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out float[] colorPoints, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetColorImageKinect ([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] camColor, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetDepthImageKinect([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] depth, out int len);


        public static void Initialize(string vocab, string cam, bool showUI)
        {
            InitializeSLAMKinect(vocab, cam, showUI);
            InitializeKinect();
        }


        public static void Run()
        {
            RunSLAMKinect();
        }

        private static float[] cameraPose = new float[16];
        public static unsafe Matrix4x4 GetCameraPose()
        {
            int size = 16;
            GetCamPoseKinect(out cameraPose, out size);
            
            Matrix4x4 pose = new Matrix4x4();

            // Creating Vector4s for every row
            for (int i = 0; i < 4; i++)
            {
                Vector4 currRow = new Vector4();
                for (int j = 0; j < 4; j++)
                {
                    currRow[j] = cameraPose[(4*i) + j];
                }
                pose.SetRow(i, currRow);
            }

            return pose;
        }


        private static float[] cameraSpacePoints = new float[512 * 424 * 3];
        public static unsafe Vector3[] GetCameraSpacePoints()
        {
            int size = 512 * 424 * 3;
            GetCamPointsKinect(out cameraSpacePoints, out size);

            Vector3[] points = new Vector3[512 * 424];
            int currIndex = 0;

            for (int i = 0; i < (512 * 424 - 3); i += 3)
            {
                float x = cameraSpacePoints[i];
                float y = cameraSpacePoints[i + 1];
                float z = cameraSpacePoints[i + 2];

                Vector3 point = new Vector3(x, y, z);
                points[currIndex] = point;
                currIndex++;
            }

            return points;
        }

        private static float[] depthToCamTable = new float[512 * 424 * 2];
        public static unsafe float[] GetDepthToCameraSpaceTableRaw()
        {
            int size = 512 * 424 * 2;
            GetDepthToCamTableKinect(out depthToCamTable, out size);
            return depthToCamTable;
        }

        public static unsafe Vector2[] GetDepthToCameraSpaceTable()
        {
            int size = 512 * 424 * 2;
            GetDepthToCamTableKinect(out depthToCamTable, out size);
            Vector2[] points = new Vector2[512 * 424];
            int currIndex = 0;
            for (int i = 0; i < size - 2; i+=2)
            {
                float x = depthToCamTable[i];
                float y = depthToCamTable[i + 1];
                points[i] = new Vector2(x, y);
                currIndex++;
            }
            return points;
        }

        private static float[] colorSpacePoints = new float[512 * 424 * 2];
        public static unsafe Vector2[] GetColorSpacePoints ()
        {
            int size = 512 * 424 * 2;
            GetColorPointsKinect(out colorSpacePoints, out size);

            Vector2[] points = new Vector2[512 * 424];
            int currIndex = 0;

            for (int i = 0; i < (512 * 424 - 2); i += 2)
            {
                float x = colorSpacePoints[i];
                float y = colorSpacePoints[i + 1];

                Vector2 point = new Vector2(x, y);
                points[currIndex] = point;
                currIndex++;
            }

            return points;
        }

        public static unsafe float[] GetCameraSpacePointsRaw()
        {
            int size = 512 * 424 * 3;
            GetCamPointsKinect(out cameraSpacePoints, out size);
            return cameraSpacePoints;
        }

        private static byte[] cameraColor = new byte[1920*1080*4];
        public static unsafe Color32[] GetCameraColor()
        {
            int size = 1920 * 1080 * 4;
            GetColorImageKinect(out cameraColor, out size);
            Color32[] colours = new Color32[1920 * 1080];
            
            int currIndex = 0;

            for (int i = 0; i < size-4; i += 4)
            {
                colours[currIndex] = new Color32(cameraColor[i + 2], cameraColor[i + 1], cameraColor[i], cameraColor[i + 3]);
                currIndex++;
                
            }

           
            return colours;
        }
       

        public static unsafe byte[] GetCameraColorRaw()
        {
            int size = 1920 * 1080 * 4;
            GetColorImageKinect(out cameraColor, out size);
            return cameraColor;
        }

        private static byte[] cameraDepth = new byte[512*424*2];
        public static unsafe byte[] GetCameraDepthRaw()
        {
            int size = 512*424*2;
            GetDepthImageKinect(out cameraDepth, out size);
            return cameraDepth;
        }

        public static Color32[] GetCameraDepth()
        {
            int size = 512 * 424;
            GetDepthImageKinect(out cameraDepth, out size);
            Color32[] colours = new Color32[size];
            for(int i = 0; i < size; i++)
            {
                // Converting a 16-bit color num to RGB
                ushort color = cameraDepth[i];
                int red = color >> 11;
                int green = (color >> 5) & 63;
                int blue = color & 31;

                red = red * 255 / 31;
                green = green * 255 / 63;
                blue = blue * 255 / 31;


                colours[i] = new Color32((byte)red, (byte)green, (byte)blue, 255);
            }

            return colours;
        }

        public static void Reset()
        {
            ResetSLAMKinect();
        }

        public static void Shutdown()
        {
            ShutdownSLAMKinect();
        }

        public static void StopTracking()
        {
            StopSLAMKinect();
        }

        public static bool HasMapChanged()
        {
            return MapChangedKinect();
        }

        public static int GetTrackingState()
        {
            return GetSLAMTrackingStateKinect();
        }

        public static void StartKinect()
        {
            InitializeKinect();
            RunKinect();
        }

        public static void StopKinectLoop()
        {
            StopKinect();
            ShutdownKinect();
        }

    }

    public class SLAMInterfaceRealSense
    {
        public static readonly int C_IMAGE_WIDTH = 640;
        public static readonly int C_IMAGE_HEIGHT = 480;
        public static readonly int D_IMAGE_WIDTH = 640;
        public static readonly int D_IMAGE_HEIGHT = 480;

        //public static readonly int C_IMAGE_WIDTH = 424;
        //public static readonly int C_IMAGE_HEIGHT = 240;
        //public static readonly int D_IMAGE_WIDTH = 480;
        //public static readonly int D_IMAGE_HEIGHT = 270;

        // Images
        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetColorImageData([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] camColor, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetDepthImageData([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] depth, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetUVMapImageData([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] map, out int len);

             
        // SLAM
        [DllImport("ORB_SLAM_DLL")]
        private static extern void InitializeSystem(string vocab, string cam, bool showUI, string address, bool useArduino, bool useCamera, bool useSLAM);

        [DllImport("ORB_SLAM_DLL")]
        private static extern void RunSystem();

        [DllImport("ORB_SLAM_DLL")]
        private static extern void ShutdownSystem();

        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetCamPoseData([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out float[] pose, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static extern bool IsSystemInReadyState();

        
        // Arduino
        [DllImport("ORB_SLAM_DLL")]
        private static unsafe extern void GetArduinoSensorData([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out byte[] rawData, out int len);

        [DllImport("ORB_SLAM_DLL")]
        private static extern void MoveServo(string num, string angle);

        // DEBUG Aruco
        //[DllImport("ORB_SLAM_DLL")]
        //private static unsafe extern void GetArucoPoseData([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] out double[] pose, out int len);

        // DEBUG Aruco
        //private static double[] arucoPose = new double[16];
        //public static unsafe double[] GetArucoPoseRaw()
        //{
        //    int size = 16;
        //    GetArucoPoseData(out arucoPose, out size);
        //    return arucoPose;
        //}

        //public static unsafe Matrix4x4 GetArucoPose()
        //{
        //    int size = 16;
        //    GetArucoPoseData(out arucoPose, out size);

        //    Matrix4x4 pose = new Matrix4x4();


        //    // Creating Vector4s for every column
        //    for (int i = 0; i < 4; i++)
        //    {
        //        Vector4 col = new Vector4();
        //        for (int j = 0; j < 4; j++)
        //        {
        //            col[j] = (float)arucoPose[(4 * i) + j];
        //        }
        //        pose.SetColumn(i, col);
        //    }

        //    Matrix4x4 m = Matrix4x4.identity;
        //    m.m11 = -1;
        //    //pose = pose.transpose;
        //    pose = pose * m;


        //    return pose;
        //}


        // Check if the system is ready
        public static bool CheckIfSystemIsInReadyState()
        {
            return IsSystemInReadyState();
        }

       
        // Start SLAM loop
        public static void StartSystem(string vocab, string cam, bool showUI, string address, bool useArduino, bool useCamera, bool useSLAM)
        {
            if (useSLAM)
                useCamera = true;
            InitializeSystem(vocab, cam, showUI, address, useArduino, useCamera, useSLAM);
            RunSystem();
        }

        // Stop the SLAM loop
        public static void StopSystem()
        {
            ShutdownSystem();
        }

        // Get the camera pos
        private static float[] cameraPose = new float[16];
        public static unsafe Matrix4x4 GetCameraPose()
        {
            int size = 16;
            GetCamPoseData(out cameraPose, out size);

            Matrix4x4 pose = new Matrix4x4();


            // Creating Vector4s for every column
            for (int i = 0; i < 4; i++)
            {
                Vector4 col = new Vector4();
                for (int j = 0; j < 4; j++)
                {
                    col[j] = cameraPose[(4 * i) + j];
                }
                pose.SetColumn(i, col);
            }
            
            Matrix4x4 m = Matrix4x4.identity;
            m.m11 = -1;
            //pose = pose.transpose;
            pose = pose * m;


            return pose;
        }

        public static unsafe float[] GetCameraPoseRaw()
        {
            int size = 16;
            GetCamPoseData(out cameraPose, out size);
            return cameraPose;
        }


        // Get the camera data (color)
        private static byte[] cameraColor = new byte[C_IMAGE_WIDTH * C_IMAGE_HEIGHT * 3];
        public static unsafe byte[] GetCameraColorRaw()
        {
            int size = C_IMAGE_WIDTH * C_IMAGE_HEIGHT * 3;
            GetColorImageData(out cameraColor, out size);
            return cameraColor;
        }
        
        // Get the camera data (depth)
        private static byte[] cameraDepth = new byte[D_IMAGE_WIDTH * D_IMAGE_HEIGHT * 2];
        public static unsafe byte[] GetCameraDepthRaw()
        {
            int size = D_IMAGE_WIDTH * D_IMAGE_HEIGHT * 2;
            GetDepthImageData(out cameraDepth, out size);
            return cameraDepth;
        }

        // Get the camera UV
        private static byte[] uvMap = new byte[D_IMAGE_WIDTH * D_IMAGE_HEIGHT * 2 * sizeof(float)];
        public static unsafe byte[] GetCameraUVMap()
        {
            int size = D_IMAGE_WIDTH * D_IMAGE_HEIGHT * 2 * sizeof(float);
            GetUVMapImageData(out uvMap, out size);
            return uvMap;
        }

        // Get the sensor data (Arduino)
        private static byte[] sensorData = new byte[500];
        public static unsafe string GetSensorData()
        {
            int size = 500;
            GetArduinoSensorData(out sensorData, out size);
            string output = System.Text.Encoding.UTF8.GetString(sensorData, 0, sensorData.Length);
            return output;
          
        }

        // Move a servo
        public static unsafe void SetServoAngle(int servo, int angle)
        {
            MoveServo(servo.ToString(), angle.ToString());
        }

    }
}
