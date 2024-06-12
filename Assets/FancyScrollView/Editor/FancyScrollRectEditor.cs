/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace FancyScrollView
{
    [CustomEditor(typeof(FancyScrollRect))]
    [CanEditMultipleObjects]
    public class FancyScrollRectEditor : Editor
    {
        SerializedProperty _viewport;
        SerializedProperty _scrollDirection;
        SerializedProperty _movementType;
        SerializedProperty _elasticity;
        SerializedProperty _scrollSensitivity;
        SerializedProperty _inertia;
        SerializedProperty _decelerationRate;
        SerializedProperty _snap;
        SerializedProperty _snapEnable;
        SerializedProperty _snapVelocityThreshold;
        SerializedProperty _snapDuration;
        SerializedProperty _snapEasing;
        SerializedProperty _draggable;
        SerializedProperty _scrollbar;
        AnimBool _showElasticity;
        AnimBool _showInertiaRelatedValues;
        AnimBool _showSnapEnableRelatedValues;

        protected void OnEnable()
        {
            _snap = serializedObject.FindProperty("snap");
            _viewport = serializedObject.FindProperty("viewport");
            _scrollDirection = serializedObject.FindProperty("scrollDirection");
            _movementType = serializedObject.FindProperty("movementType");
            _elasticity = serializedObject.FindProperty("elasticity");
            _scrollSensitivity = serializedObject.FindProperty("scrollSensitivity");
            _inertia = serializedObject.FindProperty("inertia");
            _decelerationRate = serializedObject.FindProperty("decelerationRate");
            _snapEnable = serializedObject.FindProperty("snap.enable");
            _snapVelocityThreshold = serializedObject.FindProperty("snap.velocityThreshold");
            _snapDuration = serializedObject.FindProperty("snap.duration");
            _snapEasing = serializedObject.FindProperty("snap.easing");
            _draggable = serializedObject.FindProperty("draggable");
            _scrollbar = serializedObject.FindProperty("scrollbar");
            _showElasticity = new AnimBool(Repaint);
            _showInertiaRelatedValues = new AnimBool(Repaint);
            _showSnapEnableRelatedValues = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected void OnDisable()
        {
            _showElasticity.valueChanged.RemoveListener(Repaint);
            _showInertiaRelatedValues.valueChanged.RemoveListener(Repaint);
            _showSnapEnableRelatedValues.valueChanged.RemoveListener(Repaint);
        }

        protected void SetAnimBools(bool instant)
        {
            SetAnimBool(_showElasticity,
                !_movementType.hasMultipleDifferentValues && _movementType.enumValueIndex == (int)MovementType.Elastic,
                instant);
            SetAnimBool(_showInertiaRelatedValues, !_inertia.hasMultipleDifferentValues && _inertia.boolValue, instant);
            SetAnimBool(_showSnapEnableRelatedValues, !_snapEnable.hasMultipleDifferentValues && _snapEnable.boolValue,
                instant);
        }

        public override void OnInspectorGUI()
        {
            SetAnimBools(false);

            serializedObject.Update();
            EditorGUILayout.PropertyField(_viewport);
            EditorGUILayout.PropertyField(_scrollDirection);
            EditorGUILayout.PropertyField(_movementType);
            DrawMovementTypeRelatedValue();
            EditorGUILayout.PropertyField(_scrollSensitivity);
            EditorGUILayout.PropertyField(_inertia);
            DrawInertiaRelatedValues();
            EditorGUILayout.PropertyField(_draggable);
            EditorGUILayout.PropertyField(_scrollbar);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMovementTypeRelatedValue()
        {
            using (var group = new EditorGUILayout.FadeGroupScope(_showElasticity.faded))
            {
                if (!group.visible)
                {
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_elasticity);
                }
            }
        }

        private void DrawInertiaRelatedValues()
        {
            using (var group = new EditorGUILayout.FadeGroupScope(_showInertiaRelatedValues.faded))
            {
                if (!group.visible)
                {
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_decelerationRate);
                    EditorGUILayout.PropertyField(_snap);

                    using (new EditorGUI.IndentLevelScope())
                    {
                        DrawSnapRelatedValues();
                    }
                }
            }
        }

        private void DrawSnapRelatedValues()
        {
            if (!_snap.isExpanded)
            {
                return;
            }

            EditorGUILayout.PropertyField(_snapEnable);
            using (var group = new EditorGUILayout.FadeGroupScope(_showSnapEnableRelatedValues.faded))
            {
                if (!group.visible)
                {
                    return;
                }

                EditorGUILayout.PropertyField(_snapVelocityThreshold);
                EditorGUILayout.PropertyField(_snapDuration);
                EditorGUILayout.PropertyField(_snapEasing);
            }
        }

        private static void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
            {
                a.value = value;
            }
            else
            {
                a.target = value;
            }
        }
    }
}
#endif