using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace AdventureGame
{
    internal class LevelKey
    {
        static int level = 1;  // Current level of the game
        public static bool keyboardInputEnabled = true;  // Flag indicating if keyboard input is enabled
        static SoundEffect sfx_CastleDoor;  // Sound effect for the castle door
        static SoundEffect sfx_Item;  // Sound effect for picking up an item
        public static bool doorIsLocked = false;  // Flag indicating if the door is locked

        private static List<string> happenings = new List<string>();  // List to store game events (not currently used)

        // Method to load sounds from content manager
        public static void LoadSounds(ContentManager content)
        {
            // Load sound effects from content manager
            sfx_CastleDoor = content.Load<SoundEffect>("closeAndLock");  // Load sound for castle door closing and locking
            sfx_Item = content.Load<SoundEffect>("PickItem");  // Load sound for picking up an item
        }

        // Method to handle tile-based events
        public static bool TileEvent(Vector2 position)
        {
            int x, y;
            y = TileEngine.GetRow(position);  // Get row index from tile engine based on position
            x = TileEngine.GetColumn(position);  // Get column index from tile engine based on position

            if (level == 1)
            {
                // Check specific tile coordinates for events in level 1
                if (x == 25 && y == 8) // Castle door
                {
                    if (Player.Contains("KeyCastle") == true)  // If the player has the castle key
                    {
                        sfx_CastleDoor.Play();  // Play castle door sound effect
                        keyboardInputEnabled = false;  // Disable keyboard input temporarily
                        TileEngine.ChangeTileWorld(x, y, 146);  // Change tile in the game world
                        TileEngine.ChangeTileInfo(x, y, 4);  // Change tile information
                        Player.RemoveItem("KeyCastle");  // Remove the castle key from player's inventory
                        doorIsLocked = true;
                        return false;  // Event handled, return false
                    }
                    else
                    {
                        return false;  // Player does not have the key, event not handled
                    }
                }
                if (x == 43 && y == 39)  // Key pickup location
                {
                    if (Player.Contains("KeyCastle") == false)  // If the player does not have the castle key
                    {
                        sfx_Item.Play();  // Play item pickup sound effect
                        TileEngine.ChangeTileInfo(x, y, 0);  // Change tile information
                        TileEngine.ChangeTileOnTop(x, y, 0);  // Change top tile information
                        Player.AddItem("KeyCastle");  // Add the castle key to player's inventory
                    }
                    return true;  // Event handled, return true
                }
            }
            return false;  // No event handled for the current position
        }
    }
}
