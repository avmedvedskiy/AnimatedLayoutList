using System;
using UnityEngine;
using UnityEngine.UI;

namespace AnimatedLayoutList
{
    public class ScrollRectTarget : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private ScrollRect _scrollRect;

        public float _value;

        private void Update()
        {
            SetValue(_scrollRect, _value);
        }
        
        
        public float GetValue(ScrollRect scrollRect, float value)
        {
            return scrollRect.horizontal ?
                scrollRect.horizontalNormalizedPosition :
                scrollRect.verticalNormalizedPosition;
        }
 
        public void SetValue(ScrollRect scrollRect, float value)
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