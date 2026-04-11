namespace Damdor.Finestrio
{
    public interface IWindow
    {
        IWindowAnimation GetDefaultWindowAnimation();
        ITransitionAnimation GetDefaultAnimation();
    }
}