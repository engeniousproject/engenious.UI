﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using engenious.Audio;
using engenious.Graphics;
using engenious.UI.Controls;

namespace engenious.UI
{
    /// <summary>
    /// Base-Class for all Controls
    /// </summary>
    public abstract class Control : IControl
    {
        private bool invalidDrawing;

        private Brush background = null;

        private Brush hoveredBackground = null;

        private Brush pressedBackground = null;

        private Brush disabledBackground = null;

        private Border margin = Border.All(0);

        private Border padding = Border.All(0);

        private SoundEffect clickSound = null;

        private SoundEffect hoverSound = null;

        /// <summary>
        /// Reference to the current screen manager
        /// </summary>
        public BaseScreenComponent ScreenManager { get; private set; }

        /// <summary>
        /// Sound to be played on click
        /// </summary>
        public SoundEffect ClickSound
        {
            get
            {
                return clickSound;
            }
            set
            {
                if (clickSound != value)
                {
                    clickSound = value;
                }
            }
        }

        /// <summary>
        /// Sound to be played on hover
        /// </summary>
        public SoundEffect HoverSound
        {
            get
            {
                return hoverSound;
            }
            set
            {
                if (hoverSound != value)
                {
                    hoverSound = value;
                }
            }
        }

        /// <summary>
        /// Default background
        /// </summary>
        public Brush Background
        {
            get
            {
                return background;
            }
            set
            {
                if (background != value)
                {
                    background = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Optional background on hover
        /// </summary>
        public Brush HoveredBackground
        {
            get
            {
                return hoveredBackground;
            }
            set
            {
                if (hoveredBackground != value)
                {
                    hoveredBackground = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Optional background on pressed
        /// </summary>
        public Brush PressedBackground
        {
            get { return pressedBackground; }
            set
            {
                if (pressedBackground != value)
                {
                    pressedBackground = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Brush DisabledBackground
        {
            get => disabledBackground;
            set
            {
                if (disabledBackground != value)
                {
                    disabledBackground = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Legt den äußeren Abstand des Controls fest.
        /// </summary>
        public Border Margin
        {
            get
            {
                return margin;
            }
            set
            {
                if (!margin.Equals(value))
                {
                    margin = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Legt den inneren Abstand des Controls fest.
        /// </summary>
        public Border Padding
        {
            get
            {
                return padding;
            }
            set
            {
                if (!padding.Equals(value))
                {
                    padding = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Legt fest ob der Fokus-Rahmen gezeichnet werden soll
        /// </summary>
        public bool DrawFocusFrame { get; set; }

        /// <summary>
        /// Platzhalter für jegliche Art der Referenz.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gibt den Style-Namen des Controls zurück oder legt diesen fest.
        /// </summary>
        public string Style { get; private set; }

        public Control(BaseScreenComponent manager, string style = "")
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            ScreenManager = manager;
            Style = style;

            children = new ControlCollection(this);
            children.OnInserted += ControlCollectionInsert;
            children.OnRemove += ControlCollectionRemove;
            _rootPathTemp = new List<Control>();
            _rootPath = new ReverseEnumerable<Control>();
            _rootPath.BaseList = _rootPathTemp;

            manager.ClientSizeChanged += (s, e) =>
            {
                OnResolutionChanged();
            };

            ApplySkin(typeof(Control));
        }

        protected void ApplySkin(Type type)
        {
            // ControlSkin-Loader
            if (Skin.Current != null &&
                Skin.Current.ControlSkins != null)
            {
                // Generische Datentypen
                TypeInfo info = type.GetTypeInfo();
                if (info.IsGenericType && Skin.Current.ControlSkins.ContainsKey(type.GetGenericTypeDefinition()))
                    Skin.Current.ControlSkins[type.GetGenericTypeDefinition()](this);

                // Konkrete Datentypen
                if (Skin.Current.ControlSkins.ContainsKey(type))
                    Skin.Current.ControlSkins[type](this);
            }

            // StyleSkin-Loader
            if (!string.IsNullOrEmpty(Style) && // Nur wenn der Style gesetzt ist
                type == GetType() &&            // Nur wenn der Type == dem Type des Controls entspricht (bedeutet der letzte Aufruf auf ApplySkin)
                Skin.Current != null &&
                Skin.Current.StyleSkins != null &&
                Skin.Current.StyleSkins.ContainsKey(Style))
            {
                Skin.Current.StyleSkins[Style](this);
            }
        }

        public void Update(GameTime gameTime)
        {
            HandleTransitions(gameTime);
            OnUpdate(gameTime);
            foreach (var child in Children.AgainstZOrder)
                child.Update(gameTime);
        }

        protected virtual void OnUpdate(GameTime gameTime) { }

        public void PreDraw(GameTime gameTime)
        {
            OnPreDraw(gameTime);
            foreach (var child in Children.AgainstZOrder)
                child.PreDraw(gameTime);
        }

        protected virtual void OnPreDraw(GameTime gameTime) { }

        private readonly RasterizerState _rasterizerState = new RasterizerState { ScissorTestEnable = true };

        /// <summary>
        /// Zeichenauruf für das Control (SpriteBatch ist bereits aktiviert)
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="gameTime">Vergangene Spielzeit</param>
        public void Draw(SpriteBatch batch, Rectangle renderMask, GameTime gameTime)
        {
            if (!Visible) return;

            // Controlgröße ermitteln
            Rectangle controlArea = new Rectangle(AbsolutePosition, ActualSize);
            Rectangle localRenderMask = controlArea.Intersection(renderMask);

            // Scissor-Filter aktivieren
            batch.GraphicsDevice.ScissorRectangle = localRenderMask.Transform(AbsoluteTransformation);
            batch.Begin(rasterizerState: _rasterizerState, samplerState: SamplerState.LinearWrap, transformMatrix: AbsoluteTransformation);
            OnDraw(batch, controlArea, gameTime);
            batch.End();

            foreach (var child in Children.AgainstZOrder)
            {
                Rectangle clientArea = ActualClientArea;
                clientArea.Location += AbsolutePosition;
                Rectangle clientRenderMask = clientArea.Intersection(renderMask);

                child.Draw(batch, clientRenderMask, gameTime);
            }

            invalidDrawing = false;
        }

        /// <summary>
        /// Malt das komplette Control
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="gameTime">GameTime</param>
        /// <param name="controlArea">Bereich für das Control in absoluten Koordinaten</param>
        protected virtual void OnDraw(SpriteBatch batch, Rectangle controlArea, GameTime gameTime)
        {
            // Background-Bereich ermitteln und zeichnen
            Rectangle controlWithMargin = new Rectangle(
               controlArea.X + Margin.Left,
               controlArea.Y + Margin.Top,
               controlArea.Width - Margin.Left - Margin.Right,
               controlArea.Height - Margin.Bottom - Margin.Top);
            OnDrawBackground(batch, controlWithMargin, gameTime, AbsoluteAlpha);

            // Content-Bereich ermitteln und zeichnen
            Rectangle controlWithPadding = new Rectangle(
                controlWithMargin.X + Padding.Left,
                controlWithMargin.Y + Padding.Top,
                controlWithMargin.Width - Padding.Left - Padding.Right,
                controlWithMargin.Height - Padding.Bottom - Padding.Top);
            OnDrawContent(batch, controlWithPadding, gameTime, AbsoluteAlpha);

            // Fokus-Frame
            if (Focused == TreeState.Active)
                OnDrawFocusFrame(batch, controlWithMargin, gameTime, AbsoluteAlpha);
        }

        /// <summary>
        /// Malt den Hintergrund des Controls
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="backgroundArea">Bereich für den Background in absoluten Koordinaten</param>
        /// <param name="gameTime">GameTime</param>
        /// <param name="alpha">Die Transparenz des Controls.</param>
        protected virtual void OnDrawBackground(engenious.Graphics.SpriteBatch batch, Rectangle backgroundArea, GameTime gameTime, float alpha)
        {
            // Standard Background zeichnen
            if (!Enabled && DisabledBackground != null)
                DisabledBackground.Draw(batch, backgroundArea, alpha);
            else if (Pressed && PressedBackground != null)
                PressedBackground.Draw(batch, backgroundArea, alpha);
            else if (Hovered != TreeState.None && HoveredBackground != null)
                HoveredBackground.Draw(batch, backgroundArea, alpha);
            else if (Background != null)
                Background.Draw(batch, backgroundArea, alpha);
        }

        /// <summary>
        /// Malt den Content des Controls
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="contentArea">Bereich für den Content in absoluten Koordinaten</param>
        /// <param name="gameTime">GameTime</param>
        /// <param name="alpha">Die Transparenz des Controls.</param>
        protected virtual void OnDrawContent(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
        }

        /// <summary>
        /// Malt den Fokusrahmen des Controls
        /// </summary>
        /// <param name="batch">Spritebatch</param>
        /// <param name="contentArea">Bereich für den Content in absoluten Koordinaten</param>
        /// <param name="gameTime">GameTime</param>
        /// <param name="alpha">Die Transparenz des Controls.</param>
        protected virtual void OnDrawFocusFrame(SpriteBatch batch, Rectangle contentArea, GameTime gameTime, float alpha)
        {
            if (Skin.Current.FocusFrameBrush != null && DrawFocusFrame)
                Skin.Current.FocusFrameBrush.Draw(batch, contentArea, AbsoluteAlpha);
        }

        public void InvalidateDrawing()
        {
            invalidDrawing = true;
        }

        #region Visual Tree Handling

        private bool enabled = true;

        private bool visible = true;

        private Control parent = null;

        private ControlCollection children;

        private readonly PropertyEventArgs<bool> _enabledChangedEventArgs = new PropertyEventArgs<bool>();
        /// <summary>
        /// Gibt an, ob das Control aktiv ist.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled == value) return;

                _enabledChangedEventArgs.OldValue = enabled;
                _enabledChangedEventArgs.NewValue = value;
                _enabledChangedEventArgs.Handled = false;

                enabled = value;
                InvalidateDrawing();

                if (!enabled) Unfocus();

                foreach (var child in Children)
                    child.Enabled = enabled;

                OnEnableChanged(_enabledChangedEventArgs);
                EnableChanged?.Invoke(this, _enabledChangedEventArgs);
            }
        }

        /// <summary>
        /// Ermittelt den absoluten Aktivierungsstatus von Root bis zu diesem Control.
        /// </summary>
        public bool AbsoluteEnabled
        {
            get
            {
                foreach (var item in RootPath)
                    if (!item.Enabled)
                        return false;
                return true;
            }
        }

        private readonly PropertyEventArgs<bool> _visibleChangedEventArgs = new PropertyEventArgs<bool>();
        /// <summary>
        /// Gibt an, ob das Control gerendert werden soll.
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible == value) return;


                _visibleChangedEventArgs.OldValue = visible;
                _visibleChangedEventArgs.NewValue = value;
                _visibleChangedEventArgs.Handled = false;

                visible = value;
                InvalidateDimensions();
                InvalidateDrawing();
                if (!visible) Unfocus();

                OnVisibleChanged(_visibleChangedEventArgs);
                VisibleChanged?.Invoke(this, _visibleChangedEventArgs);
            }
        }

        /// <summary>
        /// Ermittelt den absoluten Sichtbarkeitsstatus von Root bis zu diesem Control.
        /// </summary>
        public bool AbsoluteVisible
        {
            get
            {
                bool result = true;
                foreach (var item in RootPath)
                    result &= item.Visible;
                return result;
            }
        }


        private readonly CollectionEventArgs _controlCollectionInsertArgs = new CollectionEventArgs();
        private void ControlCollectionInsert(Control item, int index)
        {
            _controlCollectionInsertArgs.Control = item;
            _controlCollectionInsertArgs.Index = index;

            OnInsertControl(_controlCollectionInsertArgs);
        }

        private readonly CollectionEventArgs _controlCollectionRemoveArgs = new CollectionEventArgs();
        private void ControlCollectionRemove(Control item, int index)
        {
            _controlCollectionRemoveArgs.Control = item;
            _controlCollectionRemoveArgs.Index = index;

            OnRemoveControl(_controlCollectionRemoveArgs);
        }

        /// <summary>
        /// Gibt das Root-Element des zugehörigen Visual Tree zurück.
        /// </summary>
        public Control Root
        {
            get
            {
                Control result = this;
                while (result.Parent != null)
                    result = result.Parent;
                return result;
            }
        }

        private readonly List<Control> _rootPathTemp;
        private readonly ReverseEnumerable<Control> _rootPath;
        /// <summary>
        /// Liefert den Control-Path von Root zum aktuellen Control.
        /// </summary>
        public ReverseEnumerable<Control> RootPath
        {
            get
            {
                if (PathDirty)
                {
                    // Collect Path
                    _rootPathTemp.Clear();
                    Control pointer = this;
                    do
                    {
                        _rootPathTemp.Add(pointer);
                        pointer = pointer.Parent;
                    } while (pointer != null);


                    PathDirty = false;
                }
                return _rootPath;
            }
        }


        private readonly PropertyEventArgs<Control> _parentChangedEventArgs = new PropertyEventArgs<Control>();
        /// <summary>
        /// Gibt das Parent-Element dieses Controls zurück.
        /// </summary>
        public Control Parent
        {
            get { return parent; }
            internal set
            {
                if (parent == value) return;

                _parentChangedEventArgs.OldValue = parent;
                _parentChangedEventArgs.NewValue = value;
                _parentChangedEventArgs.Handled = false;

                parent = value;

                OnParentChanged(_parentChangedEventArgs);
                ParentChanged?.Invoke(this, _parentChangedEventArgs);
            }
        }

        /// <summary>
        /// Die Liste der enthaltenen Controls.
        /// </summary>
        protected ControlCollection Children { get { return children; } }

        /// <summary>
        /// Ein neues Control wurde in die Children-Liste eingefügt.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnInsertControl(CollectionEventArgs args) { }

        /// <summary>
        /// Ein Control wurde aus der Children-Liste entfernt.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRemoveControl(CollectionEventArgs args) { }

        /// <summary>
        /// Der Parent dieses Controls hat sich geändert.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnParentChanged(PropertyEventArgs<Control> args) { }

        protected virtual void OnEnableChanged(PropertyEventArgs<bool> args) { }

        protected virtual void OnVisibleChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Event das die Änderung des Parents signalisiert.
        /// </summary>
        public event PropertyChangedDelegate<Control> ParentChanged;

        public event PropertyChangedDelegate<bool> EnableChanged;

        public event PropertyChangedDelegate<bool> VisibleChanged;

        #endregion

        #region Resize- und Position-Management

        private bool invalidDimensions = true;

        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;

        private VerticalAlignment verticalAlignment = VerticalAlignment.Center;

        private Point actualPosition = Point.Zero;

        private Point actualSize = Point.Zero;

        private int? minWidth = null;

        private int? width = null;

        private int? maxWidth = null;

        private int? minHeight = null;

        private int? height = null;

        private int? maxHeight = null;

        private Matrix transformation = Matrix.Identity;

        private float alpha = 1f;

        /// <summary>
        /// Horizontale Ausrichtung im Dynamic-Mode.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return horizontalAlignment;
            }
            set
            {
                horizontalAlignment = value;
                InvalidateDimensions();
            }
        }

        /// <summary>
        /// Gibt eine zusätzliche Render-Transformation an.
        /// </summary>
        public Matrix Transformation
        {
            get
            {
                return transformation;
            }
            set
            {
                if (transformation != value)
                {
                    transformation = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Gibt die absolute Transformation für dieses Control zurück.
        /// </summary>
        public Matrix AbsoluteTransformation
        {
            get
            {
                Matrix result = Matrix.Identity;
                foreach (var item in RootPath)
                    result *= item.Transformation;
                return result;
            }
        }

        /// <summary>
        /// Gibt den Alpha-Wert dieses Controls an.
        /// </summary>
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (alpha != value)
                {
                    alpha = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Gibt den absoluten Alpha-Wert für dieses Control zurück.
        /// </summary>
        public float AbsoluteAlpha
        {
            get
            {
                float result = 1f;
                foreach (var item in RootPath)
                    result *= item.Alpha;
                return result;
            }
        }

        /// <summary>
        /// Vertikale Ausrichtung im Dynamic-Mode.
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return verticalAlignment;
            }
            set
            {
                verticalAlignment = value;
                InvalidateDimensions();
            }
        }

        /// <summary>
        /// Legt optional eine Mindestbreite für dieses Control fest.
        /// </summary>
        public int? MinWidth
        {
            get { return minWidth; }
            set
            {
                if (minWidth != value)
                {
                    minWidth = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Legt optional eine definierte Breite für dieses Control fest.
        /// </summary>
        public int? Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Legt optional eine Maximalbreite für dieses Control fest.
        /// </summary>
        public int? MaxWidth
        {
            get { return maxWidth; }
            set
            {
                if (maxWidth != value)
                {
                    maxWidth = value;
                    InvalidateDimensions();
                }
            }
        }


        /// <summary>
        /// Legt optional eine Mindesthöhe für dieses Control fest.
        /// </summary>
        public int? MinHeight
        {
            get { return minHeight; }
            set
            {
                if (minHeight != value)
                {
                    minHeight = value;
                    InvalidateDimensions();
                }
            }
        }


        /// <summary>
        /// Legt optional eine definierte Höhe für dieses Control fest. 
        /// </summary>
        public int? Height
        {
            get { return height; }
            set
            {
                if (height != value)
                {
                    height = value;
                    InvalidateDimensions();
                }
            }
        }

        /// <summary>
        /// Legt optional eine Maximalbreite für dieses Control fest.
        /// </summary>
        public int? MaxHeight
        {
            get { return maxHeight; }
            set
            {
                if (maxHeight != value)
                {
                    maxHeight = value;
                    InvalidateDimensions();
                }
            }
        }


        /// <summary>
        /// Gibt die tatsächliche Renderposition (exkl. Parent Offset) zurück.
        /// </summary>
        public Point ActualPosition
        {
            get
            {
                return actualPosition;
            }
            set
            {
                if (actualPosition != value)
                {
                    actualPosition = value;
                    InvalidateDrawing();
                }
                invalidDimensions = false;
            }
        }

        /// <summary>
        /// Gibt die absolute Position (global) dieses Controls zurück.
        /// </summary>
        public Point AbsolutePosition
        {
            get
            {
                Point result = Point.Zero;
                if (Parent != null)
                {
                    result += Parent.AbsolutePosition;
                    result += parent.ActualClientArea.Location;
                }
                result += ActualPosition;
                return result;
            }
        }

        /// <summary>
        /// Gibt die tatsächliche Rendergröße zurück.
        /// </summary>
        public Point ActualSize
        {
            get
            {
                return actualSize;
            }
            set
            {
                if (actualSize != value)
                {
                    actualSize = value;
                    InvalidateDrawing();
                }
                invalidDimensions = false;
            }
        }

        /// <summary>
        /// Methode zur Ermittlung des notwendigen Platzes.
        /// </summary>
        /// <param name="available">Verfügbarer Platz für dieses Control</param>
        /// <returns>Benötigte Platz inklusive allen Rändern</returns>
        public virtual Point GetExpectedSize(Point available)
        {
            if (!Visible) return Point.Zero;

            Point result = GetMinClientSize(available);
            Point client = GetMaxClientSize(available);

            // Restliche Controls
            foreach (var child in Children)
            {
                Point size = child.GetExpectedSize(client);
                result = new Point(Math.Max(result.X, size.X), Math.Max(result.Y, size.Y));
            }
            return result + Borders;
        }

        /// <summary>
        /// Ermittelt die maximale Größe des Client Bereichs für dieses Control.
        /// </summary>
        /// <param name="containerSize"></param>
        /// <returns></returns>
        public virtual Point GetMaxClientSize(Point containerSize)
        {
            int x = Width.HasValue ? Width.Value : containerSize.X;

            // Max Limiter
            if (MaxWidth.HasValue)
                x = Math.Min(MaxWidth.Value, x);

            // Min Limiter
            if (MinWidth.HasValue)
                x = Math.Max(MinWidth.Value, x);

            int y = Height.HasValue ? Height.Value : containerSize.Y;

            // Max Limiter
            if (MaxHeight.HasValue)
                y = Math.Min(MaxHeight.Value, y);

            // Min Limiter
            if (MinHeight.HasValue)
                y = Math.Max(MinHeight.Value, y);

            return new Point(x, y) - Borders;
        }

        /// <summary>
        /// Ermittelt die minimale Größe des Client Bereichs für dieses Control.
        /// </summary>
        /// <param name="containerSize"></param>
        /// <returns></returns>
        public virtual Point GetMinClientSize(Point containerSize)
        {
            Point size = CalculcateRequiredClientSpace(containerSize) + Borders;
            int x = Width.HasValue ? Width.Value : size.X;
            int y = Height.HasValue ? Height.Value : size.Y;

            // Max Limiter
            if (MaxWidth.HasValue)
                x = Math.Min(MaxWidth.Value, x);

            // Min Limiter
            if (MinWidth.HasValue)
                x = Math.Max(MinWidth.Value, x);

            // Max Limiter
            if (MaxHeight.HasValue)
                y = Math.Min(MaxHeight.Value, y);

            // Min Limiter
            if (MinHeight.HasValue)
                y = Math.Max(MinHeight.Value, y);

            return new Point(x, y) - Borders;
        }

        /// <summary>
        /// Legt die tatsächliche Größe für dieses Control fest.
        /// </summary>
        /// <param name="available">Erwartete Größe des Controls (inkl. Borders)</param>
        public virtual void SetActualSize(Point available)
        {
            if (!Visible)
            {
                SetDimension(Point.Zero, available);
                return;
            }

            Point minSize = GetExpectedSize(available);
            SetDimension(minSize, available);

            // Auf andere Controls anwenden
            foreach (var child in Children)
                child.SetActualSize(ActualClientSize);
        }

        /// <summary>
        /// Führt eine automatische Anordnung auf Basis der aktuellen Size und den Alignment-Parametern durch.
        /// </summary>
        /// <param name="containerSize"></param>
        protected virtual void SetDimension(Point actualSize, Point containerSize)
        {
            var size = new Point(
                Math.Min(containerSize.X, HorizontalAlignment == HorizontalAlignment.Stretch ? containerSize.X : actualSize.X),
                Math.Min(containerSize.Y, VerticalAlignment == VerticalAlignment.Stretch ? containerSize.Y : actualSize.Y));

            Point minSize = GetMinClientSize(containerSize) + Borders;
            Point maxSize = GetMaxClientSize(containerSize) + Borders;

            size.X = Math.Max(minSize.X, Math.Min(maxSize.X, size.X));
            size.Y = Math.Max(minSize.Y, Math.Min(maxSize.Y, size.Y));

            ActualSize = size;

            int x = 0;
            int y = 0;

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    x = (containerSize.X - ActualSize.X) / 2;
                    break;
                case HorizontalAlignment.Right:
                    x = containerSize.X - ActualSize.X;
                    break;
            }

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    y = (containerSize.Y - ActualSize.Y) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    y = containerSize.Y - ActualSize.Y;
                    break;
            }

            ActualPosition = new Point(x, y);
        }

        /// <summary>
        /// Gibt zurück, ob die Größenangaben nicht mehr aktuell sind.
        /// </summary>
        /// <returns></returns>
        public bool HasInvalidDimensions()
        {
            bool result = invalidDimensions;
            foreach (var child in Children)
                result |= child.HasInvalidDimensions();
            return result;
        }

        /// <summary>
        /// Ist für die Berechnung des Client-Contents zuständig und erleichtert das automatische Alignment.
        /// </summary>
        /// <returns></returns>
        public virtual Point CalculcateRequiredClientSpace(Point available)
        {
            return new Point();
        }

        /// <summary>
        /// Teilt dem Steuerelement mit, dass seine Größe neu berechnet werden muss.
        /// </summary>
        public void InvalidateDimensions()
        {
            invalidDimensions = true;
            InvalidateDrawing();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Auflösung des Fensters geändert wird.
        /// </summary>
        public virtual void OnResolutionChanged()
        {
            InvalidateDimensions();
        }

        /// <summary>
        /// Berechnet den Client-Bereich auf Basis der aktuellen 
        /// Position/Größe/Margin/Padding in lokalen Koordinaten.
        /// </summary>
        public Rectangle ActualClientArea
        {
            get
            {
                return new Rectangle(
                    Margin.Left + Padding.Left,
                    Margin.Top + Padding.Top,
                    ActualSize.X - Margin.Left - Padding.Left - Margin.Right - Padding.Right,
                    ActualSize.Y - Margin.Top - Padding.Top - Margin.Bottom - Padding.Bottom);
            }
        }

        /// <summary>
        /// Berechnet den Verfügbaren Client-Bereich unter Berücksichtigung der 
        /// ActualSize und den eingestellten Margins und Paddings.
        /// </summary>
        public Point ActualClientSize
        {
            get
            {
                return ActualSize - Borders;
            }
        }

        /// <summary>
        /// Ermittelt den gesamten Rand durch Margin und Padding.
        /// </summary>
        public Point Borders
        {
            get
            {
                return new Point(
                    Margin.Left + Margin.Right + Padding.Left + Padding.Right,
                    Margin.Top + Margin.Bottom + Padding.Top + Padding.Bottom);
            }
        }

        #endregion

        #region Transitions

        private Dictionary<Type, Transition> transitionMap = new Dictionary<Type, Transition>();
        private List<Transition> transitions = new List<Transition>();

        public void StartTransition(Transition transition)
        {
            if (transitionMap.ContainsKey(transition.GetType()))
            {
                // Alte Transition des selben Typs entfernen
                Transition t = transitionMap[transition.GetType()];
                transitionMap.Remove(transition.GetType());
                transitions.Remove(t);
            }

            // Neue Transition einfügen
            transitionMap.Add(transition.GetType(), transition);
            transitions.Add(transition);
        }

        private void HandleTransitions(GameTime gameTime)
        {
            // Transitions durchlaufen
            //List<Transition> drops = new List<Transition>();
            for (var i = 0; i < transitions.Count; i++)
            {
                var transition = transitions[i];
                if (!transition.Update(gameTime))
                {
                    transitionMap.Remove(transition.GetType());
                    transitions.RemoveAt(i--);
                }
                //drops.Add(transition);
            }

            // Abgelaufene Transitions wieder entfernen
            //foreach (var drop in drops)
            //{
            //    transitions.Remove(drop);
            //    transitionMap.Remove(drop.GetType());
            //}
        }

        #endregion

        #region Pointer Management

        private TreeState hovered = TreeState.None;

        private bool dropHovered = false;

        private bool pressed = false;

        private readonly PropertyEventArgs<TreeState> _hoveredChangedEventArgs = new PropertyEventArgs<TreeState>();
        /// <summary>
        /// Gibt an, ob das Control unter der Maus ist.
        /// </summary>
        public TreeState Hovered
        {
            get
            {
                return hovered;
            }
            private set
            {
                if (hovered == value) return;

                _hoveredChangedEventArgs.OldValue = hovered;
                _hoveredChangedEventArgs.NewValue = value;
                _hoveredChangedEventArgs.Handled = false;

                hovered = value;
                InvalidateDrawing();

                OnHoveredChanged(_hoveredChangedEventArgs);
                HoveredChanged?.Invoke(this, _hoveredChangedEventArgs);

                // Sound abspielen
                if (hoverSound != null && hovered == TreeState.Active && _hoveredChangedEventArgs.OldValue != TreeState.Passive)
                    hoverSound.Play();
            }
        }

        /// <summary>
        /// Gibt an ob das aktuelle Elemente gerade duch die Maus gedrückt wird.
        /// </summary>
        public bool Pressed
        {
            get { return pressed; }
            private set
            {
                if (pressed != value)
                {
                    pressed = value;
                    InvalidateDrawing();
                }
            }
        }

        /// <summary>
        /// Wird vom Parent aufgerufen wenn sich die Maus bewegt
        /// </summary>
        /// <param name="args">Parameter für Mouse-Events</param>
        /// <returns>Event verarbeitet?</returns>
        internal bool InternalMouseMove(MouseEventArgs args)
        {
            // Children first (Order by Z-Order)
            bool passive = false;
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                bool handled = child.InternalMouseMove(args);
                passive |= handled;
                args.Bubbled = handled || args.Bubbled;
            }

            // Ermitteln ob hovered ist (Aktive & Passive)
            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            bool hovered =
                args.LocalPosition.X >= 0 &&
                args.LocalPosition.Y >= 0 &&
                args.LocalPosition.X < ActualSize.X &&
                args.LocalPosition.Y < ActualSize.Y;

            // Wenn sich der Hover-Status verändert hat -> Enter & Exit-Events
            if ((Hovered != TreeState.None) != hovered)
            {
                if (hovered)
                {
                    OnMouseEnter(args);
                    if (MouseEnter != null)
                        MouseEnter(this, args);
                }
                else
                {
                    // Pressed-State abgeben
                    Pressed = false;

                    OnMouseLeave(args);
                    if (MouseLeave != null)
                        MouseLeave(this, args);
                }
            }

            // Event für Mausbewegung
            OnMouseMove(args);
            if (MouseMove != null)
                MouseMove(this, args);

            // Hover-State neu setzen
            TreeState newState = TreeState.None;
            if (hovered) newState = passive ? TreeState.Passive : TreeState.Active;
            Hovered = newState;

            return hovered;
        }

        internal bool InternalLeftMouseDown(MouseEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Fokusieren
            Focus();

            // Pressed-State aktivieren
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalLeftMouseDown(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnLeftMouseDown(args);
                LeftMouseDown?.Invoke(this, args);
            }

            return Background != null;
        }

        internal void InternalLeftMouseUp(MouseEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalLeftMouseUp(args);
            }

            // Lokales Events
            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            OnLeftMouseUp(args);
            LeftMouseUp?.Invoke(this, args);
        }

        internal bool InternalLeftMouseClick(MouseEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalLeftMouseClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnLeftMouseClick(args);
                LeftMouseClick?.Invoke(this, args);
            }

            // Click-Sound abspielen
            if (clickSound != null)
                clickSound.Play();

            return Background != null;
        }

        internal bool InternalLeftMouseDoubleClick(MouseEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalLeftMouseDoubleClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnLeftMouseDoubleClick(args);
                if (LeftMouseDoubleClick != null)
                    LeftMouseDoubleClick(this, args);
            }

            // Click-Sound abspielen
            if (clickSound != null)
                clickSound.Play();

            return Background != null;
        }

        internal bool InternalRightMouseDown(MouseEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            Focus();

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalRightMouseDown(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnRightMouseDown(args);
                if (RightMouseDown != null)
                    RightMouseDown(this, args);
            }

            return Background != null;
        }

        internal void InternalRightMouseUp(MouseEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalRightMouseUp(args);
            }

            // Lokales Events
            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            OnRightMouseUp(args);
            if (RightMouseUp != null)
                RightMouseUp(this, args);
        }

        internal bool InternalRightMouseClick(MouseEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalRightMouseClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnRightMouseClick(args);
                if (RightMouseClick != null)
                    RightMouseClick(this, args);
            }

            return Background != null;
        }

        internal bool InternalRightMouseDoubleClick(MouseEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalRightMouseDoubleClick(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnRightMouseDoubleClick(args);
                if (RightMouseDoubleClick != null)
                    RightMouseDoubleClick(this, args);
            }

            return Background != null;
        }

        internal bool InternalMouseScroll(MouseScrollEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalMouseScroll(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnMouseScroll(args);
                if (MouseScroll != null)
                    MouseScroll(this, args);
            }

            return Background != null;
        }

        internal bool InternalTouchDown(TouchEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Fokusieren
            Focus();

            // Pressed-State aktivieren
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalTouchDown(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchDown(args);
                if (TouchDown != null)
                    TouchDown(this, args);
            }

            return Background != null;
        }

        internal void InternalTouchMove(TouchEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalTouchMove(args);
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchMove(args);
                if (TouchMove != null)
                    TouchMove(this, args);
            }
        }

        internal void InternalTouchUp(TouchEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                child.InternalTouchUp(args);
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchUp(args);
                if (TouchUp != null)
                    TouchUp(this, args);
            }
        }

        internal bool InternalTouchTap(TouchEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Fokusieren
            Focus();

            // Pressed-State aktivieren
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalTouchTap(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchTap(args);
                if (TouchTap != null)
                    TouchTap(this, args);
            }

            return Background != null;
        }

        internal bool InternalTouchDoubleTap(TouchEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Fokusieren
            Focus();

            // Pressed-State aktivieren
            Pressed = true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalTouchDoubleTap(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Lokales Events
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnTouchDoubleTap(args);
                if (TouchDoubleTap != null)
                    TouchDoubleTap(this, args);
            }

            return Background != null;
        }

        private Point CalculateLocalPosition(Point global, Control control)
        {
            Point absolutePosition = control.AbsolutePosition;
            Vector2 local = Vector2.Transform(
                Matrix.Invert(control.AbsoluteTransformation),
                new Vector2(global.X - absolutePosition.X, global.Y - absolutePosition.Y));
            return new Point((int)local.X, (int)local.Y);
        }

        protected virtual void OnMouseEnter(MouseEventArgs args) { }

        protected virtual void OnMouseLeave(MouseEventArgs args) { }

        protected virtual void OnMouseMove(MouseEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste heruntergedrückt wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Event.</param>
        protected virtual void OnLeftMouseDown(MouseEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste losgelassen wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Event.</param>
        protected virtual void OnLeftMouseUp(MouseEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn mit der linken Maustaste auf das Steuerelement geklickt wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Ereignis.</param>
        protected virtual void OnLeftMouseClick(MouseEventArgs args) { }

        protected virtual void OnLeftMouseDoubleClick(MouseEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn die rechte Maustaste heruntergedrückt wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Event.</param>
        protected virtual void OnRightMouseDown(MouseEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn die rechte Maustaste losgelassen wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Event.</param>
        protected virtual void OnRightMouseUp(MouseEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn mit der rechten Maustaste auf das Steuerelement geklickt wird.
        /// </summary>
        /// <param name="args">Weitere Informationen zum Ereignis.</param>
        protected virtual void OnRightMouseClick(MouseEventArgs args) { }

        protected virtual void OnRightMouseDoubleClick(MouseEventArgs args) { }

        protected virtual void OnMouseScroll(MouseScrollEventArgs args) { }

        protected virtual void OnTouchDown(TouchEventArgs args) { }

        protected virtual void OnTouchMove(TouchEventArgs args) { }

        protected virtual void OnTouchUp(TouchEventArgs args) { }

        protected virtual void OnTouchTap(TouchEventArgs args) { }

        protected virtual void OnTouchDoubleTap(TouchEventArgs args) { }

        protected virtual void OnHoveredChanged(PropertyEventArgs<TreeState> args) { }

        public event MouseEventDelegate MouseEnter;

        public event MouseEventDelegate MouseLeave;

        public event MouseEventDelegate MouseMove;

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste heruntergedrückt wird.
        /// </summary>
        public event MouseEventDelegate LeftMouseDown;

        /// <summary>
        /// Wird aufgerufen, wenn die linke Maustaste losgelassen wird.
        /// </summary>
        public event MouseEventDelegate LeftMouseUp;

        /// <summary>
        /// Wird aufgerufen, wenn mit der linken Maustaste auf das Steuerelement geklickt wird.
        /// </summary>
        public event MouseEventDelegate LeftMouseClick;

        public event MouseEventDelegate LeftMouseDoubleClick;

        /// <summary>
        /// Wird aufgerufen, wenn die rechte Maustaste heruntergedrückt wird.
        /// </summary>
        public event MouseEventDelegate RightMouseDown;

        /// <summary>
        /// Wird aufgerufen, wenn die rechte Maustaste losgelassen wird.
        /// </summary>
        public event MouseEventDelegate RightMouseUp;

        /// <summary>
        /// Wird aufgerufen, wenn mit der rechten Maustaste auf das Steuerelement geklickt wird.
        /// </summary>
        public event MouseEventDelegate RightMouseClick;

        public event MouseEventDelegate RightMouseDoubleClick;

        public event MouseScrollEventDelegate MouseScroll;

        public event TouchEventDelegate TouchDown;

        public event TouchEventDelegate TouchMove;

        public event TouchEventDelegate TouchUp;

        public event TouchEventDelegate TouchTap;

        public event TouchEventDelegate TouchDoubleTap;

        public event PropertyChangedDelegate<TreeState> HoveredChanged;

        #endregion

        #region Keyboard Management

        internal void InternalKeyDown(KeyEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyDown(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyDown(args);
                KeyDown?.Invoke(this, args);
            }
        }

        internal void InternalKeyPress(KeyEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyPress(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyPress(args);
                KeyPress?.Invoke(this, args);
            }
        }

        internal void InternalKeyTextPress(KeyTextEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyTextPress(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyTextPress(args);
                KeyTextPress?.Invoke(this, args);
            }
        }

        internal void InternalKeyUp(KeyEventArgs args)
        {
            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                child.InternalKeyUp(args);
                if (args.Handled)
                    break;
            }

            // Bubble up
            if (!args.Handled)
            {
                OnKeyUp(args);
                KeyUp?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt wird.
        /// </summary>
        /// <param name="args">Zusätzliche Daten zum Event.</param>
        protected virtual void OnKeyDown(KeyEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste losgelassen wird.
        /// </summary>
        /// <param name="args">Zusätzliche Daten zum Event.</param>
        protected virtual void OnKeyUp(KeyEventArgs args) { }

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt ist.
        /// </summary>
        /// <param name="args">Zusätzliche Daten zum Event.</param>
        protected virtual void OnKeyPress(KeyEventArgs args) { }

        protected virtual void OnKeyTextPress(KeyTextEventArgs args)
        {

        }

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt wird.
        /// </summary>
        public event KeyEventDelegate KeyDown;

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste losgelassen wird.
        /// </summary>
        public event KeyEventDelegate KeyUp;

        /// <summary>
        /// Wird aufgerufen, wenn eine Taste gedrückt ist.
        /// </summary>
        public event KeyEventDelegate KeyPress;

        public event KeyTextEventDelegate KeyTextPress;

        #endregion

        #region Tabbing & Fokus

        private bool focused = false;

        private bool tabStop = false;

        private bool canFocus = false;

        private int tabOrder = 0;

        private int zOrder = 0;
        public bool PathDirty = true;


        private readonly PropertyEventArgs<bool> _tabStopChangedEventArgs = new PropertyEventArgs<bool>();
        /// <summary>
        /// Legt fest, ob das Control per Tab zu erreichen ist.
        /// </summary>
        public bool TabStop
        {
            get { return tabStop; }
            set
            {
                if (tabStop == value) return;

                _tabStopChangedEventArgs.OldValue = tabStop;
                _tabStopChangedEventArgs.NewValue = value;
                _tabStopChangedEventArgs.Handled = false;

                tabStop = value;

                OnTabStopChanged(_tabStopChangedEventArgs);
                TabStopChanged?.Invoke(this, _tabStopChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<bool> _canFocusChangedEventArgs = new PropertyEventArgs<bool>();
        /// <summary>
        /// Gibt an ob das Control den Fokus bekommen kann oder legt dies fest.
        /// </summary>
        public bool CanFocus
        {
            get { return canFocus; }
            set
            {
                if (canFocus == value) return;

                _canFocusChangedEventArgs.OldValue = canFocus;
                _canFocusChangedEventArgs.NewValue = value;
                _canFocusChangedEventArgs.Handled = false;

                canFocus = value;
                if (!canFocus) Unfocus();

                OnCanFocusChanged(_canFocusChangedEventArgs);
                CanFocusChanged?.Invoke(this, _canFocusChangedEventArgs);
            }
        }

        private readonly PropertyEventArgs<int> _tabOrderChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gibt die Position der Tab-Reihenfolge an.
        /// </summary>
        public int TabOrder
        {
            get { return tabOrder; }
            set
            {
                if (tabOrder == value) return;

                _tabOrderChangedEventArgs.OldValue = tabOrder;
                _tabOrderChangedEventArgs.NewValue = value;
                _tabOrderChangedEventArgs.Handled = false;

                tabOrder = value;

                OnTabOrderChanged(_tabOrderChangedEventArgs);
                TabOrderChanged?.Invoke(this, _tabOrderChangedEventArgs);
            }
        }

        /// <summary>
        /// Gibt an ob das aktuelle Control den Fokus hat.
        /// </summary>
        public TreeState Focused
        {
            get
            {
                // Aktuelles Control ist fokusiert
                if (focused) return TreeState.Active;

                // Schauen, ob irgend ein Child fokusiert ist
                foreach (var child in Children.InZOrder)
                    if (child.Focused != TreeState.None)
                        return TreeState.Passive;

                return TreeState.None;
            }
        }

        private readonly PropertyEventArgs<int> _zOrderChangedEventArgs = new PropertyEventArgs<int>();
        /// <summary>
        /// Gibt die grafische Reihenfolge der Controls 
        /// innerhalb eines Containers an. (0 ganz vorne, 9999 weiter hinten)
        /// </summary>
        public int ZOrder
        {
            get { return zOrder; }
            set
            {
                if (zOrder == value) return;

                _zOrderChangedEventArgs.OldValue = zOrder;
                _zOrderChangedEventArgs.NewValue = value;
                _zOrderChangedEventArgs.Handled = false;

                zOrder = value;

                OnZOrderChanged(_zOrderChangedEventArgs);
                ZOrderChanged?.Invoke(this, _zOrderChangedEventArgs);
            }
        }

        /// <summary>
        /// Setzt den Fokus auf dieses Control.
        /// </summary>
        public void Focus()
        {
            if (CanFocus && Visible)
                Root.SetFocus(this);
        }

        /// <summary>
        /// Entfernt den Fokus.
        /// </summary>
        public void Unfocus()
        {
            if (focused)
                Root.SetFocus(null);
        }

        /// <summary>
        /// Setzt den Fokus auf das angegebene Control für den kompletten 
        /// Visual Tree ab diesem Control abwärts.
        /// </summary>
        /// <param name="control"></param>
        internal void SetFocus(Control control)
        {
            // Rekursiver Aufruf
            foreach (var child in Children.InZOrder)
                child.SetFocus(control);

            bool hit = (control == this);
            if (focused != hit)
            {
                EventArgs args = EventArgsPool.Instance.Take();
                if (hit)
                {
                    // Unsichtbare und nicht fokusierbare Elemente ignorieren
                    if (!Visible || !CanFocus || !Enabled)
                        return;

                    focused = true;

                    // Fokus gerade erhalten
                    OnGotFocus(args);
                    GotFocus?.Invoke(this, args);
                }
                else
                {
                    focused = false;

                    // Fokus gerade verloren
                    OnLostFocus(args);
                    LostFocus?.Invoke(this, args);
                }

                EventArgsPool.Instance.Release(args);

                InvalidateDrawing();
            }
        }

        /// <summary>
        /// Tabbt den aktuellen Fokus eines Controls eine Stelle weiter.
        /// </summary>
        /// <returns>Tab konnte in diesem Ast ausgeführt werden</returns>
        internal bool InternalTabbedForward()
        {
            // Unsichtbare Elemente können nicht fokusiert werden
            if (!Visible) return false;

            bool findFocused = Focused != TreeState.None;

            // Root selektiert -> Unselekt
            if (focused)
            {
                Unfocus();
            }

            // Keine Selektion -> Erstes Element selektieren
            else if (Focused == TreeState.None && CanFocus &&
                TabStop && AbsoluteEnabled && AbsoluteVisible)
            {
                Focus();
                return true;
            }

            var controls = Children.OrderBy(c => c.TabOrder).ToArray();
            foreach (var control in controls)
            {
                // Solange skippen, bis das fokusierte Control dran ist
                if (findFocused && control.Focused != TreeState.None)
                    findFocused = false;

                if (!findFocused && control.InternalTabbedForward())
                    return true;
            }

            // Es konnte nichts fokusiert werden
            return false;
        }

        /// <summary>
        /// Tabbt den aktuellen Fokus eines Controls eine Stelle zurück.
        /// </summary>
        /// <returns>Tab konnte in diesem Ast ausgeführt werden</returns>
        internal bool InternalTabbedBackward()
        {
            // Root selektiert -> Unselekt und exit
            if (focused)
            {
                Unfocus();
                return false;
            }

            bool findFocused = Focused != TreeState.None;
            var controls = Children.OrderByDescending(c => c.TabOrder).ToArray();
            foreach (var control in controls)
            {
                // Solange skippen, bis das fokusierte Control dran ist
                if (findFocused && control.Focused != TreeState.None)
                    findFocused = false;

                if (!findFocused && control.InternalTabbedBackward())
                    return true;
            }

            // Noch kein Fokus gefunden -> root
            if (CanFocus && TabStop && AbsoluteEnabled && AbsoluteVisible)
            {
                Focus();
                return true;
            }

            return false;
        }

        protected virtual void OnTabStopChanged(PropertyEventArgs<bool> args) { }

        protected virtual void OnCanFocusChanged(PropertyEventArgs<bool> args) { }

        protected virtual void OnTabOrderChanged(PropertyEventArgs<int> args) { }

        protected virtual void OnZOrderChanged(PropertyEventArgs<int> args) { }

        protected virtual void OnGotFocus(EventArgs args) { }

        protected virtual void OnLostFocus(EventArgs args) { }

        public event PropertyChangedDelegate<bool> TabStopChanged;

        public event PropertyChangedDelegate<bool> CanFocusChanged;

        public event PropertyChangedDelegate<int> TabOrderChanged;

        public event PropertyChangedDelegate<int> ZOrderChanged;

        public event EventDelegate GotFocus;

        public event EventDelegate LostFocus;

        #endregion

        #region Drag & Drop

        internal bool InternalStartDrag(DragEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalStartDrag(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Bubble up
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnStartDrag(args);
                if (StartDrag != null)
                    StartDrag(args);
            }

            return Background != null;
        }

        internal bool InternalDropMove(DragEventArgs args)
        {
            // Children first (Order by Z-Order)
            bool passive = false;
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                bool handled = child.InternalDropMove(args);
                passive |= handled;
                args.Bubbled = handled || args.Bubbled;
            }

            args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
            bool hovered =
                args.LocalPosition.X >= 0 &&
                args.LocalPosition.Y >= 0 &&
                args.LocalPosition.X < ActualSize.X &&
                args.LocalPosition.Y < ActualSize.Y;

            // Wenn sich der DropHover Status verändert hat
            if ((hovered && ScreenManager.Dragging) != dropHovered)
            {
                if (dropHovered)
                {
                    OnDropLeave(args);
                    if (DropLeave != null)
                        DropLeave(args);
                }
                else
                {
                    OnDropEnter(args);
                    if (DropEnter != null)
                        DropEnter(args);
                }
            }

            OnDropMove(args);
            if (DropMove != null)
                DropMove(args);

            dropHovered = hovered && ScreenManager.Dragging;

            return hovered;
        }

        internal bool InternalEndDrop(DragEventArgs args)
        {
            // Ignorieren, falls nicht im Control-Bereich
            Point size = ActualSize;
            if (args.LocalPosition.X < 0 || args.LocalPosition.X >= size.X ||
                args.LocalPosition.Y < 0 || args.LocalPosition.Y >= size.Y)
                return false;

            // Ignorieren, falls nicht gehovered
            if (!Visible) return false;

            // Ignorieren, falls ausgeschaltet
            if (!Enabled) return true;

            // Children first (Order by Z-Order)
            foreach (var child in Children.InZOrder)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, child);
                args.Bubbled = child.InternalEndDrop(args) || args.Bubbled;
                if (args.Handled) break;
            }

            // Bubble up
            if (!args.Handled)
            {
                args.LocalPosition = CalculateLocalPosition(args.GlobalPosition, this);
                OnEndDrop(args);
                if (EndDrop != null)
                    EndDrop(args);
            }

            // Leave
            if (dropHovered)
            {
                OnDropLeave(args);
                if (DropLeave != null)
                    DropLeave(args);
                dropHovered = false;
            }

            return Background != null;
        }

        protected virtual void OnStartDrag(DragEventArgs args) { }

        protected virtual void OnDropMove(DragEventArgs args) { }

        protected virtual void OnDropEnter(DragEventArgs args) { }

        protected virtual void OnDropLeave(DragEventArgs args) { }

        protected virtual void OnEndDrop(DragEventArgs args) { }

        public event DragEventDelegate StartDrag;

        public event DragEventDelegate DropMove;

        public event DragEventDelegate DropEnter;

        public event DragEventDelegate DropLeave;

        public event DragEventDelegate EndDrop;

        #endregion
    }

    /// <summary>
    /// Liste der möglichen Hover- und FokusStates
    /// </summary>
    public enum TreeState
    {
        /// <summary>
        /// Keinen Flag.
        /// </summary>
        None,

        /// <summary>
        /// Dieses Control wird nur durch Children gesetzt.
        /// </summary>
        Passive,

        /// <summary>
        /// Dises Control ist aktiv gesetzt.
        /// </summary>
        Active
    }
}
