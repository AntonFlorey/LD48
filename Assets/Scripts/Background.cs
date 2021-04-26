using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{

    public GameObject[] layers;
    public Camera cam;
    public Transform subject;

    // Update is called once per frame
    void LateUpdate()
    {
        foreach(GameObject layer in layers) {
            HandleLayer(layer);
        }
    }

    void HandleLayer(GameObject layer) {
        float layerDepth = layer.transform.position.z;
        Vector2 travel = (Vector2)cam.transform.position;
        float distanceFromSubject = layerDepth - subject.position.z;
        float clippingPlane = cam.transform.position.z + (distanceFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane) - subject.position.z;
        
        float parallaxFactor = Mathf.Abs(distanceFromSubject) / clippingPlane;

        Vector2 updatedPosition = travel * parallaxFactor;

        // Simulate the position update by shifting the texture :)
        var renderer = layer.GetComponent<MeshRenderer>();
        updatedPosition = (Vector2)cam.transform.position - updatedPosition;
        renderer.material.SetTextureOffset("_MainTex", new Vector2(updatedPosition.x / transform.localScale.x , updatedPosition.y / transform.localScale.x));
    }
}
