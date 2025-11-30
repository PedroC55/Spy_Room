using UnityEngine;
using Meta.XR.MRUtilityKit;
using TMPro;

public class RoomSetupManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject setupPromptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject gameplayUI;
    
    [Header("Settings")]
    [SerializeField] private float checkInterval = 2f;
    [SerializeField] private bool autoRequestSpaceSetup = true;
    
    private bool roomReady = false;
    private bool spaceSetupRequested = false;
    private float nextCheckTime = 0f;

    void Start()
    {
        // Hide gameplay UI initially
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // Show setup prompt
        if (setupPromptCanvas != null)
        {
            setupPromptCanvas.SetActive(true);
        }

        // Check if MRUK is available
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance not found! Make sure MRUK is in the scene.");
            UpdatePromptText("Error: MRUK not found in scene!");
            return;
        }

        // Register callback for when scene is loaded
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        
        // Initial check
        CheckForRoom();
    }

    void Update()
    {
        // Keep checking if room is ready
        if (!roomReady && Time.time >= nextCheckTime)
        {
            CheckForRoom();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    private void CheckForRoom()
    {
        if (MRUK.Instance == null) return;

        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();

        if (currentRoom != null)
        {
            // Check if room has ceiling and floor
            if (currentRoom.CeilingAnchor != null && currentRoom.FloorAnchor != null)
            {
                Debug.Log("Room scan detected! Starting game...");
                OnRoomReady();
            }
            else
            {
                UpdatePromptText("Room detected but incomplete.\nPlease complete the room scan.");
                RequestSpaceSetupIfNeeded();
            }
        }
        else
        {
            UpdatePromptText("No room scan detected.\nOpening Space Setup...");
            RequestSpaceSetupIfNeeded();
        }
    }

    private void RequestSpaceSetupIfNeeded()
    {
        // Only request once and if auto-request is enabled
        if (spaceSetupRequested || !autoRequestSpaceSetup) return;

        spaceSetupRequested = true;

        #if UNITY_ANDROID && !UNITY_EDITOR
        // Show instructions to user
        UpdatePromptText("No room scan detected.\n\nPlease scan your room:\n1. Press Meta button\n2. Settings → Space Setup\n3. Scan your room\n\nThe game will start automatically after scanning.");
        Debug.Log("Waiting for user to complete Space Setup...");
        #else
        Debug.Log("Space Setup request only works on device. In Editor, room should load automatically if available.");
        UpdatePromptText("Editor Mode: Waiting for room data...\n(On device, you'll need to scan your room)");
        #endif
    }

    private void OnSceneLoaded()
    {
        Debug.Log("Scene loaded callback triggered!");
        CheckForRoom();
    }

    private void OnRoomReady()
    {
        roomReady = true;

        // Hide setup prompt
        if (setupPromptCanvas != null)
        {
            setupPromptCanvas.SetActive(false);
        }

        // Show gameplay UI
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }

        // Notify other systems that room is ready
        BroadcastMessage("OnRoomScanComplete", SendMessageOptions.DontRequireReceiver);
    }

    private void UpdatePromptText(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }
        Debug.Log($"Room Setup: {message}");
    }

    // Public method to manually trigger room check
    public void ManualRoomCheck()
    {
        spaceSetupRequested = false; // Reset flag
        CheckForRoom();
    }

    // Public method to manually request Space Setup
    public void ManualRequestSpaceSetup()
    {
        spaceSetupRequested = false; // Reset flag
        RequestSpaceSetupIfNeeded();
    }

    // Check if room is ready (other scripts can use this)
    public bool IsRoomReady()
    {
        return roomReady;
    }
}
