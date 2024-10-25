using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using System.Reflection.Metadata;
using System.Threading;
using System.Timers;

namespace AdventureGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private static int screenWidth = 1024; // Screen width
        private static int screenHeight = 768; // Screen height
        List<Enemies> enemyList = new List<Enemies>(); // List to hold enemies
        static SoundEffect[] damageSfx = new SoundEffect[3]; // Array of sound effects
        static SoundEffectInstance[] damageSfxInstance = new SoundEffectInstance[3]; // Array of sound effect instances
        Player player; // Player object
        Menu menu; // Menu object
        Enemies enemies; // Enemies object
        Texture2D healthbar; // Texture for health bar
        Rectangle healthRect; // Rectangle for health bar
        Vector2 position_enemy = new Vector2(500, 600); // Position for spawning enemies
        SpriteFont myFont; // Font for drawing text
        int spawn, score, highest, timer; // Various game variables

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.PreferredBackBufferWidth = screenWidth;
            _graphics.PreferredBackBufferHeight = screenHeight;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            player = new Player(new Vector2(200, 1100), Color.DeepSkyBlue, 0.6f);
            enemies = new Enemies(position_enemy);
            menu = new Menu();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            player.LoadContent(Content);
            TileEngine.LoadLevel(Content, "karta32.tmx", "tileMap32", new Vector2(0, 0));
            LevelKey.LoadSounds(Content);

            damageSfx[0] = Content.Load<SoundEffect>("player_damage");
            damageSfxInstance[0] = damageSfx[0].CreateInstance();
            damageSfx[1] = Content.Load<SoundEffect>("sword_death");
            damageSfxInstance[1] = damageSfx[1].CreateInstance();
            damageSfx[2] = Content.Load<SoundEffect>("player_death");
            damageSfxInstance[2] = damageSfx[2].CreateInstance();
            myFont = Content.Load<SpriteFont>("TextFont");
            healthbar = Content.Load<Texture2D>("healthbar");
            healthRect = new Rectangle(0, 0, 180, 30);
            menu.LoadContent(Content);
            for (int i = 0; i < enemyList.Count; i++)
            {
                enemyList[i].LoadContent(Content);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime);
            enemies.Update(gameTime);
            TileEngine.UpdateCamera(player.GetPosition());
            TileEngine.UpdateAnimation(gameTime);

            if (Menu.start)
            {
                spawn += gameTime.ElapsedGameTime.Milliseconds;
                timer += gameTime.ElapsedGameTime.Milliseconds;

                // Spawning new enemies
                if (spawn > 1000 && LevelKey.doorIsLocked == false)
                {
                    enemies = new Enemies(position_enemy);
                    enemies.LoadContent(Content);
                    enemyList.Add(enemies);
                    spawn = 0;
                }

                for (int i = 0; i < enemyList.Count; i++)
                {
                    if (!enemyList[i].Update(gameTime))
                    {
                        enemyList.RemoveAt(i);
                    }
                }

                // Check for collisions between player and enemies
                for (int i = enemyList.Count - 1; i >= 0; i--)
                {
                    if (enemyList[i].hitbox.Intersects(player.playerHitbox))
                    {
                        if (player.newDirection == Player.dircetionFrame.fight_east || player.newDirection == Player.dircetionFrame.fight_west || player.newDirection == Player.dircetionFrame.fight_north || player.newDirection == Player.dircetionFrame.fight_south)
                        {
                            score++;
                            damageSfx[1].Play(volume: 0.1f, pitch: 0.0f, pan: 0.0f);
                            enemyList.RemoveAt(i);
                        }
                        else
                        {
                            damageSfxInstance[0].Volume = 0.5f;
                            damageSfxInstance[2].Volume = 0.5f;
                            damageSfxInstance[0].Play();
                            healthRect.Width -= 1000000000 / 1000000000;
                            if (healthRect.Width <= 0)
                                damageSfxInstance[2].Play();
                        }
                    }
                }

                // Check for end game conditions
                if (healthRect.Width <= 0 || (enemyList.Count == 0 && LevelKey.doorIsLocked))
                {
                    Menu.end = true;
                    Menu.start = false;
                }
            }
            else
            {
                menu.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.BackToFront);

            if (Menu.start)
            {
                TileEngine.Draw(_spriteBatch);
                player.Draw(_spriteBatch);
                for (int i = 0; i < enemyList.Count; i++)
                    enemyList[i].Draw(_spriteBatch);

                _spriteBatch.DrawString(myFont, $"Ghosts: {enemyList.Count}\nTime: {timer / 1000} Seconds\nScore: {score} catches", new Vector2(5, 30), Color.BlanchedAlmond, 0, new Vector2(0, 0), 0.8f, 0, 0);
                _spriteBatch.Draw(healthbar, healthRect, Color.Red);
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
                menu.Draw(_spriteBatch);
                if (Menu.end)
                    _spriteBatch.DrawString(myFont, $"Time: {timer / 1000} Seconds\nScore: {score} catches", new Vector2(20, 50), Color.BlanchedAlmond, 0, new Vector2(0, 0), 0.8f, 0, 0);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        public static int ScreenWidth() => screenWidth;
        public static int ScreenHeight() => screenHeight;
    }
}
