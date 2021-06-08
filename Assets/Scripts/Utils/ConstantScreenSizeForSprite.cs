using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantScreenSizeForSprite : MonoBehaviour
{
    public float mOriginalCameraSize = 10.0f;
    public Camera Camera;

    void LateUpdate()
    {
        transform.localScale = Camera.orthographicSize / mOriginalCameraSize * Vector3.one;
    }
}
