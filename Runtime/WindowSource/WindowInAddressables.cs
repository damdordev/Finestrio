#if DAMDOR_FINESTRIO_ADDRESSABLES

using System;

namespace Damdor.Finestrio
{
    /// <summary>
    /// Attribute used to decorate a <see cref="IFinestrioWindow"/> class with its corresponding Addressable asset key.
    /// Enables <see cref="AddressableWindowSource"/> to automatically locate the prefab.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowInAddressables : Attribute
    {
        /// <summary>
        /// The Addressable key identifying the prefab asset.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Applies the Addressable key mapping to a specific window.
        /// </summary>
        /// <param name="key">The exact string matching the asset's Addressable name/label.</param>
        public WindowInAddressables(string key)
        {
            Key = key;
        }
    }
}

#endif
