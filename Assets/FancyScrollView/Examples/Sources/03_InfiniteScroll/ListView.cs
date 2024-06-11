/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Collections.Generic;
using UnityEngine;
using EasingCore;
using UnityEngine.Serialization;

namespace FancyScrollView.Example03
{
    class ListView : FancyListView<ItemData, Context>
    {
        [FormerlySerializedAs("scroller")]
        [SerializeField] FancyScrollRect fancyScrollRect = default;
        [SerializeField] GameObject cellPrefab = default;

        protected override GameObject CellPrefab => cellPrefab;

        protected override void Initialize()
        {
            base.Initialize();

            Context.OnCellClicked = SelectCell;

            fancyScrollRect.OnValueChanged(UpdatePosition);
            fancyScrollRect.OnSelectionChanged(UpdateSelection);
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();
        }

        public void UpdateData(IList<ItemData> items)
        {
            UpdateContents(items);
            fancyScrollRect.SetTotalCount(items.Count);
        }

        public void SelectCell(int index)
        {
            if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
            {
                return;
            }

            UpdateSelection(index);
            fancyScrollRect.ScrollTo(index, 0.35f, Ease.OutCubic);
        }
    }
}
