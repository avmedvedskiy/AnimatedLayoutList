#if ANIMATED_LIST_TWEEN
using DG.Tweening;
using UnityEngine;


namespace AnimatedLayoutList.Tween
{
    public class TweenLayoutList : BaseAnimatedLayoutList
    {
        [SerializeField] private Ease _ease;
        [SerializeField] private float _animatedSpeed;
        
        [SerializeField] private Ease _newElementEase;
        [SerializeField] private float _newElementAnimatedSpeed;
        [SerializeField] private Vector2 _newElementOffset;
        
        protected override void AnimatedChildChangePosition(ChildData child)
        {
            child.transform.DOKill();
            child.transform.DOAnchorPos(child.position, _animatedSpeed).SetEase(_ease);
        }

        protected override void AnimatedNewChild(ChildData child)
        {
            child.transform.DOKill();
            child.transform.anchoredPosition = child.position + _newElementOffset;
            child.transform.DOAnchorPos(child.position, _newElementAnimatedSpeed).SetEase(_newElementEase);
            
        }
    }
}

#endif