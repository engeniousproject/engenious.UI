using System;

namespace engenious.UI
{
    /// <summary>
    /// Base class for transition animations.
    /// </summary>
    public abstract class Transition
    {
        /// <summary>
        /// Gets the currently elapsed time.
        /// </summary>
        public TimeSpan Current { get; private set; }

        /// <summary>
        /// Gets the delay to wait till starting the transition.
        /// </summary>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Gets the duration of the transition.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the control that will be animated.
        /// </summary>
        public Control Control { get; }

        /// <summary>
        /// Gets the transition curve used for transitioning.
        /// </summary>
        /// <seealso cref="TransitionCurveDelegate"/>
        public TransitionCurveDelegate Curve { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transition"/> class.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <param name="curve">The transition curve.</param>
        /// <param name="duration">The time duration of the transition.</param>
        public Transition(Control control, TransitionCurveDelegate curve, TimeSpan duration) :
            this(control, curve, duration, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transition"/> class.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <param name="curve">The transition curve.</param>
        /// <param name="duration">The time duration of the transition.</param>
        /// <param name="delay">The time delay to wait before starting the transition.</param>
        public Transition(Control control, TransitionCurveDelegate curve, TimeSpan duration, TimeSpan delay)
        {
            Current = new TimeSpan();
            Control = control;
            Curve = curve;
            Duration = duration;
            Delay = delay;
        }

        /// <summary>
        /// Update the transition and apply it.
        /// </summary>
        /// <param name="gameTime">Contains the elapsed time since the last update, as well as total elapsed time.</param>
        /// <returns>Whether the transition is still ongoing.</returns>
        public bool Update(GameTime gameTime)
        {
            // Calculate elapsed time of transition
            Current += gameTime.ElapsedGameTime;

            // Ignore if still below delay
            if (Current < Delay) return true;

            // Determine transition progress in percent
            float position = Math.Max(0, Math.Min(1, (float)(
                (Current.TotalMilliseconds - Delay.TotalMilliseconds) / Duration.TotalMilliseconds)));
            float value = Math.Max(0, Math.Min(1, Curve(position)));

            // Apply transition to control
            ApplyValue(Control, value);

            // At end of transition
            if (position >= 1f)
            {
                Finished?.Invoke(this, Control);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Applies the transition to a control.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to apply the transition to.</param>
        /// <param name="value">Value depicting the progress of the transition in percent[0f-1f].</param>
        protected abstract void ApplyValue(Control control, float value);

        /// <summary>
        /// Creates a cloned instance of this <see cref="Transition"/> but with a different control to apply the transition to.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <returns>The cloned <see cref="Transition"/>.</returns>
        public abstract Transition Clone(Control control);

        /// <summary>
        /// Occurs when the transition was finished.
        /// </summary>
        public event TransitionDelegate Finished;

        #region Transition Curves

        /// <summary>
        /// Depicts a linear transition curve.
        /// </summary>
        /// <param name="position">Value depicting the progress of the transition in percent[0f-1f].</param>
        /// <returns>The linearly interpolated value(=<paramref name="position"/>).</returns>
        public static float Linear(float position)
        {
            return position;
        }

        /// <summary>
        /// Depicts a cubic transition curve.
        /// </summary>
        /// <param name="position">Value depicting the progress of the transition in percent[0f-1f].</param>
        /// <returns>The cubic interpolated value(=pow(<paramref name="position"/>, 2)).</returns>
        public static float Cubic(float position)
        {
            return MathF.Pow(position, 2);
        }

        #endregion
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Transition.Finished"/> event of a <see cref="Transition"/>.
    /// </summary>
    /// <param name="sender">The transition that was finished.</param>
    /// <param name="control">The control the transition was applied to.</param>
    public delegate void TransitionDelegate(Transition sender, Control control);

    /// <summary>
    /// Represents the method that will curve transitions to be able to have different interpolation speeds across the transition time.
    /// </summary>
    /// <param name="t">The point to sample the curve at in percent[0f-1f].</param>
    public delegate float TransitionCurveDelegate(float t);
}
