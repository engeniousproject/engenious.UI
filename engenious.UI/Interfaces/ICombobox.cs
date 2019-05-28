using engenious.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engenious.UI.Interfaces
{
    public interface ICombobox : IListControl
    {
        bool IsOpen { get; }

        Brush ButtonBrushOpen { get; set; }

        Brush ButtonBrushClose { get; set; }

        Brush DropdownBackgroundBrush { get; set; }
    }
}
