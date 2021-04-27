using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEnemyBehaviour : MonoBehaviour
{
    public float attackRange = 5f;
    public float secondsBetweenAttacks = 5;
    public GameObject myProjectile;
    public Transform target = null;

    private Animator myAnimator;
    private float currWaitTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        currWaitTime = secondsBetweenAttacks;
        Room myRoom = transform.parent.gameObject.GetComponent<Room>();
        if (target == null)
            target = myRoom.roomNode.manager.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        currWaitTime = Mathf.Max(0f, currWaitTime - Time.deltaTime);
        if(currWaitTime == 0 && (transform.position - target.position).magnitude <= attackRange)
		{
            Fire();
            myAnimator.Play("RangeEnemy_Attack", 0, 0);
        }
    }

    private void Fire()
	{
        currWaitTime += secondsBetweenAttacks;
        GameObject yee = Instantiate(myProjectile);
        yee.transform.position = target.position;
    }
}
