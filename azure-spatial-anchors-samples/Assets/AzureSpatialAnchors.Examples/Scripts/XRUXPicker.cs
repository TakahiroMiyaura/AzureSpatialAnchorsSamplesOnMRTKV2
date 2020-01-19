// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    /// <summary>
    /// Picks the appropriate UI game object to be used. 
    /// This allows us to have both HoloLens and Mobile UX in the same
    /// scene.
    /// </summary>
    public class XRUXPicker : MonoBehaviour
    {

        private static XRUXPicker _Instance;
        public static XRUXPicker Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<XRUXPicker>();
                }

                return _Instance;
            }
        }

        public GameObject RootTree;

#if UNITY_IOS || UNITY_ANDROID
        void Start()
        {
            RootTree.transform.localScale = RootTree.transform.localScale * 2f;
        }
#endif

        /// <summary>
        /// Gets the correct feedback text control for the demo
        /// </summary>
        /// <returns>The feedback text control if it found it</returns>
        public TextMeshPro GetFeedbackText()
        {
            GameObject sourceTree = null;

            sourceTree = RootTree;

            int childCount = sourceTree.transform.childCount;
            for (int index = 0; index < childCount; index++)
            {
                GameObject child = sourceTree.transform.GetChild(index).gameObject;
                TextMeshPro t = child.GetComponent<TextMeshPro>();
                if (t != null)
                {
                    return t;
                }

            }

            Debug.LogError("Did not find feedback text control.");
            return null;
        }
        
        /// <summary>
        /// Gets the buttons used in the demo.
        /// </summary>
        /// <returns>The buttons used in the demo.</returns>
        public PressableButton[] GetDemoButtons()
        {
            return RootTree.GetComponentsInChildren<PressableButton>(true);
        }
    }
}
