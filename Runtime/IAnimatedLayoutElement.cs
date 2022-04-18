namespace AnimatedLayoutList
{
    public interface IAnimatedLayoutElement
    {
        void AnimateNewElement(ref ChildData child);
        void AnimateChangePositionElement(ref ChildData child);
    }
}
