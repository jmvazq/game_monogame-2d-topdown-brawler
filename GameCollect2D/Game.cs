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
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        SaveFile _saveFile;
        Random _rand = new Random();

        const string _gameTitle = "Time Attack! Prototype";

        // Window
        int _windowSizeWidth = 1024;
        int _windowSizeHeight = 768;

        // Cameras
        private Camera2D _camera;
        int _cameraWidth = 640;
        int _cameraHeight = 480;

        // Game states
        bool _gameIsRunning = false;
        bool _gameOver = false;
        bool _gamePaused = false;
        bool _matchBeginning = false;

        // Input states
        GamePadState _previousGpState;
        KeyboardState _previousKbState;

        // Time tracking - seconds
        // TO DO: refactor this into a class or something later on
        int _maxGameTime = 20;
        double _lastGameTime = 0;
        double _lastTimerUpdate = 0;
        double _lastRespawn = 0;
        double _totalGameTime = 0;
        double _totalIdleTime = 0;
        double _matchBeginTimer = 0;
        int _gameTimer;

        int _tileLength = 32;

        // Fonts
        SpriteFont _labelFont;
        SpriteFont _scoreFont;

        // Scores
        bool _newHighScore = false;
        Score _score;
        string[] _highScores;

        // Game Entities
        Level _level;
        Player _playerOne, _playerTwo;
        List<ScoreModifier> _collectables;

        // Score modifier values
        int[] _scoreModValues = { 1, -1, 1 };


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
        Dictionary<string, SoundEffect> _sfx = new Dictionary<string, SoundEffect>();

        public MyGame()
        {
            _graphics = new GraphicsDeviceManager(this);

            // Start windowed
            _graphics.PreferredBackBufferWidth = _windowSizeWidth;
            _graphics.PreferredBackBufferHeight = _windowSizeHeight;
            _graphics.IsFullScreen = false;

            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            _graphics.ApplyChanges();

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

            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, _cameraWidth, _cameraHeight);
            _camera = new Camera2D(viewportAdapter);

            Window.Title = _gameTitle;

            this._gameTimer = _maxGameTime;
            this._matchBeginning = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            _labelFont = Content.Load<SpriteFont>("Label");
            _scoreFont = Content.Load<SpriteFont>("Score");

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

            _sfx.Add("pause", sfxPause);
            _sfx.Add("exit", sfxExit);
            _sfx.Add("hit", sfxHit);
            _sfx.Add("jump", sfxJump);
            _sfx.Add("itemscore", sfxItemScore);
            _sfx.Add("itemdamage", sfxItemDamage);
            _sfx.Add("itemcollect", sfxItemCollect);

            // Scores
            _saveFile = new SaveFile();

            _score = new Score(_labelFont, _scoreFont);
            _score.LabelColor1 = Color.Red;
            _score.LabelColor2 = Color.Blue;
            _score.ScoreColor1 = Color.Black;
            _score.ScoreColor2 = Color.Black;

            // Load highscores
            string[] saveData = _saveFile.Open();
            _highScores = saveData ?? (new string[] { "0", "0" });

            // Players Setup
            playerTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 0, 0, _tileLength, textureSheet.Height);
            SetupPlayers();

            // Level setup
            emptyTileTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 140, 0, _tileLength, textureSheet.Height);
            wallTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 172, 0, _tileLength, textureSheet.Height);
            scoreModTexture = Sprite.CreateSubTexture(GraphicsDevice, textureSheet, 96, 0, _tileLength, textureSheet.Height);

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
            float playerSpeed = _tileLength * 0.5f;
            Dictionary<string, SoundEffect> playerSounds = new Dictionary<string, SoundEffect>() {
                { "hit", _sfx["hit"] },
                { "jump", _sfx["jump"] },
            };

            _playerOne = new Player(playerTexture, playerSpeed, playerSounds)
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

            _playerTwo = new Player(playerTexture, playerSpeed, playerSounds)
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
            _level = new Level(emptyTileTexture, 20, 15, _tileLength);

            _level.FillObjectRange(0, 0, 1, _level.Rows, wallTexture, Passability.block);
            _level.FillObjectRange(0, 0, _level.Columns, 1, wallTexture, Passability.block);
            _level.FillObjectRange(_level.Columns - 1, 1, _level.Columns, _level.Rows, wallTexture, Passability.block);
            _level.FillObjectRange(1, _level.Rows - 1, _level.Columns, _level.Rows, wallTexture, Passability.block);
            _level.RemoveObject(0, 5);
            _level.RemoveObject(_level.Columns - 1, 5);

            _level.FillObjectRange(5, 4, 8, 5, wallTexture, Passability.block);
            _level.FillObjectRange(5, 5, 6, 7, wallTexture, Passability.block);

            _level.FillObjectRange(12, 10, 15, 11, wallTexture, Passability.block);
            _level.FillObjectRange(14, 8, 15, 11, wallTexture, Passability.block);


            _level.RemoveObject(0, _level.Rows - 6);
            _level.RemoveObject(_level.Columns - 1, _level.Rows - 6);
        }

        private void SetLevelCollectables()
        {
            if (_collectables == null)
            {
                _collectables = new List<ScoreModifier>();
            }

            // Cleanup first
            foreach (ScoreModifier collectable in _collectables)
            {
                if (!collectable.IsDisplaced)
                    _level.RemoveObject(collectable.Column, collectable.Row);
            }

            // Reposition instances
            Dictionary<String, SoundEffect> sounds = new Dictionary<String, SoundEffect>()
            {
                { "collect", _sfx["itemcollect"] },
                { "score", _sfx["itemscore"] },
                { "damage", _sfx["itemdamage"] }
            };

            if (_collectables.Count < 1)
            {
                _collectables = new List<ScoreModifier>()
                {
                    new ScoreModifier(scoreModTexture, _scoreModValues[_rand.Next(_scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, _scoreModValues[_rand.Next(_scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, _scoreModValues[_rand.Next(_scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, _scoreModValues[_rand.Next(_scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, _scoreModValues[_rand.Next(_scoreModValues.Length)], sounds),
                    new ScoreModifier(scoreModTexture, _scoreModValues[_rand.Next(_scoreModValues.Length)], sounds)
                };
            }

            foreach (ScoreModifier collectable in _collectables)
            {
                Vector2 position = GetRandomLevelPosition();
                _level.SetObject((int)position.X, (int)position.Y, collectable, Passability.passable);
            }
        }

        void RespawnCollectables()
        {
            if (this._lastRespawn < 3f)
                return;

            foreach (GameObject collectable in _collectables)
            {
                if (collectable.IsDisplaced)
                {
                    // Reposition instances
                    ((ScoreModifier)collectable).Modifier = _scoreModValues[_rand.Next(_scoreModValues.Length)];
                    Vector2 position = GetRandomLevelPosition();
                    _level.SetObject((int)position.X, (int)position.Y, collectable, Passability.passable);
                    break;
                }
            }
            this._lastRespawn = 0;
        }

        // TO DO: move to level class
        Vector2 GetRandomLevelPosition()
        {
            int column = _rand.Next(_level.Columns);
            int row = _rand.Next(_level.Rows);

            if (_level.ObjMap[column, row] != null)
                return GetRandomLevelPosition();
            else
                return new Vector2(column, row);
        }

        // TO DO: move to player class
        private void SetPlayerStartPositions()
        {
            _level.SetObject(1, 1, _playerOne);
            _level.SetObject(_level.Columns - 2, _level.Rows - 2, _playerTwo);
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
            this._score.PlayerOne = _playerOne.Score;
            this._score.PlayerTwo = _playerTwo.Score;

            if (this._gameIsRunning)
            {
                _level.Update(this, _camera, GraphicsDevice.Viewport, gameTime, _level);
                RespawnCollectables();

                this._gameTimer = _maxGameTime - (int)(_totalGameTime - _lastGameTime - _totalIdleTime);
                this._lastTimerUpdate += gameTime.ElapsedGameTime.TotalSeconds;
                this._lastRespawn += gameTime.ElapsedGameTime.TotalSeconds;

                if (this._gameTimer <= 0)
                {
                    this.GameOver();
                }
                else if (_gameTimer <= 5 && _lastTimerUpdate >= 1f)
                {
                    _lastTimerUpdate = 0;
                    PlaySound("pause");
                }
            }
            else
            {
                if (this._gamePaused || this._matchBeginning)
                {
                    _totalIdleTime += gameTime.ElapsedGameTime.TotalSeconds;
                }
                if (this._matchBeginning)
                {
                    _matchBeginTimer += gameTime.ElapsedGameTime.TotalSeconds;
                }
            }

            // Update previous input states
            _previousGpState = gpState;
            _previousKbState = kbState;

            _totalGameTime += gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }


        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = _windowSizeWidth;
                _graphics.PreferredBackBufferHeight = _windowSizeHeight;
            }

            _graphics.ApplyChanges();
        }

        private void ToggleFullScreen(GamePadState gpState, KeyboardState kbState)
        {
            if ((this._gamePaused || this._gameOver) && !this._gameIsRunning && kbState.IsKeyDown(Keys.F4))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                _graphics.ApplyChanges();
            }
        }

        private void ExitGame(GamePadState gpState, KeyboardState kbState)
        {
            if ((this._gamePaused || this._gameOver) && !this._gameIsRunning && (gpState.Buttons.Back == ButtonState.Pressed || kbState.IsKeyDown(Keys.Escape)))
            {
                PlaySound("exit");
                Exit();
            }
        }

        void PlaySound(string sfxName)
        {
            if (this._sfx.Count > 0)
            {
                SoundEffectInstance sound = _sfx[sfxName].CreateInstance();
                sound.Play();
            }
        }

        #region Game Pause

        protected override void OnDeactivated(object sender, System.EventArgs args)
        {
            if (!this._gamePaused)
                Pause();
        }

        void Pause()
        {
            if (!this._gameOver && this._gameIsRunning && !this._matchBeginning)
            {
                Window.Title = "Paused - " + _gameTitle;
                _gameIsRunning = false;
                this._gamePaused = true;
                this.PlaySound("pause");
                System.Diagnostics.Debug.WriteLine("Game paused");
            }
        }

        void Unpause()
        {
            if (!this._gameOver && !this._gameIsRunning && !this._matchBeginning) {
                Window.Title = _gameTitle;
                this._gameIsRunning = true;
                this._gamePaused = false;
                this.PlaySound("pause");
                System.Diagnostics.Debug.WriteLine("Game unpaused");

            }
        }

        void PauseUnpause(GamePadState gpState, KeyboardState kbState)
        {
            if (this._gameOver || this._matchBeginning)
            {
                return;
            }

            if ((gpState.Buttons.Start == ButtonState.Pressed && _previousGpState.Buttons.Start != ButtonState.Pressed) ||
                (kbState.IsKeyDown(Keys.P) && !_previousKbState.IsKeyDown(Keys.P)))
            {
                if (!this._gamePaused)
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
            if (this._matchBeginning && !this._gameIsRunning && !this._gamePaused)
            {
                if (this._matchBeginTimer > 4)
                {
                    this._gameIsRunning = true;
                    this._matchBeginning = false;
                    System.Diagnostics.Debug.WriteLine("BEGIN!");
                }
            }
        }

        void Restart(GamePadState gpState, KeyboardState kbState, double totalSeconds)
        {
            if ((this._gamePaused || this._gameOver) && (gpState.Buttons.Y == ButtonState.Pressed || kbState.IsKeyDown(Keys.R)))
            {
                System.Diagnostics.Debug.WriteLine("Restarting game...");

                this._lastGameTime = totalSeconds;
                this._matchBeginTimer = 0;
                this._totalIdleTime = 0;

                PlaySound("exit");

                this._gameTimer = _maxGameTime;
                this._gameOver = false;
                this._newHighScore = false;

                this._gameIsRunning = false;
                this._gamePaused = false;
                this._matchBeginning = true;

                _playerOne.Reset();
                _playerTwo.Reset();

                // Set player positions in level
                SetPlayerStartPositions();
                SetLevelCollectables();

                Window.Title = _gameTitle;
            }
        }

        void GameOver()
        {
            if (this._gameIsRunning)
            {
                System.Diagnostics.Debug.WriteLine("Time is out!");
                PlaySound("exit");
                this._gameIsRunning = false;
                this._gameOver = true;
                SaveHighScores();
            }
        }

        Player GetMatchWinner()
        {
            if (_score.PlayerOne == _score.PlayerTwo)
            {
                return null;
            }
            else if (_score.PlayerOne > _score.PlayerTwo)
            {
                return _playerOne;
            }
            else
            {
                return _playerTwo;
            }
        }

        void SaveHighScores()
        {
            if (_score.PlayerOne > int.Parse(_highScores[0]))
            {
                _highScores[0] = _score.PlayerOne.ToString();
                _newHighScore = true;
            }
            if (_score.PlayerTwo > int.Parse(_highScores[1]))
            {
                _highScores[1] = _score.PlayerTwo.ToString();
                _newHighScore = true;
            }

            // Saving high score for each player..
            if (_saveFile.Write(_highScores) && _newHighScore)
                Window.Title = "New High Scores! Saved - " + _gameTitle;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var transformMatrix = _camera.GetViewMatrix();
            _spriteBatch.Begin(transformMatrix: transformMatrix);

            this._level.Draw(_spriteBatch);
            _score.Draw(this.GraphicsDevice.Viewport, _spriteBatch);

            // Draw match timer
            _spriteBatch.DrawString(_scoreFont, this._gameTimer.ToString(), new Vector2(10, 10), (this._gameTimer <= 5 ? Color.Red : Color.Black));

            // Game Over / Time Out screen
            if (this._gameOver)
            {
                _spriteBatch.Draw(this.gameOverScreenTexture, Vector2.Zero, Color.White);
                Player winner = this.GetMatchWinner();
                string winMessage = winner == null ? "It's a TIE!" : winner.Name + " WINS!";
                Color labelColor = winner == null ? Color.Purple : winner.Color;
                _spriteBatch.DrawString(_labelFont, winMessage, new Vector2(this.Window.ClientBounds.Width - 150, 10), labelColor);
            }

            // Pause Screen
            if (this._gamePaused && !this._gameIsRunning)
            {
                _spriteBatch.Draw(this.gamePausedScreenTexture, Vector2.Zero, Color.White);
            }

            // Show "BEGIN!" message on screen at beginning of match
            if (this._matchBeginning)
            {
                if (this._matchBeginTimer <= 2)
                    _spriteBatch.Draw(this.matchBeginScreenTexture1, Vector2.Zero, Color.White);

                if (this._matchBeginTimer > 2 && _matchBeginTimer < 3)
                    _spriteBatch.Draw(this.matchBeginScreenTexture2, Vector2.Zero, Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
