// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using Meta.XR.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

namespace Meta.XR.MRUtilityKitSamples.KeyboardTracker
{
    [MetaCodeSample("MRUKSample-KeyboardTracker")]
    public class Logger : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _text;

        [SerializeField]
        TMP_Text _supportText;

        [SerializeField]
        int _maxLines = 10;

        List<string> _logs = new();

        int _count;

        void Start()
        {
            _supportText.text = $"Keyboard tracking {(OVRAnchor.TrackerConfiguration.KeyboardTrackingSupported ? "supported" : "NOT supported")}." +
                                $"\nScene permission {(Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission) ? "granted" : "NOT granted")}.";
        }

        void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        void OnLogMessageReceived(string condition, string trace, LogType type)
        {
            _logs.Insert(0, $"{++_count}> {(type == LogType.Log ? condition : $"[{type}] {condition}")}");

            if (_logs.Count > _maxLines)
            {
                _logs.RemoveRange(_maxLines, _logs.Count - _maxLines);
            }

            _text.text = string.Join(Environment.NewLine, _logs);
        }
    }
}
