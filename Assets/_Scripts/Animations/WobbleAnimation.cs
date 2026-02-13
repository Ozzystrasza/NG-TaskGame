using UnityEngine;
using DG.Tweening;

public class WobbleAnimation : MonoBehaviour
{
    [Header("Wobble")]
    public float angle = 3f;
    public float duration = 0.12f;

    [Header("Interval")]
    public float minInterval = 4f;
    public float maxInterval = 7f;

    Vector3 baseRotation;
    Sequence currentSeq;
    bool stoppedForever = false;

    void Start()
    {
        baseRotation = transform.eulerAngles;
        StartLoop();
    }

    void StartLoop()
    {
        if (stoppedForever) return;

        currentSeq = DOTween.Sequence();

        currentSeq.AppendInterval(Random.Range(minInterval, maxInterval));

        currentSeq.Append(
            transform.DORotate(baseRotation + new Vector3(0, 0, angle), duration)
                .SetEase(Ease.OutSine)
        );

        currentSeq.Append(
            transform.DORotate(baseRotation + new Vector3(0, 0, -angle), duration)
                .SetEase(Ease.OutSine)
        );

        currentSeq.Append(
            transform.DORotate(baseRotation, duration)
        );

        currentSeq.OnComplete(StartLoop);
        currentSeq.SetLink(gameObject);
    }

    public void StopForever()
    {
        stoppedForever = true;
        currentSeq?.Kill();
        transform.DORotate(baseRotation, 0.15f);
    }

    public void Restart()
    {
        stoppedForever = false;
        currentSeq?.Kill();
        StartLoop();
    }
}
