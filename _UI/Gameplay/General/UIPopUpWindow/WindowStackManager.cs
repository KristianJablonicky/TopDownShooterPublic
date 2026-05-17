using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowStackManager : SingletonMonoBehaviour<WindowStackManager>
{
    public bool WindowsOpen { get; private set; } = false; 
    public event Action WindowOpened, AllWindowsClosed;
    public event Action<bool> WindowsVisibilityChanged;

    private Stack<PopUpWindowBase> windowStack;

    protected override void OverriddenAwake()
    {
        base.OverriddenAwake();
        windowStack = new();
        enabled = false;
    }

    public void AddWindow(PopUpWindowBase newWindow)
    {
        if (windowStack.Count > 0)
        {
            var window = windowStack.Peek();
            window.BackgroundFade.SetActive(false);
        }
        windowStack.Push(newWindow);
        enabled = true;
        WindowsOpen = true;

        if (windowStack.Count == 1)
        {
            WindowOpened?.Invoke();
            WindowsVisibilityChanged?.Invoke(true);
        }
    }
    public void CloseWindow()
    {
        if (windowStack.Count == 0) return;
        windowStack.Peek().GetDeactivated();
        windowStack.Pop();
        if (windowStack.Count < 1)
        {
            enabled = false;
            WindowsOpen = false;
            AllWindowsClosed?.Invoke();
            WindowsVisibilityChanged?.Invoke(false);
        }
        else
        {
            var window = windowStack.Peek();
            window.BackgroundFade.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWindow();
        }
    }
}
