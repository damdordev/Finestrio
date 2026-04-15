using UnityEngine;

namespace Damdor.Finestrio
{
    [RequireComponent(typeof(Canvas))]
    public class Window : MonoBehaviour
    {
        public bool IsTransparent => isTransparent;

        public int IndexOnStack
        {
            get => GetCanvas().sortingOrder;
            set => GetCanvas().sortingOrder = value;
        }

        [SerializeField] private bool isTransparent;
        [SerializeField] private DefaultWindowAnimation defaultAnimations;

        private Canvas canvas;

        public IWindowAnimation GetDefaultWindowAnimation(WindowAnimationType windowAnimationType) 
            => defaultAnimations.GetDefaultAnimation(windowAnimationType);

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private Canvas GetCanvas()
        {
            if(canvas == null) canvas = GetComponent<Canvas>();
            return canvas;
        }
        
    }
}