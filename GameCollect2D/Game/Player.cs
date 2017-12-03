using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Sprites
{
    class Player : Sprite
    {
        public string Name;
        public int Lives;
        public int Score;

        protected double lastCollisionTime = 0;

        public enum State
        {
            Standing,
            Moving,
            Bouncing,
            Hurting,
            Dead
        }

        public State state = State.Standing;

        #region Constructors
        public Player(Texture2D texture, float speed) : base(texture)
        {
            this.Speed = speed;
            this.Reset();
        }

        public Player(GraphicsDevice graphicsDevice, int width, int height, Color color, float speed) : base(graphicsDevice, width, height, color)
        {
            this.Speed = speed;
            this.Reset();
        }
        #endregion

        public void Revive()
        {
            this.Lives = 5;
            // Some animation stuff here
        }

        public void Reset() {
            this.Revive();
            this.Score = 0;
        }

        public override void Update(GameTime gameTime, List<Sprite> sprites)
        {
            // We don't want to check for collisions on every frame... otherwise it looks weird
            if (gameTime.TotalGameTime.TotalSeconds - lastCollisionTime > 0.25d)
            {
                this.state = State.Moving;
            }

            if (this.state != State.Bouncing)
                Move();

            if (this.Passability == Passability.block)
            {
                //System.Diagnostics.Debug.WriteLine("Checking for collision...");
                //System.Diagnostics.Debug.WriteLine("Player is solid");
                foreach (Sprite sprite in sprites)
                {
                    if (sprite == this)
                        continue;
                    if (sprite.Passability == Passability.block)
                    {
                        if (this.Velocity.X > 0 && this.IsTouchingLeft(sprite) || this.Velocity.X < 0 && this.IsTouchingRight(sprite))
                        {
                            //System.Diagnostics.Debug.WriteLine("Collision occured on X-axis!");
                            this.state = State.Bouncing;
                            this.Velocity.X *= -1;
                            if (Object.ReferenceEquals(this.GetType(), sprite.GetType()))
                            {
                                System.Diagnostics.Debug.WriteLine(this.Name);
                                if (this.lastCollisionTime > ((Player)sprite).lastCollisionTime)
                                    this.Score += 1;
                            }
                            lastCollisionTime = gameTime.TotalGameTime.TotalSeconds;
                        }
                        if (this.Velocity.Y > 0 && this.IsTouchingTop(sprite) || this.Velocity.Y < 0 && this.IsTouchingBottom(sprite))
                        {
                            //System.Diagnostics.Debug.WriteLine("Collision occured on Y-axis!");
                            this.state = State.Bouncing;
                            this.Velocity.Y *= -1;
                            if (Object.ReferenceEquals(this.GetType(), sprite.GetType()))
                            {
                                System.Diagnostics.Debug.WriteLine(this.Name);
                                if (this.lastCollisionTime > ((Player)sprite).lastCollisionTime)
                                    this.Score += 1;
                            }
                            lastCollisionTime = gameTime.TotalGameTime.TotalSeconds;
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine("Velocity:" + Velocity.ToString());
                //System.Diagnostics.Debug.WriteLine("Position :" + Position.ToString());

                // Update position and reset velocity
                this.Position += this.Velocity;
                this.Velocity = Vector2.Zero;
            }
            else
                //System.Diagnostics.Debug.WriteLine("Player can move");

            base.Update(gameTime, sprites);
        }

        private void Move()
        {
            KeyboardState kbState = Keyboard.GetState();

            if (kbState.IsKeyDown(Input.Left))
            {
                this.state = State.Moving;
                this.MoveLeft();
            }
            else if (kbState.IsKeyDown(Input.Right))
            {
                this.state = State.Moving;
                this.MoveRight();
            }
            else if (kbState.IsKeyDown(Input.Up))
            {
                this.state = State.Moving;
                this.MoveUp();
            }
            else if (kbState.IsKeyDown(Input.Down))
            {
                this.state = State.Moving;
                this.MoveDown();
            }
            else
            {
                this.state = State.Standing;
            }
        }
    }
}
