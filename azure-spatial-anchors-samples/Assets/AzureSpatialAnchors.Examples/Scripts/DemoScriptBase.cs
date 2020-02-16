// Copyright (c) Microsoft Corporation. All rights reserved.
// Copyright (c) Takahiro Miyaura. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Input.UnityInput;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Experimental.Extensions.UX;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public abstract class DemoScriptBase : MonoBehaviour//InputInteractionBase
    {
        #region Member Variables
#if UNITY_ANDROID || UNITY_IOS
        ARRaycastManager arRaycastManager;
#endif
        private Task advanceDemoTask = null;
        protected bool isErrorActive = false;
        protected TextMeshPro feedbackBox;
        protected readonly List<string> anchorIdsToLocate = new List<string>();
        protected AnchorLocateCriteria anchorLocateCriteria = null;
        protected CloudSpatialAnchor currentCloudAnchor;
        protected CloudSpatialAnchorWatcher currentWatcher;
        protected GameObject spawnedObject = null;
        protected Material spawnedObjectMat = null;
        protected bool enableAdvancingOnSelect = true;
        protected Dictionary<string,string> explanationText = null;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab = null;

        [SerializeField]
        [Tooltip("SpatialAnchorManager instance to use for this demo. This is required.")]
        private SpatialAnchorManager cloudManager = null;
        #endregion // Unity Inspector Variables

        #region MonoBehaviour Functions

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public virtual void Start()
        {
            feedbackBox = XRUXPicker.Instance.GetFeedbackText();
            if (feedbackBox == null)
            {
                Debug.Log($"{nameof(feedbackBox)} not found in scene by XRUXPicker.");
                Destroy(this);
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                Debug.Log("Missing ARRaycastManager in scene");
            }
#endif

            if (CloudManager == null)
            {
                Debug.Break();
                feedbackBox.text = $"{nameof(CloudManager)} reference has not been set. Make sure it has been added to the scene and wired up to {this.name}.";
                return;
            }

            if (!SanityCheckAccessConfiguration())
            {
                feedbackBox.text = $"{nameof(SpatialAnchorManager.SpatialAnchorsAccountId)} and {nameof(SpatialAnchorManager.SpatialAnchorsAccountKey)} must be set on {nameof(SpatialAnchorManager)}";
            }


            if (AnchoredObjectPrefab == null)
            {
                feedbackBox.text = "CreationTarget must be set on the demo script.";
                return;
            }

            CloudManager.SessionUpdated += CloudManager_SessionUpdated;
            CloudManager.AnchorLocated += CloudManager_AnchorLocated;
            CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
            CloudManager.LogDebug += CloudManager_LogDebug;
            CloudManager.Error += CloudManager_Error;

            anchorLocateCriteria = new AnchorLocateCriteria();

        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public virtual void Update()
        {
            //OnGazeInteraction();
        }

        /// <summary>
        /// Destroying the attached Behaviour will result in the game or Scene
        /// receiving OnDestroy.
        /// </summary>
        /// <remarks>OnDestroy will only be called on game objects that have previously been active.</remarks>
        public virtual void OnDestroy()
        {
            if (CloudManager != null)
            {
                CloudManager.StopSession();
            }

            if (currentWatcher != null)
            {
                currentWatcher.Stop();
                currentWatcher = null;
            }

            CleanupSpawnedObjects();

            // Pass to base for final cleanup
            //base.OnDestroy();
        }

        #endregion

        #region CloudMaanger Functions

        /// <summary>
        /// Called when a cloud anchor is not saved successfully.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected virtual void OnSaveCloudAnchorFailed(Exception exception)
        {
            // we will block the next step to show the exception message in the UI.
            isErrorActive = true;
            Debug.LogException(exception);
            Debug.Log("Failed to save anchor " + exception.ToString());

            UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", exception.ToString()));
        }

        /// <summary>
        /// Called when a cloud anchor is saved successfully.
        /// </summary>
        protected virtual Task OnSaveCloudAnchorSuccessfulAsync()
        {
            // To be overridden.
            return Task.CompletedTask;
        }

        protected CloudSpatialAnchorWatcher CreateWatcher()
        {
            if ((CloudManager != null) && (CloudManager.Session != null))
            {
                return CloudManager.Session.CreateWatcher(anchorLocateCriteria);
            }
            else
            {
                return null;
            }
        }

        protected void SetNearbyAnchor(CloudSpatialAnchor nearbyAnchor, float DistanceInMeters, int MaxNearAnchorsToFind)
        {
            if (nearbyAnchor == null)
            {
                anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();
                return;
            }

            NearAnchorCriteria nac = new NearAnchorCriteria();
            nac.SourceAnchor = nearbyAnchor;
            nac.DistanceInMeters = DistanceInMeters;
            nac.MaxResultCount = MaxNearAnchorsToFind;

            anchorLocateCriteria.NearAnchor = nac;
        }

        protected void SetNearDevice(float DistanceInMeters, int MaxAnchorsToFind)
        {
            NearDeviceCriteria nearDeviceCriteria = new NearDeviceCriteria();
            nearDeviceCriteria.DistanceInMeters = DistanceInMeters;
            nearDeviceCriteria.MaxResultCount = MaxAnchorsToFind;

            anchorLocateCriteria.NearDevice = nearDeviceCriteria;
        }

        protected void SetGraphEnabled(bool UseGraph, bool JustGraph = false)
        {
            anchorLocateCriteria.Strategy = UseGraph ?
                (JustGraph ? LocateStrategy.Relationship : LocateStrategy.AnyStrategy) :
                LocateStrategy.VisualInformation;
        }

        /// <summary>
        /// Bypassing the cache will force new queries to be sent for objects, allowing
        /// for refined poses over time.
        /// </summary>
        /// <param name="BypassCache"></param>
        public void SetBypassCache(bool BypassCache)
        {
            anchorLocateCriteria.BypassCache = BypassCache;
        }

        /// <summary>
        /// Called when a cloud anchor is located.
        /// </summary>
        /// <param name="args">The <see cref="AnchorLocatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            // To be overridden.
        }

        /// <summary>
        /// Called when cloud anchor location has completed.
        /// </summary>
        /// <param name="args">The <see cref="LocateAnchorsCompletedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloudLocateAnchorsCompleted(LocateAnchorsCompletedEventArgs args)
        {
            Debug.Log("Locate pass complete");
        }

        /// <summary>
        /// Called when the current cloud session is updated.
        /// </summary>
        protected virtual void OnCloudSessionUpdated()
        {
            // To be overridden.
        }

        private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            Debug.LogFormat("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
            if (args.Status == LocateAnchorStatus.Located)
            {
                OnCloudAnchorLocated(args);
            }
        }

        private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            OnCloudLocateAnchorsCompleted(args);
        }

        private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            OnCloudSessionUpdated();
        }

        private void CloudManager_Error(object sender, SessionErrorEventArgs args)
        {
            isErrorActive = true;
            Debug.Log(args.ErrorMessage);

            UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", args.ErrorMessage));
        }

        private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
        {
            Debug.Log(args.Message);
        }

        #endregion

        #region UI Events

        /// <summary>
        /// Advances the demo.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public abstract Task AdvanceDemoAsync();

        /// <summary>
        /// This version only exists for Unity to wire up a button click to.
        /// If calling from code, please use the Async version above.
        /// </summary>
        public async void AdvanceDemo()
        {
            try
            {
                advanceDemoTask = AdvanceDemoAsync();
                await advanceDemoTask;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(DemoScriptBase)} - Error in {nameof(AdvanceDemo)}: {ex.Message} {ex.StackTrace}");
                feedbackBox.text = $"Demo failed, check debugger output for more information";
            }
        }

        public virtual Task EnumerateAllNearbyAnchorsAsync() { throw new NotImplementedException(); }

        public async void EnumerateAllNearbyAnchors()
        {
            try
            {
                await EnumerateAllNearbyAnchorsAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(DemoScriptBase)} - Error in {nameof(EnumerateAllNearbyAnchors)}: === {ex.GetType().Name} === {ex.ToString()} === {ex.Source} === {ex.Message} {ex.StackTrace}");
                feedbackBox.text = $"Enumeration failed, check debugger output for more information";
            }
        }

        /// <summary>
        /// returns to the launcher scene.
        /// </summary>
        public async void ReturnToLauncher()
        {
            // If AdvanceDemoAsync is still running from the gesture handler,
            // wait for it to complete before returning to the launcher.
            if (advanceDemoTask != null) { await advanceDemoTask; }

            // Return to the launcher scene
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Called when a select interaction occurs.
        /// </summary>
        /// <remarks>Currently only called for HoloLens.</remarks>
        public async virtual void OnSelectInteraction()
        {
            if (enableAdvancingOnSelect)
            {
                try
                {
                    advanceDemoTask = AdvanceDemoAsync();
                    await advanceDemoTask;
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"{nameof(DemoScriptBase)} - Error in {nameof(AdvanceDemo)}: {ex.Message} {ex.StackTrace}");
                    feedbackBox.text = $"Demo failed, check debugger output for more information";
                }
            }
#if UNITY_ANDROID || UNITY_IOS
            var mixedRealityController = CoreServices.InputSystem.DetectedControllers.FirstOrDefault(x => x.InputSource.SourceType == InputSourceType.Other) as UnityTouchController;
            List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            if (mixedRealityController != null &&
                arRaycastManager.Raycast(mixedRealityController.TouchData.position, aRRaycastHits) &&
                aRRaycastHits.Count > 0)
            {
                ARRaycastHit hit = aRRaycastHits[0];
                OnSelectObjectInteraction(hit.pose.position, hit);
            }
#elif WINDOWS_UWP || UNITY_WSA

            if (CoreServices.InputSystem.GazeProvider.GazeTarget)
            {
                OnSelectObjectInteraction(CoreServices.InputSystem.GazeProvider.HitPosition,null);
            }
#endif
        }

        /// <summary>
        /// Called when a touch object interaction occurs.
        /// </summary>
        /// <param name="hitPoint">The position.</param>
        /// <param name="target">The target.</param>
        protected virtual void OnSelectObjectInteraction(Vector3 hitPoint, object target)
        {
            if (IsPlacingObject())
            {
                Quaternion rotation = Quaternion.AngleAxis(0, Vector3.up);

                SpawnOrMoveCurrentAnchoredObject(hitPoint, rotation);
                
            }
        }

        #endregion

        #region Other Functions

        public virtual bool SanityCheckAccessConfiguration()
        {
            if (string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountId) || string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountKey))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Cleans up spawned objects.
        /// </summary>
        protected virtual void CleanupSpawnedObjects()
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
                spawnedObject = null;
            }

            if (spawnedObjectMat != null)
            {
                Destroy(spawnedObjectMat);
                spawnedObjectMat = null;
            }
        }

        protected void SetAnchorIdsToLocate(IEnumerable<string> anchorIds)
        {
            if (anchorIds == null)
            {
                throw new ArgumentNullException(nameof(anchorIds));
            }

            anchorIdsToLocate.Clear();
            anchorIdsToLocate.AddRange(anchorIds);

            anchorLocateCriteria.Identifiers = anchorIdsToLocate.ToArray();
        }

        protected void ResetAnchorIdsToLocate()
        {
            anchorIdsToLocate.Clear();
            anchorLocateCriteria.Identifiers = new string[0];
        }


        /// <summary>
        /// Gets the color of the current demo step.
        /// </summary>
        /// <returns><see cref="Color"/>.</returns>
        protected abstract Color GetStepColor();

        /// <summary>
        /// Determines whether the demo is in a mode that should place an object.
        /// </summary>
        /// <returns><c>true</c> to place; otherwise, <c>false</c>.</returns>
        protected abstract bool IsPlacingObject();
        
        /// <summary>
        /// Saves the current object anchor to the cloud.
        /// </summary>
        protected virtual async Task SaveCurrentObjectAnchorToCloudAsync()
        {
            // Get the cloud-native anchor behavior
            CloudNativeAnchor cna = spawnedObject.GetComponent<CloudNativeAnchor>();

            // If the cloud portion of the anchor hasn't been created yet, create it
            if (cna.CloudAnchor == null) { cna.NativeToCloud(); }
            
            // Get the cloud portion of the anchor
            CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;
            
            // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
            cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

            if (explanationText!= null && explanationText.Count > 0)
            {
                foreach (var key in explanationText.Keys)
                {
                    cloudAnchor.AppProperties.Add(key, explanationText[key]);

                }
            }

            int counter = 0;
            while (!CloudManager.IsReadyForCreate)
            {
                counter++;
                await Task.Delay(330);
                float createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
                feedbackBox.text = $"{counter:0000} : Move your device to capture more environment data: {createProgress:0%}";
            }

            bool success = false;

            feedbackBox.text = "Saving...";

            try
            {
                // Actually save
                await CloudManager.CreateAnchorAsync(cloudAnchor);

                // Store
                currentCloudAnchor = cloudAnchor;

                // Success?
                success = currentCloudAnchor != null;

                if (success && !isErrorActive)
                {
                    // Await override, which may perform additional tasks
                    // such as storing the key in the AnchorExchanger
                    await OnSaveCloudAnchorSuccessfulAsync();
                }
                else
                {
                    OnSaveCloudAnchorFailed(new Exception("Failed to save, but no exception was thrown."));
                }
            }
            catch (Exception ex)
            {
                OnSaveCloudAnchorFailed(ex);
            }
        }

        /// <summary>
        /// Spawns a new anchored object.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <param name="worldRot">The world rotation.</param>
        /// <returns><see cref="GameObject"/>.</returns>
        protected virtual GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot)
        {
            // Create the prefab
            GameObject newGameObject = GameObject.Instantiate(AnchoredObjectPrefab, worldPos, worldRot);

            // Attach a cloud-native anchor behavior to help keep cloud
            // and native anchors in sync.
            newGameObject.AddComponent<CloudNativeAnchor>();

            // Set the color
            newGameObject.GetComponent<MeshRenderer>().material.color = GetStepColor();
            
            // Return created object
            return newGameObject;
        }

        /// <summary>
        /// Spawns a new object.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <param name="worldRot">The world rotation.</param>
        /// <param name="cloudSpatialAnchor">The cloud spatial anchor.</param>
        /// <returns><see cref="GameObject"/>.</returns>
        protected virtual GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor)
        {
            // Create the object like usual
            GameObject newGameObject = SpawnNewAnchoredObject(worldPos, worldRot);

            ApplyCloudSpatialAnchor(cloudSpatialAnchor, newGameObject);

            // Set color
            newGameObject.GetComponent<MeshRenderer>().material.color = GetStepColor();
            // Return newly created object
            return newGameObject;
        }

        private static void ApplyCloudSpatialAnchor(CloudSpatialAnchor cloudSpatialAnchor, GameObject newGameObject)
        {
            // If a cloud anchor is passed, apply it to the native anchor
            if (cloudSpatialAnchor != null)
            {
                var cloudNativeAnchor = newGameObject.GetComponent<AnchorUpdate>();
                var handler = newGameObject.GetComponent<ManipulationHandler>();
                if (handler != null)
                {
                    handler.enabled = false;
                }

                cloudNativeAnchor.SetCloudSpatialAnchor(cloudSpatialAnchor);
            }
        }

        /// <summary>
        /// Spawns a new anchored object and makes it the current object or moves the
        /// current anchored object if one exists.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <param name="worldRot">The world rotation.</param>
        protected virtual void SpawnOrMoveCurrentAnchoredObject(Vector3 worldPos, Quaternion worldRot)
        {
            // Create the object if we need to, and attach the platform appropriate
            // Anchor behavior to the spawned object
            if (spawnedObject == null)
            {
                // Use factory method to create
                spawnedObject = SpawnNewAnchoredObject(worldPos, worldRot, currentCloudAnchor);

                // Update color
                spawnedObjectMat = spawnedObject.GetComponent<MeshRenderer>().material;
            }
            else
            {
                // Use factory method to move
                ApplyCloudSpatialAnchor(currentCloudAnchor, spawnedObject);
            }
        }

        #endregion

        protected struct DemoStepParams
        {
            public Color StepColor { get; set; }
            public string StepMessage { get; set; }
        }


        #region Public Properties
        /// <summary>
        /// Gets the prefab used to represent an anchored object.
        /// </summary>
        public GameObject AnchoredObjectPrefab { get { return anchoredObjectPrefab; } }

        /// <summary>
        /// Gets the <see cref="SpatialAnchorManager"/> instance used by this demo.
        /// </summary>
        public SpatialAnchorManager CloudManager { get { return cloudManager; } }
        #endregion // Public Properties
    }
}
