using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;
    public float smoothing;
    //public Vector2 maximumPosition;
    //public Vector2 minimumPosition;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            else
                target = player.transform;
        }
        else
        {
            if (transform.position != target.position)
            {
                Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

                //targetPosition.x = Mathf.Clamp(targetPosition.x, minimumPosition.x, maximumPosition.x);
                //targetPosition.y = Mathf.Clamp(targetPosition.y, minimumPosition.y, maximumPosition.y);

                //transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing);
                transform.position = targetPosition;
            }
        }
    }
}
