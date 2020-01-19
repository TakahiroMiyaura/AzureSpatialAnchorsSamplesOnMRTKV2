// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class CoarseRelocSettings
    {
        /// <summary>
        /// Whitelist of Bluetooth-LE beacons used to find anchors and improve the locatability
        /// of existing anchors.
        /// Add the UUIDs for your own Bluetooth beacons here to use them with Azure Spatial Anchors.
        /// </summary>
        public static readonly string[] KnownBluetoothProximityUuids =
        {
            "0000180a-0000-1000-8000-00805f9b34fb",
            "ee0c2080-8786-40ba-ab96-99b91ac981d8",
            "e20a39f4-73f5-4bc4-a12f-17d1ad07a961"
            //"e1f54e02-1e23-44e0-9c3d-512eb56adec9",
            //"01234567-8901-2345-6789-012345678903",
        };
    }
}