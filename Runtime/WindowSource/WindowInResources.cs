using System;

namespace Damdor.Finestrio
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowInResources : Attribute
    {
        public string ResourcePath { get; private set; }

        public WindowInResources(string resourcePath)
        {
            ResourcePath = resourcePath;
        }
        
    }
}