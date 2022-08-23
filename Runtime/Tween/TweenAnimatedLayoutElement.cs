#if ANIMATED_LIST_TWEEN
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnimatedLayoutList
{
    public class TweenAnimatedLayoutElement : MonoBehaviour, IAnimatedLayoutElement
    {
        [SerializeField] private Ease _ease;
        [SerializeField] private float _animatedSpeed;

        [SerializeField] private float _newElementDelay;
        [SerializeField] private Ease _newElementEase;
        [SerializeField] private float _newElementAnimatedSpeed;
        [SerializeField] private Vector2 _newElementOffset;


        [SerializeField] private float _removeDelay;
        [SerializeField] private Ease _removeElementEase;
        [SerializeField] private float _removelementAnimatedSpeed;
        [SerializeField] private Vector2 _removeElementOffset;

        public void AnimateChangePositionElement(ref ChildData child)
        {
            child.transform.DOKill();
            child.transform.DOAnchorPos(child.position, _animatedSpeed)
                .SetEase(_ease);
        }

        public void AnimateNewElement(ref ChildData child)
        {
            child.transform.DOKill();
            child.transform.anchoredPosition = child.position + _newElementOffset;
            child.transform.DOAnchorPos(child.position, _newElementAnimatedSpeed)
                .SetDelay(_newElementDelay)
                .SetEase(_newElementEase);
        }

        [ContextMenu(nameof(RemoveElement))]
        public void RemoveElement()
        {
            var t = (RectTransform)transform;
            t.DOKill();
            t.DOAnchorPos(t.anchoredPosition + _removeElementOffset, _removelementAnimatedSpeed)
                .SetDelay(_removeDelay)
                .SetEase(_removeElementEase)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
#endif