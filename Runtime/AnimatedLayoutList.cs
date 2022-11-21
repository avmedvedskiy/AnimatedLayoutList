using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnimatedLayoutList
{
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class AnimatedLayoutList : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        private enum LayoutType
        {
            Vertical,
            Horizontal
        }

        public float minWidth => _minSize.x;
        public float preferredWidth => _preferredSize.x;
        public float flexibleWidth => 0;
        public float minHeight => _minSize.y;
        public float preferredHeight => _preferredSize.y;
        public float flexibleHeight => 0;
        public int layoutPriority => 0;

        [SerializeField] private LayoutType _layoutType;
        [SerializeField] private RectOffset _padding = new RectOffset();
        [SerializeField] private float _spacing;

        [SerializeField] private TextAnchor _childAlignment = TextAnchor.MiddleCenter;

        private Vector2 _minSize;
        private Vector2 _preferredSize;
        private ChildData[] _childrenData;
        private DrivenRectTransformTracker _tracker;

        [System.NonSerialized] private RectTransform _rect;
        private RectTransform RectTransform => _rect ??= GetComponent<RectTransform>();

        private void GatherChildren()
        {
            if(!isActiveAndEnabled)
                return;
            
            //maybe need optimize
            _childrenData = transform
                .OfType<RectTransform>()
                .Where(x => x.gameObject.activeSelf)
                .Select(MapToChildData)
                .ToArray();

            ChildData MapToChildData(RectTransform rectTransform)
            {
                rectTransform.TryGetComponent<ILayoutIgnorer>(out var ignored);
                rectTransform.TryGetComponent<IAnimatedLayoutElement>(out var animatedLayoutElement);

                var data = new ChildData
                {
                    transform = rectTransform,
                    position = rectTransform.anchoredPosition,
                    size = rectTransform.sizeDelta,
                    ignorer = ignored,
                    animatedElement = animatedLayoutElement,
                    isNew = _childrenData != null &&
                            !_childrenData.Any(cd => ReferenceEquals(rectTransform, cd.transform)),
                    immediately = _childrenData == null
                };
                return data;
            }
        }

        private float GetStartOffset(int axis)
        {
            float requiredSpace = axis == 0 ? _padding.horizontal : _padding.vertical;
            float availableSpace = RectTransform.rect.size[axis];
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (axis == 0 ? _padding.left : _padding.top) + surplusSpace * alignmentOnAxis;
        }

        private float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
        {
            float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? _padding.horizontal : _padding.vertical);
            float availableSpace = RectTransform.rect.size[axis];
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (axis == 0 ? _padding.left : _padding.top) + surplusSpace * alignmentOnAxis;
        }

        private float GetAlignmentOnAxis(int axis)
        {
            if (axis == 0)
                return ((int)_childAlignment % 3) * 0.5f;
            else
                return ((int)_childAlignment / 3) * 0.5f;
        }

        public void CalculateLayoutInputHorizontal()
        {
            _minSize.x = 0;
            _preferredSize.x = 0;

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored || childData.transform == null)
                    continue;
                var childMinWidth = LayoutUtility.GetMinWidth(childData.transform);
                var childPreferredWidth = LayoutUtility.GetPreferredWidth(childData.transform);

                _minSize.x += childMinWidth + _spacing;
                _preferredSize.x += childPreferredWidth + _spacing;
            }

            _minSize.x += _padding.left + _padding.right;
            _preferredSize.x += _padding.left + _padding.right;
        }

        public void CalculateLayoutInputVertical()
        {
            _minSize.y = 0;
            _preferredSize.y = 0;

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored || childData.transform == null)
                    continue;
                var childMinHeight = LayoutUtility.GetMinHeight(childData.transform);
                var childPreferredHeight = LayoutUtility.GetPreferredHeight(childData.transform);

                _minSize.y += childMinHeight + _spacing;
                _preferredSize.y += childPreferredHeight + _spacing;
            }

            _minSize.y += _padding.top + _padding.bottom - _spacing;
            _preferredSize.y += _padding.top + _padding.bottom - _spacing;
        }

        public void SetLayoutVertical()
        {
            if (_layoutType != LayoutType.Vertical)
                return;

            _tracker.Clear();

            float requiredSize = _minSize[1] > _preferredSize[1]
                ? _minSize[1]
                : _preferredSize[1];

            float y = GetStartOffset(1, requiredSize);
            float x = GetStartOffset(0);

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored || childData.transform == null)
                    continue;

                var childMinHeight = LayoutUtility.GetMinHeight(childData.transform);
                var childPreferredHeight = LayoutUtility.GetPreferredHeight(childData.transform);

                //childData.size.x = size.x - _padding.left - _padding.right;
                childData.size.y = childPreferredHeight > childMinHeight
                    ? childPreferredHeight
                    : childMinHeight;

                childData.position.x = x + _padding.left;
                childData.position.y = -y;
                y += childData.size.y + _spacing;

                childData.pivot[0] = GetAlignmentOnAxis(0);
                childData.pivot[1] = 1;

                _tracker.Add(this, childData.transform,
                    DrivenTransformProperties.Anchors
                    | DrivenTransformProperties.Pivot
                    | DrivenTransformProperties.AnchoredPosition);
            }

            ApplyChildrenSizes();
        }

        public void SetLayoutHorizontal()
        {
            if (_layoutType != LayoutType.Horizontal)
                return;

            _tracker.Clear();

            float requiredSize = _minSize[0] > _preferredSize[0]
                ? _minSize[0]
                : _preferredSize[0];

            float x = GetStartOffset(0, requiredSize);
            float y = -GetStartOffset(1);

            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored || childData.transform == null)
                    continue;

                var childMinWidth = LayoutUtility.GetMinWidth(childData.transform);
                var childPreferredWidth = LayoutUtility.GetPreferredWidth(childData.transform);

                childData.size.x = childPreferredWidth > childMinWidth
                    ? childPreferredWidth
                    : childMinWidth;

                childData.position.y = y + _padding.top;
                childData.position.x = x;
                x += childData.size.x + _spacing;

                childData.pivot[0] = 0;
                childData.pivot[1] = 1 - GetAlignmentOnAxis(1);

                _tracker.Add(this, childData.transform,
                    DrivenTransformProperties.Anchors
                    | DrivenTransformProperties.Pivot
                    | DrivenTransformProperties.AnchoredPosition);
            }

            ApplyChildrenSizes();
        }


        private void ApplyChildrenSizes()
        {
            if (!Application.isPlaying)
                ApplyChildrenSizesImmediate();
            else
                ApplyChildrenSizesAnimated();
        }

        private void ApplyChildrenSizesImmediate()
        {
            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored || childData.transform == null)
                    continue;

                var childDataTransform = childData.transform;
                childDataTransform.anchorMin = Vector2.up;
                childDataTransform.anchorMax = Vector2.up;
                childDataTransform.pivot = childData.pivot;
                childDataTransform.sizeDelta = childData.size;

                childDataTransform.anchoredPosition = childData.position;
            }
        }

        private void ApplyChildrenSizesAnimated()
        {
            for (int i = 0; i < _childrenData.Length; i++)
            {
                ref var childData = ref _childrenData[i];
                if (childData.IsIgnored || childData.transform == null)
                    continue;

                var childDataTransform = childData.transform;
                childDataTransform.anchorMin = Vector2.up;
                childDataTransform.anchorMax = Vector2.up;
                childDataTransform.pivot = childData.pivot;
                childDataTransform.sizeDelta = childData.size;

                if (childData.isNew)
                {
                    AnimatedNewChild(ref childData);
                    childData.isNew = false;
                }
                else
                {
                    if (childData.position != childDataTransform.anchoredPosition)
                    {
                        AnimatedChildChangePosition(ref childData);
                    }
                }
            }
        }

        protected virtual void AnimatedChildChangePosition(ref ChildData child)
        {
            if (child.animatedElement == null || child.immediately)
            {
                child.transform.anchoredPosition = child.position;
            }
            else
            {
                child.animatedElement.AnimateChangePositionElement(ref child);
            }
        }

        protected virtual void AnimatedNewChild(ref ChildData child)
        {
            if (child.animatedElement == null || child.immediately)
            {
                child.transform.anchoredPosition = child.position;
            }
            else
            {
                child.animatedElement.AnimateNewElement(ref child);
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            //GatherChildren();
            SetDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
            _tracker.Clear();
        }

        private void OnTransformChildrenChanged()
        {
            //GatherChildren();
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

        private void SetDirty()
        {
            if (!IsActive())
                return;
            if(!_inProcess)
                StartCoroutine(DelayedMarkLayoutForRebuild((RectTransform)transform));
            /*
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                MarkLayoutForRebuild((RectTransform)transform);
            else
                StartCoroutine(DelayedMarkLayoutForRebuild((RectTransform)transform));
                */
        }

        private bool _inProcess;
        private void MarkLayoutForRebuild(RectTransform rectTransform)
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        IEnumerator DelayedMarkLayoutForRebuild(RectTransform rectTransform)
        {
            _inProcess = true;
            yield return null;
            GatherChildren();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            _inProcess = false;
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif
    }
}