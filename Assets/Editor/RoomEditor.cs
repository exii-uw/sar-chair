using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Room room = (Room)target;
        LoadMesh loadMesh = room.gameObject.GetComponent<LoadMesh>();
        SaveMesh saveMesh = room.gameObject.GetComponent<SaveMesh>();

        // Delete the most recent child
        if (GUILayout.Button("Delete Recent"))
        {
            EditorUtility.SetDirty(room.gameObject);
            room.DeleteRecent();
        }

        // Select a room
        if (GUILayout.Button("Select Room"))
        {
            string path = EditorUtility.OpenFolderPanel("Select a Room to Load", "Assets/SavedRooms", "");
            room.roomDirectory = path;
            loadMesh.SetRoomDir(path);
        }

        // Load a room        
        if (GUILayout.Button("Load Room"))
        {
            EditorUtility.SetDirty(room.gameObject);
            loadMesh.LoadSelectedMesh();
        }

        // Save a room        
        if (GUILayout.Button("Save Room"))
        {
            saveMesh.SaveRoom(room.roomName);
        }

        // Reset a room
        if (GUILayout.Button("Reset Room"))
        {
            EditorUtility.SetDirty(room.gameObject);
            room.ResetRoom();
        }

        // Refine a room
        if (GUILayout.Button("Refine Room"))
        {
            EditorUtility.SetDirty(room.gameObject);
            EditorUtility.SetDirty(room.reference);
            room.RefineRoom("Assets/SavedRooms/" + room.roomName + "/refinement.csv");
        }


    }
}
