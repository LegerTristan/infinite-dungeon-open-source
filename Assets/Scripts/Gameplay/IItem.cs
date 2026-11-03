using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public interface IItem
    {
        Sprite Sprite { get; }

        string Name { get; }

        string Description { get; }

        ItemType Type { get; }

        void Use(ItemUseContext context);
    }
}

