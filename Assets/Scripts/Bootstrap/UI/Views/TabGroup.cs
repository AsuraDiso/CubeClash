using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap.UI.Views
{
    public sealed class TabGroup : MonoBehaviour
    {
        [SerializeField] private List<Button> _tabs = new();
        [SerializeField] private Color _selectedColor = Color.green;
        [SerializeField] private Color _normalColor = Color.white;

        public event Action<int> SelectionChanged;

        private void Awake()
        {
            for (var i = 0; i < _tabs.Count; i++)
            {
                var index = i;
                _tabs[i].onClick.AddListener(() => Select(index, notify: true));
            }
        }

        private void OnDestroy()
        {
            foreach (var tab in _tabs)
            {
                if (tab != null)
                {
                    tab.onClick.RemoveAllListeners();
                }
            }
        }

        public void Select(int index, bool notify = false)
        {
            for (var i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i] == null)
                {
                    continue;
                }

                var colors = _tabs[i].colors;
                colors.normalColor = i == index ? _selectedColor : _normalColor;
                _tabs[i].colors = colors;
            }

            if (notify)
            {
                SelectionChanged?.Invoke(index);
            }
        }
    }
}
