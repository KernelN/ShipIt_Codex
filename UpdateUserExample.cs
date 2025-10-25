using System;
using ShipIt.TickManaging;
using UnityEngine;

public class UpdateUserExample : MonoBehaviour
{
    const float SlowUpdateTime = .4f;
    const int QuickUpdateTicks = 3;
    const float LateUpdateTime = .1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateManager tm = UpdateManager.inst;
        tm.SuscribeToScaled(SlowUpdateTime, SlowUpdate);
        tm.SuscribeToScaled(QuickUpdateTicks, QuickUpdate);
        tm.SuscribeToLateScaled(LateUpdateTime, _LateUpdate);
    }

    void OnDestroy()
    {
        UpdateManager tm = UpdateManager.inst;
        tm.RemoveFromScaled(SlowUpdateTime, SlowUpdate);
        tm.RemoveFromScaled(QuickUpdateTicks, QuickUpdate);
        tm.RemoveFromLateScaled(LateUpdateTime, _LateUpdate);
    }

    void SlowUpdate () { Debug.Log("Tick on slow update"); }

    //Only use Unity's Update() and LaterUpdate() for things that require constant visual update 
    //(eg. smooth translations, rotations, etc)
    void Update()
    {
        transform.Rotate(Vector3.up, 10f * Time.deltaTime);
    }

    void QuickUpdate() { Debug.Log("Tick on quick update"); }
    
    //Uses _ to avoid conflict with Unity's LateUpdate()
    void _LateUpdate() { Debug.Log("Tick on late update"); }
}
