using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AdventureGame
{
    // The entire class is abstract.
    // This means no objects of this class can be created directly.
    internal abstract class MovingObject
    {
        // Protected allows access to these variables/methods within this class
        // and in classes that inherit from it (like 'Star' and 'Spaceship').
        protected static Random random = new Random();  // Random number generator shared among all instances
        protected Texture2D gfx_enemy_lvl01;  // Texture for enemies (level 1)
        protected Texture2D gfx_avenger;  // Texture for the player's avatar (avenger)
        protected Vector2 position;  // Current position of the object
        protected Vector2 speed;  // Movement speed of the object
        protected Vector2 middle;  // Center point for rotation and scaling
        protected Vector2 middle_enemy1;  // Center point for enemies (not currently used)
        protected Color primaryColor;  // Color tint applied to the object
        protected Rectangle drawRect;  // Rectangle defining the drawing area of the object
        protected float scale;  // Scaling factor for the object's size
        protected float angle;  // Angle to rotate the texture
        protected float alpha;  // Transparency level (0 = fully transparent, 1 = opaque)

        // Constructor for initializing a MovingObject instance
        public MovingObject(Vector2 position, Color primaryColor, float scale = 1, float angle = 0, float alpha = 1)
        {
            this.position = position;  // Initialize position
            this.primaryColor = primaryColor;  // Initialize primary color/tint
            this.scale = scale;  // Initialize scale
            this.angle = angle;  // Initialize rotation angle
            this.alpha = alpha;  // Initialize transparency
        }

        // Abstract method that must be implemented by subclasses.
        // This method is intended to update the object's state each game frame.
        public abstract bool Update(GameTime gameTime);

        // Abstract method that must be implemented by subclasses.
        // This method is used to load content such as textures.
        public abstract void LoadContent(ContentManager content);

        // Virtual method that can be overridden by subclasses.
        // This method draws the object on the screen using provided SpriteBatch.
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // Draw the object's texture with specified parameters
            spriteBatch.Draw(gfx_avenger, position - TileEngine.GetCamera(), drawRect, primaryColor * alpha, angle, middle, scale, SpriteEffects.None, 0);
        }

        // Virtual method that returns the current position of the object.
        // It can be useful for subclasses to access the position without direct access.
        public virtual Vector2 GetPosition() => position;
    }
}
