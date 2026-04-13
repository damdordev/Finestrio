#if DAMDOR_FINESTRIO_ADDRESSABLES

using System;

namespace Damdor.Finestrio
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowInAddressables : Attribute
    {
        public string Key { get; private set; }

        public WindowInAddressables(string key)
        {
            Key = key;
        }
        
    }
}

#endif