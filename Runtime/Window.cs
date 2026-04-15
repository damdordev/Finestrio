using UnityEngine;

namespace Damdor.Finestrio
{
    public class Window : MonoBehaviour
    {
        public bool IsTransparent => isTransparent;

        [SerializeField] private bool isTransparent;
        
        IWindowAnimation GetDefaultWindowAnimation() => null;
        ITransitionAnimation GetDefaultAnimation() => null;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}