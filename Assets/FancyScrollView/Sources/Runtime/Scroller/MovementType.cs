/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine.UI;

namespace FancyScrollView
{
    public enum MovementType
    {
        Unrestricted = ScrollRect.MovementType.Unrestricted, //無制限
        Elastic = ScrollRect.MovementType.Elastic, //弾性
        Clamped = ScrollRect.MovementType.Clamped //制限
    }
}
