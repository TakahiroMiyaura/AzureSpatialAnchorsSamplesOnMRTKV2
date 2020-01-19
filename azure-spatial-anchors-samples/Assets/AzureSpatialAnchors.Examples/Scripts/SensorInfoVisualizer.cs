using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorInfoVisualizer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_IOS || UNITY_ANDROID
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif
    }
}
