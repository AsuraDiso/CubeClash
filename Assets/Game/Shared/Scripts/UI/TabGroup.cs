using System;
using System.Collections.Generic;
using Game.Shared.Scripts.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Shared.Scripts.UI
{
    public sealed class TabGroup : MonoBehaviour
    {
        [SerializeField] private List<Button> _tabs = new();
        [SerializeField] private int _defaultIndex;

        public event Action<int> SelectionChanged;

        public int SelectedIndex { get; private set; } = -1;

        private void Awake()
        {
            for (var i = 0; i < _tabs.Count; i++)
            {
                var index = i;
                _tabs[i].onClick.AddListener(() => Select(index, notify: true));
            }

            Select(_defaultIndex, notify: false);
        }

        private void OnDestroy()
        {
            foreach (var tab in _tabs)
                tab.onClick.RemoveAllListeners();
        }

        public void Select(int index, bool notify = false)
        {
            if (index < 0 || index >= _tabs.Count)
                return;

            SelectedIndex = index;
            RefreshVisuals();

            if (notify)
                SelectionChanged?.Invoke(index);
        }

        private void RefreshVisuals()
        {
            var theme = UiThemeAccess.Current;
            var selectedColor = theme != null ? theme.TabSelected : Color.white;
            var unselectedColor = theme != null ? theme.TabUnselected : new Color(1f, 1f, 1f, 0.55f);

            for (var i = 0; i < _tabs.Count; i++)
            {
                var graphic = _tabs[i].targetGraphic;
                if (graphic == null)
                    continue;

                graphic.color = i == SelectedIndex ? selectedColor : unselectedColor;
            }
        }
    }
}
