using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectToLocationOverTime : MonoBehaviour
{
    Vector3 target;
    Vector3 moveDelta;
    float timeTarget;

    public float currentTime = 0f;
    public void init(Vector3 target, float timeTarget)
    {
        this.target = target;
        this.timeTarget = timeTarget;
        moveDelta = target - transform.position;
    }

    void Update()
    {
        // update current time tracker
        currentTime += Time.deltaTime;

        // move object towards target
        Vector3 move = moveDelta * (Time.deltaTime / timeTarget);
        transform.position += move;

        // remove if we have been alive for long enough
        if (currentTime > timeTarget)
            Destroy(this);
    }
}
