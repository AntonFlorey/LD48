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
    [SerializeField] private float attackSpeed = 0.2f;
    [SerializeField] private float projectileRange = 20f;
    [SerializeField] private float attackSpacing = 0.5f;

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
            StartCoroutine(Fire());
            myAnimator.Play("RangeEnemy_Attack", 0, 0);
        }
    }

    private IEnumerator Fire()
	{
        currWaitTime += secondsBetweenAttacks;
        Vector2 direction = (target.position - transform.position).normalized;
        float currLength = attackSpacing;
        while (currLength <= projectileRange)
		{
            Debug.Log("test");
            var v = new Vector2(transform.position.x, transform.position.y) + direction * currLength;
            GameObject yee = Instantiate(myProjectile, new Vector3(v.x, v.y, myProjectile.transform.position.z), Quaternion.identity);
            currLength += attackSpacing;
            yield return new WaitForSeconds(attackSpeed);
		}
        yield return null;
    }
}
