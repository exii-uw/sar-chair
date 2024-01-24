using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils {
    public class MeshMetaData
    {
        [Serializable]
        public struct RoomMetaData
        {
            public string roomName;
            public double saveTime;
            public int numSegments;
            public Segment[] segments;

            // Camera position (Aruco)
            public Vector3 arucoEuler;
            public Vector3 arucoPosition;

            public RoomMetaData(string name, double saveTime, int childCount, Vector3 arucoEuler, Vector3 arucoPosition) : this()
            {
                this.roomName = name;
                this.saveTime = saveTime;
                this.numSegments = childCount;
                this.segments = new Segment[childCount];
                this.arucoEuler = arucoEuler;
                this.arucoPosition = arucoPosition;
            }
        }

        [Serializable]
        public struct Segment
        {
            public string name;
            public Vector3 eulerAngles;
            public Vector3 position;
            public string meshPath;
            public string colorPath;
            public string uvPath;

            public Segment(string name, Vector3 eulerAngles, Vector3 position, string basePath) : this()
            {
                this.name = name;
                this.eulerAngles = eulerAngles;
                this.position = position;

                this.meshPath = basePath + ".asset";
                this.colorPath = basePath + "_color.png";
                this.uvPath = basePath + "_uv.png";
            }
        }
    }
}
