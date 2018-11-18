using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EngeniousUI.Controls
{
    public class TabPage : ContainerControl
    {
        public string Title;

        public TabPage(BaseScreenComponent manager, string title) : base(manager)
        {
            Title = title;
        }
    }
}
