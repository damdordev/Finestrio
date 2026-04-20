using UnityEngine;
using UnityEngine.UIElements;

namespace Damdor.Finestrio
{
    [RequireComponent(typeof(UIDocument))]
    public class UiElementsWindow : Window
    {
        public override bool IsTransparent => isTransparent;
        public override int IndexOnStack { get; set; }
        
        [SerializeField] private bool isTransparent;

        public override IWindowAnimation GetDefaultWindowAnimation(WindowAnimationType windowAnimationType) => null;

        public override void SetVisible(bool visible) {}
    }
}