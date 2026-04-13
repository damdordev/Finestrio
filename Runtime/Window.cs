using UnityEngine;

namespace Damdor.Finestrio
{
    public class Window : MonoBehaviour
    {
        IWindowAnimation GetDefaultWindowAnimation() => null;
        ITransitionAnimation GetDefaultAnimation() => null;
    }
}