using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using TMPro;

public class RoomMeshScanner : MonoBehaviour
{
    private ARMeshManager meshManager;

    // TextMeshProUGUI references
    public TextMeshProUGUI roomSizeText;
    public TextMeshProUGUI roomCenterText;
    public TextMeshProUGUI cameraPositionText;

    // List to store all accumulated vertices in world space
    private List<Vector3> allWorldVertices = new List<Vector3>();

    // Min and Max bounds for the room
    private Vector3 roomMinBound = Vector3.positiveInfinity;
    private Vector3 roomMaxBound = Vector3.negativeInfinity;
    private Vector3 roomCenter;

    void Start()
    {
        meshManager = GetComponent<ARMeshManager>();
        meshManager.meshesChanged += OnMeshesChanged;
    }

    // Called whenever meshes are updated
    void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        foreach (var mesh in args.added)
        {
            MeshFilter meshFilter = mesh.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                Mesh unityMesh = meshFilter.mesh;
                Vector3[] vertices = unityMesh.vertices;

                // Transform vertices to world space and accumulate them
                foreach (Vector3 vertex in vertices)
                {
                    Vector3 worldVertex = mesh.transform.TransformPoint(vertex);
                    allWorldVertices.Add(worldVertex);
                }

                Debug.Log($"Accumulated {vertices.Length} new vertices. Total: {allWorldVertices.Count} vertices.");
            }
        }

        // Optionally, update room bounds after each mesh change
        UpdateRoomBounds();
        CalculateUserPositionInRoom();
    }

    // Method to calculate the room's bounds using all accumulated vertices
    public void UpdateRoomBounds()
    {
        foreach (var vertex in allWorldVertices)
        {
            roomMinBound = Vector3.Min(roomMinBound, vertex);
            roomMaxBound = Vector3.Max(roomMaxBound, vertex);
        }

        roomCenter = (roomMaxBound + roomMinBound) / 2;
        Vector3 roomSize = roomMaxBound - roomMinBound;

        // Update TextMeshProUGUI fields
        roomSizeText.text = $"Room Size: {roomSize}";
        roomCenterText.text = $"Room Center: {roomCenter}";

        //Debug.Log($"Room Size: {roomSize}");
        //Debug.Log($"Room Center: {roomCenter}");
    }

    // Calculate the user's (phone's) position relative to the room
    public void CalculateUserPositionInRoom()
    {
        // Get the camera's world position
        Vector3 cameraPosition = Camera.main.transform.position;

        // Calculate relative position to the room center
        Vector3 relativePosition = cameraPosition - roomCenter;

        //Debug.Log($"Camera World Position: {cameraPosition}");
        //Debug.Log($"Relative Position in Room: {relativePosition}");
        cameraPositionText.text = $"Camera Position: {relativePosition}";
    }

    private void OnDestroy()
    {
        meshManager.meshesChanged -= OnMeshesChanged;
    }
}
