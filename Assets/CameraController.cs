using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
	public PlayerController playeer;

    public float lerpSpeed;

	private Camera myCam;

	private void Start()
	{
		myCam = this.GetComponent<Camera>();
	}

	void Update()
    {

    }

	private void LateUpdate()
	{
		FollowPlayer();
	}

	void FollowPlayer()
	{
		if (playeer.currentRoomNode.myRoom == null)
			Debug.Log("ohhh noooo!!!");
		transform.position = Vector3.Lerp(transform.position, new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z), lerpSpeed);
		// Clamp dis shit

		Bounds b = playeer.currentRoomNode.myRoom.getBounds();
		float widthOffset = myCam.orthographicSize * myCam.aspect;
		float heightOffset = myCam.orthographicSize;
		float clampX = Mathf.Clamp(transform.position.x, b.min.x + widthOffset, b.max.x - widthOffset);
		float clampY = Mathf.Clamp(transform.position.y, b.min.y + heightOffset, b.max.y - heightOffset);
		transform.position = new Vector3(clampX, clampY, transform.position.z);
	}
}
