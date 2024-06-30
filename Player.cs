using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGame
{
    internal class Player : AnimatedObject {
        // Public static variables
        public static int maxSpeed = 100;             // Maximum speed of the player
        public static SoundEffect[] sfx_walk = new SoundEffect[5];  // Array of walking sound effects
        public static SoundEffectInstance walkSfxInstance;  // Instance of the walking sound effect

        // Private variables
        bool isWalking = true;                        // Flag indicating if the player is walking
        bool isFighting = false;                      // Flag indicating if the player is fighting
        public Rectangle playerHitbox;                // Hitbox rectangle of the player
        Vector2 feet;                                 // Position of the player's feet
        int currentTileType;                          // Current type of tile the player is on
        int waitIamClosingTheDoor = 0;                // Timer for door closing action
        static SoundEffect sfx_sword;                 // Static sword sound effect
        static SoundEffectInstance sfx_swordInstance; // Instance of the sword sound effect
        private KeyboardState previousKeyboardState;  // Previous state of the keyboard
        int timer;                                    // Timer variable for various actions

        // Enum definitions
        enum figureState { standing, walking, fighting }  // Player states
        figureState currenState;                    // Current state of the player
        public dircetionFrame newDirection;         // New direction frame enum

        // Direction frame enum
        public enum dircetionFrame {
            walk_south = 1,  walk_southwest = 4,    run_south = 7,    run_southwest = 10,
            walk_west = 13,  walk_southeast = 16,   run_west = 19,    run_southeast = 22,
            walk_east = 25,  walk_northwest = 28,   run_east = 31,    run_northwest = 34,
            walk_north = 37, walk_northeast = 40,   run_north = 43,   run_northeast = 46,
            fight_south = 48, fight_west = 60,      fight_east = 72,  fight_north = 84
        }
        dircetionFrame facing;                      // Direction the player is facing

        // Constructor for Player class
        public Player(Vector2 position, Color primaryColor, float scale = 1, float angle = 0, float alpha = 1)
            : base(position, primaryColor, scale, angle, alpha) {
            spriteSize = new Vector2(100, 100);     // Size of the player sprite in pixels
            drawRect = new Rectangle(0, 0, (int)spriteSize.X, (int)spriteSize.Y);   // Rectangle for clipping current frame
            currenState = figureState.standing;     // Initial state of the player (standing)
            currentFrame = 1;                       // Initial frame index
            animSpeed = 100;                        // Animation speed
            animTimer = 0;                          // Animation timer
            frameInRow = 12;                        // Frames per row in sprite sheet

            // Animation sequence for walking and running
            animSequence = new int[] {
                1, 2, 0,  4, 5, 3,  7, 8, 6, 10, 11, 9,
                13, 14, 12, 16, 17, 15, 19, 20, 18, 22, 23, 21,
                25, 26, 24, 28, 29, 27, 31, 32, 30, 34, 35, 33,
                37, 38, 36, 40, 41, 39, 43, 44, 42, 46, 47, 45,
                // FIGHT gfx
                49, 50, 48, 0, 0, 0,  0, 0, 0,  0, 0, 0,
                61, 62, 60, 0, 0, 0,  0, 0, 0,  0, 0, 0,
                73, 74, 72, 0, 0, 0,  0, 0, 0,  0, 0, 0,
                85, 86, 84, 0, 0, 0,  0, 0, 0,  0, 0, 0
            };

            facing = dircetionFrame.walk_south;     // Initial facing direction
            SetFrame(currentFrame);                 // Set initial frame
        }
    
            

        public override void LoadContent(ContentManager content) {
            gfx_avenger = content.Load<Texture2D>("avenger");  // Load player texture

            middle = new Vector2(gfx_avenger.Width / 24, gfx_avenger.Height / 16);  // Calculate middle position of the player
            feet.X = 0;  // Set feet X position to 0
            feet.Y = middle.Y * 0.6f;  // Set feet Y position based on middle position

            // Load different walking sound effects for various surfaces
            sfx_walk[0] = content.Load<SoundEffect>("Steps_Sand");
            sfx_walk[1] = content.Load<SoundEffect>("Steps_Grass");
            sfx_walk[2] = content.Load<SoundEffect>("Steps_ShallowWater");
            sfx_walk[3] = content.Load<SoundEffect>("Steps_Wood");
            sfx_walk[4] = content.Load<SoundEffect>("Steps_Stone");

            sfx_sword = content.Load<SoundEffect>("sword_hit");  // Load sword hit sound effect
            sfx_swordInstance = sfx_sword.CreateInstance();  // Create instance of sword sound effect

            walkSfxInstance = sfx_walk[0].CreateInstance();  // Create instance of walking sound effect for the first surface
            walkSfxInstance.IsLooped = true;  // Set walking sound effect to loop
        }

        public bool GetTileProperties(Vector2 place) {
            int tileType = TileEngine.GetTileType(place);  // Get type of tile at current position
            int currentTiletype = TileEngine.GetTileType(place - speed);  // Get type of tile at current position minus speed

            // Collision with wall or outside world
            if (tileType == 4 || tileType == -1)
            {
                return false;
            }

            // Specific tile type checks for collisions
            if (currentTiletype == 19)
            {
                if (tileType == 49)
                    return false;
                if (tileType == 43)
                    return false;
            }
            if (currentTiletype == 43)
            {
                if (tileType == 25)
                    return false;
                if (tileType == 19)
                    return false;
                if (tileType == 31)
                    return false;
            }
            if (currentTiletype == 7)
            {
                if (tileType == 31)
                    return false;
                if (tileType == 25)
                    return false;
            }
            if (currentTiletype == 31)
            {
                if (tileType == 7)
                    return false;
                if (tileType == 25)
                    return false;
                if (tileType == 49)
                    return false;
            }
            if (currentTiletype == 49)
            {
                if (tileType == 19)
                    return false;
                if (tileType == 31)
                    return false;
            }

            if (tileType == currentTileType)  // Same surface type, no change in behavior
            {
                return true;
            }

            // New surface type encountered
            if (tileType == 11)
            {
                return LevelKey.TileEvent(place);  // Invoke tile event handler
            }

            currentTileType = tileType;  // Update current tile type

            walkSfxInstance.Stop();  // Stop current walking sound
            walkSfxInstance = sfx_walk[tileType % 6].CreateInstance();  // Create new walking sound instance based on surface type
            walkSfxInstance.IsLooped = true;  // Set new walking sound to loop
            walkSfxInstance.Play();  // Play new walking sound

            return true;
        }

        public override bool Update(GameTime gameTime) {
            KeyboardState currentKeyboardState = Keyboard.GetState();  // Get the current state of the keyboard

            speed = Vector2.Zero;  // Reset the speed vector

            newDirection = dircetionFrame.walk_south;  // Default direction is walking south

            // Adjust speed: get tired after running
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && isWalking)
            {
                isWalking = false;
                maxSpeed += 40;  // Increase max speed
            }
            if (currentKeyboardState.IsKeyUp(Keys.LeftShift) && !isWalking)
            {
                isWalking = true;
                maxSpeed -= 50;  // Decrease max speed
            }
            if (maxSpeed == 60)
                maxSpeed = 100;  // Ensure max speed doesn't drop too low

            // Disable keyboard input temporarily
            if (LevelKey.keyboardInputEnabled == false)
            {
                waitIamClosingTheDoor++;
                if (waitIamClosingTheDoor == 60)
                    LevelKey.keyboardInputEnabled = true;  // Re-enable keyboard input after a delay
            }

            // Process keyboard input for movement and combat
            if (LevelKey.keyboardInputEnabled == true && (Menu.start || !Menu.end))
            {
                // Handle movement and combat directions
                if (currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_north;
                    speed.Y = -1f;  // Move north
                }
                if (currentKeyboardState.IsKeyDown(Keys.W) && currentKeyboardState.IsKeyDown(Keys.A) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_northwest;
                    speed.Y = -1f;
                    speed.X = -1f;  // Move northwest
                }
                if (currentKeyboardState.IsKeyDown(Keys.W) && currentKeyboardState.IsKeyDown(Keys.D) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_northeast;
                    speed.Y = -1f;
                    speed.X = 1f;  // Move northeast
                }
                if (currentKeyboardState.IsKeyDown(Keys.S) && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_south;
                    speed.Y = 1f;  // Move south
                }
                if (currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.A) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_southwest;
                    speed.X = -1f;
                    speed.Y = 1f;  // Move southwest
                }
                if (currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.D) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_southeast;
                    speed.X = 1f;
                    speed.Y = 1f;  // Move southeast
                }
                if (currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_west;
                    speed.X = -1f;  // Move west
                }
                if (currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.run_east;
                    speed.X = 1f;  // Move east
                }

                // Handle walking directions
                if (currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_north;
                    speed.Y = -1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_north;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.W) && currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_northwest;
                    speed.Y = -1;
                    speed.X = -1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_north;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.W) && currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_northeast;
                    speed.Y = -1;
                    speed.X = 1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_north;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.S) && !currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_south;
                    speed.Y = 1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_south;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_southwest;
                    speed.X = -1;
                    speed.Y = 1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_south;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_southeast;
                    speed.X = 1;
                    speed.Y = 1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_south;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_west;
                    speed.X = -1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_west;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
                if (currentKeyboardState.IsKeyDown(Keys.D) && !currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S) && !currentKeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    newDirection = dircetionFrame.walk_east;
                    speed.X = 1;
                    timer++;
                    if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                    {
                        newDirection = dircetionFrame.fight_east;
                        isFighting = true;  // Start fighting
                    }
                    if (timer == 30)
                    {
                        previousKeyboardState = currentKeyboardState;
                        timer = 0;
                    }
                }
            }

            // Play sword sound effect during combat
            if (newDirection == dircetionFrame.fight_east || newDirection == dircetionFrame.fight_west || newDirection == dircetionFrame.fight_north || newDirection == dircetionFrame.fight_south)
            {
                sfx_swordInstance.Volume = 0.3f;
                sfx_swordInstance.Play();
            }

            speed.Normalize();  // Normalize speed vector

            // Calculate movement
            speed *= maxSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move player if allowed by tile properties
            if (speed.Length() > 0 && GetTileProperties(position + feet + speed))
                position += speed;  // Update player position
            else
                speed = Vector2.Zero;  // Stop movement if blocked or no speed

            // Check if walking or standing and update animation
            if (speed.Length() > 0 || isFighting == true)
            {
                if (currenState == figureState.standing)
                {
                    currenState = figureState.walking;
                    walkSfxInstance.Play();  // Play walking sound if standing
                }

                if (facing != newDirection)
                {
                    if (isFighting == true)
                        facing = facing - 48;  // Adjust facing for combat animation
                    facing = newDirection;  // Update facing direction

                    currentFrame = (int)facing;  // Update current animation frame
                    SetFrame(currentFrame);  // Set animation frame
                    isFighting = false;  // End fighting state
                }
            }
            else
            {
                if (currenState == figureState.walking)
                {
                    walkSfxInstance.Pause();  // Pause walking sound if not moving
                    currenState = figureState.standing;  // Update standing state

                    if (!isWalking)
                        facing = facing - 6;  // Adjust facing if not walking

                    currentFrame = (int)facing;  // Update current animation frame
                    SetFrame(currentFrame);  // Set animation frame
                }
            }

            if (currenState == figureState.walking)
                SetAnimation(gameTime);  // Update animation if walking

            // Update player hitbox position
            playerHitbox = new Rectangle((int)(position.X - middle.X / 3), (int)(position.Y - middle.Y / 3), 19, 38);

            return true;  // Return true to indicate update success
        }
        

        public override void Draw(SpriteBatch spriteBatch) {
            // Draw the player character sprite
            spriteBatch.Draw(gfx_avenger, position - TileEngine.GetCamera(), drawRect, Color.White, angle, middle, scale, SpriteEffects.None, 0.3f);

            // Uncomment below if you want to draw the maxSpeed value for debugging
            // spriteBatch.DrawString(myFont, "" + maxSpeed, new Vector2(50, 50), Color.Black);
        }

        // Inventory management
        private static List<string> inventory = new List<string>();

        // Check if the inventory contains the specified item
        public static bool Contains(string item) {
            return inventory.Contains(item);
        }

        // Add an item to the inventory if it doesn't already exist
        public static void AddItem(string addItem) {
            if (!inventory.Contains(addItem))
                inventory.Add(addItem);
        }

        // Remove an item from the inventory
        public static void RemoveItem(string removeItem) {
            inventory.Remove(removeItem);
        }

    }
}
