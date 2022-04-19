using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnimatedLayoutList
{
    public class AutoScrollToTarget : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private ScrollRect _scrollRect;

        private Vector2 _lastTargetPosition;

        private void OnEnable()
        {
            if(_target != null)
                _lastTargetPosition = _target.anchoredPosition;
        }

        private void Update()
        {
            if(_target == null)
                return;


            if (_lastTargetPosition != _target.anchoredPosition)
            {
                UpdateScrollToTarget();
                _lastTargetPosition = _target.anchoredPosition;
            }
        }

        public void UpdateScrollToTarget()
        {
            SetValue(_scrollRect,GetNextScrollValue());
        }

        private float GetNextScrollValue()
        {
            float contentHeight = _scrollRect.content.sizeDelta.y;
            float targetHeight = _target.sizeDelta.y;
            float targetPosition = Mathf.Abs(_target.anchoredPosition.y) + targetHeight / 2f;
            float position = 1f - targetPosition / contentHeight;

            float offset = targetHeight / contentHeight;

            position += offset * (position - 0.5f);
            return Mathf.Clamp01(position);
        }


        private float GetValue(ScrollRect scrollRect)
        {
            return scrollRect.horizontal
                ? scrollRect.horizontalNormalizedPosition
                : scrollRect.verticalNormalizedPosition;
        }

        private void SetValue(ScrollRect scrollRect, float value)
        {
            if (scrollRect.horizontal)
            {
                scrollRect.horizontalNormalizedPosition = value;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = value;
            }
        }
    }
}