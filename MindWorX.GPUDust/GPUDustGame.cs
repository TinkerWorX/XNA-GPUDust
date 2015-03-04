using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Text;

namespace MindWorX.GPUDust
{
    public class GPUDustGame : Game
    {
        private const Single NONE = 0.00f;
        private const Single DUST = 0.02f;
        private const Single WALL = 0.04f;
        private const Single FLUID = 0.06f;

        private const Single SAND = 0.02f;
        private const Single DIRT = 0.04f;
        private const Single CONCRETE = 0.06f;
        private const Single FIRE = 0.08f;
        private const Single WATER = 0.10f;
        private const Single PLANT = 0.12f;

        private const Single MOVE_NONE = 0.00f;
        private const Single MOVE_RIGHT = 0.02f;
        private const Single MOVE_DOWN = 0.04f;
        private const Single MOVE_LEFT = 0.06f;
        private const Single MOVE_UP = 0.08f;

        private GraphicsDeviceManager graphics;

        private SpriteBatch spriteBatch;

        private RenderTarget2D newBuffer;

        private RenderTarget2D oldBuffer;

        private Texture2D pixelTexture;

        private Effect startPhase;

        private Effect dustPhase;

        private Effect fluidPhase;

        private Effect movePhase;

        private Effect reactivePhase;

        private Effect colorPhase;

        private FPSCounterComponent fpsCounter;

        private Vector2[,] spray = new Vector2[25, 25];

        public GPUDustGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = 1280;
            this.graphics.PreferredBackBufferHeight = 720;
            this.graphics.SynchronizeWithVerticalRetrace = false;

            this.IsMouseVisible = true;

            this.IsFixedTimeStep = false;
            this.TargetElapsedTime = TimeSpan.FromMilliseconds(5);
        }

        private void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }

        private void SwapBuffers()
        {
            this.Swap(ref this.newBuffer, ref this.oldBuffer);
        }

        protected override void Initialize()
        {
            this.Content.RootDirectory = "Content";

            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.newBuffer = new RenderTarget2D(this.GraphicsDevice, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
            this.oldBuffer = new RenderTarget2D(this.GraphicsDevice, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);

            for (int x = 0; x < spray.GetLength(0); x++)
            {
                for (int y = 0; y < spray.GetLength(1); y++)
                {
                    spray[x, y] = new Vector2(x - spray.GetLength(0) / 2.00f, y - spray.GetLength(1) / 2.00f);
                }
            }

            this.fpsCounter = new FPSCounterComponent(this);
            this.Components.Add(this.fpsCounter);

            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;

            base.Initialize();
        }

        private void SetupPhaseParameters()
        {
            var defaultTransformation = Matrix.CreateTranslation(-0.50f, -0.50f, 0.00f) * Matrix.CreateOrthographicOffCenter(0.00f, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height, 0.00f, 0.00f, 1.00f);

            this.startPhase.Parameters["MatrixTransform"].SetValue(defaultTransformation);
            this.startPhase.Parameters["XUnit"].SetValue(new Vector2(1.00f / this.GraphicsDevice.Viewport.Width, 0.00f));
            this.startPhase.Parameters["YUnit"].SetValue(new Vector2(0.00f, 1.00f / this.GraphicsDevice.Viewport.Height));

            this.dustPhase.Parameters["MatrixTransform"].SetValue(defaultTransformation);
            this.dustPhase.Parameters["XUnit"].SetValue(new Vector2(1.00f / this.GraphicsDevice.Viewport.Width, 0.00f));
            this.dustPhase.Parameters["YUnit"].SetValue(new Vector2(0.00f, 1.00f / this.GraphicsDevice.Viewport.Height));

            this.fluidPhase.Parameters["MatrixTransform"].SetValue(defaultTransformation);
            this.fluidPhase.Parameters["XUnit"].SetValue(new Vector2(1.00f / this.GraphicsDevice.Viewport.Width, 0.00f));
            this.fluidPhase.Parameters["YUnit"].SetValue(new Vector2(0.00f, 1.00f / this.GraphicsDevice.Viewport.Height));

            this.movePhase.Parameters["MatrixTransform"].SetValue(defaultTransformation);
            this.movePhase.Parameters["XUnit"].SetValue(new Vector2(1.00f / this.GraphicsDevice.Viewport.Width, 0.00f));
            this.movePhase.Parameters["YUnit"].SetValue(new Vector2(0.00f, 1.00f / this.GraphicsDevice.Viewport.Height));

            this.reactivePhase.Parameters["MatrixTransform"].SetValue(defaultTransformation);
            this.reactivePhase.Parameters["XUnit"].SetValue(new Vector2(1.00f / this.GraphicsDevice.Viewport.Width, 0.00f));
            this.reactivePhase.Parameters["YUnit"].SetValue(new Vector2(0.00f, 1.00f / this.GraphicsDevice.Viewport.Height));

            this.colorPhase.Parameters["MatrixTransform"].SetValue(defaultTransformation);
            this.colorPhase.Parameters["XUnit"].SetValue(new Vector2(1.00f / this.GraphicsDevice.Viewport.Width, 0.00f));
            this.colorPhase.Parameters["YUnit"].SetValue(new Vector2(0.00f, 1.00f / this.GraphicsDevice.Viewport.Height));

            this.newBuffer = new RenderTarget2D(this.GraphicsDevice, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
            this.oldBuffer = new RenderTarget2D(this.GraphicsDevice, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);

            this.GraphicsDevice.SetRenderTarget(this.newBuffer);
            this.GraphicsDevice.Clear(new Color(NONE, NONE, NONE, MOVE_NONE));
            this.GraphicsDevice.SetRenderTarget(this.oldBuffer);
            this.GraphicsDevice.Clear(new Color(NONE, NONE, NONE, MOVE_NONE));
        }

        private void Window_ClientSizeChanged(Object sender, EventArgs e)
        {
            this.SetupPhaseParameters();
        }

        protected override void LoadContent()
        {
            this.pixelTexture = new Texture2D(this.GraphicsDevice, 1, 1);
            this.pixelTexture.SetData(new[] { Color.White });

            this.startPhase = this.Content.Load<Effect>(@"StartPhase");
            this.dustPhase = this.Content.Load<Effect>(@"DustPhase");
            this.fluidPhase = this.Content.Load<Effect>(@"FluidPhase");
            this.movePhase = this.Content.Load<Effect>(@"MovePhase");
            this.reactivePhase = this.Content.Load<Effect>(@"ReactivePhase");
            this.colorPhase = this.Content.Load<Effect>(@"ColorPhase");

            this.SetupPhaseParameters();
        }

        protected override void Update(GameTime gameTime)
        {
            this.Window.Title = this.GraphicsDevice.Viewport.Bounds + " @ " + this.fpsCounter.FPS + " fps";

            base.Update(gameTime);
        }

        private Single[] directions = new[] { MOVE_DOWN, MOVE_RIGHT, MOVE_LEFT };

        protected override void Draw(GameTime gameTime)
        {
            // START
            this.SwapBuffers();
            this.GraphicsDevice.SetRenderTarget(this.newBuffer);
            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, this.startPhase);
            this.spriteBatch.Draw(this.oldBuffer, Vector2.Zero, Color.White);
            this.spriteBatch.End();

            for (var i = 0; i < 1; i++)
            {
                this.Swap(ref directions[1], ref directions[2]);
                for (var n = 0; n < this.directions.Length; n++)
                {
                    var direction = this.directions[n];

                    // DUST
                    this.SwapBuffers();
                    this.GraphicsDevice.SetRenderTarget(this.newBuffer);
                    this.dustPhase.Parameters["Direction"].SetValue(direction);
                    this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, this.dustPhase);
                    this.spriteBatch.Draw(this.oldBuffer, Vector2.Zero, Color.White);
                    this.spriteBatch.End();

                    // FLUID
                    this.SwapBuffers();
                    this.GraphicsDevice.SetRenderTarget(this.newBuffer);
                    this.fluidPhase.Parameters["Direction"].SetValue(direction);
                    this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, this.fluidPhase);
                    this.spriteBatch.Draw(this.oldBuffer, Vector2.Zero, Color.White);
                    this.spriteBatch.End();

                    // MOVEMENT
                    this.SwapBuffers();
                    this.GraphicsDevice.SetRenderTarget(this.newBuffer);
                    this.movePhase.Parameters["Direction"].SetValue(direction);
                    this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, this.movePhase);
                    this.spriteBatch.Draw(this.oldBuffer, Vector2.Zero, Color.White);
                    this.spriteBatch.End();
                }
            }

            // REACTIONS
            this.SwapBuffers();
            this.GraphicsDevice.SetRenderTarget(this.newBuffer);
            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, this.reactivePhase);
            this.spriteBatch.Draw(this.oldBuffer, Vector2.Zero, Color.White);
            this.spriteBatch.End();


            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            if (this.IsActive)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    {
                        var list = this.spray.Cast<Vector2>().ToList();
                        list.Shuffle();
                        foreach (var offset in list.Take(16))
                        {
                            this.spriteBatch.Draw(this.pixelTexture, new Rectangle(Mouse.GetState().X + (Int32)offset.X, Mouse.GetState().Y + (Int32)offset.Y, 1, 1), new Color(FLUID, WATER, NONE, NONE));
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                    {
                        this.spriteBatch.Draw(this.pixelTexture, new Rectangle(Mouse.GetState().X - 2, Mouse.GetState().Y - 2, 5, 5), new Color(WALL, CONCRETE, NONE, NONE));
                    }
                    else
                    {
                        var list = this.spray.Cast<Vector2>().ToList();
                        list.Shuffle();
                        foreach (var offset in list.Take(16))
                        {
                            this.spriteBatch.Draw(this.pixelTexture, new Rectangle(Mouse.GetState().X + (Int32)offset.X, Mouse.GetState().Y + (Int32)offset.Y, 1, 1), new Color(DUST, DIRT, NONE, NONE));
                        }
                    }
                }
                if (Mouse.GetState().RightButton == ButtonState.Pressed)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    {
                        this.spriteBatch.Draw(this.pixelTexture, new Rectangle(Mouse.GetState().X - 12, Mouse.GetState().Y - 12, 25, 25), new Color(NONE, NONE, NONE, NONE));
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                    {
                        this.spriteBatch.Draw(this.pixelTexture, new Rectangle(Mouse.GetState().X - 2, Mouse.GetState().Y - 2, 5, 5), new Color(WALL, PLANT, NONE, NONE));
                    }
                    else
                    {
                        var list = this.spray.Cast<Vector2>().ToList();
                        list.Shuffle();
                        foreach (var offset in list.Take(16))
                        {
                            this.spriteBatch.Draw(this.pixelTexture, new Rectangle(Mouse.GetState().X + (Int32)offset.X, Mouse.GetState().Y + (Int32)offset.Y, 1, 1), new Color(NONE, FIRE, NONE, NONE));
                        }
                    }
                }
            }
            this.spriteBatch.End();

            this.GraphicsDevice.SetRenderTarget(null);
            this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, this.colorPhase);
            this.spriteBatch.Draw(this.newBuffer, Vector2.Zero, Color.White);
            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
