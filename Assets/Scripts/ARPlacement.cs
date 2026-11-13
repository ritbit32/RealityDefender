using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Part 1 - The Arena.
/// Tap a detected horizontal plane to spawn the Game Base. The base is aligned to the
/// surface normal, faces the player, gets an ARAnchor so it stays locked in place,
/// and plane detection is switched off so it can never be moved again.
/// Put this on the XR Origin (same object as ARRaycastManager / ARPlaneManager / ARAnchorManager).
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class ARPlacement : MonoBehaviour
{
    [Header("Prefabs & References")]
    [SerializeField] GameObject gameBasePrefab;
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] Camera arCamera;

    [Header("UI")]
    [Tooltip("Text like 'Scan the floor, then tap to place'. Hidden after placement.")]
    [SerializeField] GameObject placementHintUI;
    [Tooltip("FIRE button + score. Hidden until the base is placed.")]
    [SerializeField] GameObject gameplayUI;

    static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager _raycastManager;
    GameObject _spawnedBase;

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
        if (arCamera == null) arCamera = Camera.main;

        if (gameplayUI) gameplayUI.SetActive(false);
        if (placementHintUI) placementHintUI.SetActive(true);
    }

    void Update()
    {
        if (_spawnedBase != null) return;                 // already placed -> immovable
        if (!TryGetTapPosition(out Vector2 screenPos)) return;

        if (!_raycastManager.Raycast(screenPos, s_Hits, TrackableType.PlaneWithinPolygon))
            return;

        // Hits are sorted by distance; take the closest horizontal (upward-facing) plane.
        foreach (var hit in s_Hits)
        {
            ARPlane plane = planeManager.GetPlane(hit.trackableId);
            if (plane == null || plane.alignment != PlaneAlignment.HorizontalUp) continue;

            PlaceBase(hit.pose);
            break;
        }
    }

    void PlaceBase(Pose hitPose)
    {
        // Align "up" with the surface normal so the base is never tilted.
        Vector3 up = hitPose.up;

        // Yaw the base so its front (+Z) faces the player, but keep it flat on the surface.
        Vector3 toCamera = arCamera.transform.position - hitPose.position;
        Vector3 forward = Vector3.ProjectOnPlane(toCamera, up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(arCamera.transform.forward, up);

        Quaternion rotation = Quaternion.LookRotation(forward.normalized, up);

        _spawnedBase = Instantiate(gameBasePrefab, hitPose.position, rotation);

        // Anchor it to the real world (needs an ARAnchorManager on the XR Origin).
        if (_spawnedBase.GetComponent<ARAnchor>() == null)
            _spawnedBase.AddComponent<ARAnchor>();

        // Stop detecting planes and hide the existing plane visuals.
        planeManager.enabled = false;
        foreach (ARPlane plane in planeManager.trackables)
            plane.gameObject.SetActive(false);

        if (placementHintUI) placementHintUI.SetActive(false);
        if (gameplayUI) gameplayUI.SetActive(true);
    }

    static bool TryGetTapPosition(out Vector2 position)
    {
#if ENABLE_INPUT_SYSTEM
        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
        {
            position = touchscreen.primaryTouch.position.ReadValue();
            return true;
        }
        var mouse = Mouse.current; // handy for testing with XR Simulation in the editor
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            position = mouse.position.ReadValue();
            return true;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            position = Input.GetTouch(0).position;
            return true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            position = Input.mousePosition;
            return true;
        }
#endif
        position = default;
        return false;
    }
}
