namespace engenious.UI.Controls
{
    /// <summary>
    /// Screen container control page.
    /// </summary>
    public abstract class Screen : ContainerControl
    {
        private bool _isVisibleScreen = false;

        private bool _isActiveScreen = false;

        /// <summary>
        /// Gets the title of this screen.
        /// <remarks>The <see cref="Control.ScreenManager"/> uses this <see cref="Title"/> as the window title.</remarks>
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        /// Gets whether this screen is an overlay.
        /// <remarks>If this is set to <c>true</c> underlying screens will still be rendered.</remarks>
        /// </summary>
        public bool IsOverlay { get; protected set; }

        /// <summary>
        /// Gets whether this screen will be added in the <see cref="BaseScreenComponent.History"/> or not.
        /// <remarks>
        /// If this is set to <c>false</c> this screen will be skipped on <see cref="BaseScreenComponent.NavigateBack"/>.
        /// E.g. this can be useful for loading screens.
        /// </remarks>
        /// </summary>
        public bool InHistory { get; protected set; }

        /// <summary>
        /// Gets the default <see cref="MouseMode"/> for this screen.
        /// <remarks>This <see cref="MouseMode"/> gets automatically set on navigating to this screen.</remarks>
        /// </summary>
        public MouseMode DefaultMouseMode { get; protected set; }

        private readonly PropertyEventArgs<bool> _isActiveScreenChangedEventArgs = new PropertyEventArgs<bool>();
        /// <summary>
        /// Gets whether this <see cref="Screen"/> is currently active.
        /// </summary>
        public bool IsActiveScreen
        {
            get => _isActiveScreen;
            internal set
            {
                if (_isActiveScreen == value) return;

                _isActiveScreenChangedEventArgs.NewValue = value;
                _isActiveScreenChangedEventArgs.OldValue = _isActiveScreen;
                _isActiveScreenChangedEventArgs.Handled = false;

                _isActiveScreen = value;
                OnIsActiveScreenChanged(_isActiveScreenChangedEventArgs);
                IsActiveScreenChanged?.Invoke(this, _isActiveScreenChangedEventArgs);
            }
        }
        private readonly PropertyEventArgs<bool> _isVisibleScreenChangedEventArgs = new PropertyEventArgs<bool>();
        /// <summary>
        /// Gets whether this screen is visible in the current screen rendering stack.
        /// </summary>
        public bool IsVisibleScreen
        {
            get => _isVisibleScreen;
            internal set
            {
                if (_isVisibleScreen == value) return;

                _isVisibleScreenChangedEventArgs.OldValue = _isVisibleScreen;
                _isVisibleScreenChangedEventArgs.NewValue = value;
                _isVisibleScreenChangedEventArgs.Handled = false;

                _isVisibleScreen = value;

                OnIsVisibleScreenChanged(_isVisibleScreenChangedEventArgs);
                IsVisibleScreenChanged?.Invoke(this, _isVisibleScreenChangedEventArgs);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="style">The style to use for this control.</param>
        /// <param name="manager">The <see cref="BaseScreenComponent"/>.</param>
        public Screen(string style = "", BaseScreenComponent? manager = null)
            : base(style, manager)
        {
            Title = string.Empty;
            IsOverlay = false;
            InHistory = true;
            IsVisibleScreen = false;
            IsActiveScreen = false;
            DefaultMouseMode = MouseMode.Free;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            Margin = Border.All(0);
            Padding = Border.All(20);

            ApplySkin(typeof(Screen));
        }

        internal void InternalNavigateTo(NavigationEventArgs args)
        {
            OnNavigateTo(args);
        }

        internal void InternalNavigatedTo(NavigationEventArgs args)
        {
            OnNavigatedTo(args);
        }

        internal void InternalNavigateFrom(NavigationEventArgs args)
        {
            OnNavigateFrom(args);
        }

        internal void InternalNavigatedFrom(NavigationEventArgs args)
        {
            OnNavigatedFrom(args);
        }

        /// <summary>
        /// Gets called on trying to navigate to this screen.
        /// <remarks>The actual navigation can still be canceled from here.</remarks>
        /// </summary>
        /// <param name="args">A <see cref="NavigationEventArgs"/> that contains the event data.</param>
        protected virtual void OnNavigateTo(NavigationEventArgs args) { }

        /// <summary>
        /// Gets called when navigation to this screen occured.
        /// </summary>
        /// <param name="args">A <see cref="NavigationEventArgs"/> that contains the event data.</param>
        protected virtual void OnNavigatedTo(NavigationEventArgs args) { }

        /// <summary>
        /// Gets called on trying to navigate away from this screen.
        /// <remarks>The actual navigation can still be canceled from here.</remarks>
        /// </summary>
        /// <param name="args">A <see cref="NavigationEventArgs"/> that contains the event data.</param>
        protected virtual void OnNavigateFrom(NavigationEventArgs args) { }
        
        /// <summary>
        /// Gets called when navigation to this screen occured.
        /// </summary>
        /// <param name="args">A <see cref="NavigationEventArgs"/> that contains the event data.</param>
        protected virtual void OnNavigatedFrom(NavigationEventArgs args) { }

        /// <summary>
        /// Raises the <see cref="IsActiveScreenChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnIsActiveScreenChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Raises the <see cref="IsVisibleScreenChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="PropertyEventArgs{Boolean}"/> that contains the event data.</param>
        protected virtual void OnIsVisibleScreenChanged(PropertyEventArgs<bool> args) { }

        /// <summary>
        /// Occurs when the <see cref="IsActiveScreen"/> property got changed.
        /// </summary>
        public event PropertyChangedDelegate<bool>? IsActiveScreenChanged;

        /// <summary>
        /// Occurs when the <see cref="IsVisibleScreen"/> property got changed.
        /// </summary>
        public event PropertyChangedDelegate<bool>? IsVisibleScreenChanged;
    }
}
