using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public Ladder linkedWith;
    public bool waiting;

    public void Teleport(PlayerController play)
    {
        if (waiting) return;
        linkedWith.waiting = true;
        play.movePoint.position = linkedWith.transform.position;
        play.transform.position = linkedWith.transform.position;
    }
}
