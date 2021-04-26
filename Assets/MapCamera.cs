using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{

    public float speed;
    [SerializeField] MiniMap myMap;

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal1");
        float y = Input.GetAxis("Vertical1");
        Bounds b = myMap.bound;
        if(b != null)
		{
            transform.position += new Vector3(0f, y, 0f) * Time.deltaTime * speed;
            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, b.min.x, b.max.x), Mathf.Clamp(transform.localPosition.y, b.min.y, b.max.y), transform.localPosition.z);
        }
    }
}
