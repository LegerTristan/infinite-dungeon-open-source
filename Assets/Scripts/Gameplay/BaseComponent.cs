using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public abstract class BaseComponent : MonoBehaviour
    {
        protected abstract bool IsValid();

        protected virtual void Start()
        {
            if (!IsValid())
            {
                Debug.LogError($"[{GetType().ToString()}] Component not valid ! Something is missing in  {gameObject.name}!");
                enabled = false;
                gameObject.SetActive(false);
            }
        }
    }
}

