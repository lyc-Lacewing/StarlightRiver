using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
    {
        public static void GlowingMushrooms(GenerationProgress progress)
        {
            progress.CurrentPassWeight = 0.2f;
            progress.Message = "Growing glowing mushrooms...";

            for (int i = 0; i < 3; ++i)
            {
                Point p = new Point(WorldGen.genRand.Next(100, Main.maxTilesX - 200), WorldGen.genRand.Next((int)Main.worldSurface, Main.maxTilesY - 300));
                SpawnBouncyMushroom(p);
            }
        }

        public static void SpawnBouncyMushroom(Point spawnPoint)
        {
            int width = WorldGen.genRand.Next(50, 70);
            int height = WorldGen.genRand.Next(80, 100);

            GrassLines(spawnPoint, width, height, out List<Point> endPoints, out List<Movement> movements);

            // ADJUST WIDTH AND HEIGHT
            int furthestX = spawnPoint.X;
            int furthestY = spawnPoint.Y;

            foreach (var item in endPoints)
            {
                if (item.X > furthestX)
                    furthestX = item.X;
                if (item.Y > furthestY)
                    furthestY = item.Y;
            }

            width = furthestX - spawnPoint.X;
            height = furthestY - spawnPoint.Y;

            // GEN LINES
            //int paint = WorldGen.genRand.NextBool() ? PaintID.White : PaintID.DeepLime;

            for (int i = 0; i < endPoints.Count - 2; ++i)
            {
                Point currentPoint = endPoints[i];
                Movement currentMove = movements[i];

                bool skipCorner = (i > 0 && movements[i - 1] == Movement.Down) || (currentMove == Movement.Up);
                if (skipCorner && WorldGen.genRand.Next(5) != 0 && i < endPoints.Count - 2) //Cut corners to make more convincing "cliffs"
                {
                    GenLine(currentPoint, endPoints[i + 2], TileID.MushroomGrass);
                    ++i;
                    continue;
                }

                GenLine(currentPoint, endPoints[i + 1], TileID.MushroomGrass);
            }

            // ADD BOUNCY MUSHROOMS [WIP]
            //int bigMushCount = WorldGen.genRand.Next(5, 9);

            //for (int i = 0; i < bigMushCount; ++i)
            //{
            //    while (true)
            //    {
            //        int x = WorldGen.genRand.Next(spawnPoint.X, spawnPoint.X + width);
            //        int y = WorldGen.genRand.Next(spawnPoint.Y, spawnPoint.Y + height);

            //        if (WorldGen.PlaceObject(x, y, Terraria.ModLoader.ModContent.TileType<Tiles.Mushroom.JellyShroom>()))
            //            break;
            //    }
            //}
        }

        /// <summary>Places the "outline" of grass tiles.</summary>
        /// <param name="spawnPoint">Top-left of the biome.</param>
        /// <param name="width">Initial width for the biome.</param>
        /// <param name="height">Initial height for the biome.</param>
        /// <param name="endPoints">Resulting list of points.</param>
        /// <param name="movements">Resulting list of movements.</param>
        private static void GrassLines(Point spawnPoint, int width, int height, out List<Point> endPoints, out List<Movement> movements)
        {
            int totalRepeats = 80; //idk what I'm doing with this but w/e
            Movement movement = Movement.Down;

            Vector2 fidelity = new Vector2(width / 6f, height / 10f);
            Vector2 currentPoint = spawnPoint.ToVector2() + new Vector2(fidelity.X, 0);

            void MoveInBounds(float xOff, float yOff) => currentPoint = new Vector2(MathHelper.Clamp(currentPoint.X + xOff, 0, Main.maxTilesX),
                  MathHelper.Clamp(currentPoint.Y + yOff, 0, Main.maxTilesY));

            int basinRepeats = WorldGen.genRand.Next(2, 5);

            float HorizontalChange(int dir)
            {
                return (dir == 1 ? (fidelity.X * WorldGen.genRand.NextFloat(1.2f, 2f)) + WorldGen.genRand.Next((int)(fidelity.X / 2f))
                    : fidelity.X - WorldGen.genRand.Next((int)(fidelity.X / 4f))) * dir;
            }

            endPoints = new List<Point>();
            movements = new List<Movement>();

            for (int i = 0; i < totalRepeats; ++i)
            {
                void CheckIfBasin()
                {
                    if (basinRepeats > 0 && currentPoint.Y >= spawnPoint.Y + height)
                        movement = Movement.Basin;
                }

                endPoints.Add(currentPoint.ToPoint());
                movements.Add(movement);

                if (currentPoint.Y < spawnPoint.Y)
                    break;

                switch (movement)
                {
                    case Movement.Down:
                    case Movement.DownWall:
                        MoveInBounds(WorldGen.genRand.Next(-2, 3), (fidelity.Y / 2f) + WorldGen.genRand.Next((int)fidelity.Y));
                        movement = movement == Movement.Down ? Movement.Left : Movement.Right;

                        if (movement == Movement.DownWall)
                            CheckIfBasin();
                        break;
                    case Movement.Left:
                        MoveInBounds(HorizontalChange(-1), WorldGen.genRand.Next(-2, 3));
                        movement = basinRepeats <= 0 ? Movement.UpWall : Movement.DownWall;

                        CheckIfBasin();
                        break;
                    case Movement.Right:
                        MoveInBounds(HorizontalChange(1), WorldGen.genRand.Next(-2, 3));
                        movement = basinRepeats <= 0 ? Movement.Up : Movement.Down;

                        CheckIfBasin();
                        break;
                    case Movement.Up:
                    case Movement.UpWall:
                        MoveInBounds(WorldGen.genRand.Next(-2, 3), (fidelity.Y / -2f) - WorldGen.genRand.Next((int)fidelity.Y));
                        movement = movement == Movement.Up ? Movement.Left : Movement.Right;

                        CheckIfBasin();
                        break;
                    case Movement.Basin:
                        MoveInBounds(WorldGen.genRand.Next((int)(fidelity.X * 1.25f)), WorldGen.genRand.Next(-5, 6));

                        if (basinRepeats-- <= 0)
                            movement = Movement.UpWall;
                        break;
                    default: //???? why are you here
                        throw new Exception("Literally how did you get this error?");
                }
            }
        }

        public static void GenLine(Point start, Point end, int tileType, int paintID = -1)
        {
            Vector2 position = start.ToVector2();
            float repeats = Vector2.Distance(position, end.ToVector2());

            for (float i = 0; i < repeats; i += 1f)
            {
                position = Vector2.Lerp(start.ToVector2(), end.ToVector2(), i / repeats);
                WorldGen.PlaceTile((int)position.X, (int)position.Y, tileType, true, false, -1, 0);

                if (paintID != -1)
                    WorldGen.paintTile((int)position.X, (int)position.Y, (byte)paintID, false);
            }
        }

        public enum Movement : int
        {
            Down = 0, //Going down
            DownWall, //Going down, go out next
            Left, //Going left
            Right, //Going right
            Up, //Going up
            UpWall, //Going up, go in next
            Basin, //Flat area at the bottom
        }
    }
}
