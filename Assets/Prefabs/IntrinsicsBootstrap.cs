using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CameraIntrinsicsParams
{
    public float fx, fy, cx, cy, znear, zfar, proWidth, proHeight;
}

[RequireComponent(typeof(Camera))]
public class IntrinsicsBootstrap : MonoBehaviour
{

    public string CameraIntrinsicsJson = "projector_intrinsics.json";
    
    // Start is called before the first frame update
    void Awake()
    {

        //var filePath = Path.Combine(Application.streamingAssetsPath, CameraIntrinsicsJson);
        var filePath = "Assets/Prefabs/" + CameraIntrinsicsJson;
        print(filePath);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            CameraIntrinsicsParams intrinsics = JsonUtility.FromJson<CameraIntrinsicsParams>(dataAsJson);

            var cam = gameObject.GetComponent<Camera>();

            float width = intrinsics.proWidth;
            float height = intrinsics.proHeight;

            cam.aspect = (float)width / height;
            float fieldOfViewRad = 2.0f * (float)Math.Atan((((double)(height)) / 2.0) / intrinsics.fy);
            float fieldOfViewDeg = fieldOfViewRad / (float)Math.PI * 180.0f;
            cam.fieldOfView = fieldOfViewDeg;


            var projectionMatrix = GetProjectionMatrix(intrinsics);

            var camPrev = cam.projectionMatrix;
            cam.projectionMatrix = IntrinsicsBootstrap.ConvertRHtoLH(projectionMatrix);
            
            

        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }

    }


    /// <summary>
    /// Flips the right handed matrix to left handed matrix by inverting X coordinate.
    /// </summary>
    public static Matrix4x4 ConvertRHtoLH(Matrix4x4 inputRHMatrix)
    {
        Matrix4x4 flipRHtoLH = Matrix4x4.identity;
        flipRHtoLH[0, 0] = -1;
        return flipRHtoLH * inputRHMatrix * flipRHtoLH;
    }


    private Matrix4x4 GetProjectionMatrix(CameraIntrinsicsParams intrinsicParams)
    {
        float c_x = intrinsicParams.cx;
        float c_y = intrinsicParams.cy;
        float width = intrinsicParams.proWidth;
        float height = intrinsicParams.proHeight;
        float f_x = intrinsicParams.fx;
        float f_y = intrinsicParams.fy;
        float zNear = intrinsicParams.znear;
        float zFar = intrinsicParams.zfar;

        //the intrinsics are in Kinect coordinates: X - left, Y - up, Z, forward
        //we need the coordinates to be: X - right, Y - down, Z - forward
        c_x = intrinsicParams.proWidth - c_x;
        c_y = intrinsicParams.proHeight - c_y;

        // http://spottrlabs.blogspot.com/2012/07/opencv-and-opengl-not-always-friends.html
        // http://opencv.willowgarage.com/wiki/Posit
        Matrix4x4 projMat = new Matrix4x4();
        projMat[0, 0] = (float)(2.0 * f_x / width);
        projMat[1, 1] = (float)(2.0 * f_y / height);
        projMat[2, 0] = (float)(-1.0f + 2 * c_x / width);
        projMat[2, 1] = (float)(-1.0f + 2 * c_y / height);

        // Note this changed from previous code
        // see here: http://www.songho.ca/opengl/gl_projectionmatrix.html
        projMat[2, 2] = -(zFar + zNear) / (zFar - zNear);
        projMat[3, 2] = -2.0f * zNear * zFar / (zFar - zNear);
        projMat[2, 3] = -1;

        // Transpose tp fit Unity's column major matrix (in contrast to vision raw major ones).
        projMat = projMat.transpose;
        return projMat;
    }

}
