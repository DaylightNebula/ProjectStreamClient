using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{

    public float destroySeconds = 1f;

    float time;
    void Update()
    {
        time += Time.deltaTime;

        if (time > destroySeconds) Destroy(gameObject);
    }
}
