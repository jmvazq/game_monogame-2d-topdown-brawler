using System.Collections.Generic;
﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using GameEngine;
using GameEngine.Models;
using GameEngine.Sprites;

/* IDEAS for the game
 * 
 * 1) Time match (x seconds)
 * 2) Hit Score can be accumulated by hitting the other player
 * 3) Get items for bonuses to your score or to use against the other player
 * 3) At the end of the match, player with the highest hit score wins
 * 4) IN THE FUTURE: make it possible for players to be killed and respawn after a cooldown period (like Smash Meelee's Time Attack)
 * 6) IN THE FUTURE: item time attack mode = score with items only
 * */

namespace MyGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MyGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SaveFile saveFile;

        const string gameTitle = "Time Attack! Prototype";

        bool gameIsRunning = false;
        bool gameOver = false;

        int maxGameTime = 20; // seconds
        double lastGameTime = 0;
        int timer;

        SpriteFont labelFont;
        SpriteFont scoreFont;
        Score score;
        string[] highScores;

        int tileLength = 32;
        Player playerOne, playerTwo;

        Texture2D textureSheet;

        List<Sprite> sprites;

        // Sounds
        Dictionary<string, SoundEffect> sfx = new Dictionary<string, SoundEffect>();

        public MyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.Title = gameTitle;
            base.Initialize();

            this.timer = maxGameTime;
            this.gameIsRunning = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            labelFont = Content.Load<SpriteFont>("Label");
            scoreFont = Content.Load<SpriteFont>("Score");

            // Load textures
            textureSheet = Content.Load<Texture2D>("sprite_textures");
            Rectangle playerTextureR = new Rectangle(0, 0, tileLength, textureSheet.Height);
            // Load sounds
            SoundEffect sfxPause = Content.Load<SoundEffect>("sounds/sfx_pause");
            SoundEffect sfxExit = Content.Load<SoundEffect>("sounds/sfx_exit");
            SoundEffect sfxHit = Content.Load<SoundEffect>("sounds/sfx_hit");
            SoundEffect sfxJump = Content.Load<SoundEffect>("sounds/sfx_jump");
            SoundEffect sfxItemScore = Content.Load<SoundEffect>("sounds/sfx_item_score");
            SoundEffect sfxItemDamage = Content.Load<SoundEffect>("sounds/sfx_item_damage");
            SoundEffect sfxItemCollect = Content.Load<SoundEffect>("sounds/sfx_item_collect");

            sfx.Add("pause", sfxPause);
            sfx.Add("exit", sfxExit);
            sfx.Add("hit", sfxHit);
            sfx.Add("jump", sfxJump);
            sfx.Add("itemscore", sfxItemScore);
            sfx.Add("itemdamage", sfxItemDamage);
            sfx.Add("itemcollect", sfxItemCollect);

            // Scores
            saveFile = new SaveFile();

            score = new Score(labelFont, scoreFont);
            score.LabelColor1 = Color.Red;
            score.LabelColor2 = Color.Blue;
            score.ScoreColor1 = Color.Black;
            score.ScoreColor2 = Color.Black;

            string[] saveData = saveFile.Open();
            highScores = saveData ?? (new string[] { "0", "0" });

            // Players Setup
            Texture2D playerTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 0, 0, tileLength, textureSheet.Height);

            float playerSpeed = tileLength;

            playerOne = new Player(playerTexture, playerSpeed)
            {
                Name = "Player 1",
                Input = new Input(),
                Position = new Vector2(
                    (Window.ClientBounds.Width - playerTexture.Width) / 2 - (playerTexture.Width / 2) - 5,
                    (Window.ClientBounds.Height - playerTexture.Height) / 2),
                Passability = Passability.block,
                Color = Color.Red
            };

            playerTwo = new Player(playerTexture, playerSpeed)
            {
                Name = "Player 2",
                Passability = Passability.block,
                Input = new Input()
                {
                    Left = Keys.A,
                    Right = Keys.D,
                    Up = Keys.W,
                    Down = Keys.S
                },
                Position = new Vector2(
                    (Window.ClientBounds.Width - playerTexture.Width) / 2 + (playerTexture.Width / 2) + 5, 
                    (Window.ClientBounds.Height - playerTexture.Height) / 2),
                Color = Color.Blue
            };

            // Other objects' setup

            // Add all objects to sprites list
            sprites = new List<Sprite>()
            {
                playerOne, playerTwo
            };
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            this.Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState gpState = GamePad.GetState(PlayerIndex.One);
            KeyboardState kbState = Keyboard.GetState();

            // Pause game
            if (!gameOver && (gpState.Buttons.Start == ButtonState.Pressed || kbState.IsKeyDown(Keys.Space)))
                PauseUnpause();

            // Exit game
            if (gpState.Buttons.Back == ButtonState.Pressed || kbState.IsKeyDown(Keys.Escape))
                Exit();

            // Restart game
            if (gpState.Buttons.X == ButtonState.Pressed || kbState.IsKeyDown(Keys.R))
            {
                lastGameTime = gameTime.TotalGameTime.TotalSeconds;
                Restart();
            }

            // Update game scores
            // TO DO: I don't like this solution. Refactor scores later.
            this.score.PlayerOne = playerOne.Score;
            this.score.PlayerTwo = playerTwo.Score;

            if (this.gameIsRunning)
            {
                foreach (Sprite sprite in sprites)
                {
                    sprite.Update(gameTime, sprites);
                }

                this.timer = maxGameTime - ((int)gameTime.TotalGameTime.TotalSeconds - (int)lastGameTime);
                if (this.timer <= 0)
                {
                    this.GameOver();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Azure);

            spriteBatch.Begin();

            foreach (Sprite sprite in sprites)
        void PlaySound(string sfxName)
        {
            if (this.sfx.Count > 0)
            {
                sprite.Draw(spriteBatch);
                SoundEffectInstance sound = sfx[sfxName].CreateInstance();
                sound.Play();
            }
        }

            score.Draw(this, spriteBatch);

            // draw timer
            spriteBatch.DrawString(scoreFont, this.timer.ToString(), new Vector2(10, 10), Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        void PauseUnpause()
        {
            if (gameIsRunning)
            {
                Window.Title = "Paused - " + gameTitle;
            }
            else
            {
                Window.Title = gameTitle;
            }
            gameIsRunning = !gameIsRunning;
        }

        void Restart()
        {
            System.Diagnostics.Debug.WriteLine("Restarting game...");
            this.timer = maxGameTime;
            this.gameIsRunning = true;
            this.gameOver = false;

            playerOne.Reset();
            playerOne.Position = new Vector2(
                    (Window.ClientBounds.Width - playerOne.Width) / 2 - (playerOne.Width / 2) - 5,
                    (Window.ClientBounds.Height - playerOne.Height) / 2);
            playerTwo.Position = new Vector2(
                    (Window.ClientBounds.Width - playerTwo.Width) / 2 + (playerTwo.Width / 2) + 5,
                    (Window.ClientBounds.Height - playerTwo.Height) / 2);
            
            playerTwo.Reset();

            Window.Title = gameTitle;
        }

        void GameOver()
        {
            if (this.gameIsRunning)
            {
                System.Diagnostics.Debug.WriteLine("Time is out!");
                PlaySound("exit");
                this.gameIsRunning = false;
                this.gameOver = true;
                SaveHighScores();
            }
        }

        void SaveHighScores()
        {
            bool newHighScore = false;

            if (score.PlayerOne > int.Parse(highScores[0]))
            {
                highScores[0] = score.PlayerOne.ToString();
                newHighScore = true;
            }
            if (score.PlayerTwo > int.Parse(highScores[1]))
            {
                highScores[1] = score.PlayerTwo.ToString();
                newHighScore = true;
            }

            // saving high score for each player
            if (saveFile.Write(highScores) && newHighScore)
                Window.Title = "New High Scores! Saved - " + gameTitle;
        }
    }
}
