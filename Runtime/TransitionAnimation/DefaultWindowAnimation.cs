using System;
using UnityEngine;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Provides standard window animations configured for different transition types.
    /// Typically used as a serialized field in a base window class to assign generic animations from the Inspector.
    /// </summary>
    [Serializable]
    public class DefaultWindowAnimation
    {
        [SerializeField] private MonoWindowAnimation add;
        [SerializeField] private MonoWindowAnimation remove;
        [SerializeField] private MonoWindowAnimation pause;
        [SerializeField] private MonoWindowAnimation resume;

        /// <summary>
        /// Retrieves the predefined animation for the specified transition state.
        /// </summary>
        /// <param name="type">The type of animation state required (Add, Remove, Pause, Resume).</param>
        /// <returns>An <see cref="IWindowAnimation"/> configured for the requested state.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an unknown <see cref="WindowAnimationType"/> is provided.</exception>
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
