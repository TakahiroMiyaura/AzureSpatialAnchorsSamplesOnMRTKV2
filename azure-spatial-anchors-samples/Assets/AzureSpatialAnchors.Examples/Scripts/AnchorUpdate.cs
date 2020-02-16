using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ManipulationHandler))]
public class AnchorUpdate : MonoBehaviour
{

    private CloudNativeAnchor cna;
    private ManipulationHandler handler;
    private bool isManipulate;

    void Start()
    {
        handler = GetComponent<ManipulationHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cna == null)
        {
            // Get the cloud-native anchor behavior
            cna = GetComponent<CloudNativeAnchor>();
        }

        // Warn and exit if the behavior is missing
        if (cna == null)
        {
            Debug.LogWarning($"The object {name} is missing the {nameof(CloudNativeAnchor)} behavior.");
            return;
        }

        if(isManipulate)
        {
            // No. Just set the pose.
            cna.SetPose(transform.position, transform.rotation);
        }
    }

    public void OnManipulationStared(ManipulationEventData data)
    {
        isManipulate = true;
    }

    public void OnManipulationEnded(ManipulationEventData data)
    {
        isManipulate = false;
    }

    [SerializeField]
    private AnchorsUpdateEvent AnchorsUpdate = new AnchorsUpdateEvent();


    public void SetCloudSpatialAnchor(CloudSpatialAnchor cloudSpatialAnchor)
    {
        // Is there a cloud anchor to apply
        if (cloudSpatialAnchor != null)
        {
            if (cna == null)
            {
                // Get the cloud-native anchor behavior
                cna = GetComponent<CloudNativeAnchor>();
            }
            // Yes. Apply the cloud anchor, which also sets the pose.
            cna.CloudToNative(cloudSpatialAnchor);
            if (handler != null)
            {
                handler.enabled = false;
            }

            if (AnchorsUpdate != null)
            {
                AnchorsUpdate.Invoke(cna,cloudSpatialAnchor);
            }
        }
    }
    [Serializable] public class AnchorsUpdateEvent : UnityEvent<CloudNativeAnchor,CloudSpatialAnchor> { }
}
