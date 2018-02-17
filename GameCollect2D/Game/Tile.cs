using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Sprites
{
    class Tile : Sprite
    {
        double lastUpdateTime;
        double updateTimer;

        public enum State
        {
            Normal,
            Touching
        }

        public State state = State.Normal;

        public Tile(Texture2D texture, Vector2 position, Passability passability) : base(texture)
        {
            this.Position = position;
            this.Passability = passability;
        }
    }
}
