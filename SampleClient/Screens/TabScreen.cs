using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using engenious.UI;
using engenious.UI.Controls;

namespace SampleClient.Screens
{
    class TabScreen : Screen
    {
        public TabScreen(BaseScreenComponent manager) : base(manager: manager)
        {
            //Create Tab Pages
            TabPage tabPage = new TabPage("Tab 1");
            TabPage tabPage2 = new TabPage("Tab 2");
            TabPage tabPage3 = new TabPage("Tab 3");

            //Create Tab Control & Add Pages
            TabControl tab = new TabControl();
            tab.Pages.Add(tabPage);
            tab.Pages.Add(tabPage2);
            tab.Pages.Add(tabPage3);
            tab.VerticalAlignment = VerticalAlignment.Stretch;
            tab.HorizontalAlignment = HorizontalAlignment.Stretch;

            //Add Text to Page 1
            StackPanel panel = new StackPanel();
            tabPage.Controls.Add(panel);
            panel.VerticalAlignment = VerticalAlignment.Stretch;
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.Controls.Add(new Label() { Text = "Content on Page 1" });
            panel.Controls.Add(new Label() { Text = "Content on Page 1\nWith new Line", WordWrap = true });
            panel.Controls.Add(new Label() { Text = "Content on Page 1\nWith new LineWrap Ohoh", LineWrap = true });
            panel.Controls.Add(new Label() { Text = "Content on Page 1 1very 2very 3very 4very 5very 6very 7very 8very 9very 10very 11very 12very 13very 14very 15very 16very 17very 18very 19very 20very 21very 22very 23very 24very 25very 26very 27very 28very 29very 30very 31very 32very 33very 34very 35very 36very 37very 38very 39very Should be wrapped BeforeLineBreak\nLineBreak :)\nAfterLine Wrap 1very 2very 3very 4very 5very 6very 7very 8very 9very 10very 11very 12very 13very 14very 15very 16very 17very 18very 19very 20very 21very 22very 23very 24very 25very 26very 27very loooooong text (For Line Wrap Testing :))", WordWrap = true });

            //Add "Create Tab" to page 2
            Button createPage = new TextButton("Create Tab");
            createPage.LeftMouseClick += (s, e) =>
            {
                TabPage page = new TabPage("NEW TAB");
                page.Controls.Add(new Label() { Text = "This is a new tab page" });
                tab.Pages.Add(page);
            };
            tabPage2.Controls.Add(createPage);

            //Add "Remove this page" to page 3
            Button removePage3 = new TextButton("Remove this Page");
            removePage3.LeftMouseClick += (s, e) =>
            {
                tab.Pages.Remove(tabPage3);
            };
            tabPage3.Controls.Add(removePage3);


            Controls.Add(tab);
        }
    }
}
