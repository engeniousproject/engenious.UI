﻿using engenious;
using engenious.Audio;
using engenious.Content;
using engenious.UI;
using engenious.UI.Controls;

namespace SampleClient
{
    internal class CustomSkin : Skin
    {
        public CustomSkin(ContentManagerBase content) : base(content)
        {
            SoundEffect click = content.Load<SoundEffect>("click1");
            SoundEffect hover = content.Load<SoundEffect>("rollover5");

            StyleSkins.Add("special", (c) =>
            {
                if (c is Button)
                {
                    c.Width = 200;
                    Button button = c as Button;
                    button.ClickSound = click;
                    button.HoverSound = hover;
                    button.HoveredBackground = SolidColorBrush.Blue;
                }
            });
        }
    }
}
