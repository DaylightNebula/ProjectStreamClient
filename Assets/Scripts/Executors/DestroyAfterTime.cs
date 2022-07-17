using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{

    public float destroySeconds = 1f;
    public Object objectToDestroy;

    float time;
    void Update()
    {
        time += Time.deltaTime;

        if (time > destroySeconds)
        {
            if (objectToDestroy != null) Destroy(objectToDestroy);
            Destroy(this);
        }
    }
}
