using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using static System.Formats.Asn1.AsnWriter;

namespace AdventureGame
{
    public class Menu
    {
        SpriteFont myFont;  // Font used for drawing text
        public Vector2 middle = new Vector2();  // Position vector for middle point (not currently used)
        public Vector2 startposition = new Vector2();  // Position vector for start option
        public Vector2 exitposition = new Vector2();  // Position vector for exit option
        public Vector2 controlposition = new Vector2();  // Position vector for controls information
        public Rectangle drawRect = new Rectangle(0, 0, 500, 250);  // Rectangle defining the area to draw the menu
        public Vector2 startend = new Vector2();  // End position for start option (not currently used)
        public Vector2 exitend = new Vector2();  // End position for exit option (not currently used)
        public static bool start;  // Flag indicating if the game should start
        public static bool end;  // Flag indicating if the game is over

        public Menu()
        {
            start = false;  // Initialize start flag to false
            end = false;  // Initialize end flag to false
        }

        // Load content method for loading sprite font
        public virtual void LoadContent(ContentManager content)
        {
            myFont = content.Load<SpriteFont>("TextFont");  // Load the sprite font for text rendering
        }

        // Update method for menu interaction
        public virtual void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();  // Get current keyboard state

            if (!start)  // If game has not started
            {
                if (keyboardState.IsKeyDown(Keys.Enter))  // Check if Enter key is pressed
                    start = true;  // Set start flag to true to begin the game
            }
        }

        // Draw method for rendering menu components
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!start && !end)  // If game has not started and not ended
            {
                // Render introductory message and instructions
                string message = "Your mission is to find the key which\n is lost somewhere in the map and do\n not forget to capture the ghosts which escaped...\n Good luck warrior";
                spriteBatch.DrawString(myFont, message, new Vector2(30, 30), Color.BlanchedAlmond, 0, new Vector2(0, 0), 0.8f, 0, 0);
                spriteBatch.DrawString(myFont, "Ghost Buster", new Vector2(350, 350), Color.BlanchedAlmond, 0, new Vector2(0, 0), 2f, 0, 0);
                spriteBatch.DrawString(myFont, "Press ENTER to START", new Vector2(430, 450), Color.BlanchedAlmond, 0, new Vector2(0, 0), 0.6f, 0, 0);
                spriteBatch.DrawString(myFont, "Controls: W,A,S,D to walk + Shift to run / + Space to catch \n\nNote! You're unable to run and catch ghosts simultaneously!", new Vector2(5, 700), Color.BlanchedAlmond, 0, new Vector2(0, 0), 0.6f, 0, 0);
            }
            if (end)  // If game has ended
            {
                start = false;  // Reset start flag to false
                spriteBatch.DrawString(myFont, "Game Over", new Vector2(350, 350), Color.BlanchedAlmond, 0, new Vector2(0, 0), 2f, 0, 0);
            }
        }
    }
}
