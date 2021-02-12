using System;
using System.Collections.Generic;
using engenious.Content;
using engenious.Graphics;
using engenious.UI.Controls;
using engenious.UI.Interfaces;

namespace engenious.UI
{
    /// <summary>
    /// Class used for setting ui elements default properties.
    /// </summary>
    public class Skin
    {
        #region Singleton

        /// <summary>
        /// Gets a simple <see cref="Color.White"/> pixel used for drawing plain colored regions.
        /// </summary>
        public static Texture2D Pix { get; internal set; }

        /// <summary>
        /// Gets or sets the current <see cref="Skin"/>.
        /// </summary>
        public static Skin Current { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Skin"/> class.
        /// </summary>
        /// <param name="content">The content manager used to load fonts and other dependencies for this skin.</param>
        /// <exception cref="Exception">Thrown when the needed content could not be loaded.</exception>
        public Skin(ContentManagerBase content)
        {
            ControlSkins = new Dictionary<Type, Action<Control>>();
            StyleSkins = new Dictionary<string, Action<Control>>();

            SplitterWidth = 4;
            ScrollbarWidth = 10;
            ScrollerMinSize = 10;

            BackgroundColor = new Color(20, 147, 73);

            TextColorBlack = new Color(0, 0, 0);
            TextColorGray = new Color(86, 86, 86);
            TextColorWhite = new Color(255, 255, 255);

            try
            {
                HeadlineFont = content.Load<SpriteFont>("engenious.UI:///Fonts/HeadlineFont");
                TextFont = content.Load<SpriteFont>("engenious.UI:///Fonts/GameFont");
                BoldFont = content.Load<SpriteFont>("engenious.UI:///Fonts/BoldFont");
            }
            catch(Exception e)
            {
                throw new Exception("Could not load font.", e);
            }

            FocusFrameBrush = new BorderBrush(LineType.Dotted, Color.Black, 1);

            ButtonBrush = new BorderBrush(Color.White, LineType.Solid, Color.LightGray, 1);
            ButtonHoverBrush = new BorderBrush(Color.LightGray, LineType.Solid, Color.LightGray, 1);
            ButtonPressedBrush = new BorderBrush(Color.LightGray, LineType.Solid, Color.Black, 1);
            ButtonDisabledBrush = new BorderBrush(Color.Gray, LineType.Solid, Color.Gray, 1);
            PanelBrush = new BorderBrush(Color.White, LineType.Solid, Color.Black, 1);

            VerticalScrollBackgroundBrush = new BorderBrush(Color.White, LineType.Solid, Color.Black, 1);
            HorizontalScrollBackgroundBrush = new BorderBrush(Color.White, LineType.Solid, Color.Black, 1);
            VerticalScrollKnobBrush = new BorderBrush(Color.LightGray, LineType.Solid, Color.Black, 1);
            HorizontalScrollKnobBrush = new BorderBrush(Color.LightGray, LineType.Solid, Color.Black, 1);
            HorizontalSplitterBrush = new BorderBrush(Color.White, LineType.Solid, Color.LightGray, 1);
            VerticalSplitterBrush = new BorderBrush(Color.White, LineType.Solid, Color.LightGray, 1);

            ProgressBarBrush = new BorderBrush(Color.Blue, LineType.Solid, Color.Black, 1);

            TextboxBackgroundBrush = new BorderBrush(Color.LightGray, LineType.Solid, Color.DarkGray);

            SelectedItemBrush = new BorderBrush(Color.Red);

            // =============
            // Skin methods
            // =============

            // Control
            ControlSkins.Add(typeof(Control), (c) =>
            {
                Control control = c as Control;
                control.Margin = Border.All(0);
                control.Padding = Border.All(0);
                control.HorizontalAlignment = HorizontalAlignment.Center;
                control.VerticalAlignment = VerticalAlignment.Center;
            });

            // Label
            ControlSkins.Add(typeof(Label), (c) =>
            {
                Label label = c as Label;
                label.VerticalTextAlignment = VerticalAlignment.Center;
                label.HorizontalTextAlignment = HorizontalAlignment.Left;
                label.Font = Current.TextFont;
                label.TextColor = Current.TextColorBlack;
                label.Padding = Border.All(5);
            });

            // Button
            ControlSkins.Add(typeof(Button), (c) =>
            {
                Button button = c as Button;
                button.Margin = Border.All(2);
                button.Padding = Border.All(5);
                button.Background = Current.ButtonBrush;
                button.HoveredBackground = Current.ButtonHoverBrush;
                button.PressedBackground = Current.ButtonPressedBrush;
                button.DisabledBackground = Current.ButtonDisabledBrush;
            });

            // Textbox
            ControlSkins.Add(typeof(Textbox), (c) =>
            {
                Textbox tb = c as Textbox;
                tb.Background = TextboxBackgroundBrush;
                tb.Padding = Border.All(5);
                tb.TextColor = Color.Black;
            });

            // Splitter
            ControlSkins.Add(typeof(Splitter), (c) =>
            {
                Splitter splitter = c as Splitter;
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                splitter.Orientation = Orientation.Horizontal;
                splitter.SplitterSize = Current.SplitterWidth;
                splitter.SplitterPosition = 200;
                splitter.SplitterBrushHorizontal = Current.HorizontalSplitterBrush;
                splitter.SplitterBrushVertical = Current.VerticalSplitterBrush;
            });

            // Listbox
            ControlSkins.Add(typeof(Listbox<>), (c) =>
            {
                c.Background = new BorderBrush(Color.LightGray);
                c.DrawFocusFrame = true;

                var listbox = c as IListControl;
                listbox.SelectedItemBrush = new BorderBrush(Color.LightSkyBlue);
            });

            // Combobox
            ControlSkins.Add(typeof(Combobox<>), (c) =>
            {
                c.Background = new BorderBrush(Color.LightGray);
                c.DrawFocusFrame = true;
                c.Height = 30;
                c.Padding = Border.All(5);

                var combobox = c as ICombobox;
                combobox.SelectedItemBrush = new BorderBrush(Color.LightSkyBlue);
                combobox.DropdownBackgroundBrush = new BorderBrush(Color.LightSlateGray);
            });

            // Scrollcontainer
            ControlSkins.Add(typeof(ScrollContainer), (c) =>
            {
                ScrollContainer scrollContainer = c as ScrollContainer;
                scrollContainer.HorizontalScrollbar.Height = Current.ScrollbarWidth;
                scrollContainer.VerticalScrollbar.Width = Current.ScrollbarWidth;
                scrollContainer.ScrollerMinSize = Current.ScrollerMinSize;
                scrollContainer.VerticalScrollbar.Background = Current.VerticalScrollBackgroundBrush;
                scrollContainer.VerticalScrollbar.KnobBrush = Current.VerticalScrollKnobBrush;
                scrollContainer.HorizontalScrollbar.Background = Current.HorizontalScrollBackgroundBrush;
                scrollContainer.HorizontalScrollbar.KnobBrush = Current.HorizontalScrollKnobBrush;
            });

            // StackPanel
            ControlSkins.Add(typeof(StackPanel), (c) =>
            {
                StackPanel stackPanel = c as StackPanel;
            });


            // Progressbar
            ControlSkins.Add(typeof(ProgressBar), (c) =>
            {
                ProgressBar progressBar = c as ProgressBar;
                progressBar.BarBrush = Current.ProgressBarBrush;
                progressBar.Background = new BorderBrush(Current.BackgroundColor);
            });

            //Slider
            ControlSkins.Add(typeof(Slider), (c) =>
            {
                Slider s = c as Slider;
                s.Orientation = Orientation.Horizontal;
                s.Background = new BorderBrush(Color.LightGray);
                s.KnobBrush = new BorderBrush(Color.SlateGray);
                s.KnobSize = 20;
            });

            ControlSkins.Add(typeof(Checkbox), (c) =>
            {
                Checkbox checkbox = c as Checkbox;
                checkbox.BoxBrush = new BorderBrush(Color.Black);
                checkbox.InnerBoxBrush = new BorderBrush(Color.LightGray);
                checkbox.HookBrush = new BorderBrush(Color.LawnGreen);
                checkbox.Width = 20;
                checkbox.Height = 20;
            });

            ControlSkins.Add(typeof(TabControl), (c) =>
            {
                TabControl tabControl = c as TabControl;
                tabControl.TabBrush = new BorderBrush(Color.LightGray);
                tabControl.TabActiveBrush = new BorderBrush(Color.Gray);
                tabControl.TabPageBackground = new BorderBrush(Color.Gray);
                tabControl.TabSpacing = 1;
            });
            
            ControlSkins.Add(typeof(Panel), (c) =>
            {
                Panel panel = c as Panel;
                panel.Background = PanelBrush;
            });
        }

        #region Basic Values

        /// <summary>
        /// Gets or sets the default width of splitters in a <see cref="Splitter"/> control.
        /// </summary>
        public int SplitterWidth { get; set; }

        /// <summary>
        /// Gets or sets the default width of scroll bars.
        /// </summary>
        public int ScrollbarWidth { get; set; }

        /// <summary>
        /// Gets or sets the default minimum scrollbar knob size.
        /// </summary>
        public int ScrollerMinSize { get; set; }

        #endregion

        #region Basic Colors

        /// <summary>
        /// Gets or sets the default background color.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the default black text color.
        /// </summary>
        public Color TextColorBlack { get; set; }

        /// <summary>
        /// Gets or sets the default gray text color.
        /// </summary>
        public Color TextColorGray { get; set; }

        /// <summary>
        /// Gets or sets the default white text color.
        /// </summary>
        public Color TextColorWhite { get; set; }

        #endregion

        #region Fonts

        /// <summary>
        /// Gets or sets the default <see cref="SpriteFont"/> used for rendering headlines.
        /// </summary>
        public SpriteFont HeadlineFont { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="SpriteFont"/> used for rendering normal text.
        /// </summary>
        public SpriteFont TextFont { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="SpriteFont"/> used for rendering bold text.
        /// </summary>
        public SpriteFont BoldFont { get; set; }

        #endregion

        #region Textures

        #endregion

        #region Brushes

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for <see cref="Panel"/> controls.
        /// </summary>
        public Brush PanelBrush { get; set; }

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for <see cref="Button"/> controls.
        /// </summary>
        public Brush ButtonBrush { get; set; }

        /// <summary>
        /// Gets or sets the default hover background <see cref="Brush"/> for <see cref="Button"/> controls.
        /// </summary>
        public Brush ButtonHoverBrush { get; set; }

        /// <summary>
        /// Gets or sets the default pressed background <see cref="Brush"/> for <see cref="Button"/> controls.
        /// </summary>
        public Brush ButtonPressedBrush { get; set; }

        /// <summary>
        /// Gets or sets the default disabled background <see cref="Brush"/> for <see cref="Button"/> controls.
        /// </summary>
        public Brush ButtonDisabledBrush { get; set; }

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for vertical scroll bars.
        /// </summary>
        public Brush VerticalScrollBackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for horizontal scroll bars.
        /// </summary>
        public Brush HorizontalScrollBackgroundBrush { get; set; }

        /// <summary>
        /// Gets or sets the default knob <see cref="Brush"/> for vertical scroll bars.
        /// </summary>
        public Brush VerticalScrollKnobBrush { get; set; }

        /// <summary>
        /// Gets or sets the default knob <see cref="Brush"/> for horizontal scroll bars.
        /// </summary>
        public Brush HorizontalScrollKnobBrush { get; set; }

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for horizontal <see cref="Splitter"/> controls.
        /// </summary>
        public Brush HorizontalSplitterBrush { get; set; }

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for vertical <see cref="Splitter"/> controls.
        /// </summary>
        public Brush VerticalSplitterBrush { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="Brush"/> for for the focus frame of controls.
        /// </summary>
        public Brush FocusFrameBrush { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="Brush"/> for selected items in list controls.
        /// </summary>
        public Brush SelectedItemBrush { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="Brush"/> for progress in a <see cref="ProgressBar"/>.
        /// </summary>
        public Brush ProgressBarBrush { get; set; }

        /// <summary>
        /// Gets or sets the default background <see cref="Brush"/> for <see cref="Textbox"/> controls.
        /// </summary>
        public Brush TextboxBackgroundBrush { get; set; }

        #endregion

        /// <summary>
        /// Gets a dictionary containing delegates that apply a skin to a control of a specific type.
        /// </summary>
        public Dictionary<Type, Action<Control>> ControlSkins { get; }

        /// <summary>
        /// Gets a dictionary containing delegates that apply a skin to a control of a specific style.
        /// </summary>
        public Dictionary<string, Action<Control>> StyleSkins { get; }
    }
}
