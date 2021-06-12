using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is a 2d camera manipulator for 2d scenes.
/// </summary>

public class CameraManiipulator2D : MonoBehaviour
{
    public Camera mCamera;
    public FixedTouchField mTouchField;

    public float mPanSpeed = 0.5f;

    private float mCameraSizeMax;// = 100.0f;
    private float mCameraSizeMin = 1.0f;

    public bool PanMode { get; set; } = true;

    #region UI variables
    public Slider mSliderZoom;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mCameraSizeMax = mCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if(PanMode)
        {
            float x = mTouchField.TouchDist.x * Time.deltaTime * mPanSpeed * (1.1f - mSliderZoom.value);
            float y = mTouchField.TouchDist.y * Time.deltaTime * mPanSpeed * (1.1f - mSliderZoom.value);

            mCamera.transform.position -= new Vector3(x, y, 0.0f);
        }
    }

    public void Zoom(float value)
    {
        mCamera.orthographicSize = mCameraSizeMax - value * (mCameraSizeMax - mCameraSizeMin);
    }

    public void Pan()
    {

    }

    #region UI functions
    public void OnSliderChanged()
    {
        Zoom(mSliderZoom.value);
    }
    #endregion
}
