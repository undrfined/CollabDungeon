using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private int ticks;
    public LayerMask coliderMask;
    public bool neutral = false;
    public PlayerController target;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void Tick()
    {
        if (neutral)
        {
            var mult = Random.Range(0, 2) == 0 ? -1 : 1;
            var to = Random.Range(0, 2) == 0 ? new Vector3(1f * mult, 0f, 0f) : new Vector3(0f, 1f * mult, 0f);

            while (Physics2D.OverlapCircle(transform.position + to, 0.2f, coliderMask))
            {
                mult = Random.Range(0, 2) == 0 ? -1 : 1;
                to = Random.Range(0, 2) == 0 ? new Vector3(1f * mult, 0f, 0f) : new Vector3(0f, 1f * mult, 0f);
            }
            transform.position += to;
        } else
        {
            var position = target;
            var to = target.movePoint.position - transform.position;
            if (to.x == 0f && to.y == 0f) return;
            if (Mathf.Abs(to.x) > Mathf.Abs(to.y))
            {
                to = new Vector3(to.x > 0 ? 1f : -1f, 0f, 0f);
                if(!Physics2D.OverlapCircle(transform.position + to, 0.2f, coliderMask))
                {
                    transform.position += to;
                }
            } else
            {
                to = new Vector3(0f, to.y > 0 ? 1f : -1f, 0f);
                if (!Physics2D.OverlapCircle(transform.position + to, 0.2f, coliderMask))
                {
                    transform.position += to;
                }
            }
        }

    }
}
