using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Emit;
using System.Xml;
using System.Collections.Generic;

namespace AdventureGame
{
    static class TileEngine
    {
        private static Texture2D tileMap;                       // Texture that contains all tiles
        private static Vector2 cameraPosition;                  // Camera position in the world
        private static int mapTileWide;                         // Number of tiles per row in the texture
        private static Rectangle sourceRect;                    // Rectangle defining the portion of tilemap to draw
        private static int screenRows, screenCols;              // Number of rows and columns that fit on the screen

        static int tileWidth, tileHeight;                       // Size of a single tile
        static int worldWidth, worldHeight;                     // Size of the world in number of tiles
        static int firstgid;                                    // First global ID of the tileset

        static ushort[,] worldData;                             // Tile data for the visible world
        static ushort[,] onTopData;                             // Tiles that are on top of the main world
        static ushort[,] walkBehindData;                        // Tiles that can be walked behind
        static byte[,] tileInfo;                                // Collision and terrain information for tiles
        static string layer;                                    // Layer of the map being loaded

        static ushort[] animData = new ushort[667];             // Animation data for tiles
        static int animTimer = 0;                               // Timer for animation swapping
        static int timerSwap = 250;                             // Interval after which to swap animations
        static bool timeToSwap = false;                         // Flag to indicate if it's time to swap animations


        public static void LoadLevel(ContentManager content, string mapFile, string tilemap, Vector2 startPosition) {
            tileMap = content.Load<Texture2D>(tilemap);  // Load our tilemap texture.
            cameraPosition = startPosition;              // Set the initial camera position.
            sourceRect = new Rectangle(0, 0, tileWidth, tileHeight);  // Define the initial source rectangle for drawing tiles.
            ReadTmx(mapFile);                            // Read the TMX file to initialize world data.
            mapTileWide = tileMap.Width / tileWidth;     // Calculate the number of tiles per row in the tilemap texture.
            screenRows = Game1.ScreenHeight() / tileHeight;   // Calculate the number of screen rows based on screen size and tile height.
            screenCols = Game1.ScreenWidth() / tileWidth;     // Calculate the number of screen columns based on screen size and tile width.

            // Initialize animation data for the world tiles.
            for (int i = 0; i < 667; i++)
            {
                animData[i] = (ushort)i;
            }

            // Set up animation sequences for specific tiles in the world data.
            ushort q = 368;
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    animData[q] = (ushort)(q + 10);
                    q++;
                }
                q += 14;
            }

            q = 378;
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    animData[q] = (ushort)(q - 10);
                    q++;
                }
                q += 14;
            }
        }

        // Reads the TMX file and extracts necessary map data.
        private static void ReadTmx(string file)
        {
            using (XmlReader xmlReader = XmlReader.Create(@file))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.IsStartElement())
                    {
                        string name = xmlReader.Name;

                        if (xmlReader.HasAttributes)
                        {
                            while (xmlReader.MoveToNextAttribute())  // Loop through all attributes.
                            {
                                // Attributes are processed in other sections based on their names.
                            }
                            xmlReader.MoveToElement(); // Back to element level
                        }

                        if (name == "map")
                        {
                            while (xmlReader.MoveToNextAttribute())
                            { 
                                if (xmlReader.Name.Contains("tilewidth"))
                                {
                                    tileWidth = int.Parse(xmlReader.Value);
                                }
                                if (xmlReader.Name.Contains("tileheight"))
                                {
                                    tileHeight = int.Parse(xmlReader.Value);
                                }
                                if (xmlReader.Name == "width")
                                {
                                    worldWidth = int.Parse(xmlReader.Value);
                                }
                                if (xmlReader.Name == "height")
                                {
                                    worldHeight = int.Parse(xmlReader.Value);
                                }
                            }
                            sourceRect = new Rectangle(0, 0, tileWidth, tileHeight);
                        }

                        if (name == "layer")
                        {
                            while (xmlReader.MoveToNextAttribute())
                            {
                                if (xmlReader.Name == "id")
                                {
                                    layer = xmlReader.Value;
                                }
                            }
                        }

                        if (name == "data")
                        {
                            string[] content = xmlReader.ReadElementContentAsString().Split(",");


                            if (layer == "3")
                            {
                                int index = 0;
                                tileInfo = new byte[worldWidth, worldHeight];
                                firstgid = 668;
                                for (int i = 0; i < worldHeight; i++)
                                {
                                    for (int j = 0; j < worldWidth; j++)
                                    {
                                        content[index] = content[index].Replace("\n", "");
                                        tileInfo[j, i] = Convert.ToByte(ushort.Parse((content[index])) - firstgid);
                                        index++;
                                    }
                                }
                            }

                            if (layer == "2")
                            {
                                int index = 0;
                                onTopData = new ushort[worldWidth, worldHeight];
                                for (int i = 0; i < worldHeight; i++)
                                {
                                    for (int j = 0; j < worldWidth; j++)
                                    {
                                        content[index] = content[index].Replace("\n", "");
                                        onTopData[j, i] = ushort.Parse((content[index]));
                                        index++;
                                    }
                                }
                            }

                            if (layer == "1")
                            {
                                int index = 0;
                                worldData = new ushort[worldWidth, worldHeight];
                                for (int i = 0; i < worldHeight; i++)
                                {
                                    for (int j = 0; j < worldWidth; j++)
                                    {
                                        content[index] = content[index].Replace("\n", "");
                                        worldData[j, i] = ushort.Parse((content[index]));
                                        index++;
                                    }
                                }
                            }

                            if (layer == "4")
                            {
                                int index = 0;
                                walkBehindData = new ushort[worldWidth, worldHeight];
                                for (int i = 0; i < worldHeight; i++)
                                {
                                    for (int j = 0; j < worldWidth; j++)
                                    {
                                        content[index] = content[index].Replace("\n", "");
                                        walkBehindData[j, i] = ushort.Parse((content[index]));
                                        index++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateCamera(Vector2 centerPos)
        {
            cameraPosition.X = centerPos.X - Game1.ScreenWidth() / 2;
            cameraPosition.Y = centerPos.Y - Game1.ScreenHeight() / 2;

            //Kontrollerar så att kameran inte hamnar utanför världen.
            

            if (cameraPosition.X > tileWidth * (worldWidth - screenCols))
                cameraPosition.X = tileWidth * (worldWidth - screenCols);

            if (cameraPosition.Y > tileHeight * (worldHeight - screenRows))
                cameraPosition.Y = tileHeight * (worldHeight - screenRows);

            if (cameraPosition.X < 0)
                cameraPosition.X = 0;

            if (cameraPosition.Y < 0)
                cameraPosition.Y = 0;
        }

        public static Vector2 GetCamera() => cameraPosition;

        public static int GetTileType(Vector2 position)
        {
            int x = (int)position.X / tileWidth;
            int y = (int)position.Y / tileHeight;

            // Skickar tillbaka -1 som ett tecken på att man är utanför världen.
            if (x <= 0 || x >= worldWidth || y <= 0 || y >= worldHeight)
            {
                return -1;
            }

            return tileInfo[x, y];
        }

        public static int GetRow(Vector2 position) => (int)position.Y / tileHeight;

        public static int GetColumn(Vector2 position) => (int)position.X / tileWidth;

        public static void ChangeTileInfo(int x, int y, byte n)
        {
            if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight)
                tileInfo[x, y] = n;
        }

        public static void ChangeTileWorld(int x, int y, ushort n)
        {
            if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight)
                worldData[x, y] = n;
        }

        public static void ChangeTileOnTop(int x, int y, ushort n)
        {
            if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight)
                onTopData[x, y] = n;
        }

        public static void UpdateAnimation(GameTime gameTime)
        {
            animTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animTimer>timerSwap)
            {
                animTimer = 0;
                timeToSwap = true;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            //Räkna vilken rad och kolumn som är längst upp till vänster på skärmen.
            int startX = (int)(cameraPosition.X / tileWidth);
            int startY = (int)(cameraPosition.Y / tileHeight);

            //Räkna ut vilken rad och kolumn som är längst ner till höger på skärmen.
            int endX = startX + screenCols;
            int endY = startY + screenRows;

            Vector2 position = Vector2.Zero; //Positionen för den tile som ska ritas ut.

            for (int y = startY; y <= endY && y < worldData.GetLength(1); y++)
            {
                for (int x = startX; x <=endX && x < worldData.GetLength(0) ; x++)
                {
                    position.X = (int)(x * tileWidth - cameraPosition.X);
                    position.Y = (int)(y * tileHeight - cameraPosition.Y);

                    int index = worldData[x, y] -1;     //Minus ett för att talen i filen böejar på 0 = tom.
                    ushort animIndex = (ushort)index;
                    if (timeToSwap)
                    {
                        animIndex = animData[index];
                        worldData[x, y] = (ushort)(animIndex + 1);
                    }

                    if (animIndex >= 0)
                    {
                        sourceRect.X = animIndex % mapTileWide * tileWidth;
                        sourceRect.Y = animIndex / mapTileWide * tileHeight;
                        spriteBatch.Draw(tileMap, position, sourceRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
                    }

                    index = onTopData[x, y] - 1;     //Minus ett för att talen i filen böejar på 0 = tom.
                    if (index >= 0)
                    {
                        sourceRect.X = index % mapTileWide * tileWidth;
                        sourceRect.Y = index / mapTileWide * tileHeight;
                        spriteBatch.Draw(tileMap, position, sourceRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    }

                    index = walkBehindData[x, y] - 1;     //Minus ett för att talen i filen böejar på 0 som betyder att den är tom.
                    if (index >= 0)
                    {
                        sourceRect.X = index % mapTileWide * tileWidth;
                        sourceRect.Y = index / mapTileWide * tileHeight;
                        spriteBatch.Draw(tileMap, position, sourceRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.2f);
                    }
                }
            }
            timeToSwap = false;
        }
    }
}