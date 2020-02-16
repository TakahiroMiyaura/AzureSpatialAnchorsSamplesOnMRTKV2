// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Experimental.Extensions.UX;
using UnityEngine;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class AzureSpatialAnchorsIgniteDemoScript : DemoScriptBase
    {
        public enum DemoModes
        {
            CreateAnchorsMode=0,
            LocateAnchorMode

        }

        internal enum AppState
        {
            DemoStepSettingMode = 0,
            DemoStepCreateSession,
            DemoStepConfigSession,
            DemoStepStartSession,
            DemoStepCreateLocationProvider,
            DemoStepConfigureSensors,
            DemoStepSettingExplanationText,
            DemoStepCreateLocalAnchor,
            DemoStepSaveCloudAnchor,
            DemoStepSavingCloudAnchor,
            DemoStepReplay,
            DemoStepStopSession,
            DemoStepCreateSessionForQuery,
            DemoStepStartSessionForQuery,
            DemoStepLookForAnchorsNearDevice,
            DemoStepLookingForAnchorsNearDevice,
            DemoStepStopWatcher,
            DemoStepStopSessionForQuery,
            DemoStepComplete
        }

        #region Member Variables
        private readonly Dictionary<AppState, DemoStepParams> stateParams = new Dictionary<AppState, DemoStepParams>
        {
            { AppState.DemoStepSettingMode,new DemoStepParams() { StepMessage = "Set to demo mode.", StepColor = Color.clear }},
            { AppState.DemoStepCreateSession,new DemoStepParams() { StepMessage = "Next: Create Azure Spatial Anchors Session", StepColor = Color.clear }},
            { AppState.DemoStepConfigSession,new DemoStepParams() { StepMessage = "Next: Configure Azure Spatial Anchors Session", StepColor = Color.clear }},
            { AppState.DemoStepStartSession,new DemoStepParams() { StepMessage = "Next: Start Azure Spatial Anchors Session", StepColor = Color.clear }},
            { AppState.DemoStepCreateLocationProvider,new DemoStepParams() { StepMessage = "Next: Create Location Provider", StepColor = Color.clear }},
            { AppState.DemoStepConfigureSensors,new DemoStepParams() { StepMessage = "Next: Configure Sensors", StepColor = Color.clear }},
            { AppState.DemoStepCreateLocalAnchor,new DemoStepParams() { StepMessage = "Tap a surface to add the Local Anchor.\nAnd Manipulate the Local Anchor position.", StepColor = Color.blue }},
            { AppState.DemoStepSettingExplanationText,new DemoStepParams() { StepMessage = "Input an explanation text.", StepColor = Color.blue }},
            { AppState.DemoStepSaveCloudAnchor,new DemoStepParams() { StepMessage = "Next: Save Local Anchor to cloud", StepColor = Color.yellow }},
            { AppState.DemoStepSavingCloudAnchor,new DemoStepParams() { StepMessage = "Saving local Anchor to cloud...", StepColor = Color.yellow }},
            { AppState.DemoStepReplay,new DemoStepParams() { StepMessage = "Success to the anchor locate.", StepColor = Color.green }},
            { AppState.DemoStepStopSession,new DemoStepParams() { StepMessage = "Next: Stop Azure Spatial Anchors Session", StepColor = Color.green }},
            { AppState.DemoStepCreateSessionForQuery,new DemoStepParams() { StepMessage = "Next: Create Azure Spatial Anchors Session for query", StepColor = Color.clear }},
            { AppState.DemoStepStartSessionForQuery,new DemoStepParams() { StepMessage = "Next: Start Azure Spatial Anchors Session for query", StepColor = Color.clear }},
            { AppState.DemoStepLookForAnchorsNearDevice,new DemoStepParams() { StepMessage = "Next: Look for Anchors near device", StepColor = Color.clear }},
            { AppState.DemoStepLookingForAnchorsNearDevice,new DemoStepParams() { StepMessage = "Looking for Anchors near device...", StepColor = Color.clear }},
            { AppState.DemoStepStopWatcher,new DemoStepParams() { StepMessage = "Next: Stop Watcher", StepColor = Color.yellow }},
            { AppState.DemoStepStopSessionForQuery,new DemoStepParams() { StepMessage = "Next: Stop Azure Spatial Anchors Session for query", StepColor = Color.grey }},
            { AppState.DemoStepComplete,new DemoStepParams() { StepMessage = "Next: Restart demo", StepColor = Color.clear }}
        };

        private AppState _currentAppState = AppState.DemoStepSettingMode;
        private List<string> locateAnchorIdentifers = new List<string>();
        
        AppState currentAppState
        {
            get
            {
                return _currentAppState;
            }
            set
            {
                if (_currentAppState != value)
                {
                    Debug.LogFormat("State from {0} to {1}", _currentAppState, value);
                    _currentAppState = value;
                    if (spawnedObjectMat != null)
                    {
                        spawnedObjectMat.color = stateParams[_currentAppState].StepColor;
                    }

                    if (!isErrorActive)
                    {
                        feedbackBox.text = stateParams[_currentAppState].StepMessage;
                    }
                    EnableCorrectUIControls();
                }
            }
        }

        private PlatformLocationProvider locationProvider;
        private List<GameObject> allDiscoveredAnchors = new List<GameObject>();
        private DemoModes demoMode = DemoModes.CreateAnchorsMode;
        private SystemKeyboardInputHelper keyboardInputHelper;

        public SensorStatus GeoLocationStatus
        {
            get
            {
                if (locationProvider == null)
                    return SensorStatus.MissingSensorFingerprintProvider;
                if (!locationProvider.Sensors.GeoLocationEnabled)
                    return SensorStatus.DisabledCapability;
                switch (locationProvider.GeoLocationStatus)
                {
                    case GeoLocationStatusResult.Available:
                        return SensorStatus.Available;
                    case GeoLocationStatusResult.DisabledCapability:
                        return SensorStatus.DisabledCapability;
                    case GeoLocationStatusResult.MissingSensorFingerprintProvider:
                        return SensorStatus.MissingSensorFingerprintProvider;
                    case GeoLocationStatusResult.NoGPSData:
                        return SensorStatus.NoData;
                    default:
                        return SensorStatus.MissingSensorFingerprintProvider;
                }
            }
        }

        public SensorStatus WifiStatus
        {
            get
            {
                if (locationProvider == null)
                    return SensorStatus.MissingSensorFingerprintProvider;
                if (!locationProvider.Sensors.WifiEnabled)
                    return SensorStatus.DisabledCapability;
                switch (locationProvider.WifiStatus)
                {
                    case WifiStatusResult.Available:
                        return SensorStatus.Available;
                    case WifiStatusResult.DisabledCapability:
                        return SensorStatus.DisabledCapability;
                    case WifiStatusResult.MissingSensorFingerprintProvider:
                        return SensorStatus.MissingSensorFingerprintProvider;
                    case WifiStatusResult.NoAccessPointsFound:
                        return SensorStatus.NoData;
                    default:
                        return SensorStatus.MissingSensorFingerprintProvider;
                }
            }
        }

        public SensorStatus BluetoothStatus
        {
            get
            {
                if (locationProvider == null)
                    return SensorStatus.MissingSensorFingerprintProvider;
                if (!locationProvider.Sensors.BluetoothEnabled)
                    return SensorStatus.DisabledCapability;
                switch (locationProvider.BluetoothStatus)
                {
                    case BluetoothStatusResult.Available:
                        return SensorStatus.Available;
                    case BluetoothStatusResult.DisabledCapability:
                        return SensorStatus.DisabledCapability;
                    case BluetoothStatusResult.MissingSensorFingerprintProvider:
                        return SensorStatus.MissingSensorFingerprintProvider;
                    case BluetoothStatusResult.NoBeaconsFound:
                        return SensorStatus.NoData;
                    default:
                        return SensorStatus.MissingSensorFingerprintProvider;
                }
            }
        }

        #endregion

        #region Unity Inspector Variables

        #endregion

        #region MonoBehaviour Functions

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public override void Start()
        {

            Debug.Log(">>Azure Spatial Anchors Demo Script Start");

            base.Start();

            if (!SanityCheckAccessConfiguration())
            {
                return;
            }
            feedbackBox.text = stateParams[currentAppState].StepMessage;

            Debug.Log("Azure Spatial Anchors Demo script started");

            enableAdvancingOnSelect = false;

            EnableCorrectUIControls();
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (spawnedObjectMat != null)
            {
                float rat = 0.1f;
                float createProgress = 0f;
                if (CloudManager.SessionStatus != null)
                {
                    createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
                }
                rat += (Mathf.Min(createProgress, 1) * 0.9f);
                spawnedObjectMat.color = GetStepColor() * rat;
            }
        }

        #endregion

        #region CloudManger Functions

        protected override void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            base.OnCloudAnchorLocated(args);
            Debug.Log("Located:"+args.Status+" , "+args.Identifier);
            if (args.Status == LocateAnchorStatus.Located)
            {
                CloudSpatialAnchor cloudAnchor = args.Anchor;
                

                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    currentAppState = AppState.DemoStepStopWatcher;
                    Pose anchorPose = Pose.identity;
                    
#if UNITY_ANDROID || UNITY_IOS
                    // anchorPose = currentCloudAnchor.GetPose();
#endif

                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.
                    GameObject spawnedObject = SpawnNewAnchoredObject(anchorPose.position, anchorPose.rotation, cloudAnchor);
                    allDiscoveredAnchors.Add(spawnedObject);
                    anchorIdsToLocate.Add(args.Identifier);
                });
            }
        }

        protected override async Task OnSaveCloudAnchorSuccessfulAsync()
        {
            await base.OnSaveCloudAnchorSuccessfulAsync();

            Debug.Log("Anchor created, yay!");

            // Sanity check that the object is still where we expect
            Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
            anchorPose = currentCloudAnchor.GetPose();
#endif
            // HoloLens: The position will be set based on the unityARUserAnchor that was located.

            SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

            currentAppState = AppState.DemoStepReplay;
        }

        protected override void OnSaveCloudAnchorFailed(Exception exception)
        {
            base.OnSaveCloudAnchorFailed(exception);
        }

        private void ConfigureSession()
        {
            const float distanceInMeters = 8.0f;
            const int maxAnchorsToFind = 25;
            SetNearDevice(distanceInMeters, maxAnchorsToFind);
        }

        private void ConfigureSensors()
        { 
            locationProvider.Sensors.GeoLocationEnabled = SensorPermissionHelper.HasGeoLocationPermission();
            locationProvider.Sensors.WifiEnabled = SensorPermissionHelper.HasWifiPermission();
            locationProvider.Sensors.BluetoothEnabled = SensorPermissionHelper.HasBluetoothPermission();
            locationProvider.Sensors.KnownBeaconProximityUuids = CoarseRelocSettings.KnownBluetoothProximityUuids;

        }

        #endregion

        #region UI Events

        public async override Task AdvanceDemoAsync()
        {
            if (currentAppState == AppState.DemoStepSettingMode)
            {
                switch (demoMode)
                {
                    case DemoModes.CreateAnchorsMode:
                        currentAppState = AppState.DemoStepCreateSession;
                        break;
                    case DemoModes.LocateAnchorMode:
                        currentAppState = AppState.DemoStepCreateSessionForQuery;
                        break;
                }
                return;
            }
            switch (demoMode)
            {
                case DemoModes.CreateAnchorsMode:
                    await CreateAnchorsDemoAsync();
                    break;

                case DemoModes.LocateAnchorMode:
                    await LocateAnchorsDemoAsync();
                    break;

            }

        }

        private async Task LocateAnchorsDemoAsync()
        {
            switch (currentAppState)
            {
                case AppState.DemoStepCreateSessionForQuery:
                    CleanupSpawnedObjects();
                    currentCloudAnchor = null;
                    if (CloudManager.Session == null)
                    {
                        await CloudManager.CreateSessionAsync();
                        ConfigureSession();
                        await CloudManager.StartSessionAsync();
                    }
                    else
                    {
                        CloudManager.StopSession();
                        await CloudManager.ResetSessionAsync();
                        ConfigureSession();
                    }
                    locationProvider = new PlatformLocationProvider();
                    CloudManager.Session.LocationProvider = locationProvider;
                    SensorPermissionHelper.RequestSensorPermissions();
                    ConfigureSensors();
                    currentAppState = AppState.DemoStepLookForAnchorsNearDevice;
                    break;
                case AppState.DemoStepLookForAnchorsNearDevice:
                    currentAppState = AppState.DemoStepLookingForAnchorsNearDevice;
                    currentWatcher = CreateWatcher();
                    break;
                case AppState.DemoStepLookingForAnchorsNearDevice:
                    break;
                case AppState.DemoStepStopWatcher:
                    if (currentWatcher != null)
                    {
                        currentWatcher.Stop();
                        currentWatcher = null;
                    }
                    break;
                case AppState.DemoStepStopSessionForQuery:
                    CloudManager.StopSession();
                    currentWatcher = null;
                    locationProvider = null;
                    currentAppState = AppState.DemoStepComplete;
                    break;
                case AppState.DemoStepComplete:
                    currentCloudAnchor = null;
                    currentAppState = AppState.DemoStepSettingMode;
                    CleanupSpawnedObjects();
                    break;
                default:
                    Debug.Log("Shouldn't get here for app state " + currentAppState.ToString());
                    break;
            }
        }

        private async Task CreateAnchorsDemoAsync()
        {
            switch (currentAppState)
            {
                case AppState.DemoStepCreateSession:
                    CleanupSpawnedObjects();
                    currentCloudAnchor = null;
                    if (CloudManager.Session == null)
                    {
                        await CloudManager.CreateSessionAsync();
                        ConfigureSession();
                        await CloudManager.StartSessionAsync();
                    }
                    else
                    {
                        CloudManager.StopSession();
                        await CloudManager.ResetSessionAsync();
                        ConfigureSession();
                    }
                    locationProvider = new PlatformLocationProvider();
                    CloudManager.Session.LocationProvider = locationProvider;
                    
                    SensorPermissionHelper.RequestSensorPermissions();
                    ConfigureSensors();
                    currentAppState = AppState.DemoStepCreateLocalAnchor;
                    break;
                case AppState.DemoStepCreateLocalAnchor:
                    if (spawnedObject != null)
                    {
                        currentAppState = AppState.DemoStepSettingExplanationText;
                    }

                    break;
                case AppState.DemoStepSettingExplanationText:
                    keyboardInputHelper = spawnedObject.GetComponentInChildren<SystemKeyboardInputHelper>();
                    if (keyboardInputHelper != null && !string.IsNullOrEmpty(keyboardInputHelper.text))
                    {
                        explanationText = new Dictionary<string, string>();
                        explanationText.Add("Explanation",keyboardInputHelper.text);
                        keyboardInputHelper = null;
                        currentAppState = AppState.DemoStepSaveCloudAnchor;
                    }
                    break;
                case AppState.DemoStepSaveCloudAnchor:
                    currentAppState = AppState.DemoStepSavingCloudAnchor;
                    await SaveCurrentObjectAnchorToCloudAsync();
                    currentAppState = AppState.DemoStepReplay;
                    spawnedObject = null;
                    currentCloudAnchor = null;
                    break;
                case AppState.DemoStepReplay:
                    break;
                case AppState.DemoStepStopSession:
                    CloudManager.StopSession();
                    CleanupSpawnedObjects();
                    await CloudManager.ResetSessionAsync();
                    locationProvider = null;
                    currentAppState = AppState.DemoStepComplete;
                    break;
                case AppState.DemoStepComplete:
                    currentCloudAnchor = null;
                    currentAppState = AppState.DemoStepSettingMode;
                    CleanupSpawnedObjects();
                    break;
            }
        }

        public void SetReplay()
        {
            currentAppState = AppState.DemoStepCreateLocalAnchor;
            AdvanceDemo();
        }

        public void SetNextStep()
        {
            currentAppState = AppState.DemoStepStopSession;
            AdvanceDemo();

        }

        public void SetCreateAnchorsMode()
        {
            demoMode = DemoModes.CreateAnchorsMode;
            
        }

        public void SetLocateAnchorMode()
        {
            demoMode = DemoModes.LocateAnchorMode;
        }

        public void OnApplicationFocus(bool focusStatus)
        {
#if UNITY_ANDROID
            // We may get additional permissions at runtime. Enable the sensors once app is resumed
            if (focusStatus && locationProvider != null)
            {
                ConfigureSensors();
            }
#endif
        }

        public override void OnSelectInteraction()
        {
            if(spawnedObject == null)
            { 
                base.OnSelectInteraction();
            }
        }


        #endregion

        #region Other Functions

        private void EnableCorrectUIControls()
        {
            int continueButtonIndex = 2;
            int finishButtonIndex = 3;

            switch (currentAppState)
            {
                case AppState.DemoStepSettingMode:
                    XRUXPickerForCoarseRelocDemo.Instance.GetToggleInteractable()[0].gameObject.SetActive(true);
                    break;
                case AppState.DemoStepCreateSession:
                case AppState.DemoStepCreateSessionForQuery:
                    XRUXPickerForCoarseRelocDemo.Instance.GetToggleInteractable()[0].gameObject.SetActive(false);
                    break;
            }


            var pressableButtons = XRUXPickerForCoarseRelocDemo.Instance.GetDemoButtons();
            switch (currentAppState)
            {
                case AppState.DemoStepReplay:
                    pressableButtons[continueButtonIndex].gameObject.SetActive(true);
                    pressableButtons[finishButtonIndex].gameObject.SetActive(true);
                    break;
                default:
                    pressableButtons[continueButtonIndex].gameObject.SetActive(false);
                    pressableButtons[finishButtonIndex].gameObject.SetActive(false);
                    break;
            }
        }


        protected override GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot,
            CloudSpatialAnchor cloudSpatialAnchor)
        {
            var spwnObject = base.SpawnNewAnchoredObject(worldPos, worldRot, cloudSpatialAnchor);

            if (demoMode == DemoModes.LocateAnchorMode)
            {
                var systemKeyboardInputHelper = spwnObject.GetComponentInChildren<SystemKeyboardInputHelper>();
                if (cloudSpatialAnchor != null
                    && cloudSpatialAnchor.AppProperties != null
                    && cloudSpatialAnchor.AppProperties.ContainsKey("Explanation"))
                {

                    Debug.Log("Error :" + cloudSpatialAnchor.AppProperties["Explanation"]);
                    systemKeyboardInputHelper.text = cloudSpatialAnchor.AppProperties["Explanation"];
                    systemKeyboardInputHelper.enabled = false;
                }
                else
                {
                    systemKeyboardInputHelper.gameObject.SetActive(false);
                }
            }

            return spwnObject;
        }

        protected override bool IsPlacingObject()
        {
            return currentAppState == AppState.DemoStepCreateLocalAnchor;
        }

        protected override Color GetStepColor()
        {
            return stateParams[currentAppState].StepColor;
        }

        protected override void CleanupSpawnedObjects()
        {
            base.CleanupSpawnedObjects();

            foreach (GameObject anchor in allDiscoveredAnchors)
            {
                DestroyImmediate(anchor);
            }
            allDiscoveredAnchors.Clear();
        }

        #endregion
    }
}