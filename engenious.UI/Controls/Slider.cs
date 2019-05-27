using System;
using engenious.Graphics;
using engenious.Input;

namespace engenious.UI.Controls
{
    /// <summary>
    /// Das Slider-Control erlaubt das Verschieben eines Reglers per Maus oder Tastatur.
    /// </summary>
    public class Slider : Control
    {
        private int range;
        /// <summary>
        /// Gibt die grafische Ausrichtung des Sliders zurück oder legt diese fest.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gibt den Brush, mit dem der Slider-Vordergrund gemalt werden soll, zurück oder legt diesen fest.
        /// </summary>
        public Brush KnobBrush { get; set; }

        /// <summary>
        /// Gibt den Maximalwert zurück oder legt diesen fest.
        /// </summary>
        public int Range
        {
            get => range;
            set
            {
                range = value;
                Value = Math.Min(range, Value);
            }
        }

        /// <summary>
        /// Gibt die Breite/Höhe des Sliders zurück oder legt diesen fest.
        /// </summary>
        public int KnobSize { get; set; }

        /// <summary>
        /// Der aktuelle Wert
        /// </summary>
        private int sliderValue;

        /// <summary>
        /// Gibt den aktuellen Wert zurück oder legt diesen fest.
        /// </summary>
        public int Value
        {
            get
            {
                return sliderValue;
            }
            set
            {
                sliderValue = Math.Max(0, Math.Min(Range, value));
                RecalculateKnob();
                if(ValueChanged != null)
                    ValueChanged.Invoke(sliderValue);
            }
        }

        public bool Invert { get; set; }

        public int BigStep { get; set; } = 0;

        public int SmallStep { get; set; } = 1;


        /// <summary>
        /// Gibt an ob die Maus geklickt wird während das Control fokussiert ist
        /// </summary>
        private bool mouseClickActive = false;

        private Rectangle knob;

        /// <summary>
        /// Wird ausgelöst wenn sich der Wert ändert
        /// </summary>
        public event ValueChangedDelegate ValueChanged;

        /// <summary>
        /// Erzeugt einen neuen Slider.
        /// </summary>
        /// <param name="manager">Der verwendete <see cref="BaseScreenComponent"/></param>
        /// <param name="style">(Optional) Der zu verwendende Style.</param>
        public Slider(BaseScreenComponent manager, string style = "")
            : base(manager, style)
        {
            CanFocus = true;
            TabStop = true;

            Range = 100;
            Value = 0;

            ApplySkin(typeof(Slider));
        }

        /// <summary>
        /// Malt den Content des Controls
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="contentArea">Bereich für den Content in absoluten Koordinaten</param>
        /// <param name="gameTime">GameTime</param>
        /// <param name="alpha">Die Transparenz des Controls.</param>
        protected override void OnDrawContent(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            RecalculateKnob();

            //Zeichne Background
            Background.Draw(batch, contentArea, alpha);

            KnobBrush.Draw(batch, new Rectangle(knob.Location + contentArea.Location, knob.Size), alpha);
        }

        private void RecalculateKnob()
        {

            Rectangle drawableKnobSpace = new Rectangle(
                KnobSize / 2,
                KnobSize / 2,
                ActualClientArea.Width - KnobSize,
                ActualClientArea.Height - KnobSize);

            int val = Invert ? Range - Value : Value;

            Rectangle sliderKnob = new Rectangle();
            if (Orientation == Orientation.Horizontal)
            {

                //Berechne die Position des SliderKnobs     
                sliderKnob.Y = 0;                                                       //Y Koordinate des Knobs
                sliderKnob.Width = KnobSize;                                                              //Der Slider ist SliderWidth breit
                float WidthRange = ((float)drawableKnobSpace.Width / Range);                        //Berechnet wieviel Pixel 1 in Value wert ist
                sliderKnob.X = (int)Math.Round(drawableKnobSpace.X + (WidthRange * val) - KnobSize / 2);    //Berechnet die X Position des Knobs
                sliderKnob.Height = ActualClientArea.Height;                                             //Der SliderKnob ist immer so hoch wie der Slider
            }
            else
            {
                //Berechne die Position des SliderKnobs     
                sliderKnob.X = 0;                                                                             //Der SliderKnob beginnt immer am oberen Rand des Sliders
                sliderKnob.Height = KnobSize;                                                             //Der Slider ist SliderWidthpx hoch
                float HeightRange = ((float)drawableKnobSpace.Height / Range);                              //Berechnet wieviel Pixel 1 in Value wert ist
                sliderKnob.Y = (int)Math.Round(drawableKnobSpace.Y + drawableKnobSpace.Height - (HeightRange * val) - KnobSize / 2);    //Berechnet die X Position des Knobs
                sliderKnob.Width = ActualClientArea.Width;                                               //Der SliderKnob ist immer so breit wie der Slider
            }

            knob = sliderKnob;
        }

        Point grabPoint;

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste heruntergedrückt wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Event.</param>
        protected override void OnLeftMouseDown(MouseEventArgs args)
        {
            

            if(knob.Intersects(args.LocalPosition))
            {
                grabPoint = args.LocalPosition - (knob.Location + new Point(knob.Width / 2, knob.Height / 2)); 
                mouseClickActive = true;
            }
            else if(BigStep == 0)
            {
                if (Orientation == Orientation.Horizontal)
                    Value = (args.LocalPosition.X - KnobSize / 2) * Range / (ActualClientArea.Width - KnobSize);
                else
                    Value = Range - (args.LocalPosition.Y - KnobSize / 2) * Range / (ActualClientArea.Height - KnobSize);

                if (Invert)
                    Value = Range - Value;
            }
            else
            {
                int scrollDirection = Orientation ==
                    Orientation.Horizontal && args.LocalPosition.X < knob.X || Orientation == Orientation.Vertical && args.LocalPosition.Y < knob.Y
                    ? 1 : -1;
                Value += BigStep * scrollDirection * (Invert ? -1 : 1);
            }

        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);
            //Berechnen des Werts wenn die Maus gehalten wird & das Control ausgewählt ist
            if (mouseClickActive)
            {

                Point localMousePos = args.LocalPosition;
                //Wenn der Slider Horizontal ist
                if (Orientation == Orientation.Horizontal)
                {
                    //Berechne den Wert des Sliders
                    Value = (args.LocalPosition.X - KnobSize / 2 - grabPoint.X) * Range / (ActualClientArea.Width - KnobSize);
                    if (localMousePos.X <= 0) Value = 0;                     //Wenn die Maus Position kleiner als 0 ist -> Value = 0
                    if (localMousePos.X >= ActualClientArea.Width) Value = Range; //Wenn die Maus Position größer als die Breite des Controls -> Value = Range
                }

                //Wenn der Slider vertikal ist
                if (Orientation == Orientation.Vertical)
                {
                    //Berechne den Wert des Sliders
                    Value = Range - (args.LocalPosition.Y - KnobSize / 2 - grabPoint.Y) * Range / (ActualClientArea.Height - KnobSize);
                    if (Value <= 0) Value = 0;                      //Wenn die Maus Position kleiner als 0 ist -> Value = 0
                    if (Value >= Range) Value = Range;              //Wenn die Maus Position größer als die Breite des Controls -> Value = Range
                }
                if (Invert)
                    Value = Range - Value;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste losgelassen wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Event.</param>
        protected override void OnLeftMouseUp(MouseEventArgs args)
        {
            mouseClickActive = false;
        }

        /// <summary>
        /// Delegat zur Änderung des Slider-Wertes.
        /// </summary>
        /// <param name="Value">Der neue Wert.</param>
        public delegate void ValueChangedDelegate(int Value);

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt ist.
        /// </summary>
        /// <param name="args">Zusätzliche Daten zum Event.</param>
        protected override void OnKeyPress(KeyEventArgs args)
        {
            if (Focused != TreeState.Active)
                return;


            if (Orientation == Orientation.Horizontal)
            {
                if (args.Key == Keys.Right && Value < Range)
                    Value++;
                if (args.Key == Keys.Left && Value > 0)
                    Value--;
            }
            else
            {
                if (args.Key == Keys.Up && Value < Range)
                    Value++;
                if (args.Key == Keys.Down && Value > 0)
                    Value--;
            }

            if (args.Key == Keys.D0)
                Value = 0;
        }

        protected override void OnMouseScroll(MouseScrollEventArgs args)
        {
            Value += args.Steps * SmallStep * (Invert ? -1 : 1);
            args.Handled = true;
            base.OnMouseScroll(args);
        }
    }
}
