/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using EasingCore;
using UnityEngine.Serialization;

namespace FancyScrollView.Example07
{
    class Example07 : MonoBehaviour
    {
        [FormerlySerializedAs("scrollView")]
        [SerializeField] ListView listView = default;
        [SerializeField] InputField paddingTopInputField = default;
        [SerializeField] InputField paddingBottomInputField = default;
        [SerializeField] InputField spacingInputField = default;
        [SerializeField] InputField dataCountInputField = default;
        [SerializeField] InputField selectIndexInputField = default;
        [SerializeField] Dropdown alignmentDropdown = default;

        void Start()
        {
            listView.OnCellClicked(index => selectIndexInputField.text = index.ToString());

            paddingTopInputField.onValueChanged.AddListener(_ =>
                TryParseValue(paddingTopInputField, 0, 999, value => listView.PaddingTop = value));
            paddingTopInputField.text = listView.PaddingTop.ToString();

            paddingBottomInputField.onValueChanged.AddListener(_ =>
                TryParseValue(paddingBottomInputField, 0, 999, value => listView.PaddingBottom = value));
            paddingBottomInputField.text = listView.PaddingBottom.ToString();

            spacingInputField.onValueChanged.AddListener(_ =>
                TryParseValue(spacingInputField, 0, 100, value => listView.Spacing = value));
            spacingInputField.text = listView.Spacing.ToString();

            alignmentDropdown.AddOptions(Enum.GetNames(typeof(Alignment)).Select(x => new Dropdown.OptionData(x)).ToList());
            alignmentDropdown.onValueChanged.AddListener(_ => SelectCell());
            alignmentDropdown.value = (int)Alignment.Middle;

            selectIndexInputField.onValueChanged.AddListener(_ => SelectCell());
            selectIndexInputField.text = "10";

            dataCountInputField.onValueChanged.AddListener(_ =>
                TryParseValue(dataCountInputField, 1, 99999, GenerateCells));
            dataCountInputField.text = "20";

            listView.JumpTo(10);
        }

        void TryParseValue(InputField inputField, int min, int max, Action<int> success)
        {
            if (!int.TryParse(inputField.text, out int value))
            {
                return;
            }

            if (value < min || value > max)
            {
                inputField.text = Mathf.Clamp(value, min, max).ToString();
                return;
            }

            success(value);
        }

        void SelectCell()
        {
            if (listView.DataCount == 0)
            {
                return;
            }

            TryParseValue(selectIndexInputField, 0, listView.DataCount - 1, index =>
                listView.ScrollTo(index, 0.3f, Ease.InOutQuint, (Alignment)alignmentDropdown.value));
        }

        void GenerateCells(int dataCount)
        {
            var items = Enumerable.Range(0, dataCount)
                .Select(i => new ItemData($"Cell {i}"))
                .ToArray();

            listView.UpdateData(items);
            SelectCell();
        }
    }
}
