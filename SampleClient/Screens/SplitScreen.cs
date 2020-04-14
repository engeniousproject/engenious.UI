using System;
using System.IO;
using System.Threading;
using engenious;
using engenious.Graphics;
using engenious.Input;
using engenious.UI;
using engenious.UI.Controls;

namespace SampleClient.Screens
{
    internal class SplitScreen : Screen
    {
        private Textbox textbox;

        public SplitScreen(BaseScreenComponent manager) : base(manager)
        {
            Background = new BorderBrush(Color.Gray);                       //Hintergrundfarbe festlegen

            Button backButton = new TextButton(manager, "Back") //Neuen TextButton erzeugen
            {
                HorizontalAlignment = HorizontalAlignment.Left,          //Links
                VerticalAlignment = VerticalAlignment.Top               //Oben
            };
            backButton.LeftMouseClick += (s, e) => { manager.NavigateBack(); }; //KlickEvent festlegen
            Controls.Add(backButton);                                           //Button zum Screen hinzufügen



            //ScrollContainer
            ScrollContainer scrollContainer = new ScrollContainer(manager)  //Neuen ScrollContainer erzeugen
            {
                VerticalAlignment = VerticalAlignment.Stretch,              // 100% Höhe
                HorizontalAlignment = HorizontalAlignment.Stretch           //100% Breite
            };
            Controls.Add(scrollContainer);                                  //ScrollContainer zum Root(Screen) hinzufügen



            //Stackpanel - SubControls werden Horizontal oder Vertikal gestackt
            StackPanel panel = new StackPanel(manager);                 //Neues Stackpanel erzeugen
            panel.VerticalAlignment = VerticalAlignment.Stretch;        //100% Höhe
            scrollContainer.Content = panel;                            //Ein Scroll Container kann nur ein Control beherbergen
            panel.ControlSpacing = 20;

            //Label
            Label label = new Label(manager) { Text = "Control Showcase" }; //Neues Label erzeugen
            panel.Controls.Add(label);                                      //Label zu Panel hinzufügen

            //Button
            Button button = new TextButton(manager, "Dummy Button"); //Neuen TextButton erzeugen
            panel.Controls.Add(button);                                 //Button zu Panel hinzufügen

            //Progressbar
            ProgressBar pr = new ProgressBar(manager)                   //Neue ProgressBar erzeugen
            {
                Value = 80,                                            //Aktueller Wert
                Height = 20,                                            //Höhe
                Width = 200                                             //Breite
            };
            panel.Controls.Add(pr);                                     //ProgressBar zu Panel hinzufügen
                        
            //ListBox
            Listbox<string> list = new Listbox<string>(manager)
            {
                MaxHeight = 100,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            list.TemplateGenerator = (item) =>                          //Template Generator festlegen
            {
                return new Label(manager) { Text = item, Padding = new Border(15,5,15,5), HorizontalAlignment = HorizontalAlignment.Stretch };              //Control (Label) erstellen
            };
            panel.Controls.Add(list);                                   //Liste zu Panel hinzufügen

            list.Items.Add("Item 1");                                    //Items zur Liste hinzufügen
            list.Items.Add("Item 2");                                     //...
            list.Items.Add("Item 3");                                     //...
            list.Items.Add("Item 4");                                     //...
            list.Items.Add("Item 5");                                     //...
            list.Items.Add("Item 6");                                     //...
            list.Items.Add("Item 7");                                     //...

            //Combobox
            Combobox<string> combobox = new Combobox<string>(manager)   //Neue Combobox erstellen
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,       //Breite
                ButtonBrushOpen = new TextureBrush(Manager.Content.Load<Texture2D>("arrow_down"), TextureBrushMode.Stretch),
                ButtonBrushClose = new TextureBrush(Manager.Content.Load<Texture2D>("arrow_up"), TextureBrushMode.Stretch),
            };
            combobox.TemplateGenerator = (item) =>                      //Template Generator festlegen
            {
                return new Label(manager) { Text = item };              //Control (Label) erstellen
            };
            panel.Controls.Add(combobox);                               //Combobox zu Panel  hinzufügen

            combobox.Items.Add("Item 1");                                    //Items zur Liste hinzufügen
            combobox.Items.Add("Item 2");                                     //...
            combobox.Items.Add("Item 3");                                     //...
            combobox.Items.Add("Item 4");                                     //...
            combobox.Items.Add("Item 5");                                     //...
            combobox.Items.Add("Item 6");                                     //...
            combobox.Items.Add("Item 7");                                     //...

            //Slider Value Label
             Label labelSliderHorizontal = new Label(manager);

             //Horizontaler Slider
             Slider sliderHorizontal = new Slider(manager)
             {
                 Width = 150,
                 Height = 20,
                 Invert = true
             };
             sliderHorizontal.ValueChanged += (value) => { labelSliderHorizontal.Text = "Value: " + value; }; //Event on Value Changed
             panel.Controls.Add(sliderHorizontal);
             labelSliderHorizontal.Text = "Value: " + sliderHorizontal.Value;                                 //Set Text initially
             panel.Controls.Add(labelSliderHorizontal);

             //Slider Value Label
             Label labelSliderVertical = new Label(manager);

             //Vertikaler Slider
             Slider sliderVertical = new Slider(manager)
             {
                 Range = 100,
                 Height = 200,
                 Width = 20,
                 Orientation = Orientation.Vertical,
                 BigStep = 20,
                 KnobSize = 50,
                 Invert = true
             };
             sliderVertical.ValueChanged += (value) => { labelSliderVertical.Text = "Value: " + value; };
             panel.Controls.Add(sliderVertical);
             labelSliderVertical.Text = "Value: " + sliderVertical.Value;
             panel.Controls.Add(labelSliderVertical);

             Checkbox checkbox = new Checkbox(manager);
             panel.Controls.Add(checkbox);


             //Textbox   
             textbox = new Textbox(manager)                      //Neue TextBox erzeugen
             {
                 HorizontalAlignment = HorizontalAlignment.Stretch,          //100% Breite,
                 MaxWidth = 200,
                 Text = "TEXTBOX!",                                      //Voreingestellter text
             };

             Button clearTextbox = new TextButton(manager, "Clear Textbox");
             clearTextbox.LeftMouseClick += (s, e) =>
             {
                 textbox.SelectionStart = 0;
                 textbox.Text = "";
             };
             panel.Controls.Add(clearTextbox);
             panel.Controls.Add(textbox);                                //Textbox zu Panel hinzufügen

            var but = new TextButton(manager, "Hello World");
            but.LeftMouseClick += (e, a) =>
            {
                var t = new Thread(() => {
                    manager.Invoke(() =>
                    {
                        Background = new BorderBrush(Color.Green);

                    });
                });
                t.Start();
            };
            panel.Controls.Add(but);

        }

        static Texture2D LoadTexture2DFromFile(string path, GraphicsDevice device)
        {
            using (Stream stream = File.OpenRead(path))
            {
                return Texture2D.FromStream(device, stream);
            }
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            if (args.Key == Keys.RightAlt)
            {
                textbox.Text = "";
            }
            base.OnKeyDown(args);
        }
    }

        
}
