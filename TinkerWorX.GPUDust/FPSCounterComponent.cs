using Microsoft.Xna.Framework;
using System;

namespace TinkerWorX.GPUDust
{
    public class FPSCounterComponent : DrawableGameComponent
    {
        private Int32 counter;

        private TimeSpan elapsedTime = TimeSpan.Zero;

        public FPSCounterComponent(Game game) : base(game) { }

        public Int32 FPS { get; private set; }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime.TotalSeconds >= 1)
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                FPS = counter;
                counter = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            counter += 1;
        }
    }
}
