using UnityEngine;

//This class splits the difference between targets to keep them all in frame
//This hopefully covers the "Camera follows player" project requirement? :)
//Credit where it's due, ChatGPT helped a lot with figuring out the math on this one
public class DynamicCamera : MonoBehaviour
{
    //We keep an array of targets that we want to keep in frame
    public Transform[] targets;
    public float smoothTime = 0.2f;

    private float fixedX;
    private float fixedY;

    private float velocity;
    

    void Start()
    {
        //We want to keep a static angle so that the controls make sense
        //To that end, don't let the camera move on anything but the Z axis (left and right)
        fixedX = transform.position.x;
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        if (targets == null || targets.Length == 0)
            return;

        float center = GetCenter();
        

        float newDistance = Mathf.SmoothDamp(transform.position.z, center, ref velocity, smoothTime);

        transform.position = new Vector3(fixedX, fixedY, newDistance);
    }

    float GetCenter()
    {
        float minDistance = Mathf.Infinity;
        float maxDistance = Mathf.NegativeInfinity;
        

        foreach (Transform target in targets)
        {
            if (target == null) continue;

            minDistance = Mathf.Min(minDistance, target.position.z);
            maxDistance = Mathf.Max(maxDistance, target.position.z);
        }

        return (minDistance + maxDistance) / 2f;
    }
}