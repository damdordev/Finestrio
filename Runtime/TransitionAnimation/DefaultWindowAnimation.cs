using System;
using UnityEngine;

namespace Damdor.Finestrio
{
    [Serializable]
    public class DefaultWindowAnimation
    {
        [SerializeField] private MonoWindowAnimation add;
        [SerializeField] private MonoWindowAnimation remove;
        [SerializeField] private MonoWindowAnimation pause;
        [SerializeField] private MonoWindowAnimation resume;

        public IWindowAnimation GetDefaultAnimation(WindowAnimationType type) => type switch
        {
            WindowAnimationType.Add => add,
            WindowAnimationType.Remove => remove,
            WindowAnimationType.Pause => pause,
            WindowAnimationType.Resume => resume,
            _ => throw new ArgumentOutOfRangeException($"Unknown window animation type: {type}")
        };
    }
}