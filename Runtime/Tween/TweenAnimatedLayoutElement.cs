#if ANIMATED_LIST_TWEEN
using DG.Tweening;
using UnityEngine;

namespace AnimatedLayoutList
{
    public class TweenAnimatedLayoutElement : MonoBehaviour, IAnimatedLayoutElement
    {
        [SerializeField] private Ease _ease;
        [SerializeField] private float _animatedSpeed;

        [SerializeField] private Ease _newElementEase;
        [SerializeField] private float _newElementAnimatedSpeed;
        [SerializeField] private Vector2 _newElementOffset;

        public void AnimateChangePositionElement(ref ChildData child)
        {
            child.transform.DOKill();
            child.transform.DOAnchorPos(child.position, _animatedSpeed).SetEase(_ease);
        }

        public void AnimateNewElement(ref ChildData child)
        {
            child.transform.DOKill();
            child.transform.anchoredPosition = child.position + _newElementOffset;
            child.transform.DOAnchorPos(child.position, _newElementAnimatedSpeed).SetEase(_newElementEase);
        }
    }
}
#endif