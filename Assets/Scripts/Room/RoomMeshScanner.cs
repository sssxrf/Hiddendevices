using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI; // Required for Button
using UnityEngine.Events; // Required for UnityEvent

public class RoomMeshScanner : MonoBehaviour
{
    private ARMeshManager meshManager;

    // TextMeshProUGUI references
    public TextMeshProUGUI roomSizeText;
    public TextMeshProUGUI roomCenterText;
    public TextMeshProUGUI cameraPositionText;
    public Button confirmRoomButton; // Button to confirm room establishment

    // UnityEvent that other scripts can listen to
    public UnityEvent OnRoomConfirmed;

    // List to store all accumulated vertices in world space
    private List<Vector3> allWorldVertices = new List<Vector3>();

    // Min and Max bounds for the room
    private Vector3 roomMinBound = Vector3.positiveInfinity;
    private Vector3 roomMaxBound = Vector3.negativeInfinity;
    private Vector3 roomCenter;

    // Flag to check if room is established
    private bool isRoomEstablished = false;

    void Start()
    {
        meshManager = GetComponent<ARMeshManager>();
        if (meshManager == null)
        {
            Debug.LogError("ARMeshManager component missing.");
            enabled = false;
            return;
        }

        meshManager.meshesChanged += OnMeshesChanged;

        // Attach the confirm button to the ConfirmRoom method
        confirmRoomButton.onClick.AddListener(ConfirmRoom);
    }

    // Called whenever meshes are updated
    void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        if (isRoomEstablished) return; // Stop updating room bounds if room is established

        allWorldVertices.Clear(); // Clear existing vertices

        foreach (var mesh in args.added)
        {
            MeshFilter meshFilter = mesh.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                Mesh unityMesh = meshFilter.mesh;
                Vector3[] vertices = unityMesh.vertices;

                // Transform and accumulate vertices in world space
                foreach (Vector3 vertex in vertices)
                {
                    Vector3 worldVertex = mesh.transform.TransformPoint(vertex);
                    allWorldVertices.Add(worldVertex);
                }
            }
        }

        UpdateRoomBounds(); // Update bounds with the latest vertices
    }

    // Method to calculate the room's bounds using all accumulated vertices
    public void UpdateRoomBounds()
    {
        if (allWorldVertices.Count == 0) return;

        roomMinBound = Vector3.positiveInfinity;
        roomMaxBound = Vector3.negativeInfinity;

        foreach (var vertex in allWorldVertices)
        {
            roomMinBound = Vector3.Min(roomMinBound, vertex);
            roomMaxBound = Vector3.Max(roomMaxBound, vertex);
        }

        roomCenter = (roomMaxBound + roomMinBound) / 2;
        Vector3 roomSize = roomMaxBound - roomMinBound;

        roomSizeText.text = $"Room Size: {roomSize}";
        roomCenterText.text = $"Room Center: {roomCenter}";
    }

    // Called when the "Confirm Room" button is clicked
    public void ConfirmRoom()
    {
        isRoomEstablished = true;
        Debug.Log("Room has been established.");

        // Trigger the OnRoomConfirmed event
        OnRoomConfirmed?.Invoke();
    }

    // Continuously update the camera's position relative to the fixed room center
    void Update()
    {
        if (isRoomEstablished)
        {
            CalculateUserPositionInRoom();
        }
    }

    // Calculate the user's (phone's) position relative to the room
    public void CalculateUserPositionInRoom()
    {
        // Get the camera's world position
        Vector3 cameraPosition = Camera.main.transform.position;

        // Calculate relative position to the room center
        Vector3 relativePosition = cameraPosition - roomCenter;

        cameraPositionText.text = $"Camera Position: {relativePosition}";
    }

    private void OnDestroy()
    {
        if (meshManager != null)
        {
            meshManager.meshesChanged -= OnMeshesChanged;
        }
    }
}
