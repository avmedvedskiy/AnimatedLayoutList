using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnimatedLayoutList
{

    public struct ChildData
    {
        public RectTransform transform;
        public ILayoutIgnorer ignorer;
        public IAnimatedLayoutElement animatedElement;
        public Vector2 position;
        public Vector2 size;
        public bool isNew;

        public bool IsIgnored => ignorer?.ignoreLayout ?? false;
    }
}