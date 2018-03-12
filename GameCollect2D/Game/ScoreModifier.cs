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
    class ScoreModifier : GameObject
    {
        int _modifier = 1;

        public int Modifier
        {
            get
            {
                return _modifier;
            }
            set
            {
                _modifier = value;
                SetColor();
            }
        }

        public ScoreModifier(Texture2D texture) : base(texture)
        {
            this.Position = Vector2.Zero;
            this._tilePosition = Vector2.Zero;
        }

        public ScoreModifier(Texture2D texture, int modifier) : this(texture)
        {
            this._modifier = modifier;
            SetColor();
        }

        public ScoreModifier(Texture2D texture, int modifier, Dictionary<String, SoundEffect> sounds) : this(texture, modifier)
        {
            this._sfx = sounds;
        }

        public override void Update(Viewport viewport, GameTime gameTime, Level level, List<Sprite> sprites)
        {
            base.Update(viewport, gameTime, level, sprites);
        }

        void SetColor()
        {
            if (_modifier > 0)
            {
                this.Color = Color.Green;
            }
            else if (_modifier < 0)
            {
                this.Color = Color.DeepPink;
            }
        }
    }
}
