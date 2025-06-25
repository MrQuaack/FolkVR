using UnityEngine;
using DG.Tweening;

public class FloatUpDown : MonoBehaviour
{
    public float moveDistance = 0.5f;  // How far up and down
    public float duration = 1.5f;      // Time it takes to go up or down

    void Start()
    {
        // Move up and down relative to current position
        transform.DOMoveY(transform.position.y + moveDistance, duration)
            .SetLoops(-1, LoopType.Yoyo)  // Loop forever, up and down
            .SetEase(Ease.InOutSine);     // Smooth motion
    }
}
