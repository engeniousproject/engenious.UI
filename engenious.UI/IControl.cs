using engenious.Audio;
using engenious.Graphics;

namespace engenious.UI
{
    public interface IControl
    {
        /// <summary>
        /// Reference to the current screen manager
        /// </summary>
        BaseScreenComponent ScreenManager { get; }

        /// <summary>
        /// Sound to be played on click
        /// </summary>
        SoundEffect ClickSound { get; set; }

        /// <summary>
        /// Sound to be played on hover
        /// </summary>
        SoundEffect HoverSound { get; set; }

        /// <summary>
        /// Default background
        /// </summary>
        Brush Background { get; set; }

        /// <summary>
        /// Optional background on hover
        /// </summary>
        Brush HoveredBackground { get; set; }

        /// <summary>
        /// Optional background on pressed
        /// </summary>
        Brush PressedBackground { get; set; }

        /// <summary>
        /// Optional background when <see cref="Enabled"/> is <see langword="false"/>
        /// </summary>
        Brush DisabledBackground { get; set; }

        /// <summary>
        /// Legt den äußeren Abstand des Contro { return zOrder; }ls fest.
        /// </summary> { return zOrder; }
        Border Margin { get; set; }

        /// <summary>
        /// Legt den inneren Abstand des Controls fest.
        /// </summary>
        Border Padding { get; set; }

        /// <summary>
        /// Legt fest ob der Fokus-Rahmen gezeichnet werden soll
        /// </summary>
        bool DrawFocusFrame { get; set; }

        /// <summary>
        /// Platzhalter für jegliche Art der Referenz.
        /// </summary>
        object Tag { get; set; }

        /// <summary>
        /// Gibt den Style-Namen des Controls zurück oder legt diesen fest.
        /// </summary>
        string Style { get; }

        /// <summary>
        /// Gibt an, ob das Control aktiv ist.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Ermittelt den absoluten Aktivierungsstatus von Root bis zu diesem Control.
        /// </summary>
        bool AbsoluteEnabled { get; }

        /// <summary>
        /// Gibt an, ob das Control gerendert werden soll.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Ermittelt den absoluten Sichtbarkeitsstatus von Root bis zu diesem Control.
        /// </summary>
        bool AbsoluteVisible { get; }

        /// <summary>
        /// Gibt das Root-Element des zugehörigen Visual Tree zurück.
        /// </summary>
        Control Root { get; }

        /// <summary>
        /// Liefert den Control-Path von Root zum aktuellen Control.
        /// </summary>
        ReverseEnumerable<Control> RootPath { get; }

        /// <summary>
        /// Gibt das Parent-Element dieses Controls zurück.
        /// </summary>
        Control Parent { get; }

        /// <summary>
        /// Horizontale Ausrichtung im Dynamic-Mode.
        /// </summary>
        HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gibt eine zusätzliche Render-Transformation an.
        /// </summary>
        Matrix Transformation { get; set; }

        /// <summary>
        /// Gibt die absolute Transformation für dieses Control zurück.
        /// </summary>
        Matrix AbsoluteTransformation { get; }

        /// <summary>
        /// Gibt den Alpha-Wert dieses Controls an.
        /// </summary>
        float Alpha { get; set; }

        /// <summary>
        /// Gibt den absoluten Alpha-Wert für dieses Control zurück.
        /// </summary>
        float AbsoluteAlpha { get; }

        /// <summary>
        /// Vertikale Ausrichtung im Dynamic-Mode.
        /// </summary>
        VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Legt optional eine Mindestbreite für dieses Control fest.
        /// </summary>
        int? MinWidth { get; set; }

        /// <summary>
        /// Legt optional eine definierte Breite für dieses Control fest.
        /// </summary>
        int? Width { get; set; }

        /// <summary>
        /// Legt optional eine Maximalbreite für dieses Control fest.
        /// </summary>
        int? MaxWidth { get; set; }

        /// <summary>
        /// Legt optional eine Mindesthöhe für dieses Control fest.
        /// </summary>
        int? MinHeight { get; set; }

        /// <summary>
        /// Legt optional eine definierte Höhe für dieses Control fest. 
        /// </summary>
        int? Height { get; set; }

        /// <summary>
        /// Legt optional eine Maximalbreite für dieses Control fest.
        /// </summary>
        int? MaxHeight { get; set; }

        /// <summary>
        /// Gibt die tatsächliche Renderposition (exkl. Parent Offset) zurück.
        /// </summary>
        Point ActualPosition { get; set; }

        /// <summary>
        /// Gibt die absolute Position (global) dieses Controls zurück.
        /// </summary>
        Point AbsolutePosition { get; }

        /// <summary>
        /// Gibt die tatsächliche Rendergröße zurück.
        /// </summary>
        Point ActualSize { get; set; }

        /// <summary>
        /// Berechnet den Client-Bereich auf Basis der aktuellen 
        /// Position/Größe/Margin/Padding in lokalen Koordinaten.
        /// </summary>
        Rectangle ActualClientArea { get; }

        /// <summary>
        /// Berechnet den Verfügbaren Client-Bereich unter Berücksichtigung der 
        /// ActualSize und den eingestellten Margins und Paddings.
        /// </summary>
        Point ActualClientSize { get; }

        /// <summary>
        /// Ermittelt den gesamten Rand durch Margin und Padding.
        /// </summary>
        Point Borders { get; }

        /// <summary>
        /// Gibt an, ob das Control unter der Maus ist.
        /// </summary>
        TreeState Hovered { get; }

        /// <summary>
        /// Gibt an ob das aktuelle Elemente gerade duch die Maus gedrückt wird.
        /// </summary>
        bool Pressed { get; }

        /// <summary>
        /// Legt fest, ob das Control per Tab zu erreichen ist.
        /// </summary>
        bool TabStop { get; set; }

        /// <summary>
        /// Gibt an ob das Control den Fokus bekommen kann oder legt dies fest.
        /// </summary>
        bool CanFocus { get; set; }

        /// <summary>
        /// Gibt die Position der Tab-Reihenfolge an.
        /// </summary>
        int TabOrder { get; set; }

        /// <summary>
        /// Gibt an ob das aktuelle Control den Fokus hat.
        /// </summary>
        TreeState Focused { get; }

        /// <summary>
        /// Gibt die grafische Reihenfolge der Controls 
        /// innerhalb eines Containers an. (0 ganz vorne, 9999 weiter hinten)
        /// </summary>
        int ZOrder { get; set; }

        void Update(GameTime gameTime);
        void PreDraw(GameTime gameTime);

        /// <summary>
        /// Zeichenauruf für das Control (SpriteBatch ist bereits aktiviert)
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="gameTime">Vergangene Spielzeit</param>
        void Draw(SpriteBatch batch, Rectangle renderMask, GameTime gameTime);

        void InvalidateDrawing();

        /// <summary>
        /// Event das die Änderung des Parents signalisiert.
        /// </summary>
        event PropertyChangedDelegate<Control> ParentChanged;

        event PropertyChangedDelegate<bool> EnableChanged;
        event PropertyChangedDelegate<bool> VisibleChanged;

        /// <summary>
        /// Methode zur Ermittlung des notwendigen Platzes.
        /// </summary>
        /// <param name="available">Verfügbarer Platz für dieses Control</param>
        /// <returns>Benötigte Platz inklusive allen Rändern</returns>
        Point GetExpectedSize(Point available);

        /// <summary>
        /// Ermittelt die maximale Größe des Client Bereichs für dieses Control.
        /// </summary>
        /// <param name="containerSize"></param>
        /// <returns></returns>
        Point GetMaxClientSize(Point containerSize);

        /// <summary>
        /// Ermittelt die minimale Größe des Client Bereichs für dieses Control.
        /// </summary>
        /// <param name="containerSize"></param>
        /// <returns></returns>
        Point GetMinClientSize(Point containerSize);

        /// <summary>
        /// Legt die tatsächliche Größe für dieses Control fest.
        /// </summary>
        /// <param name="available">Erwartete Größe des Controls (inkl. Borders)</param>
        void SetActualSize(Point available);

        /// <summary>
        /// Gibt zurück, ob die Größenangaben nicht mehr aktuell sind.
        /// </summary>
        /// <returns></returns>
        bool HasInvalidDimensions();

        /// <summary>
        /// Ist für die Berechnung des Client-Contents zuständig und erleichtert das automatische Alignment.
        /// </summary>
        /// <returns></returns>
        Point CalculcateRequiredClientSpace(Point available);

        /// <summary>
        /// Teilt dem Steuerelement mit, dass seine Größe neu berechnet werden muss.
        /// </summary>
        void InvalidateDimensions();

        /// <summary>
        /// Wird aufgerufen, wenn die Auflösung des Fensters geändert wird.
        /// </summary>
        void OnResolutionChanged();

        void StartTransition(Transition transition);
        event MouseEventDelegate MouseEnter;
        event MouseEventDelegate MouseLeave;
        event MouseEventDelegate MouseMove;

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste heruntergedrückt wird.
        /// </summary>
        event MouseEventDelegate LeftMouseDown;

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste losgelassen wird.
        /// </summary>
        event MouseEventDelegate LeftMouseUp;

        /// <summary>
        /// Wird aufgerufen, wenn mit der linken Maustaste auf das Steuerelement geklickt wird.
        /// </summary>
        event MouseEventDelegate LeftMouseClick;

        event MouseEventDelegate LeftMouseDoubleClick;

        /// <summary>
        /// Wird aufgerufen, wenn die rechte Maustaste heruntergedrückt wird.
        /// </summary>
        event MouseEventDelegate RightMouseDown;

        /// <summary>
        /// Wird aufgerufen, wenn die rechte Maustaste losgelassen wird.
        /// </summary>
        event MouseEventDelegate RightMouseUp;

        /// <summary>
        /// Wird aufgerufen, wenn mit der rechten Maustaste auf das Steuerelement geklickt wird.
        /// </summary>
        event MouseEventDelegate RightMouseClick;

        event MouseEventDelegate RightMouseDoubleClick;
        event MouseScrollEventDelegate MouseScroll;
        event TouchEventDelegate TouchDown;
        event TouchEventDelegate TouchMove;
        event TouchEventDelegate TouchUp;
        event TouchEventDelegate TouchTap;
        event TouchEventDelegate TouchDoubleTap;
        event PropertyChangedDelegate<TreeState> HoveredChanged;

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt wird.
        /// </summary>
        event KeyEventDelegate KeyDown;

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste losgelassen wird.
        /// </summary>
        event KeyEventDelegate KeyUp;

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt ist.
        /// </summary>
        event KeyEventDelegate KeyPress;

        event KeyTextEventDelegate KeyTextPress;

        /// <summary>
        /// Setzt den Fokus auf dieses Control.
        /// </summary>
        void Focus();

        /// <summary>
        /// Entfernt den Fokus.
        /// </summary>
        void Unfocus();

        event PropertyChangedDelegate<bool> TabStopChanged;
        event PropertyChangedDelegate<bool> CanFocusChanged;
        event PropertyChangedDelegate<int> TabOrderChanged;
        event PropertyChangedDelegate<int> ZOrderChanged;
        event EventDelegate GotFocus;
        event EventDelegate LostFocus;
        event DragEventDelegate StartDrag;
        event DragEventDelegate DropMove;
        event DragEventDelegate DropEnter;
        event DragEventDelegate DropLeave;
        event DragEventDelegate EndDrop;
    }
}