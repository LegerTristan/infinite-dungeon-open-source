using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public abstract class BaseTag : ScriptableObject
    {
        [SerializeField]
        string tagName = null;

        [SerializeField, Range(1, 5)]
        int repeatDelay = 1;

        public string TagName => tagName;

        public int RepeatDelay => repeatDelay;

        protected abstract TagType TagType { get; }

        public bool IsTagType(TagType type)
        {
            return TagType == type;
        }

        public override string ToString()
        {
            return TagName;
        }
    }
}
