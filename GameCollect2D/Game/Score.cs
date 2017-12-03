using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    class Score
    {
        SpriteFont labelFont;
        SpriteFont scoreFont;

        public int PlayerOne = 0;
        public int PlayerTwo = 0;

        public Color LabelColor1 = Color.Black;
        public Color LabelColor2 = Color.Black;

        public Color ScoreColor1 = Color.White;
        public Color ScoreColor2 = Color.White;

        public Score(SpriteFont label, SpriteFont score)
        {
            this.labelFont = label;
            this.scoreFont = score;
        }

        public void Draw(Game game, SpriteBatch spriteBatch)
        {
            // Draw Player 1 score
            spriteBatch.DrawString(labelFont, "Score", new Vector2(10, game.Window.ClientBounds.Bottom - 75), this.LabelColor1);
            spriteBatch.DrawString(scoreFont, this.PlayerOne.ToString(), new Vector2(70, game.Window.ClientBounds.Bottom - 77), this.ScoreColor1);

            // Draw Player 2 score
            spriteBatch.DrawString(labelFont, "Score", new Vector2(game.Window.ClientBounds.Right - 106, game.Window.ClientBounds.Bottom - 75), this.LabelColor2);
            spriteBatch.DrawString(scoreFont, this.PlayerTwo.ToString(), new Vector2(game.Window.ClientBounds.Right - 48, game.Window.ClientBounds.Bottom - 77), this.ScoreColor2);
        }
    }
}
