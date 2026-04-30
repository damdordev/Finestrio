using UnityEngine;
using UnityEngine.UIElements;

namespace Damdor.Finestrio
{
    [RequireComponent(typeof(UIDocument))]
    public class UiElementsFinestrioWindow : MonoBehaviour, IFinestrioWindow
    {
        public bool IsTransparent => isTransparent;
        public int IndexOnStack { get; set; }
        
        [SerializeField] private bool isTransparent;

        public IWindowAnimation GetDefaultWindowAnimation(WindowAnimationType windowAnimationType) => null;

        public void SetVisible(bool visible) {}
    }
}