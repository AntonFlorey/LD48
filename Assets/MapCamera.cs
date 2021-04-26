using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{

    public float speed;
    [SerializeField] MiniMap myMap;
    private Vector3 initialLocalPos;

    private void Start()
    {
        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        Bounds b = myMap.bound;
        if (b == null)
            return;  // not loaded.
        var depth = myMap.myManager.currentStage.stageDepth;
        var maxDepth = myMap.myManager.levelStageDepth;
        var yPos = myMap.spacing / 2f * (maxDepth / 2f - depth);
        var newPos = new Vector3(0, yPos, 0);
        transform.localPosition = new Vector3(Mathf.Clamp(newPos.x, b.min.x, b.max.x), 
            Mathf.Clamp(newPos.y, b.min.y, b.max.y), transform.localPosition.z);
        // Debug.Log(depth+ " "+yPos + " " + initialLocalPos);
    }
}
