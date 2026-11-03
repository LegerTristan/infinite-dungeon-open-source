using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance = null;

        public static T Instance => instance;

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[Singleton] => Instance of {typeof(T).Name} already exists on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
