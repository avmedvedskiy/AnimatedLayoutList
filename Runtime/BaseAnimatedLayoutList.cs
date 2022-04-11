using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnimatedLayoutList
{

    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public abstract class BaseAnimatedLayoutList : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        [SerializeField] private Vector4 _padding;
        [SerializeField] private float _spacing;

        private Vector2 _minSize;
        private Vector2 _preferredSize;
        private ChildData[] _childrenData = Array.Empty<ChildData>();
        protected DrivenRectTransformTracker _tracker;

        public float minWidth { get; }
        public float preferredWidth { get; }
        public float flexibleWidth { get; }
        public float minHeight { get; }
        public float preferredHeight { get; }
        public float flexibleHeight { get; }
        public int layoutPriority { get; }


        private void GatherChildren()
        {
            _childrenData = transform
                .OfType<RectTransform>()
                .Select(MapToChildData)
                .ToArray();

            ChildData MapToChildData(RectTransform rectTransform)
            {
                rectTransform.TryGetComponent<ILayoutIgnorer>(out var ignored);

                return new ChildData()
                {
                    transform = rectTransform,
                    ignorer = ignored,
                    isNew = !_childrenData.Any(cd => ReferenceEquals(rectTransform, cd.transform))
                };
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GatherChildren();
            SetDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _tracker.Clear();
        }

        private void OnTransformChildrenChanged()
        {
            GatherChildren();
            SetDirty();
        }

        private void SetDirty()
        {
            if (!IsActive())
                return;
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
            else
                StartCoroutine(DelayedSetDirty((RectTransform)transform));
        }

        IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            yield return null;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            SetDirty();
        }

        public void CalculateLayoutInputHorizontal()
        {
            _minSize.x = 0;
            _preferredSize.x = 0;

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored)
                    continue;
                var childMinWidth = LayoutUtility.GetMinWidth(childData.transform);
                var childPreferredWidth = LayoutUtility.GetPreferredWidth(childData.transform);

                if (childMinWidth > _minSize.x)
                    _minSize.x = childMinWidth;

                if (childPreferredWidth > _preferredSize.x)
                    _preferredSize.x = childPreferredWidth;
            }

            _minSize.x += _padding.x + _padding.z;
            _preferredSize.x += _padding.x + _padding.z;
        }

        public void CalculateLayoutInputVertical()
        {
            _minSize.y = 0;
            _preferredSize.y = 0;

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored)
                    continue;
                var childMinHeight = LayoutUtility.GetMinHeight(childData.transform);
                var childPreferredHeight = LayoutUtility.GetPreferredHeight(childData.transform);

                _minSize.y += childMinHeight + _spacing;
                _preferredSize.y += childPreferredHeight + _spacing;
            }

            _minSize.y += _padding.y + _padding.w - _spacing;
            _preferredSize.y += _padding.y + _padding.w - _spacing;
        }

        public void SetLayoutHorizontal()
        {
        }

        public void SetLayoutVertical()
        {
            _tracker.Clear();
            var size = ((RectTransform)transform).rect.size;
            var y = _padding.y;

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored)
                    continue;

                var childMinHeight = LayoutUtility.GetMinHeight(childData.transform);
                var childPreferredHeight = LayoutUtility.GetPreferredHeight(childData.transform);

                childData.size.x = size.x - _padding.x - _padding.z;
                childData.size.y = childPreferredHeight > childMinHeight
                    ? childPreferredHeight
                    : childMinHeight;

                childData.position.x = _padding.x;
                childData.position.y = -y;
                y += childData.size.y + _spacing;

                _tracker.Add(this, childData.transform,
                    DrivenTransformProperties.Anchors
                    | DrivenTransformProperties.Pivot
                    | DrivenTransformProperties.AnchoredPosition
                    | DrivenTransformProperties.SizeDelta);
            }

            ApplyChildrenSizes();
        }

        private void ApplyChildrenSizes()
        {
            if (!UnityEngine.Application.isPlaying)
                ApplyChildrenSizesImmediate();
            else
                ApplyChildrenSizesAnimated();

        }

        private void ApplyChildrenSizesImmediate()
        {
            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored)
                    continue;

                childData.transform.anchorMin = Vector2.up;
                childData.transform.anchorMax = Vector2.up;
                childData.transform.pivot = Vector2.up;

                childData.transform.sizeDelta = childData.size;
                childData.transform.anchoredPosition = childData.position;
            }
        }

        private void ApplyChildrenSizesAnimated()
        {
            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored)
                    continue;

                var childTransform = childData.transform;
                childTransform.anchorMin = Vector2.up;
                childTransform.anchorMax = Vector2.up;
                childTransform.pivot = Vector2.up;

                childTransform.sizeDelta = childData.size;

                if (childData.isNew)
                {
                    AnimatedNewChild(childData);
                    childData.isNew = false;
                }
                else
                {
                    if (childData.position != childTransform.anchoredPosition)
                    {
                        AnimatedChildChangePosition(childData);
                    }

                }
            }
        }

        protected abstract void AnimatedChildChangePosition(ChildData child);
        protected abstract void AnimatedNewChild(ChildData child);
    }
}