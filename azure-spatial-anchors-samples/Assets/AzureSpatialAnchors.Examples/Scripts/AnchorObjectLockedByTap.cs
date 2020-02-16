// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class AnchorObjectLockedByTap : MonoBehaviour
{
    public bool IsLock;


    public void SetLockState(BaseInputEventData data)
    {
        IsLock = !IsLock;
    }
}