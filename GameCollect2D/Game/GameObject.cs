using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace GameEngine.Sprites
{
    class GameObject : Sprite
    {
        protected Vector2 _tilePosition;

        public bool IsDisplaced = true;

        public int Column
        {
            get
            {
                return (int)_tilePosition.X;
            }
            set
            {
                _tilePosition.X = value;
            }
        }
        public int Row
        {
            get
            {
                return (int)_tilePosition.Y;
            }
            set
            {
                _tilePosition.Y = value;
            }
        }

        protected Dictionary<string, SoundEffect> _sfx;

        public GameObject(Texture2D texture) : base(texture)
        {
            IsDisplaced = true;
            Position = Vector2.Zero;
            _tilePosition = Vector2.Zero;
            Passability = Passability.passable;
        }

        public GameObject(Texture2D texture, Dictionary<String, SoundEffect> sounds) : base(texture)
        {
            _sfx = sounds;
        }

        public GameObject(Texture2D texture, int column, int row) : this(texture)
        {
            _tilePosition = new Vector2(column, row);
        }

        public GameObject(Texture2D texture, int column, int row, Passability passability) : this(texture, column, row)
        {
            Passability = passability;
        }

        public GameObject(GraphicsDevice graphicsDevice, int width, int height, Color color, float speed) : base(graphicsDevice, width, height, color)
        {

        }

        public override void Update(Viewport viewport, GameTime gameTime, Level level, List<Sprite> sprites)
        {
            base.Update(viewport, gameTime, level, sprites);
        }

        public void PlaySound(string sfxName)
        {
            SoundEffectInstance sound = _sfx[sfxName].CreateInstance();
            sound.Play();
        }
    }
}
