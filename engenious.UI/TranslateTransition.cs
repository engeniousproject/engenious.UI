using System;

namespace engenious.UI
{
    /// <summary>
    /// Transition for translating controls.
    /// </summary>
    public class TranslateTransition : Transition
    {
        private readonly Point _from;
        private readonly Point _to;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaTransition"/> class.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <param name="curve">The transition curve.</param>
        /// <param name="duration">The time duration of the transition.</param>
        /// <param name="to">The desired location at the end of the transition.</param>
        public TranslateTransition(Control control, TransitionCurveDelegate curve, TimeSpan duration, Point to)
            : this(control, curve, duration, TimeSpan.Zero, to) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateTransition"/> class.
        /// </summary>
        /// <param name="control">The control to apply the transition to.</param>
        /// <param name="curve">The transition curve.</param>
        /// <param name="duration">The time duration of the transition.</param>
        /// <param name="delay">The time delay to wait before starting the transition.</param>
        /// <param name="to">The desired location at the end of the transition.</param>
        public TranslateTransition(Control control, TransitionCurveDelegate curve, TimeSpan duration, TimeSpan delay, Point to) 
            : base(control, curve, duration, delay)
        {
            _from = new Point(
                (int)control.Transformation.Translation.X, 
                (int)control.Transformation.Translation.Y);
            _to = to;
        }

        /// <inheritdoc />
        protected override void ApplyValue(Control control, float value)
        {
            Point diff = _to - _from;

            Matrix trans = control.Transformation;
            trans.M14 += _from.X + (diff.X * value);
            trans.M24 += _from.Y + (diff.Y * value);
            control.Transformation = trans;
        }

        /// <inheritdoc />
        public override Transition Clone(Control control)
        {
            return new TranslateTransition(control, Curve, Duration, Delay, _to);
        }
    }
}
