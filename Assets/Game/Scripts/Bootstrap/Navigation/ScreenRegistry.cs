using System;
using System.Collections.Generic;
using Game.Scripts.Bootstrap.UI.Views;
using Game.Shared.Scripts.UI;
using UnityEngine;

namespace Game.Scripts.Bootstrap.Navigation
{
    public sealed class ScreenRegistry
    {
        private readonly Dictionary<Type, ScreenPanel> _panels = new();

        public Type DefaultControllerType { get; private set; }

        public void Register<TController>(ScreenView view) where TController : class
        {
            _panels[typeof(TController)] = new ScreenPanel(view.gameObject, view as INavigableView);
            DefaultControllerType ??= typeof(TController);
        }

        internal bool TryGet(Type controllerType, out ScreenPanel panel) =>
            _panels.TryGetValue(controllerType, out panel);

        internal IEnumerable<ScreenPanel> All => _panels.Values;

        internal sealed class ScreenPanel
        {
            public ScreenPanel(GameObject root, INavigableView navigable)
            {
                Root = root;
                Navigable = navigable;
            }

            public GameObject Root { get; }
            public INavigableView Navigable { get; }
        }
    }
}
