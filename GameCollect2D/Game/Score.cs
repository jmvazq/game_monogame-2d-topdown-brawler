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

        int p1 = 0;
        int p2 = 0;

        public int PlayerOne {
            get
            {
                return p1;
            }
            set
            {
                if (value < 0)
                    p1 = 0;
                else
                    p1 = value;
            }
        }
        public int PlayerTwo
        {
            get
            {
                return p2;
            }
            set
            {
                if (value < 0)
                    p2 = 0;
                else
                    p2 = value;
            }
        }

        public Color LabelColor1 = Color.Black;
        public Color LabelColor2 = Color.Black;

        public Color ScoreColor1 = Color.White;
        public Color ScoreColor2 = Color.White;

        public Score(SpriteFont label, SpriteFont score)
        {
            this.labelFont = label;
            this.scoreFont = score;
        }

        public void Draw(Viewport viewport, SpriteBatch spriteBatch)
        {
            // Draw Player 1 score
            spriteBatch.DrawString(labelFont, "Score", new Vector2(10, viewport.Height - 32), this.LabelColor1);
            spriteBatch.DrawString(scoreFont, this.p1.ToString(), new Vector2(70,  viewport.Height - 35), this.ScoreColor1);

            // Draw Player 2 score
            spriteBatch.DrawString(labelFont, "Score", new Vector2(viewport.Width - 106, viewport.Height - 32), this.LabelColor2);
            spriteBatch.DrawString(scoreFont, this.p2.ToString(), new Vector2(viewport.Width - 48, viewport.Height - 35), this.ScoreColor2);
        }
    }
}
