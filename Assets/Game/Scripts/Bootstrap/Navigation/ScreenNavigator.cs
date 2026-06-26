using System;
using System.Collections.Generic;
using Game.Scripts.Core.Settings;
using UnityEngine;
using VContainer;

namespace Game.Scripts.Bootstrap.Navigation
{
    public sealed class ScreenNavigator
    {
        private readonly IObjectResolver _resolver;
        private readonly IHapticsService _haptics;
        private readonly ScreenRegistry _screens;
        private readonly Stack<Type> _backStack = new();
        private object _currentController;
        private Type _currentType;

        public ScreenNavigator(IObjectResolver resolver, IHapticsService haptics, ScreenRegistry screens)
        {
            _resolver = resolver;
            _haptics = haptics;
            _screens = screens;

            WireBackButtons();
            HideAll();
        }

        public void Show<TController>() where TController : class
        {
            _backStack.Clear();
            ShowCore(typeof(TController));
        }

        public void NavigateTo<TController>() where TController : class
        {
            if (_currentType != null)
                _backStack.Push(_currentType);

            ShowCore(typeof(TController));
        }

        public void GoBack()
        {
            _haptics.PlayLight();
            HideCurrent();

            if (_backStack.Count > 0)
                ShowCore(_backStack.Pop());
            else
                ShowCore(_screens.DefaultControllerType);
        }

        private void WireBackButtons()
        {
            foreach (var panel in _screens.All)
            {
                if (panel.Navigable == null)
                    continue;

                panel.Navigable.BackClicked += GoBack;
            }
        }

        private void ShowCore(Type controllerType)
        {
            HideCurrent();

            if (!_screens.TryGet(controllerType, out var panel))
                throw new InvalidOperationException($"No screen registered for {controllerType.Name}.");

            _currentType = controllerType;
            _currentController = _resolver.Resolve(controllerType);
            SetPanelActive(panel, true);

            if (_currentController is IScreenShownHandler shown)
                shown.OnScreenShown();
        }

        private void HideCurrent()
        {
            if (_currentType == null)
                return;

            if (_currentController is IScreenHiddenHandler hidden)
                hidden.OnScreenHidden();

            if (_screens.TryGet(_currentType, out var panel))
                SetPanelActive(panel, false);

            _currentController = null;
            _currentType = null;
        }

        private void HideAll()
        {
            foreach (var panel in _screens.All)
                SetPanelActive(panel, false);

            _currentController = null;
            _currentType = null;
            _backStack.Clear();
        }

        private static void SetPanelActive(ScreenRegistry.ScreenPanel panel, bool isActive) =>
            panel.Root.SetActive(isActive);
    }
}
