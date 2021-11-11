using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld
    {
        private static int GrassID => TileID.GoldBrick;
        private static int MushroomTopID => TileID.BlueDynastyShingles;
        private static int MushroomStemID => TileID.MushroomBlock;

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
            int height = WorldGen.genRand.Next(70, 90);

            GrassLines(spawnPoint, width, height, new Vector2(1.3f, 1.1f), out List<MushroomNode> nodes);

            nodes.Add(new MushroomNode(new Point(nodes.Last().position.X, spawnPoint.Y), Movement.Up, CliffState.None));
            WorldGen.PlaceTile(nodes.Last().position.X, spawnPoint.Y + 1, GrassID); //Cleans up a consistent hole in the 'outline'

            // ADJUST WIDTH AND HEIGHT
            int furthestX = spawnPoint.X;
            int furthestY = spawnPoint.Y;

            foreach (var item in nodes)
            {
                if (item.position.X > furthestX)
                    furthestX = item.position.X;
                if (item.position.Y > furthestY)
                    furthestY = item.position.Y;
            }

            width = furthestX - spawnPoint.X;
            height = (furthestY - spawnPoint.Y) + WorldGen.genRand.Next(3, 7);

            // GEN LINES
            //int paint = WorldGen.genRand.NextBool() ? PaintID.White : PaintID.DeepLime;

            for (int i = 0; i < nodes.Count - 2; ++i)
            {
                Point currentPoint = nodes[i].position;
                Movement currentMove = nodes[i].move;

                bool skipCorner = (i > 0 && nodes[i - 1] .move == Movement.Down) || (currentMove == Movement.Up);
                if (skipCorner && WorldGen.genRand.Next(5) != 0 && i < nodes.Count - 2) //Cut corners to make more convincing "cliffs"
                {
                    GenStraightLine(currentPoint, nodes[i + 2].position, GrassID);
                    ++i;
                    continue;
                }

                GenStraightLine(currentPoint, nodes[i + 1].position, GrassID);
            }

            // GENERATE MUD WALLS

            GenerateMudWalls(spawnPoint, width, height);
        }

        /// <summary>Recursively fills out the given area with mud.</summary>
        /// <param name="spawnPoint">Top-left point of the area.</param>
        /// <param name="width">Width of the area.</param>
        /// <param name="height">Height of the area.</param>
        private static void GenerateMudWalls(Point spawnPoint, int width, int height)
        {
            bool TileAt(int x, int y) => Framing.GetTileSafely(x, y).active();

            void RecursiveFill(int x, int y, (int, int) minMaxX, int repeats)
            {
                if (repeats > 9000 || x < minMaxX.Item1 || x > minMaxX.Item2) //stop these stackoverflows >:(
                    return;

                if (!TileAt(x, y))
                    WorldGen.PlaceTile(x, y, TileID.Mud, true, true, -1, 0);
                else
                    return;

                if (x > spawnPoint.X)
                    RecursiveFill(x - 1, y, minMaxX, repeats + 1);
                if (x < spawnPoint.X + width)
                    RecursiveFill(x + 1, y, minMaxX, repeats + 1);

                if (y > spawnPoint.Y)
                    RecursiveFill(x , y - 1, minMaxX, repeats + 1);
                if (y < spawnPoint.Y + height)
                    RecursiveFill(x, y + 1, minMaxX, repeats + 1);
            }

            const int Offset = 1; //Do I need this? Maybe. Am I paranoid? Yes.
            RecursiveFill(spawnPoint.X + Offset, spawnPoint.Y + Offset, (spawnPoint.X, spawnPoint.X + (int)(width / 2f)), 0);
            RecursiveFill(spawnPoint.X + Offset, spawnPoint.Y + height - Offset, (spawnPoint.X, spawnPoint.X + (int)(width / 2f)), 0);
            RecursiveFill(spawnPoint.X + width - Offset, spawnPoint.Y + Offset, (spawnPoint.X, spawnPoint.X + (int)(width / 2f)), 0);
            RecursiveFill(spawnPoint.X + width - Offset, spawnPoint.Y + height - Offset, (spawnPoint.X, spawnPoint.X + (int)(width / 2f)), 0);
        }

        /// <summary>Places the "outline" of grass tiles.</summary>
        /// <param name="spawnPoint">Top-left of the biome.</param>
        /// <param name="width">Initial width for the biome.</param>
        /// <param name="height">Initial height for the biome.</param>
        /// <param name="endPoints">Resulting list of points.</param>
        /// <param name="movements">Resulting list of movements.</param>
        private static void GrassLines(Point spawnPoint, int width, int height, Vector2 fidelityMultiplier, out List<MushroomNode> nodes)
        {
            int totalRepeats = 80; //idk what I'm doing with this but w/e
            Movement movement = Movement.Down;
            CliffState cliff = CliffState.None;
            float cliffStrength = 1f;

            Vector2 fidelity = new Vector2(width / 6f, height / 10f) * fidelityMultiplier;
            Vector2 currentPoint = spawnPoint.ToVector2() + new Vector2(fidelity.X * 1.4f, 0);

            void MoveInBounds(float xOff, float yOff) => currentPoint = new Vector2(MathHelper.Clamp(currentPoint.X + xOff, 0, Main.maxTilesX),
                  MathHelper.Clamp(currentPoint.Y + yOff, 0, Main.maxTilesY));

            int basinRepeats = WorldGen.genRand.Next(2, 5);

            float HorizontalChange(int dir)
            {
                float strength = cliff == CliffState.JutOut || cliff == CliffState.Return ? cliffStrength : 1f;

                float offset = (fidelity.X * WorldGen.genRand.NextFloat(1.2f, 2f)) + WorldGen.genRand.Next((int)(fidelity.X / 2f));
                if (dir != 1)
                    offset = fidelity.X - WorldGen.genRand.Next((int)(fidelity.X / 4f));
                return offset * dir * strength;
            }

            nodes = new List<MushroomNode>();

            for (int i = 0; i < totalRepeats; ++i)
            {
                void CheckIfBasin()
                {
                    if (basinRepeats > 0 && currentPoint.Y >= spawnPoint.Y + height)
                    {
                        movement = Movement.Basin;

                        if (cliff != CliffState.None)
                            cliff = CliffState.None;
                    }
                }

                MushroomNode node = new MushroomNode(currentPoint.ToPoint(), movement, cliff);
                nodes.Add(node);

                if (currentPoint.Y < spawnPoint.Y)
                    break;

                switch (movement)
                {
                    case Movement.Down:
                    case Movement.DownWall:
                        MoveInBounds(WorldGen.genRand.Next(-2, 3), (fidelity.Y / 2f) + WorldGen.genRand.Next((int)fidelity.Y));

                        if (movement == Movement.DownWall)
                        {
                            CheckIfBasin();

                            if (movement != Movement.Basin && WorldGen.genRand.NextBool(4))
                            {
                                cliff = CliffState.JutOut;
                                cliffStrength = WorldGen.genRand.NextFloat(1.8f, 2.8f);
                            }
                        }
                        else
                        {
                            if (cliff == CliffState.Side)
                                cliff = CliffState.Return;
                        }

                        movement = movement == Movement.Down ? Movement.Left : Movement.Right;
                        break;
                    case Movement.Left:
                        MoveInBounds(HorizontalChange(-1), WorldGen.genRand.Next(-2, 3));
                        movement = basinRepeats <= 0 ? Movement.UpWall : Movement.DownWall;

                        if (cliff == CliffState.Return)
                            cliff = CliffState.None;
                        if (cliff == CliffState.JutOut)
                            cliff = CliffState.Side;

                        CheckIfBasin();
                        break;
                    case Movement.Right:
                        MoveInBounds(HorizontalChange(1), WorldGen.genRand.Next(-2, 3));
                        movement = basinRepeats <= 0 ? Movement.Up : Movement.Down;

                        if (cliff == CliffState.Return)
                            cliff = CliffState.None;
                        if (cliff == CliffState.JutOut)
                            cliff = CliffState.Side;

                        CheckIfBasin();
                        break;
                    case Movement.Up:
                    case Movement.UpWall:
                        MoveInBounds(WorldGen.genRand.Next(-2, 3), (fidelity.Y / -2f) - WorldGen.genRand.Next((int)fidelity.Y));

                        if (movement == Movement.Up && WorldGen.genRand.NextBool(4))
                        {
                            cliff = CliffState.JutOut;
                            cliffStrength = WorldGen.genRand.NextFloat(1.8f, 2.8f);
                        }
                        else if (movement == Movement.UpWall && cliff == CliffState.Side)
                            cliff = CliffState.Return;

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

        public static void GenStraightLine(Point start, Point end, int tileType, int paintID = -1)
        {
            Vector2 position = start.ToVector2() + new Vector2(0.5f);
            float repeats = Vector2.Distance(position, end.ToVector2());

            for (float i = 0; i < repeats; i += 0.5f)
            {
                position = Vector2.Lerp(start.ToVector2(), end.ToVector2(), i / repeats);
                WorldGen.KillTile((int)position.X, (int)position.Y, false, false, true);
                WorldGen.PlaceTile((int)position.X, (int)position.Y, tileType, true);

                if (paintID != -1)
                    WorldGen.paintTile((int)position.X, (int)position.Y, (byte)paintID, false);
            }
        }

        public static void GenSpline(Vector2 start, Vector2 middle, Vector2 end)
        {
            for (float i = 0; i < 8; i += 0.00675f)
            {
                Point currentPos = PointOnSpline(start, middle, end, i, 1f).ToTileCoordinates();
                WorldGen.PlaceTile(currentPos.X, currentPos.Y, GrassID);
            }
        }

        public static Vector2 PointOnSpline(Vector2 start, Vector2 middle, Vector2 end, float progress, float factor)
        {
            if (progress < factor)
                return Vector2.Hermite(start, middle - start, middle, end - start, progress * (1 / factor));
            else
                return Vector2.Hermite(middle, end - start, end, end - middle, (progress - factor) * (1 / (1 - factor)));
        }

        public static float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
        {
            float total = 0;
            Vector2 prevPoint = start;

            for (int k = 0; k < steps; k++)
            {
                Vector2 testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
                total += Vector2.Distance(prevPoint, testPoint);

                prevPoint = testPoint;
            }

            return total;
        }

        public static void GenMassiveMushroom()
        {

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

        public enum CliffState : int
        {
            None, //No cliff
            JutOut, //Pushing out towards the centre
            Side, //The side of the cliff
            Return //Returning to out away from the centre
        }

        public struct MushroomNode
        {
            internal Point position;
            internal Movement move;
            internal CliffState cliff;

            public MushroomNode(Point p, Movement m, CliffState c)
            {
                position = p;
                move = m;
                cliff = c;
            }
        }
    }
}
