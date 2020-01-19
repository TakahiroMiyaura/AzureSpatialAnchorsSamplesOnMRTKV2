// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Linq;
using UnityEngine;

public class SensorSettings : MonoBehaviour
{
    private static SensorSettings instance;

    public GameObject Contents;

    private ToggleSensorType[] sensorCheckers;

    public static SensorSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SensorSettings>();
            }

            return instance;
        }
    }

    public bool UseGeoLocation
    {
        get
        {
            bool result = false;
            foreach (var toggleSensorType in sensorCheckers)
            {
                if (toggleSensorType.CheckType == UpdateSensorState.Sensor.GeoLocation)
                {
                    result = toggleSensorType.isToggleOn;
                    break;
                }
            }
            return result;
        }
    }


    public bool UseWifi
    {
        get
        {
            bool result = false;
            foreach (var toggleSensorType in sensorCheckers)
            {
                if (toggleSensorType.CheckType == UpdateSensorState.Sensor.Wifi)
                {
                    result = toggleSensorType.isToggleOn;
                    break;
                }
            }
            return result;
        }
    }


    public bool UseBluetooth
    {
        get
        {
            bool result = false;
            foreach (var toggleSensorType in sensorCheckers)
            {
                if (toggleSensorType.CheckType == UpdateSensorState.Sensor.Bluetooth)
                {
                    result = toggleSensorType.isToggleOn;
                    break;
                }
            }
            return result;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        sensorCheckers = GetComponentsInChildren<ToggleSensorType>(true);
        Contents = gameObject.transform.GetChild(0).gameObject;
    }
}