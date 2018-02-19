using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
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
        Random rand = new Random();

        const string gameTitle = "Time Attack! Prototype";

        // Cameras
        private Camera2D camera;

        // Game states
        bool gameIsRunning = false;
        bool gameOver = false;
        bool gamePaused = false;
        bool matchBeginning = false;

        // Input states
        GamePadState previousGpState;
        KeyboardState previousKbState;

        // Time tracking - seconds
        // TO DO: refactor this into a class or something later on
        int maxGameTime = 20;
        double lastGameTime = 0;
        double lastTimerUpdate = 0;
        double lastRespawn = 0;
        double totalGameTime = 0;
        double totalIdleTime = 0;
        double matchBeginTimer = 0;
        int gameTimer;

        int tileLength = 32;

        // Fonts
        SpriteFont labelFont;
        SpriteFont scoreFont;

        // Scores
        bool newHighScore = false;
        Score score;
        string[] highScores;

        // Game Entities
        Level level;
        Player playerOne, playerTwo;
        List<ScoreModifier> collectables;

        // Score modifier values
        int[] scoreModValues = { 1, -1, 1 };


        // Textures
        Texture2D textureSheet;
        Texture2D playerTexture;
        Texture2D emptyTileTexture;
        Texture2D wallTexture;
        Texture2D scoreModTexture;
        Texture2D gameOverScreenTexture;
        Texture2D gamePausedScreenTexture;
        Texture2D matchBeginScreenTexture1;
        Texture2D matchBeginScreenTexture2;


        // Sounds
        Dictionary<string, SoundEffect> sfx = new Dictionary<string, SoundEffect>();

        public MyGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // Start windowed
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
            graphics.IsFullScreen = false;

            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            graphics.ApplyChanges();

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
            base.Initialize();

            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 640, 480);
            camera = new Camera2D(viewportAdapter);

            Window.Title = gameTitle;

            this.gameTimer = maxGameTime;
            this.matchBeginning = true;
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
            gameOverScreenTexture = Content.Load<Texture2D>("screen_gameover");
            gamePausedScreenTexture = Content.Load<Texture2D>("screen_paused");
            matchBeginScreenTexture1 = Content.Load<Texture2D>("screen_beginmatch_01");
            matchBeginScreenTexture2 = Content.Load<Texture2D>("screen_beginmatch_02");

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

            // Load highscores
            string[] saveData = saveFile.Open();
            highScores = saveData ?? (new string[] { "0", "0" });

            // Players Setup
            playerTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 0, 0, tileLength, textureSheet.Height);
            SetupPlayers();

            // Level setup
            emptyTileTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 140, 0, tileLength, textureSheet.Height);
            wallTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 172, 0, tileLength, textureSheet.Height);
            scoreModTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 96, 0, tileLength, textureSheet.Height);

            SetLevel();

            // Set player positions in level
            SetPlayerStartPositions();

            // Set collectable item positions
            SetLevelCollectables();
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

        private void SetupPlayers()
        {
            float playerSpeed = tileLength * 0.5f;
            Dictionary<string, SoundEffect> playerSounds = new Dictionary<string, SoundEffect>() {
                { "hit", sfx["hit"] },
                { "jump", sfx["jump"] },
            };

            playerOne = new Player(playerTexture, playerSpeed, playerSounds)
            {
                Name = "Player 1",
                Input = new Input()
                {
                    Left = Keys.A,
                    Right = Keys.D,
                    Up = Keys.W,
                    Down = Keys.S,
                    Jump = Keys.Space
                },
                PlayerIndex = PlayerIndex.One,
                Position = Vector2.Zero,
                Passability = Passability.block,
                Color = Color.Red
            };

            playerTwo = new Player(playerTexture, playerSpeed, playerSounds)
            {
                Name = "Player 2",
                Passability = Passability.block,
                Input = new Input(),
                PlayerIndex = PlayerIndex.Two,
                Position = Vector2.Zero,
                Color = Color.Blue
            };
        }

        #region Level

        private void SetLevel()
        {
            // Set level contents - objects can be interacted with, tiles are only aesthetic... for now
            level = new Level(emptyTileTexture, GraphicsDevice.Viewport.Width / tileLength, GraphicsDevice.Viewport.Height / tileLength, tileLength);

            level.FillObjectRange(0, 0, 1, level.Rows, wallTexture, Passability.block);
            level.FillObjectRange(0, 0, level.Columns, 1, wallTexture, Passability.block);
            level.FillObjectRange(level.Columns - 1, 1, level.Columns, level.Rows, wallTexture, Passability.block);
            level.FillObjectRange(1, level.Rows - 1, level.Columns, level.Rows, wallTexture, Passability.block);
            level.RemoveObject(0, 5);
            level.RemoveObject(level.Columns - 1, 5);

            level.FillObjectRange(5, 4, 8, 5, wallTexture, Passability.block);
            level.FillObjectRange(5, 5, 6, 7, wallTexture, Passability.block);

            level.FillObjectRange(12, 10, 15, 11, wallTexture, Passability.block);
            level.FillObjectRange(14, 8, 15, 11, wallTexture, Passability.block);


            level.RemoveObject(0, level.Rows - 6);
            level.RemoveObject(level.Columns - 1, level.Rows - 6);
        }

        private void SetLevelCollectables()
        {
            if (collectables == null)
            {
                collectables = new List<ScoreModifier>();
            }

            // Cleanup first
            foreach (ScoreModifier collectable in collectables)
            {
                if (!collectable.IsDisplaced)
                    level.RemoveObject(collectable.Column, collectable.Row);
            }

            // Reposition instances
            Dictionary<String, SoundEffect> sounds = new Dictionary<String, SoundEffect>()
            {
                { "collect", sfx["itemcollect"] },
                { "score", sfx["itemscore"] },
                { "damage", sfx["itemdamage"] }
            };

            if (collectables.Count < 1)
            {
                collectables = new List<ScoreModifier>()
                {
                    new ScoreModifier(scoreModTexture, scoreModValues[rand.Next(scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, scoreModValues[rand.Next(scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, scoreModValues[rand.Next(scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, scoreModValues[rand.Next(scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, scoreModValues[rand.Next(scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, scoreModValues[rand.Next(scoreModValues.Length)], sounds)
                };
            }

            foreach (ScoreModifier collectable in collectables)
            {
                Vector2 position = GetRandomLevelPosition();
                level.SetObject((int)position.X, (int)position.Y, collectable, Passability.passable);
            }
        }

        void RespawnCollectables()
        {
            if (this.lastRespawn < 3f)
                return;

            foreach (GameObject collectable in collectables)
            {
                if (collectable.IsDisplaced)
                {
                    // Reposition instances
                    ((ScoreModifier)collectable).Modifier = scoreModValues[rand.Next(scoreModValues.Length)];
                    Vector2 position = GetRandomLevelPosition();
                    level.SetObject((int)position.X, (int)position.Y, collectable, Passability.passable);
                    break;
                }
            }
            this.lastRespawn = 0;
        }

        // TO DO: move to level class
        Vector2 GetRandomLevelPosition()
        {
            int column = rand.Next(level.Columns);
            int row = rand.Next(level.Rows);

            if (level.ObjMap[column, row] != null)
                return GetRandomLevelPosition();
            else
                return new Vector2(column, row);
        }

        // TO DO: move to player class
        private void SetPlayerStartPositions()
        {
            level.SetObject(1, 1, playerOne);
            level.SetObject(level.Columns - 2, level.Rows - 2, playerTwo);
        }

        #endregion

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState gpState = GamePad.GetState(PlayerIndex.One);
            KeyboardState kbState = Keyboard.GetState();

            double totalSeconds = gameTime.TotalGameTime.TotalSeconds;

            // Toggle fullscreen
            ToggleFullScreen(gpState, kbState);

            // Pause game
            PauseUnpause(gpState, kbState);

            // Exit game
            ExitGame(gpState, kbState);

            // Restart game
            Restart(gpState, kbState, totalSeconds);

            // Begin match
            BeginMatch();

            // Update game scores
            // TO DO: I don't like this solution. Refactor scores later.
            this.score.PlayerOne = playerOne.Score;
            this.score.PlayerTwo = playerTwo.Score;

            if (this.gameIsRunning)
            {
                level.Update(GraphicsDevice.Viewport, gameTime, level);
                RespawnCollectables();

                this.gameTimer = maxGameTime - (int)(totalGameTime - lastGameTime - totalIdleTime);
                this.lastTimerUpdate += gameTime.ElapsedGameTime.TotalSeconds;
                this.lastRespawn += gameTime.ElapsedGameTime.TotalSeconds;

                if (this.gameTimer <= 0)
                {
                    this.GameOver();
                }
                else if (gameTimer <= 5 && lastTimerUpdate >= 1f)
                {
                    lastTimerUpdate = 0;
                    PlaySound("pause");
                }
            }
            else
            {
                if (this.gamePaused || this.matchBeginning)
                {
                    totalIdleTime += gameTime.ElapsedGameTime.TotalSeconds;
                }
                if (this.matchBeginning)
                {
                    matchBeginTimer += gameTime.ElapsedGameTime.TotalSeconds;
                }
            }

            // Update previous input states
            previousGpState = gpState;
            previousKbState = kbState;

            totalGameTime += gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }


        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                graphics.PreferredBackBufferWidth = 640;
                graphics.PreferredBackBufferHeight = 480;
            }

            graphics.ApplyChanges();
        }

        private void ToggleFullScreen(GamePadState gpState, KeyboardState kbState)
        {
            if ((this.gamePaused || this.gameOver) && !this.gameIsRunning && kbState.IsKeyDown(Keys.F4))
            {
                graphics.IsFullScreen = !graphics.IsFullScreen;
                graphics.ApplyChanges();
            }
        }

        private void ExitGame(GamePadState gpState, KeyboardState kbState)
        {
            if ((this.gamePaused || this.gameOver) && !this.gameIsRunning && (gpState.Buttons.Back == ButtonState.Pressed || kbState.IsKeyDown(Keys.Escape)))
            {
                PlaySound("exit");
                Exit();
            }
        }

        void PlaySound(string sfxName)
        {
            if (this.sfx.Count > 0)
            {
                SoundEffectInstance sound = sfx[sfxName].CreateInstance();
                sound.Play();
            }
        }

        #region Game Pause

        protected override void OnDeactivated(object sender, System.EventArgs args)
        {
            if (!this.gamePaused)
                Pause();
        }

        void Pause()
        {
            if (!this.gameOver && this.gameIsRunning && !this.matchBeginning)
            {
                Window.Title = "Paused - " + gameTitle;
                gameIsRunning = false;
                this.gamePaused = true;
                this.PlaySound("pause");
                System.Diagnostics.Debug.WriteLine("Game paused");
            }
        }

        void Unpause()
        {
            if (!this.gameOver && !this.gameIsRunning && !this.matchBeginning) {
                Window.Title = gameTitle;
                this.gameIsRunning = true;
                this.gamePaused = false;
                this.PlaySound("pause");
                System.Diagnostics.Debug.WriteLine("Game unpaused");

            }
        }

        void PauseUnpause(GamePadState gpState, KeyboardState kbState)
        {
            if (this.gameOver || this.matchBeginning)
            {
                return;
            }

            if ((gpState.Buttons.Start == ButtonState.Pressed && previousGpState.Buttons.Start != ButtonState.Pressed) ||
                (kbState.IsKeyDown(Keys.P) && !previousKbState.IsKeyDown(Keys.P)))
            {
                if (!this.gamePaused)
                {
                    Pause();
                }
                else
                {
                    Unpause();
                }
            }
        }

        #endregion

        void BeginMatch()
        {
            if (this.matchBeginning && !this.gameIsRunning && !this.gamePaused)
            {
                if (this.matchBeginTimer > 4)
                {
                    this.gameIsRunning = true;
                    this.matchBeginning = false;
                    System.Diagnostics.Debug.WriteLine("BEGIN!");
                }
            }
        }

        void Restart(GamePadState gpState, KeyboardState kbState, double totalSeconds)
        {
            if ((this.gamePaused || this.gameOver) && (gpState.Buttons.Y == ButtonState.Pressed || kbState.IsKeyDown(Keys.R)))
            {
                System.Diagnostics.Debug.WriteLine("Restarting game...");

                this.lastGameTime = totalSeconds;
                this.matchBeginTimer = 0;
                this.totalIdleTime = 0;

                PlaySound("exit");

                this.gameTimer = maxGameTime;
                this.gameOver = false;
                this.newHighScore = false;

                this.gameIsRunning = false;
                this.gamePaused = false;
                this.matchBeginning = true;

                playerOne.Reset();
                playerTwo.Reset();

                // Set player positions in level
                SetPlayerStartPositions();
                SetLevelCollectables();

                Window.Title = gameTitle;
            }
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

        Player GetMatchWinner()
        {
            if (score.PlayerOne == score.PlayerTwo)
            {
                return null;
            }
            else if (score.PlayerOne > score.PlayerTwo)
            {
                return playerOne;
            }
            else
            {
                return playerTwo;
            }
        }

        void SaveHighScores()
        {
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

            // Saving high score for each player..
            if (saveFile.Write(highScores) && newHighScore)
                Window.Title = "New High Scores! Saved - " + gameTitle;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var transformMatrix = camera.GetViewMatrix();
            spriteBatch.Begin(transformMatrix: transformMatrix);

            this.level.Draw(spriteBatch);
            score.Draw(this.GraphicsDevice.Viewport, spriteBatch);

            // Draw match timer
            spriteBatch.DrawString(scoreFont, this.gameTimer.ToString(), new Vector2(10, 10), (this.gameTimer <= 5 ? Color.Red : Color.Black));

            // Game Over / Time Out screen
            if (this.gameOver)
            {
                spriteBatch.Draw(this.gameOverScreenTexture, Vector2.Zero, Color.White);
                Player winner = this.GetMatchWinner();
                string winMessage = winner == null ? "It's a TIE!" : winner.Name + " WINS!";
                Color labelColor = winner == null ? Color.Purple : winner.Color;
                spriteBatch.DrawString(labelFont, winMessage, new Vector2(this.Window.ClientBounds.Width - 150, 10), labelColor);
            }

            // Pause Screen
            if (this.gamePaused && !this.gameIsRunning)
            {
                spriteBatch.Draw(this.gamePausedScreenTexture, Vector2.Zero, Color.White);
            }

            // Show "BEGIN!" message on screen at beginning of match
            if (this.matchBeginning)
            {
                if (this.matchBeginTimer <= 2)
                    spriteBatch.Draw(this.matchBeginScreenTexture1, Vector2.Zero, Color.White);

                if (this.matchBeginTimer > 2 && matchBeginTimer < 3)
                    spriteBatch.Draw(this.matchBeginScreenTexture2, Vector2.Zero, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
