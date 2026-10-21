using System;
using System.Collections.Generic;
using UnityEngine;

public class AnchorTutorialUIManager : MonoBehaviour
{
    /// <summary>
    /// Anchor Tutorial UI manager singleton instance
    /// </summary>
    public static AnchorTutorialUIManager Instance;

    [SerializeField]
    private GameObject _saveableAnchorPrefab;

    [SerializeField]
    private GameObject _saveablePreview;

    [SerializeField]
    private Transform _saveableTransform;

    [SerializeField]
    private GameObject _nonSaveableAnchorPrefab;

    [SerializeField]
    private GameObject _nonSaveablePreview;

    [SerializeField]
    private Transform _nonSaveableTransform;

    private List<OVRSpatialAnchor> _anchorInstances = new(); // Active instances (red and green)

    private HashSet<Guid> _anchorUuids = new(); // Simulated external location, like PlayerPrefs

    private Action<bool, OVRSpatialAnchor.UnboundAnchor> _onLocalized;
    
    private bool TryGetFloorPoint(out Vector3 point)
{
    point = Vector3.zero;

    // Position et direction de la manette gauche
    Transform controller = _saveableTransform;

    Ray ray = new Ray(
        controller.position,
        controller.forward
    );

    if (Physics.Raycast(ray, out RaycastHit hit, 10f))
    {
        point = hit.point;
        return true;
    }

    return false;
}


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _onLocalized = OnLocalized;
        }
        else
        {
            Destroy(this);
        }
    }

    // This script responds to five button events:
    //
    // Left trigger: Create a saveable (green) anchor.
    // Right trigger: Create a non-saveable (red) anchor.
    // A: Load, Save and display all saved anchors (green only)
    // X: Destroy all runtime anchors (red and green)
    // Y: Erase all anchors (green only)
    // others: no action
    void Update()
    {
        
if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
{
    if (TryGetFloorPoint(out Vector3 point))
    {
        // Placer la capsule au point visé
        var go = Instantiate(
            _saveableAnchorPrefab,
            point,
            Quaternion.identity
        );

        SetupAnchorAsync(
            go.AddComponent<OVRSpatialAnchor>(),
            saveAnchor: true
        );
    }
    else
    {
        Debug.Log("Aucune surface détectée.");
    }
}

        else if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)) // Create a red capsule
        {
            // Create a red (non-savable) spatial anchor.
            var go = Instantiate(_nonSaveableAnchorPrefab, _nonSaveableTransform.position, _nonSaveableTransform.rotation); // Anchor b
            SetupAnchorAsync(go.AddComponent<OVRSpatialAnchor>(), saveAnchor: false);
        }
        else if (OVRInput.GetDown(OVRInput.Button.One)) // a button
        {
            LoadAllAnchors();
        }
        else if (OVRInput.GetDown(OVRInput.Button.Three)) // x button
        {
            // Destroy all anchors from the scene, but don't erase them from storage
            foreach (var anchor in _anchorInstances)
            {
                Destroy(anchor.gameObject);
            }

            // Clear the list of running anchors
            _anchorInstances.Clear();
        }
        else if (OVRInput.GetDown(OVRInput.Button.Four)) // y button
        {
            EraseAllAnchors();
        }
    }

    // You need to make sure the anchor is ready to use before you save it.
    // Also, only save if specified
    private async void SetupAnchorAsync(OVRSpatialAnchor anchor, bool saveAnchor)
    {
        // Keep checking for a valid and localized anchor state
        if (!await anchor.WhenLocalizedAsync())
        {
            Debug.LogError($"Unable to create anchor.");
            Destroy(anchor.gameObject);
            return;
        }

        // Add the anchor to the list of all instances
        _anchorInstances.Add(anchor);

        // Save the saveable (green) anchors only
        if (saveAnchor && (await anchor.SaveAnchorAsync()).Success)
        {
            // Remember UUID so you can load the anchor later
            _anchorUuids.Add(anchor.Uuid);
        }
    }

    /******************* Load Anchor Methods **********************/
    public async void LoadAllAnchors()
    {
        // Load and localize
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(_anchorUuids, unboundAnchors);

        if (result.Success)
        {
            foreach (var anchor in unboundAnchors)
            {
                anchor.LocalizeAsync().ContinueWith(_onLocalized, anchor);
            }
        }
        else
        {
            Debug.LogError($"Load anchors failed with {result.Status}.");
        }
    }

    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        var pose = unboundAnchor.Pose;
        var go = Instantiate(_saveableAnchorPrefab, pose.position, pose.rotation);
        var anchor = go.AddComponent<OVRSpatialAnchor>();

        unboundAnchor.BindTo(anchor);

        // Add the anchor to the running total
        _anchorInstances.Add(anchor);
    }

    /******************* Erase Anchor Methods *****************/
    // If the Y button is pressed, erase all anchors saved
    // in the headset, but don't destroy them. They should remain displayed.
    public async void EraseAllAnchors()
    {
        var result = await OVRSpatialAnchor.EraseAnchorsAsync(anchors: null, uuids: _anchorUuids);
        if (result.Success)
        {
            // Erase our reference lists
            _anchorUuids.Clear();

            Debug.Log($"Anchors erased.");
        }
        else
        {
            Debug.LogError($"Anchors NOT erased {result.Status}");
        }
    }
}
