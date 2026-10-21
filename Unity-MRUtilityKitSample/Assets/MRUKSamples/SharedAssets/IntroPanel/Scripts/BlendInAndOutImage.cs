// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.UI;

public class BlendInAndOutImage : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve _curve = new(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(2, 1),
        new Keyframe(2.5f, 0), new Keyframe(4, 0));

    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
        _curve.postWrapMode = WrapMode.Loop;
    }

    private void Update()
    {
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _curve.Evaluate(Time.time));
    }
}
