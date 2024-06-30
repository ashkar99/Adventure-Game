using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGame
{
    internal class Enemies : AnimatedObject
    {
        int multiEnemy; // Used to determine the type of enemy
        string enemyType; // Holds the enemy type string
        public Rectangle hitbox; // The hitbox for collision detection
        enum directionFrame { walk_south = 1, walk_west = 5, walk_east = 9, walk_north = 13 } // Directions for enemy movement animation frames
        directionFrame facing; // Current direction the enemy is facing

        // Constructor to initialize an enemy with its position and scale
        public Enemies(Vector2 position_enemy, float scale = 1) : base(position_enemy, Color.White, scale)
        {
            speed = new Vector2(0, 0); // Initial speed
            position = position_enemy; // Position of the enemy
            drawRect = new Rectangle(0, 0, 64, 64); // Rectangle for drawing the sprite
            spriteSize = new Vector2(64, 64); // Size of the sprite in pixels
            currentFrame = 1; // Initial frame
            animSpeed = 100; // Speed of the animation
            animTimer = 0; // Timer for the animation
            frameInRow = 4; // Number of frames per row in the sprite sheet
            animSequence = new int[] { 1, 2, 3, 0, 5, 6, 7, 4, 9, 10, 11, 8, 13, 14, 15, 12 }; // Animation sequence
            facing = directionFrame.walk_south; // Initial direction
        }

        // Load enemy content
        public override void LoadContent(ContentManager content)
        {
            multiEnemy = new Random().Next(1, 5); // Randomly select enemy type
            switch (multiEnemy)
            {
                case 1:
                    enemyType = "ghost_lvl01";
                    break;
                case 2:
                    enemyType = "ghost_lvl02";
                    break;
                case 3:
                    enemyType = "ghost_lvl03";
                    break;
                case 4:
                    enemyType = "ghost_lvl04";
                    break;
            }
            gfx_enemy_lvl01 = content.Load<Texture2D>(enemyType); // Load the texture based on the enemy type
            middle_enemy1 = new Vector2(gfx_enemy_lvl01.Width / 8, gfx_enemy_lvl01.Height / 8); // Middle point of the sprite
            position = new Vector2(random.Next(775, 825), random.Next(290, 300)); // Randomize the position within a range
        }

        // Get tile properties to determine collision
        public bool GetTileProperties(Vector2 place)
        {
            int tileType = TileEngine.GetTileType(place); // Get the type of the tile at the given place
            int currentTiletype = TileEngine.GetTileType(place - speed); // Get the type of the current tile

            // Collision detection against walls or out-of-bounds areas
            if (tileType == 4 || tileType == -1)
            {
                return false;
            }
            if (currentTiletype == 19)
            {
                if (tileType == 49 || tileType == 43)
                    return false;
            }
            if (currentTiletype == 43)
            {
                if (tileType == 25 || tileType == 31)
                    return false;
            }
            if (currentTiletype == 7)
            {
                if (tileType == 25)
                    return false;
            }
            if (currentTiletype == 31)
            {
                if (tileType == 7 || tileType == 25)
                    return false;
            }
            if (currentTiletype == 49)
            {
                if (tileType == 19)
                    return false;
            }

            return true;
        }

        // Update the enemy state
        public override bool Update(GameTime gameTime)
        {
            directionFrame newDirection = facing; // Current direction

            // Determine new speed and direction if not moving
            while (speed.X == 0 && speed.Y == 0)
            {
                speed = new Vector2(random.Next(-1, 2), random.Next(-1, 2));
            }
            if (speed.X == 1)
            {
                newDirection = directionFrame.walk_east;
            }
            if (speed.X == -1)
            {
                newDirection = directionFrame.walk_west;
            }
            if (speed.Y == -1)
            {
                newDirection = directionFrame.walk_north;
            }
            if (speed.Y == 1)
            {
                newDirection = directionFrame.walk_south;
            }

            speed.Normalize(); // Normalize the speed vector to ensure its length is 1 without changing direction
            speed *= 40 * (float)gameTime.ElapsedGameTime.TotalSeconds; // Scale speed by elapsed game time

            // Update position if moving and no collision
            if (speed.Length() > 0 && GetTileProperties(position + speed))
            {
                position += speed;
                SetAnimation(gameTime);
            }
            else
            {
                speed = Vector2.Zero;
            }

            // Update facing direction if it has changed
            if (facing != newDirection)
            {
                facing = newDirection;
                currentFrame = (int)facing;
                SetFrame(currentFrame);
            }

            // Update hitbox based on current position
            hitbox = new Rectangle((int)(position.X + middle_enemy1.X / 4), (int)(position.Y + middle_enemy1.X / 4), 19, 19);

            return true;
        }

        // Draw the enemy on the screen
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(gfx_enemy_lvl01, position - TileEngine.GetCamera(), drawRect, Color.White, angle, middle, 0.6f, SpriteEffects.None, 0.3f);
        }
    }
}
