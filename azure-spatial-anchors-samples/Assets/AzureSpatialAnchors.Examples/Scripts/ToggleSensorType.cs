// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Linq;
using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ToggleSensorType : MonoBehaviour
{

    private Interactable interactableObj;
    private UpdateSensorState sensorState;

    public UpdateSensorState.Sensor CheckType;

    public bool isToggleOn
    {
        get { return interactableObj.CurrentDimension == 1; }
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        interactableObj = GetComponent<Interactable>();
    }
}