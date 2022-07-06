using engenious;
using engenious.UI;
using engenious.UI.Controls;

namespace SampleClient.Screens
{
    internal class StartScreen : Screen
    {
        public StartScreen(BaseScreenComponent manager) : base(manager: manager)
        {
            Background = new BorderBrush(Color.DarkRed);

            StackPanel stack = new StackPanel();
            Controls.Add(stack);

            // Button zur Controls Demo
            Button controlScreenButton = new TextButton("Controls", style: "special");          //Button mit speziellen Style erstellen
            controlScreenButton.LeftMouseClick += (s, e) =>                                      //Click Event festlegen
            {
                manager.NavigateToScreen(new SplitScreen(manager));                     //Screen wechseln
            };
            stack.Controls.Add(controlScreenButton);                                                   //Button zu Root hinzufügen

            // Button zur Mouse Capture Demo
            Button capturedMouseButton = new TextButton("Captured Mouse", style: "special");
            capturedMouseButton.LeftMouseClick += (s, e) => manager.NavigateToScreen(new MouseCaptureScreen(manager));
            ((TextButton) capturedMouseButton).Label.FitText = true;
            capturedMouseButton.MinHeight = 100;
            capturedMouseButton.MinWidth = 300;
            stack.Controls.Add(capturedMouseButton);

            Button tabDemoScreen = new TextButton("Tab Demo", style: "special");
            tabDemoScreen.LeftMouseClick += (s, e) => manager.NavigateToScreen(new TabScreen(manager));
            stack.Controls.Add(tabDemoScreen);

            Button dragDropScreen = new TextButton("Drag & Drop", style: "special");
            dragDropScreen.LeftMouseClick += (s, e) => manager.NavigateToScreen(new DragDropScreen(manager));
            stack.Controls.Add(dragDropScreen);

            Button disabeldButton = new TextButton("Disabled", style: "special");
            disabeldButton.Enabled = false;
            stack.Controls.Add(disabeldButton);
        }
    }
}
