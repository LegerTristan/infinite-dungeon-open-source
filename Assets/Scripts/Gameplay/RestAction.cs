using System;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    [Serializable]
    public class RestAction
    {
        Action callback = null;

        [SerializeField]
        string actionName = null;

        [SerializeField, TextArea]
        string actionDescription = null;

        public string ActionName => actionName;

        public string ActionDescription => actionDescription;

        public void BindToAction(Action newCallback)
        {
            callback = newCallback;
        }

        public void Execute()
        {
            callback?.Invoke();
        }
    }

}