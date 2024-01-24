using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace utils
{
    public static class Utils
    {

        public static Matrix4x4 CopyMatrix4x4(Matrix4x4 src)
        {
            Matrix4x4 dst = new Matrix4x4();
            for (int i = 0; i < 4; i++)
                dst.SetColumn(i, src.GetColumn(i));
            return dst;
        }

        public static Matrix4x4 SubtractMatrix4x4(Matrix4x4 oldMat, Matrix4x4 newMat)
        {
            Matrix4x4 dst = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                Vector4 diff = newMat.GetColumn(i) - oldMat.GetColumn(i);
                dst.SetColumn(i, diff);
            }

            return dst;
        }

        public static double GetTimestamp()
        {
            double time = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return Math.Ceiling(time);
        }

        public static void SaveImage(Texture2D texture, string filePath)
        {

            byte[] bytes = texture.EncodeToPNG();
            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < bytes.Length; i++)
            {
                writer.Write(bytes[i]);
            }
            writer.Close();
            stream.Close();
        }

        // Change a gameobject's colour
        public static void ToggleState(bool toggled, GameObject gameObject)
        {

            if (!toggled)
                gameObject.GetComponent<Renderer>().material.color = Color.white;

            else
                gameObject.GetComponent<Renderer>().material.color = Color.red;
        }

        // Convert 0-360 angle to -180-180
        public static float ConvertEuler(float angle)
        {

            return Mathf.Repeat(angle + 180, 360) - 180;
        }

        // Find children that start with a prefix
        public static Transform[] FindObjectsByName(string prefix, Transform parent)
        {
            List<Transform> children = new List<Transform>();
            foreach(Transform child in parent)
            {
                if (child.name.StartsWith(prefix))
                    children.Add(child);
            }

            return children.ToArray();
        }

    }
}

