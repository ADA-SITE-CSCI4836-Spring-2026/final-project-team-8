using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private readonly Stack<UIView> _viewStack = new();

    public void Show(UIView view)
    {
        if (_viewStack.Count > 0)
            _viewStack.Peek().Hide();

        _viewStack.Push(view);
        view.Show();
    }

    public void Hide()
    {
        if (_viewStack.Count == 0) return;

        _viewStack.Pop().Hide();

        if (_viewStack.Count > 0)
            _viewStack.Peek().Show();
    }

    public void HideAll()
    {
        while (_viewStack.Count > 0)
            _viewStack.Pop().Hide();
    }

    public T GetView<T>() where T : UIView
    {
        foreach (UIView view in _viewStack)
            if (view is T typed) return typed;
        return null;
    }
}

/// <summary>Base class for all UI screens/views.</summary>
public abstract class UIView : MonoBehaviour
{
    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
