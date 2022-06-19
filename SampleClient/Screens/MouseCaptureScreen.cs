﻿using engenious;
using engenious.Input;
using engenious.UI;
using engenious.UI.Controls;

namespace SampleClient.Screens
{
    internal class MouseCaptureScreen : Screen
    {
        private Point position = new Point();

        private Label output;

        public MouseCaptureScreen(BaseScreenComponent manager) : base(manager: manager)
        {
            DefaultMouseMode = MouseMode.Captured;

            Background = new BorderBrush(Color.Green);

            StackPanel stack = new StackPanel();
            Controls.Add(stack);

            Label title = new Label()
            {
                TextColor = Color.White,
                Text = "Press ESC to return to Main Screen",
            };

            output = new Label()
            {
                TextColor = Color.White,
                Text = position.ToString(),
            };

            stack.Controls.Add(title);
            stack.Controls.Add(output);
        }

        protected override void OnKeyPress(KeyEventArgs args)
        {
            if (args.Key == Keys.Escape)
            {
                args.Handled = true;
                ScreenManager.NavigateBack();
            }

            base.OnKeyPress(args);
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            if (args.MouseMode == MouseMode.Captured)
            {
                position = args.GlobalPosition;
            }

            output.Text = position.ToString();

            base.OnMouseMove(args);
        }
    }
}
