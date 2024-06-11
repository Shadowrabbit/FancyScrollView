/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FancyScrollView.Example05
{
    class Example05 : MonoBehaviour
    {
        [FormerlySerializedAs("scrollView")]
        [SerializeField] ListView listView = default;
        [SerializeField] Button prevCellButton = default;
        [SerializeField] Button nextCellButton = default;
        [SerializeField] Text selectedItemInfo = default;

        void Start()
        {
            prevCellButton.onClick.AddListener(listView.SelectPrevCell);
            nextCellButton.onClick.AddListener(listView.SelectNextCell);
            listView.OnSelectionChanged(OnSelectionChanged);

            var items = Enumerable.Range(0, 20)
                .Select(i => new ItemData($"Cell {i}"))
                .ToList();

            listView.UpdateData(items);
            listView.UpdateSelection(10);
            listView.JumpTo(10);
        }

        void OnSelectionChanged(int index)
        {
            selectedItemInfo.text = $"Selected item info: index {index}";
        }
    }
}
