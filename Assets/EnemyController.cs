using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private Collider2D Collider;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        Collider = GetComponent<Collider2D>();
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        CheckCollision();
    }

    private void CheckCollision()
    {
        //Debug.Log(GetComponentsInParent<Collider2D>().First().IsTouching(Collider));
        //if (GetComponentsInParent<Collider2D>().Count(c => c.IsTouching(Collider)) > 0)
        if (GetComponentsInParent<Collider2D>().Where(c => c.IsTouching(Collider)).Count() > 0)
            Debug.Log("HIT");
    }
}
