// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Linq;
using MixedRealityToolkit.Experimental.Extensions.UX;
using Microsoft.MixedReality.Toolkit.UI;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    /// <summary>
    /// Picks the appropriate UI game object to be used in the SharedAnchor demo.
    /// This allows us to have both HoloLens and Mobile UX in the same
    /// scene.
    /// </summary>
    public class XRUXPickerForCoarseRelocDemo : XRUXPicker
    {
        private static XRUXPickerForCoarseRelocDemo _Instance;
        public new static XRUXPickerForCoarseRelocDemo Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<XRUXPickerForCoarseRelocDemo>();
                }

                return _Instance;
            }
        }

        /// <summary>
        /// Gets the Toggle interactable used in the demo.
        /// </summary>
        /// <returns>The input field used in the demo.</returns>
        public Interactable[] GetToggleInteractable()
        {
            return RootTree.GetComponentsInChildren<Interactable>(true).Where(x=> x.ButtonMode == SelectionModes.Toggle).ToArray();
        }
    }
}