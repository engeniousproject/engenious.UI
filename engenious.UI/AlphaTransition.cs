using System;

namespace engenious.UI
{
    /// <summary>
    /// Transition for alpha fading of controls.
    /// </summary>
    public sealed class AlphaTransition : Transition
    {
        private readonly float _from;
        private readonly float _to;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaTransition"/> class.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <param name="curve">The transition curve.</param>
        /// <param name="duration">The time duration of the transition.</param>
        /// <param name="to">The desired transparency value at the end of the transition.</param>
        public AlphaTransition(Control control, TransitionCurveDelegate curve, TimeSpan duration, float to)
            : this(control, curve, duration, TimeSpan.Zero, to) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaTransition"/> class.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <param name="curve">The transition curve.</param>
        /// <param name="duration">The time duration of the transition.</param>
        /// <param name="delay">The time delay to wait before starting the transition.</param>
        /// <param name="to">The desired transparency value at the end of the transition.</param>
        public AlphaTransition(Control control, TransitionCurveDelegate curve, TimeSpan duration, TimeSpan delay, float to) 
            : base(control, curve, duration, delay)
        {
            _from = control.Alpha;
            _to = to;
        }


        /// <inheritdoc />
        protected override void ApplyValue(Control control, float position)
        {
            float value = (_to - _from) * position;
            control.Alpha = _from + value;
        }

        /// <inheritdoc />
        public override Transition Clone(Control control)
        {
            return new AlphaTransition(control, Curve, Duration, Delay, _to);
        }
    }
}
