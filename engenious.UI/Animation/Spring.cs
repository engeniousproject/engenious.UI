using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace engenious.UI.Animation
{
    public class Spring
    {
        public float Mass { get; set; }
        public float Friction { get; set; }

        public float Tension { get; set; }

        public float Target { get; set; }

        public float Velocity { get; set; }

        public float Current { get; set; }

        public float Precision { get; set; }

        public bool Locked { get; set; }

        public bool Resting { get; private set; }

        public void Update(GameTime gameTime)
        {
            if (Locked)
                return;
            
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            

            while(deltaTime > 0)
            {
                float dt = Math.Min(deltaTime, 0.016f);

                float f = -Tension * (Current - Target);
                float damping = -Friction * Velocity;
                float acceleration = (f + damping) / Mass;

                Velocity = Velocity + acceleration * dt;
                Current = Current + Velocity * dt;

                if (Math.Abs(Current - Target) <  Precision && Velocity < Precision)
                {
                    Resting = true;
                    Current = Target;
                    Velocity = 0;
                    return;
                }

                deltaTime -= dt;
            }

            Resting = false;
        }
    }
}
