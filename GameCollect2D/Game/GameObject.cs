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
        protected Vector2 tilePosition;

        public bool IsDisplaced = true;

        public int Column
        {
            get
            {
                return (int)tilePosition.X;
            }
            set
            {
                tilePosition.X = value;
            }
        }
        public int Row
        {
            get
            {
                return (int)tilePosition.Y;
            }
            set
            {
                tilePosition.Y = value;
            }
        }

        protected Dictionary<string, SoundEffect> sfx;

        public GameObject(Texture2D texture) : base(texture)
        {
            this.IsDisplaced = true;
            this.Position = Vector2.Zero;
            this.tilePosition = Vector2.Zero;
            this.Passability = Passability.passable;
        }

        public GameObject(Texture2D texture, Dictionary<String, SoundEffect> sounds) : base(texture)
        {
            sfx = sounds;
        }

        public GameObject(Texture2D texture, int column, int row) : this(texture)
        {
            this.tilePosition = new Vector2(column, row);
        }

        public GameObject(Texture2D texture, int column, int row, Passability passability) : this(texture, column, row)
        {
            this.Passability = passability;
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
            SoundEffectInstance sound = sfx[sfxName].CreateInstance();
            sound.Play();
        }
    }
}
