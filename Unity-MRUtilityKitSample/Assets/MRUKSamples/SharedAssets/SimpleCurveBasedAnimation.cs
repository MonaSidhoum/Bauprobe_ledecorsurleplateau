// Copyright (c) Meta Platforms, Inc. and affiliates.


using System.Collections;
using UnityEngine;

public class SimpleCurveBasedAnimation : MonoBehaviour
{
    [SerializeField] private AnimationCurve _positionX = new(new Keyframe(0, 0), new Keyframe(1, 0));
    [SerializeField] private AnimationCurve _positionY = new(new Keyframe(0, 0), new Keyframe(1, 0));
    [SerializeField] private AnimationCurve _positionZ = new(new Keyframe(0, 0), new Keyframe(1, 0));
    [SerializeField] private AnimationCurve _rotationX = new(new Keyframe(0, 0), new Keyframe(1, 0));
    [SerializeField] private AnimationCurve _rotationY = new(new Keyframe(0, 0), new Keyframe(1, 0));
    [SerializeField] private AnimationCurve _rotationZ = new(new Keyframe(0, 0), new Keyframe(1, 0));
    [SerializeField] private AnimationCurve _scaleX = new(new Keyframe(0, 1), new Keyframe(1, 1));
    [SerializeField] private AnimationCurve _scaleY = new(new Keyframe(0, 1), new Keyframe(1, 1));
    [SerializeField] private AnimationCurve _scaleZ = new(new Keyframe(0, 1), new Keyframe(1, 1));
    [SerializeField] private bool _playOnStart;
    [SerializeField] private bool _loop;

    [Tooltip("If enabled the object will start with the first frame of the animation baked in its transform.")]
    [SerializeField]
    private bool _bakeInitialState;

    public float _timeOffset;
    private AnimationCurve[] _allAnimCurves;
    private Vector3 startingLocalPosition;
    private Vector3 startingLocalEuler;

    private Vector3 startingLocalScale;

    // Start is called before the first frame update
    private void Start()
    {
        _allAnimCurves = new[]
            { _positionX, _positionY, _positionZ, _rotationX, _rotationY, _rotationZ, _scaleX, _scaleY, _scaleZ };
        startingLocalPosition = transform.localPosition;
        startingLocalEuler = transform.localEulerAngles;
        startingLocalScale = transform.localScale;
        if (_playOnStart)
        {
            StartCoroutine(Play());
        }

        if (_bakeInitialState)
        {
            transform.Translate(new Vector3(_positionX.Evaluate(0), _positionY.Evaluate(0), _positionZ.Evaluate(0)),
                Space.Self);
            transform.Rotate(new Vector3(_rotationX.Evaluate(0), _rotationY.Evaluate(0), _rotationZ.Evaluate(0)),
                Space.Self);
            transform.localScale = new Vector3(_scaleX.Evaluate(0) * startingLocalScale.x,
                _scaleY.Evaluate(0) * startingLocalScale.y, _scaleZ.Evaluate(0) * startingLocalScale.z);
        }
    }

    [ContextMenu("StartAnimation")]
    public void StartAnimation()
    {
        EnsureInitialized();
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    [ContextMenu("StartAnimationReversed")]
    public void ReverseAnimation()
    {
        EnsureInitialized();
        StopAllCoroutines();
        StartCoroutine(PlayReverse());
    }

    private IEnumerator Play()
    {
        var longestAnim = _positionX;
        foreach (var c in _allAnimCurves)
        {
            if (c.keys[c.keys.Length - 1].time > longestAnim.keys[longestAnim.keys.Length - 1].time)
            {
                longestAnim = c;
            }
        }

        var step = 0 + _timeOffset;
        if (!_loop)
        {
            while (step < longestAnim.keys[longestAnim.keys.Length - 1].time)
            {
                UpdateTransform(step);
                step += Time.deltaTime;
                yield return null;
            }

            GoToFinalState();
        }
        else
        {
            foreach (var c in _allAnimCurves)
            {
                if (c.postWrapMode != WrapMode.Loop)
                {
                    c.postWrapMode = WrapMode.Loop;
                    c.preWrapMode = WrapMode.Loop;
                }
            }

            while (true)
            {
                UpdateTransform(step);
                step += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator PlayReverse()
    {
        var longestAnim = FindLongestAnimCurve();
        var step = longestAnim.keys[longestAnim.keys.Length - 1].time + _timeOffset;
        if (!_loop)
        {
            while (step > 0)
            {
                UpdateTransform(step);
                step -= Time.deltaTime;
                yield return null;
            }

            GoToInitialState();
        }
        else
        {
            foreach (var c in _allAnimCurves)
            {
                if (c.postWrapMode != WrapMode.Loop)
                {
                    c.postWrapMode = WrapMode.Loop;
                    c.preWrapMode = WrapMode.Loop;
                }
            }

            while (true)
            {
                UpdateTransform(step);
                print(step);
                step -= Time.deltaTime;
                yield return null;
            }
        }
    }

    private void UpdateTransform(float step)
    {
        transform.localPosition = startingLocalPosition;
        transform.Translate(
            new Vector3(_positionX.Evaluate(step), _positionY.Evaluate(step), _positionZ.Evaluate(step)), Space.Self);

        transform.localEulerAngles = startingLocalEuler;
        transform.Rotate(new Vector3(_rotationX.Evaluate(step), _rotationY.Evaluate(step), _rotationZ.Evaluate(step)),
            Space.Self);

        transform.localScale = new Vector3(_scaleX.Evaluate(step) * startingLocalScale.x,
            _scaleY.Evaluate(step) * startingLocalScale.y, _scaleZ.Evaluate(step) * startingLocalScale.z);
    }

    private void EnsureInitialized()
    {
        if (_allAnimCurves == null)
        {
            _allAnimCurves = new[]
                { _positionX, _positionY, _positionZ, _rotationX, _rotationY, _rotationZ, _scaleX, _scaleY, _scaleZ };
            startingLocalPosition = transform.localPosition;
            startingLocalEuler = transform.localEulerAngles;
            startingLocalScale = transform.localScale;
        }
    }

    public void StopAndReset()
    {
        print("Stop and reset");
        StopAllCoroutines();
        GoToInitialState();
    }

    private AnimationCurve FindLongestAnimCurve()
    {
        var longestAnim = _positionX;
        foreach (var c in _allAnimCurves)
        {
            if (c.keys[c.keys.Length - 1].time > longestAnim.keys[longestAnim.keys.Length - 1].time)
            {
                longestAnim = c;
            }
        }

        return longestAnim;
    }

    private void GoToInitialState()
    {
        if (_bakeInitialState)
        {
            transform.Translate(new Vector3(_positionX.Evaluate(0), _positionY.Evaluate(0), _positionZ.Evaluate(0)),
                Space.Self);
            transform.Rotate(new Vector3(_rotationX.Evaluate(0), _rotationY.Evaluate(0), _rotationZ.Evaluate(0)),
                Space.Self);
            transform.localScale = new Vector3(_scaleX.Evaluate(0) * startingLocalScale.x,
                _scaleY.Evaluate(0) * startingLocalScale.y, _scaleZ.Evaluate(0) * startingLocalScale.z);
        }
        else
        {
            transform.localPosition = startingLocalPosition;
            transform.localEulerAngles = startingLocalEuler;
            transform.localScale = startingLocalScale;
        }
    }

    private void GoToFinalState()
    {
        var longestAnimCurve = FindLongestAnimCurve();
        var lastKeyFrame = longestAnimCurve.keys[longestAnimCurve.keys.Length - 1].time;
        transform.localPosition = startingLocalPosition + new Vector3(_positionX.Evaluate(lastKeyFrame),
            _positionY.Evaluate(lastKeyFrame), _positionZ.Evaluate(lastKeyFrame));
        transform.localEulerAngles = startingLocalEuler + new Vector3(_rotationX.Evaluate(lastKeyFrame),
            _rotationY.Evaluate(lastKeyFrame), _rotationZ.Evaluate(lastKeyFrame));
        transform.localScale = new Vector3(_scaleX.Evaluate(lastKeyFrame) * startingLocalScale.x,
            _scaleY.Evaluate(lastKeyFrame) * startingLocalScale.y,
            _scaleZ.Evaluate(lastKeyFrame) * startingLocalScale.z);
    }
}
