using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }


    public void Activate(PlayerController player)
    {
        anim.SetTrigger("Activate");
        player.TakeDamage(1);
    }
}
