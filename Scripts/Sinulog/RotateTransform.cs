using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTransform : MonoBehaviour
{
    [SerializeField]
    private List<Transform> targetTransforms;

    [SerializeField]
    private float angularSpeed = 10.0f;

    [SerializeField]
    private float offset = 10.0f;

    [SerializeField]
    public float offsetSpeed = 1.0f;

    private List<float> rotationOffsets;

    private void Awake()
    {
        rotationOffsets = new List<float>();
        GenerateRotationOffsets();
        ApplyInitialRotationOffsets();
    }

    private void Update()
    {
        RotateTransformsAroundYXAxis();
        UpdateGlobalOffset();
        UpdateRotationOffsetsList();
    }

    public void UpdateOffsetSpeed(float newOffsetSpeed)
    {
        offsetSpeed = newOffsetSpeed;
    }

    private void GenerateRotationOffsets()
    {
        rotationOffsets.Clear();
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            rotationOffsets.Add(offset * (i + 1));
        }
    }

    private void ApplyInitialRotationOffsets()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            targetTransforms[i].Rotate(rotationOffsets[i], rotationOffsets[i], 0);
        }
    }

    private void RotateTransformsAroundYXAxis()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            float angle = angularSpeed * Time.deltaTime;
            targetTransforms[i].Rotate(0, angle, 0);
        }
    }

    private void UpdateGlobalOffset()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            offset += offsetSpeed * (i + 1) * Time.deltaTime;
        }
    }

    private void UpdateRotationOffsetsList()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            rotationOffsets[i] = offset * (i + 1);
        }
    }
}
