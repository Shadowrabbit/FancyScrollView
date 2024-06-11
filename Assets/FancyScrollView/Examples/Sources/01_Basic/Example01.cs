/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FancyScrollView.Example01
{
    class Example01 : MonoBehaviour
    {
        [FormerlySerializedAs("scrollView")]
        [SerializeField] ListView listView = default;

        void Start()
        {
            var items = Enumerable.Range(0, 20)
                .Select(i => new ItemData($"Cell {i}"))
                .ToArray();

            listView.UpdateData(items);
        }
    }
}
