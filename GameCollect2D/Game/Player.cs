using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Sprites
{
    class Player : GameObject
    {
        public string Name;
        public int Lives;

        int _score;
        public int Score {
            set
            {
                if (value < 0)
                    _score = 0;
                else
                    _score = value;
            }
            get
            {
                return _score;
            }
        }

        protected double _lastCollisionTime = 0;
        protected double _lastJumpTime = 0;
        double _lastUpdateTime = 0;
        double _updateTimer = 0;

        Vector2 _jumpStartPosition = Vector2.Zero;

        GamePadState _previousGpState;
        KeyboardState _previousKbState;

        public enum State
        {
            Standing,
            Moving,
            Bouncing,
            Dodging, // currently acts like a jump
            Hurting,
            Dead
        }

        public enum AnimationState
        {
            Normal,
            Dodging,
            Hurting,
            Dead
        }

        public State state = State.Standing;

        public PlayerIndex PlayerIndex;

        #region Constructors
        public Player(Texture2D texture, float speed, Dictionary<string, SoundEffect> sounds) : base(texture, sounds)
        {
            this.Speed = speed;

            this.Reset();
        }

        #endregion

        public void Revive()
        {
            this.Lives = 5;
            this.state = State.Standing;
            // TO-DO: Add some animation stuff here
        }

        public void Reset() {
            this.Revive();
            this.Score = 0;
        }

        public override void Update(Viewport viewport, GameTime gameTime, Level level, List<Sprite> sprites)
        {
            GamePadState gpState = GamePad.GetState(this.PlayerIndex);
            KeyboardState kbState = Keyboard.GetState();

            double totalSeconds = gameTime.TotalGameTime.TotalSeconds;
            double elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;

            _updateTimer += elapsedSeconds;

            if (this.state == State.Dead)
                return;

            bool canCollision = false;
            // We don't want to check for collisions on every frame... otherwise it looks weird
            if (totalSeconds - _lastCollisionTime > 0.25d || this.state == State.Dodging)
            {
                canCollision = true;
                if (this.state != State.Dead && this.state != State.Dodging)
                    this.state = State.Standing;
            }

            if (this.state != State.Bouncing && _updateTimer > 0.03d)
            {
                Move(gpState, kbState);
                Jump(totalSeconds, gpState, kbState);
                _updateTimer = 0;
            }

            CheckScreenBoundaries(viewport);

            if (canCollision)
            {
                CheckCollisions(gameTime, level, sprites);
            }

            GetCollectables(gameTime, level, sprites);

            // Update position and reset velocity
            this.Position += this._velocity;

            this._velocity = Vector2.Zero;

            _lastUpdateTime = gameTime.TotalGameTime.TotalSeconds;
            _previousGpState = gpState;
            _previousKbState = kbState;

            base.Update(viewport, gameTime, level, sprites);
        }

        private void CheckScreenBoundaries(Viewport viewport)
        {
            // Check for screen boundaries and update position accordingly
            if (this.BoundingBox.Right + this.Velocity.X > viewport.Width)
            {
                this.Position.X = 0;
            }
            else if (this.BoundingBox.Left + this.Velocity.X < 0)
            {
                this.Position.X = viewport.Bounds.Right - this.Speed;
            }
            if (this.BoundingBox.Bottom + this.Velocity.Y > viewport.Height)
            {
                this.Position.Y = 0;
            }
            else if (this.BoundingBox.Top + this.Velocity.Y < 0)
            {
                this.Position.Y = viewport.Bounds.Bottom - this.Speed;
            }
        }

        private void CheckCollisions(GameTime gameTime, Level level, List<Sprite> sprites)
        {
            // TO DO: needs refactor
            if (this.Passability == Passability.block)
            {
                // Check for collisions with other sprites
                foreach (Sprite sprite in sprites)
                {
                    if (sprite == this)
                        continue;
                    if (sprite.Passability == Passability.block)
                    {
                        bool collided = false;
                        int xCollision = 0;
                        int yCollision = 0;
                        float collisionVelocity = 0;

                        if (
                            (this._velocity.X > 0 && this.IsTouchingLeft(sprite)) || 
                            (this._velocity.X < 0 && this.IsTouchingRight(sprite))
                        )
                        {
                            // Collision occurred on X-axis
                            xCollision = 1;
                            this.state = State.Bouncing;
                            collided = true;
                        }
                        if (
                            (this._velocity.Y > 0 && this.IsTouchingTop(sprite)) || 
                            (this._velocity.Y < 0 && this.IsTouchingBottom(sprite))
                        )
                        {
                            // Collision occurred on Y-axis
                            yCollision = 1;
                            this.state = State.Bouncing;
                            collided = true;
                        }

                        if (collided)
                        {
                            if (Object.ReferenceEquals(this.GetType(), sprite.GetType()))
                            {
                                collisionVelocity = -2;

                                if (((Player)sprite).state != State.Dodging && this._lastCollisionTime > ((Player)sprite)._lastCollisionTime)
                                {
                                    SoundEffectInstance sfxHit = this._sfx["hit"].CreateInstance();
                                    sfxHit.Play();
                                    this.Score += 1;
                                }
                            }
                            else
                            {
                                collisionVelocity = 0;
                                // Play different sound
                            }

                            this._velocity.X *= collisionVelocity * xCollision;
                            this._velocity.Y *= collisionVelocity * yCollision;

                            _lastCollisionTime = gameTime.TotalGameTime.TotalSeconds;
                        }
                    }
                }
            }
        }

        private void GetCollectables(GameTime gameTime, Level level, List<Sprite> sprites)
        {
            // TO DO: needs refactor
                // Check for collisions with other sprites
                foreach (GameObject sprite in sprites)
                {
                    if (sprite == this)
                        continue;
                    if (sprite.GetType() == typeof(ScoreModifier))
                    {
                        bool pickup = false;

                        if (
                            (this._velocity.X > 0 && this.IsTouchingLeft(sprite)) ||
                            (this._velocity.X < 0 && this.IsTouchingRight(sprite))
                        )
                        {
                            // Collision occurred on X-axis
                            pickup = true;
                        }
                        if (
                            (this._velocity.Y > 0 && this.IsTouchingTop(sprite)) ||
                            (this._velocity.Y < 0 && this.IsTouchingBottom(sprite))
                        )
                        {
                            // Collision occurred on Y-axis
                            pickup = true;
                        }

                        if (pickup)
                        {
                            System.Diagnostics.Debug.WriteLine("Picking up item");
                            int modifier = ((ScoreModifier)sprite).Modifier;
                            if (modifier > 0)
                                ((ScoreModifier)sprite).PlaySound("score");
                            else if (modifier < 0)
                                ((ScoreModifier)sprite).PlaySound("damage");

                        this.Score += ((ScoreModifier)sprite).Modifier;
                            level.RemoveObject(sprite.Column, sprite.Row);
                        }
                    }
                }
        }

        private void Move(GamePadState gpState, KeyboardState kbState)
        {
            if (this.state == State.Dodging)
                return;

            // Check gamepad availability
            GamePadCapabilities gpCapabilities = GamePad.GetCapabilities(this.PlayerIndex);

            if ((gpCapabilities.HasLeftXThumbStick && gpState.ThumbSticks.Left.X < -0.5f) || gpState.DPad.Left == ButtonState.Pressed || kbState.IsKeyDown(Input.Left))
            {
                this.state = State.Moving;
                this.MoveLeft();
            }
            else if ((gpCapabilities.HasLeftXThumbStick && gpState.ThumbSticks.Left.X > 0.5f) || gpState.DPad.Right == ButtonState.Pressed || kbState.IsKeyDown(Input.Right))
            {
                this.state = State.Moving;
                this.MoveRight();
            }
            else if ((gpCapabilities.HasLeftXThumbStick && gpState.ThumbSticks.Left.Y > 0.5f) || gpState.DPad.Up == ButtonState.Pressed || kbState.IsKeyDown(Input.Up))
            {
                this.state = State.Moving;
                this.MoveUp();
            }
            else if ((gpCapabilities.HasLeftXThumbStick && gpState.ThumbSticks.Left.Y < -0.5f) || gpState.DPad.Down == ButtonState.Pressed || kbState.IsKeyDown(Input.Down))
            {
                this.state = State.Moving;
                this.MoveDown();
            }
        }

        private void Jump(double totalSeconds, GamePadState gpState, KeyboardState kbState)
        {
            bool startJump = false;

            if (this.state == State.Moving || this.state == State.Standing)
            {
                if (gpState.Buttons.A == ButtonState.Pressed || kbState.IsKeyDown(Input.Jump))
                {
                    startJump = true;
                }
            }
            else if (this.state == State.Dodging)
            {
                if (totalSeconds - this._lastJumpTime > 0.20d)
                {
                    this.Position = this._jumpStartPosition;
                    this.state = State.Standing;
                    this._lastJumpTime = 0;
                }
            }

            if (startJump)
            {
                this.state = State.Dodging;
                SoundEffectInstance sfxDodge = this._sfx["jump"].CreateInstance();
                sfxDodge.Play();
                this._jumpStartPosition = this.Position;
                this._velocity = new Vector2(0, -this.Speed*0.5f);
                this._lastJumpTime = totalSeconds;
                System.Diagnostics.Debug.WriteLine(this.Name + " jumped.");
            }
        }
    }
}
