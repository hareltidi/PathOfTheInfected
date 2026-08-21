using TidiTweening;
using UnityEngine;

public class TweenTest : MonoBehaviour
{
    private Vector3 _startScale;
    [SerializeField] public EaseType enterEaseType;
    [SerializeField] public EaseType exitEaseType;

    void Start()
    {
        _startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TidiTweenManager.TweenVector3(this, transform.localScale, _startScale * 2, 0.75f,
                (value) => { transform.localScale = value; }).SetEase(enterEaseType);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            TidiTweenManager
                .TweenVector3(this, transform.localScale, _startScale, 0.75f,
                    (value) => { transform.localScale = value; }).SetEase(exitEaseType);
        }
    }
}
