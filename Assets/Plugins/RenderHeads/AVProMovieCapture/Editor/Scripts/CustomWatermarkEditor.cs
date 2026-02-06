using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

//-----------------------------------------------------------------------------
// Copyright 2012-2025 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CustomWatermark), true)]
    public class CustomWatermarkEditor : UnityEditor.Editor
    {
        // Text
        private string[] positionNames = new string[9] {
            "Top Left", "Middle Left", "Bottom Left",
            "Top Centre", "Middle Centre", "Bottom Centre",
            "Top Right", "Middle Right", "Bottom Right",
        };
        private string _noPostActionWarning = "You Have not enabled \"Support Watermark\" " +
            "within the Capture component, you must enable this for Watermark to be drawn properly";
        private string _noCaptureComponentWarning = "No Capture Component Selected, Please" +
            "Select a Capture Component for Custom Watermark to be Injected.";
        private string _customMovementWarning = "This allows you to write a custom movement function" +
            "and hook into the Func: CustomMovementFunction, this requires C# knowledge, for more information" +
            "please visit the docs";

        // Actual Component
        CustomWatermark _master;

        // ALL
        private SerializedProperty _propCaptureComponent;
        // Camera + Screen + Camera360 
        private SerializedProperty _propTargetCamera;
        // Camera360 + Camera360ODS
        private SerializedProperty _proprenderToFace;
        // Camera + Screen + Camera360
        private SerializedProperty _propWatermarkMaterial;
        // Texture + WebcamTexture
        private SerializedProperty _propWatermarkBlitMaterial;
        // ALL
        private SerializedProperty _propWatermarkTexture;
        private SerializedProperty _propWatermarkSize;
        // Position Options:
        private SerializedProperty _propPositionOption;
        private SerializedProperty _propOffsetArea;
        private SerializedProperty _propRateOfChange;
        // Anchor
        private SerializedProperty _propAnchorPosition;
        // Lerp
        private SerializedProperty _propLerpSpeed;
        private SerializedProperty _propUseDeltaTime;
        // Curve
        private SerializedProperty _propCurveTraversalSpeed;
        private SerializedProperty _propCurve;
        private SerializedProperty _propLoop;
        // Custom Position
        private SerializedProperty _propPosition;

        // Component Options Anim
        private AnimBool _noCaptureComponent = new AnimBool();
        private AnimBool _cameraOption = new AnimBool();
        private AnimBool _screenOption = new AnimBool();
        private AnimBool _cam360Option = new AnimBool();
        private AnimBool _cam360ODSOption = new AnimBool();
        private AnimBool _textureOption = new AnimBool();
        private AnimBool _webcamTextureOption = new AnimBool();

        // Position Options Anim
        private AnimBool _anchorPosition = new AnimBool();
        private AnimBool _randomPosition = new AnimBool();
        private AnimBool _lerpRandomPosition = new AnimBool();
        private AnimBool _curveBasedPosition = new AnimBool();
        private AnimBool _customPosition = new AnimBool();
        private AnimBool _customFunction = new AnimBool();

        private void OnEnable()
        {
            _master = (CustomWatermark)target;
            
            _propCaptureComponent = serializedObject.AssertFindProperty("_captureComponent");

            _propTargetCamera = serializedObject.AssertFindProperty("_targetCamera");
            _proprenderToFace = serializedObject.AssertFindProperty("_renderToFace");

            _propWatermarkTexture = serializedObject.AssertFindProperty("_watermarkTexture");
            _propWatermarkMaterial = serializedObject.AssertFindProperty("_mat");
            _propWatermarkBlitMaterial = serializedObject.AssertFindProperty("_textureBlitMaterial");
            _propPositionOption = serializedObject.AssertFindProperty("_positionOption");
            _propWatermarkSize = serializedObject.AssertFindProperty("_watermarkSize");

            _propAnchorPosition = serializedObject.AssertFindProperty("_anchorPosition");

            _propOffsetArea = serializedObject.AssertFindProperty("_offsetArea");

            _propRateOfChange = serializedObject.AssertFindProperty("_rateOfChange");

            _propLerpSpeed = serializedObject.AssertFindProperty("_lerpSpeed");
            _propUseDeltaTime = serializedObject.AssertFindProperty("_useDeltaTime");

            _propCurveTraversalSpeed = serializedObject.AssertFindProperty("_curveTraversalSpeed");
            _propCurve = serializedObject.AssertFindProperty("_curve");
            _propLoop = serializedObject.AssertFindProperty("_loop");

            _propPosition = serializedObject.AssertFindProperty("_position");

            // Capture Type Anim
            _noCaptureComponent.valueChanged.AddListener(Repaint);
            _cameraOption.valueChanged.AddListener(Repaint);
            _screenOption.valueChanged.AddListener(Repaint);
            _cam360Option.valueChanged.AddListener(Repaint);
            _cam360ODSOption.valueChanged.AddListener(Repaint);
            _textureOption.valueChanged.AddListener(Repaint);
            _webcamTextureOption.valueChanged.AddListener(Repaint);
            // value set
            _noCaptureComponent.value = GetCurrentPosition() == -1;
            _cameraOption.value = GetCurrentPosition() == 0;
            _screenOption.value = GetCurrentPosition() == 1;
            _cam360Option.value = GetCurrentPosition() == 2;
            _cam360ODSOption.value = GetCurrentPosition() == 3;
            _textureOption.value = GetCurrentPosition() == 4;
            _webcamTextureOption.value = GetCurrentPosition() == 5;

            // Position Anim
            _anchorPosition.valueChanged.AddListener(Repaint);
            _randomPosition.valueChanged.AddListener(Repaint);
            _lerpRandomPosition.valueChanged.AddListener(Repaint);
            _curveBasedPosition.valueChanged.AddListener(Repaint);
            _customPosition.valueChanged.AddListener(Repaint);
            _customFunction.valueChanged.AddListener(Repaint);
            // value set
            _anchorPosition.value = _propPositionOption.enumValueIndex == 0;
            _randomPosition.value = _propPositionOption.enumValueIndex == 1;
            _lerpRandomPosition.value = _propPositionOption.enumValueIndex == 2;
            _curveBasedPosition.value = _propPositionOption.enumValueIndex == 3;
            _customPosition.value = _propPositionOption.enumValueIndex == 4;
            _customFunction.value = _propPositionOption.enumValueIndex == 5;
        }

        // Used by AnimBool to get current selected capture type (to show the correct info)
        private int GetCurrentPosition()
        {
            int result = 0;
            result = _master.GetCaptureType();
            return result;
        }

        private void OnDisable()
        {
            _anchorPosition.valueChanged.RemoveListener(Repaint);
            _randomPosition.valueChanged.RemoveListener(Repaint);
            _lerpRandomPosition.valueChanged.RemoveListener(Repaint);
            _curveBasedPosition.valueChanged.RemoveListener(Repaint);
            _customPosition.valueChanged.RemoveListener(Repaint);
            _customFunction.valueChanged.RemoveListener(Repaint);
            _noCaptureComponent.valueChanged.RemoveListener(Repaint);
            _cameraOption.valueChanged.RemoveListener(Repaint);
            _screenOption.valueChanged.RemoveListener(Repaint);
            _cam360Option.valueChanged.RemoveListener(Repaint);
            _cam360ODSOption.valueChanged.RemoveListener(Repaint);
            _textureOption.valueChanged.RemoveListener(Repaint);
            _webcamTextureOption.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            // Allways show
            EditorGUILayout.PropertyField(_propCaptureComponent);
            _noCaptureComponent.target = GetCurrentPosition() == -1;
            _cameraOption.target = GetCurrentPosition() == 0;
            _screenOption.target = GetCurrentPosition() == 1;
            _cam360Option.target = GetCurrentPosition() == 2;
            _cam360ODSOption.target = GetCurrentPosition() == 3;
            _textureOption.target = GetCurrentPosition() == 4;
            _webcamTextureOption.target = GetCurrentPosition() == 5;
            EditorGUILayout.Space(5);

            // No componented selected show warning and then nothing else, as will not function if capture component not selected
            if (EditorGUILayout.BeginFadeGroup(_noCaptureComponent.faded))
            {
                EditorGUILayout.HelpBox(_noCaptureComponentWarning, MessageType.Warning);
                EditorGUILayout.EndFadeGroup();
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUI.indentLevel++;
            // Capture from Camera
            if (EditorGUILayout.BeginFadeGroup(_cameraOption.faded))
            {
                EditorGUILayout.PropertyField(_propWatermarkMaterial);
                EditorGUILayout.PropertyField(_propTargetCamera);
            }
            EditorGUILayout.EndFadeGroup();
            // Capture from Screen
            if (EditorGUILayout.BeginFadeGroup(_screenOption.faded))
            {
                EditorGUILayout.PropertyField(_propWatermarkMaterial);
                EditorGUILayout.PropertyField(_propTargetCamera);
            }
            EditorGUILayout.EndFadeGroup();
            // Capture from Camera 360
            if (EditorGUILayout.BeginFadeGroup(_cam360Option.faded))
            {
                EditorGUILayout.PropertyField(_propWatermarkMaterial);
                EditorGUILayout.PropertyField(_propTargetCamera);
                EditorGUILayout.PropertyField(_proprenderToFace);
            }
            EditorGUILayout.EndFadeGroup();
            // Capture from Camera 360 ODS
            if (EditorGUILayout.BeginFadeGroup(_cam360ODSOption.faded))
            {
                if (!_master.DoesODSSupportWatermark())
                {
                    EditorGUILayout.HelpBox(_noPostActionWarning, MessageType.Warning);
                }
                EditorGUILayout.PropertyField(_proprenderToFace);
            }
            EditorGUILayout.EndFadeGroup();
            // Capture from Texture
            if (EditorGUILayout.BeginFadeGroup(_textureOption.faded))
            {
                EditorGUILayout.PropertyField(_propWatermarkBlitMaterial);
            }
            EditorGUILayout.EndFadeGroup();
            // Capture from WebCamTexture
            if (EditorGUILayout.BeginFadeGroup(_webcamTextureOption.faded))
            {
                EditorGUILayout.PropertyField(_propWatermarkBlitMaterial);
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space(10);
            EditorGUI.indentLevel--;
            // General Properties
            EditorGUILayout.PropertyField(_propWatermarkTexture);
            EditorGUILayout.PropertyField(_propWatermarkSize);

            // Position Options
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_propPositionOption);
            _anchorPosition.target = _propPositionOption.enumValueIndex == 0;
            _randomPosition.target = _propPositionOption.enumValueIndex == 1;
            _lerpRandomPosition.target = _propPositionOption.enumValueIndex == 2;
            _curveBasedPosition.target = _propPositionOption.enumValueIndex == 3;
            _customPosition.target = _propPositionOption.enumValueIndex == 4;
            _customFunction.target = _propPositionOption.enumValueIndex == 5;

            EditorGUI.indentLevel++;
            // Anchor Position
            if (EditorGUILayout.BeginFadeGroup(_anchorPosition.faded))
            {
                int gridSize = 3;
                float squareSize = 25f;
                float padding = 5f;

                EditorGUILayout.BeginHorizontal(GUILayout.Width(gridSize * gridSize));

                for (int row = 0; row < gridSize; row++)
                {
                    EditorGUILayout.BeginVertical();

                    for (int col = 0; col < gridSize; col++)
                    {
                        int index = row * gridSize + col;
                        Rect squareRect = GUILayoutUtility.GetRect(squareSize, squareSize);
                        squareRect.x += 15;
                        if (squareRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            _propAnchorPosition.intValue = index;
                            Event.current.Use(); // Consume the event to prevent further processing
                        }
                        UnityEngine.Color squareColor = (_propAnchorPosition.intValue == index) ? UnityEngine.Color.green : UnityEngine.Color.gray;
                        EditorGUI.DrawRect(squareRect, squareColor);
                        EditorGUILayout.Space(padding);

                        if (row == 2 && col == 1)
                        {
                            if (_propAnchorPosition.intValue != -1)
                            {
                                Rect labelRect = new Rect(squareRect.x + squareSize * 2, squareRect.y, 100, squareSize);
                                EditorGUI.LabelField(labelRect, positionNames[_propAnchorPosition.intValue]);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(padding);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();

            // Random Position
            if (EditorGUILayout.BeginFadeGroup(_randomPosition.faded))
            {
                EditorGUILayout.PropertyField(_propRateOfChange);
                EditorGUILayout.PropertyField(_propOffsetArea, label: new GUIContent("Area"));
            }
            EditorGUILayout.EndFadeGroup();
            // Lerp Position
            if (EditorGUILayout.BeginFadeGroup(_lerpRandomPosition.faded))
            {
                EditorGUILayout.PropertyField(_propLerpSpeed);
                EditorGUILayout.PropertyField(_propUseDeltaTime);
                EditorGUILayout.PropertyField(_propOffsetArea, label: new GUIContent("Area"));
            }
            EditorGUILayout.EndFadeGroup();
            // Curve Position
            if (EditorGUILayout.BeginFadeGroup(_curveBasedPosition.faded))
            {
                EditorGUILayout.PropertyField(_propCurveTraversalSpeed);
                EditorGUILayout.PropertyField(_propCurve);
                EditorGUILayout.PropertyField(_propLoop);
            }
            EditorGUILayout.EndFadeGroup();
            // Custom Position
            if (EditorGUILayout.BeginFadeGroup(_customPosition.faded))
            {
                EditorGUILayout.PropertyField(_propPosition);
            }
            EditorGUILayout.EndFadeGroup();
            // Custom Function
            if (EditorGUILayout.BeginFadeGroup(_customFunction.faded))
            {
                if (_master.CustomMovementFunc != null)
                {
                    var invocations = _master.CustomMovementFunc.GetInvocationList();
                    if (invocations.Length == 0)
                    {
                        EditorGUILayout.HelpBox(_customMovementWarning, MessageType.Warning);
                    }
                    else
                    {
                        for (int i = 0; i < invocations.Length; i++)
                        {
                            EditorGUILayout.LabelField($"Invocation {i}: {invocations[i].Method.Name}");
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(_customMovementWarning, MessageType.Warning);
                }
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
