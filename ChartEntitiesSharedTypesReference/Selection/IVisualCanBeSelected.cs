using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChartCanvasNamespace.Selection
{
    public enum CanBeSelectedItemTypeEnum { EntityBorder, TextWithShape, Text, LineConnection, Thumb }

    public interface IVisualCanBeSelected : IEquatable<IVisualCanBeSelected>
    {
        CanBeSelectedItemTypeEnum Type { get; }
        bool IsSelected { get; set; }
        ModifierKeys ModifiersWhenSelectingSelf { get; }
    }
}
