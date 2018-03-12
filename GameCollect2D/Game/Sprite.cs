using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using GameEngine.Models;

namespace GameEngine.Sprites
{
    class Sprite
    {
        protected Texture2D _texture;

        protected Vector2 _velocity;

        public Passability Passability = Passability.passable;
        public Input Input;
        public Color Color = Color.White;
        public Vector2 Position;
        public float Speed = 0;

        public int Width
        {
            get
            {
                return _texture.Width;
            }
        }

        public int Height
        {
            get
            {
                return _texture.Height;
            }
        }

        public Rectangle BoundingBox {
            get {
                return new Rectangle(
                    (int)Position.X, 
                    (int)Position.Y, 
                    _texture.Width, 
                    _texture.Height);
            }
        }

        #region Constructors
        public Sprite(Texture2D texture)
        {
            this._texture = texture;
        }

        public Sprite(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            _texture = this.CreateTexture(graphicsDevice, width, height, pixel => color);
            this.Color = color;
        }
        #endregion

        public virtual void Update(GameTime gameTime, List<Sprite> sprites)
        {
        }

        public virtual void Update(Game game, Camera2D camera, Viewport viewport, GameTime gameTime, Level level, List<Sprite> sprites)
        {
            this.Update(gameTime, sprites);
        }

        public virtual void Update(Viewport viewport, GameTime gameTime, Level level, List<Sprite> sprites)
        {
            this.Update(gameTime, sprites);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Color);
        }

        #region Movement
        public void MoveLeft()
        {
            System.Diagnostics.Debug.WriteLine("Moving left");
            this._velocity.X = -this.Speed;
        }

        public void MoveRight()
        {
            System.Diagnostics.Debug.WriteLine("Moving right");
            this._velocity.X = this.Speed;
        }

        public void MoveUp()
        {
            System.Diagnostics.Debug.WriteLine("Moving up");
            this._velocity.Y = -this.Speed;
        }

        public void MoveDown()
        {
            System.Diagnostics.Debug.WriteLine("Moving down");
            this._velocity.Y = this.Speed;
        }
        #endregion

        #region Collision
        protected bool IsTouchingLeft(Sprite sprite)
        {
            return this.BoundingBox.Right + this._velocity.X > sprite.BoundingBox.Left &&
                this.BoundingBox.Left < sprite.BoundingBox.Left &&
                this.BoundingBox.Bottom > sprite.BoundingBox.Top &&
                this.BoundingBox.Top < sprite.BoundingBox.Bottom;
        }

        protected bool IsTouchingRight(Sprite sprite)
        {
            return this.BoundingBox.Left + this._velocity.X < sprite.BoundingBox.Right &&
                this.BoundingBox.Right > sprite.BoundingBox.Right &&
                this.BoundingBox.Bottom > sprite.BoundingBox.Top &&
                this.BoundingBox.Top < sprite.BoundingBox.Bottom;
        }

        protected bool IsTouchingTop(Sprite sprite)
        {
            return this.BoundingBox.Bottom + this._velocity.Y > sprite.BoundingBox.Top &&
                this.BoundingBox.Top < sprite.BoundingBox.Top &&
                this.BoundingBox.Right > sprite.BoundingBox.Left &&
                this.BoundingBox.Left < sprite.BoundingBox.Right;
        }

        protected bool IsTouchingBottom(Sprite sprite)
        {
            return this.BoundingBox.Top + this._velocity.Y < sprite.BoundingBox.Bottom &&
                this.BoundingBox.Bottom > sprite.BoundingBox.Bottom &&
                this.BoundingBox.Right > sprite.BoundingBox.Left &&
                this.BoundingBox.Left < sprite.BoundingBox.Right;
        }
        #endregion

        private Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height, Func<int, Color> paint)
        {
            //initialize a texture
            Texture2D texture = new Texture2D(graphicsDevice, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = paint(pixel);
            }

            //set the color
            texture.SetData(data);

            return texture;
        }

        public static Texture2D CreateSubTexture(GraphicsDevice graphicsDevice, Texture2D originalTexture, int startX, int startY, int width, int height)
        {
            Rectangle sourceRectangle = new Rectangle(startX, startY, width, height);

            Texture2D subTexture = new Texture2D(
                graphicsDevice,
                sourceRectangle.Width,
                sourceRectangle.Height);

            Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];
            originalTexture.GetData(0, sourceRectangle, data, 0, data.Length);
            subTexture.SetData(data);

            return subTexture;
        }
    }
}
