﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
	public abstract class FurnitureLoader : IOrderedLoadable
    {
        private readonly string name = "Nameless";
        private readonly string path = "StarlightRiver/Tiles/Placeholders/";

        private readonly Color color = Color.White;
        private readonly Color glowColor = Color.Blue;
        private readonly int dust = DustID.Dirt;
        private readonly int material = ItemID.DirtBlock;

        public FurnitureLoader(string name, string path, Color color, Color glowColor, int dust, int material = ItemID.None)
        {
            this.name = name;
            this.path = path;
            this.color = color;
            this.glowColor = glowColor;
            this.dust = dust;
            this.material = material;
        }

        public float Priority { get => 1f; }

        public void Load()
        {
            var Mod = StarlightRiver.Instance;

            Add("Bathtub", new GenericBathtub(color, dust, name + "Bathtub"), Mod, 14);
            Add("Bed", new GenericBed(color, dust, name + "Bed"), Mod, 15);
            Add("Bookcase", new GenericBookcase(color, dust, name + "Bookcase"), Mod, 20);
            Add("Candelabra", new GenericCandelabra(glowColor, dust, name + "Candelabra"), Mod, 5);
            Add("Candle", new GenericCandle(glowColor, dust, name + "Candle"), Mod, 4);
            Add("Chair", new GenericChair(color, dust, name + "Chair"), Mod, 4);
            Add("Chandelier", new GenericChandelier(glowColor, dust, name + "Chandelier"), Mod, 4);
            Add("Clock", new GenericClock(color, dust, name + "Clock"), Mod, 10);
            Add("Dresser", new GenericDresser(color, dust, name + "Dresser"), Mod, 16);
            Add("Lamp", new GenericLamp(glowColor, dust, name + "Lamp"), Mod, 3);
            Add("Lantern", new GenericLantern(glowColor, dust, name + "Lantern"), Mod, 6);
            Add("Piano", new Generic3x2(color, dust, name + "Piano"), Mod, 15);
            Add("Sink", new GenericSink(color, dust, name + "Sink"), Mod, 6);
            Add("Sofa", new Generic3x2(color, dust, name + "Sofa"), Mod, 5);
            Add("Table", new GenericSolidWithTop(color, dust, name + "Table"), Mod, 8);
            Add("Workbench", new GenericWorkbench(color, dust, name + "Workbench"), Mod, 10);

            //special stuff for the door
            Mod.AddTile(name + "DoorClosed", new GenericDoorClosed(color, dust, name + "DoorClosed"), path + name + "DoorClosed");
            Mod.AddTile(name + "DoorOpen", new GenericDoorOpen(color, dust, name + "DoorOpen"), path + name + "DoorOpen");
            Mod.AddItem(name + "Door", new GenericFurnitureItem(name + " " + "DoorClosed", path + name + "DoorItem", 6, material));
        }

        public void Unload() { }

        private void Add(string typename, ModTile tile, Mod Mod, int craftingQuantity)
        {
            Mod.AddTile(name + typename, tile, path + name + typename);
            Mod.AddItem(name + typename, new GenericFurnitureItem(name + " " + typename, path + name + typename + "Item", craftingQuantity, material));
        }
    }

    class GenericFurnitureItem : QuickTileItem
    {
        private readonly string name;
        private readonly int craftingQuantity;
        private readonly int craftingMaterial;
        private readonly string texture;

        public GenericFurnitureItem(string name, string texture, int craftingQuantity, int craftingMaterial) : base(name.Replace("Closed", ""), "", StarlightRiver.Instance.TileType(name.Replace(" ", "")), 0)
        {
            this.name = name;
            this.craftingQuantity = craftingQuantity;
            this.craftingMaterial = craftingMaterial;
            this.texture = texture;
        }

        public override bool CloneNewInstances => true;

        public override string Texture => texture;

        public override void SafeSetDefaults()
        {
            Item.maxStack = 99;
            Item.value = 30;
        }

        public override void AddRecipes()
        {
            if (craftingMaterial != ItemID.None)
            {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(craftingMaterial, craftingQuantity);

                if (name.Contains("Candle") || name.Contains("Lamp") || name.Contains("Lantern") || name.Contains("Candelabra"))
                    recipe.AddIngredient(ItemID.Torch);

                if (name.Contains("Candelabra"))
                    recipe.AddIngredient(ItemID.Torch, 3);

                if (name.Contains("Chandelier"))
                {
                    recipe.AddIngredient(ItemID.Torch, 4);
                    recipe.AddIngredient(ItemID.Chain);
                }

                if (name.Contains("Bed"))
                    recipe.AddIngredient(ItemID.Silk, 5);

                if (name.Contains("Bookcase"))
                    recipe.AddIngredient(ItemID.Book, 10);

                if (name.Contains("Clock"))
                {
                    recipe.AddRecipeGroup(RecipeGroupID.IronBar, 3);
                    recipe.AddIngredient(ItemID.Glass, 6);
                }

                if (name.Contains("Piano"))
                {
                    recipe.AddIngredient(ItemID.Bone, 4);
                    recipe.AddIngredient(ItemID.Book);
                }

                if (name.Contains("Sink"))
                    recipe.AddIngredient(ItemID.WaterBucket);

                if (name.Contains("Sofa"))
                    recipe.AddIngredient(ItemID.Silk, 2);


                recipe.AddTile(TileID.WorkBenches);
            }
        }
    }

    abstract class Furniture : ModTile
    {
        protected readonly Color color;
        protected readonly int dust;
        protected readonly string name;

        public Furniture(Color color, int dust, string name)
        {
            this.color = color;
            this.dust = dust;
            this.name = name;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, Mod.ItemType(name));
    }

    //Bathtub
    class GenericBathtub : Furniture
    {
        public GenericBathtub(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 4, 0);
            QuickBlock.QuickSetFurniture(this, 4, 2, dust, SoundID.Dig, false, color);
            adjTiles = new int[] { TileID.Bathtubs };
        }
    }

    //bed
    class GenericBed : Furniture
    {
        public GenericBed(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 4, 0);
            QuickBlock.QuickSetFurniture(this, 4, 2, dust, SoundID.Dig, false, color);
            TileID.Sets.HasOutlines[Type] = true;
            
            adjTiles = new int[] { TileID.Beds };
            bed = true;
        }

        public override bool HasSmartInteract() => true;

        public override bool RightClick(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int spawnX = i - tile.TileFrameX / 18;
            int spawnY = j + 2;
            spawnX += tile.TileFrameX >= 72 ? 5 : 2;
            if (tile.TileFrameY % 38 != 0)
            {
                spawnY--;
            }
            Player.FindSpawn();
            if (Player.SpawnX == spawnX && Player.SpawnY == spawnY)
            {
                Player.RemoveSpawn();
                Main.NewText("Spawn point removed!", 255, 240, 20, false);
            }
            else if (Player.CheckSpawn(spawnX, spawnY))
            {
                Player.ChangeSpawn(spawnX, spawnY);
                Main.NewText("Spawn point set!", 255, 240, 20, false);
            }
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            Player.cursorItemIconID = Mod.ItemType(this.name);
        }
    }

    //bookcase
    class GenericBookcase : Furniture
    {
        public GenericBookcase(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            TileObjectData.newTile.Origin = new Point16(0, 4);
            QuickBlock.QuickSetFurniture(this, 3, 4, dust, SoundID.Dig, false, color, true);
            adjTiles = new int[] { TileID.Bookcases };
        }
    }

    //Candelabra
    class GenericCandelabra : Furniture
    {
        public GenericCandelabra(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 2, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Candelabras };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            int newX = i;
            int newY = j;
            if (tile.TileFrameX % 36 == 18) 
                newX = i - 1;
            if (tile.TileFrameY % 36 == 18) 
                newY = j - 1;

            for (int k = 0; k < 2; k++)
                for (int l = 0; l < 2; ++l)
                {
                    Main.tile[newX + k, newY + l].TileFrameX += (short)(Main.tile[newX + k, newY + l].TileFrameX >= 36 ? -36 : 36);
                    Wiring.SkipWire(newX + k, newY + l);
                }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 36) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Candle
    class GenericCandle : Furniture
    {
        public GenericCandle(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 1, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Candles };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            tile.TileFrameX += (short)(tile.TileFrameX >= 18 ? -18 : 18);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 18) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Chair
    class GenericChair : Furniture
    {
        public GenericChair(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 2, dust, SoundID.Dig, false, color);

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
            adjTiles = new int[] { TileID.Chairs };
        }
    }

    //Chandelier
    class GenericChandelier : Furniture
    {
        public GenericChandelier(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 3, dust, SoundID.Dig, false, color, Origin: new Point16(1, 1));
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Chandeliers };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j); //Initial tile

            int newX = i - tile.TileFrameX % 54 / 18; //Adjustments
            int newY = j - tile.TileFrameY % 54 / 18;

            tile = Framing.GetTileSafely(newX, newY); //Top-left tile

            for (int k = 0; k < 3; k++) //Changes frames properly
            {
                for (int l = 0; l < 3; ++l)
                {
                    Main.tile[newX + k, newY + l].TileFrameX += (short)(Main.tile[newX + k, newY + l].TileFrameX >= 54 ? -54 : 54);
                    Wiring.SkipWire(newX + k, newY + l);
                }
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 54) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Clock
    class GenericClock : Furniture
    {
        public GenericClock(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            TileObjectData.newTile.Origin = new Point16(0, 5);
            QuickBlock.QuickSetFurniture(this, 2, 5, dust, SoundID.Dig, false, color);
            TileID.Sets.HasOutlines[Type] = true;
            
            adjTiles = new int[] { TileID.GrandfatherClocks };
        }

        public override bool HasSmartInteract() => true;

        public override bool RightClick(int x, int y)
        {
            string text = "AM";
            //Get current weird time
            double time = Main.time;
            if (!Main.dayTime)
            {
                //if it's night add this number
                time += 54000.0;
            }
            //Divide by seconds in a day * 24
            time = time / 86400.0 * 24.0;
            //Dunno why we're taking 19.5. Something about hour formatting
            time = time - 7.5 - 12.0;
            //Format in readable time
            if (time < 0.0)
            {
                time += 24.0;
            }
            if (time >= 12.0)
            {
                text = "PM";
            }
            int intTime = (int)time;
            //Get the decimal points of time.
            double deltaTime = time - intTime;
            //multiply them by 60. Minutes, probably
            deltaTime = (int)(deltaTime * 60.0);
            //This could easily be replaced by deltaTime.ToString()
            string text2 = string.Concat(deltaTime);
            if (deltaTime < 10.0)
            {
                //if deltaTime is eg "1" (which would cause time to display as HH:M instead of HH:MM)
                text2 = "0" + text2;
            }
            if (intTime > 12)
            {
                //This is for AM/PM time rather than 24hour time
                intTime -= 12;
            }
            if (intTime == 0)
            {
                //0AM = 12AM
                intTime = 12;
            }
            //Whack it all together to get a HH:MM format
            var newText = string.Concat("Time: ", intTime, ":", text2, " ", text);
            Main.NewText(newText, 255, 240, 20);
            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
                Main.clock = true;
        }

        public override void MouseOver(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            Player.cursorItemIconID = Mod.ItemType(this.name);
        }
    }

    //Door
    class GenericDoorClosed : Furniture
    {
        public GenericDoorClosed(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 1);
            TileObjectData.addAlternate(0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 2);
            TileObjectData.addAlternate(0);

            QuickBlock.QuickSetFurniture(this, 1, 3, dust, SoundID.Dig, false, color, Origin: new Point16(0, 0));

            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.NotReallySolid[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

            
            adjTiles = new int[] { TileID.ClosedDoor };
            openDoorID = Mod.TileType(name.Replace("Closed", "Open"));
        }

        public override bool HasSmartInteract() => true;

        public override void MouseOver(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            Player.cursorItemIconID = Mod.ItemType(name.Replace("Closed", ""));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, Mod.ItemType(name.Replace("Closed", "")));
    }

    //Open Door
    class GenericDoorOpen : Furniture
    {
        public GenericDoorOpen(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);

            TileObjectData.newTile.LavaDeath = true;

            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;

            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 1);
            TileObjectData.addAlternate(0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 2);
            TileObjectData.addAlternate(0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 0);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 1);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 2);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(1);

            QuickBlock.QuickSetFurniture(this, 2, 3, dust, SoundID.Dig, false, color, Origin: new Point16(0, 0));

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
            TileID.Sets.HousingWalls[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            
            adjTiles = new int[] { TileID.OpenDoor };
            closeDoorID = Mod.TileType(name.Replace("Open", "Closed"));
        }

        public override bool HasSmartInteract() => true;

        public override void MouseOver(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            Player.cursorItemIconID = Mod.ItemType(name.Replace("Open", ""));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, Mod.ItemType(name.Replace("Open", "")));
    }

    //Piano, Sofa
    class Generic3x2 : Furniture
    {
        public Generic3x2(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 2, dust, SoundID.Dig, false, color);

            if(name.Contains("Piano"))
                adjTiles = new int[] { TileID.Pianos };
        }
    }    

    //dresser
    class GenericDresser : Furniture
    {
        public GenericDresser(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileContainer[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            TileObjectData.newTile.HookCheck = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
            QuickBlock.QuickSetFurniture(this, 3, 2, dust, SoundID.Dig, false, color);
            
            adjTiles = new int[] { TileID.Dressers };
            dresser = name.Substring(0, name.Length - 7) + " Dresser";
            dresserDrop = Mod.ItemType(name);
        }

        public override bool HasSmartInteract() => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 48, 32, dresserDrop);
            Chest.DestroyChest(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameY == 0)
            {
                Main.CancelClothesWindow(true);
                Main.mouseRightRelease = false;
                int left = (int)(Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameX / 18);
                left %= 3;
                left = Player.tileTargetX - left;
                int top = Player.tileTargetY - (int)(Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameY / 18);
                if (Player.sign > -1)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuClose);
                    Player.sign = -1;
                    Main.editSign = false;
                    Main.npcChatText = string.Empty;
                }
                if (Main.editChest)
                {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                    Main.editChest = false;
                    Main.npcChatText = string.Empty;
                }
                if (Player.editedChestName)
                {
                    NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[Player.chest].name), Player.chest, 1f, 0f, 0f, 0, 0, 0);
                    Player.editedChestName = false;
                }
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    if (left == Player.chestX && top == Player.chestY && Player.chest != -1)
                    {
                        Player.chest = -1;
                        Recipe.FindRecipes();
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
                        Main.stackSplit = 600;
                    }
                }
                else
                {
                    Player.flyingPigChest = -1;
                    int num213 = Chest.FindChest(left, top);
                    if (num213 != -1)
                    {
                        Main.stackSplit = 600;
                        if (num213 == Player.chest)
                        {
                            Player.chest = -1;
                            Recipe.FindRecipes();
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuClose);
                        }
                        else if (num213 != Player.chest && Player.chest == -1)
                        {
                            Player.chest = num213;
                            Main.playerInventory = true;
                            Main.recBigList = false;
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuOpen);
                            Player.chestX = left;
                            Player.chestY = top;
                        }
                        else
                        {
                            Player.chest = num213;
                            Main.playerInventory = true;
                            Main.recBigList = false;
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                            Player.chestX = left;
                            Player.chestY = top;
                        }
                        Recipe.FindRecipes();
                    }
                }
            }
            else
            {
                Main.playerInventory = false;
                Player.chest = -1;
                Recipe.FindRecipes();
                Main.dresserX = Player.tileTargetX;
                Main.dresserY = Player.tileTargetY;
                Main.OpenClothesWindow();
            }
            return true;
        }

        public override void MouseOverFar(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            int left = Player.tileTargetX;
            int top = Player.tileTargetY;
            left -= (int)(tile.TileFrameX % 54 / 18);
            if (tile.TileFrameY % 36 != 0)
            {
                top--;
            }
            int chestIndex = Chest.FindChest(left, top);
            Player.cursorItemIconID = -1;
            if (chestIndex < 0)
            {
                Player.cursorItemIconEnabledText = Language.GetTextValue("LegacyDresserType.0");
            }
            else
            {
                if (Main.chest[chestIndex].name != "")
                {
                    Player.cursorItemIconEnabledText = Main.chest[chestIndex].name;
                }
                else
                {
                    Player.cursorItemIconEnabledText = chest;
                }
                if (Player.cursorItemIconEnabledText == chest)
                {
                    Player.cursorItemIconID = Mod.ItemType(this.name);
                    Player.cursorItemIconEnabledText = "";
                }
            }
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            if (Player.cursorItemIconEnabledText == "")
            {
                Player.cursorItemIconEnabled = false;
                Player.cursorItemIconID = 0;
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
            int left = Player.tileTargetX;
            int top = Player.tileTargetY;
            left -= (int)(tile.TileFrameX % 54 / 18);
            if (tile.TileFrameY % 36 != 0)
            {
                top--;
            }
            int num138 = Chest.FindChest(left, top);
            Player.cursorItemIconID = -1;
            if (num138 < 0)
            {
                Player.cursorItemIconEnabledText = Language.GetTextValue("LegacyDresserType.0");
            }
            else
            {
                if (Main.chest[num138].name != "")
                {
                    Player.cursorItemIconEnabledText = Main.chest[num138].name;
                }
                else
                {
                    Player.cursorItemIconEnabledText = chest;
                }
                if (Player.cursorItemIconEnabledText == chest)
                {
                    Player.cursorItemIconID = Mod.ItemType(this.name);
                    Player.cursorItemIconEnabledText = "";
                }
            }
            Player.noThrow = 2;
            Player.cursorItemIconEnabled = true;
            if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameY > 0)
            {
                Player.cursorItemIconID = ItemID.FamiliarShirt;
            }
        }
    }

    //Lamp
    class GenericLamp : Furniture
    {
        public GenericLamp(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 3, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.Lamps };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            int newY = j - tile.TileFrameY % 54 / 18;

            tile = Framing.GetTileSafely(i, newY);

            for (int l = 0; l < 3; ++l)
            {
                Main.tile[i, newY + l].TileFrameX += (short)(Main.tile[i, newY + l].TileFrameX >= 18 ? -18 : 18);
                Wiring.SkipWire(i, newY + l);
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 18 && tile.TileFrameY == 0) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Lantern
    class GenericLantern : Furniture
    {
        public GenericLantern(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
            QuickBlock.QuickSetFurniture(this, 1, 2, dust, SoundID.Dig, false, color);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            adjTiles = new int[] { TileID.HangingLanterns };
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            int newY = j - tile.TileFrameY % 36 / 18;
            //Main.NewText("O: " + tile.TileFrameY % 36 / 18);

            tile = Framing.GetTileSafely(i, newY);

            //Main.NewText("G: " + tile.TileFrameX);

            for (int l = 0; l < 2; l++)
            {
                Main.tile[i, newY + l].TileFrameX += (short)(Main.tile[i, newY + l].TileFrameX >= 18 ? -18 : 18);
                //Wiring.SkipWire(i, newY + l);
            }

            //Main.NewText("G: " + tile.TileFrameX);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX < 18 && tile.TileFrameY == 18) (r, g, b) = (color.R / 255f, color.G / 255f, color.B / 255f);
        }
    }

    //Sink
    class GenericSink : Furniture
    {
        public GenericSink(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 2, dust, SoundID.Dig, false, color);
            adjTiles = new int[] { TileID.Sinks };
        }
    }

    //Table
    class GenericSolidWithTop : Furniture
    {
        public GenericSolidWithTop(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 2, dust, SoundID.Dig, false, color, true);
            adjTiles = new int[] { TileID.Tables };
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        }
    }

    //Workbench
    class GenericWorkbench : Furniture
    {
        public GenericWorkbench(Color color, int dust, string name) : base(color, dust, name) { }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 1, dust, SoundID.Dig, false, color, true);
            adjTiles = new int[] { TileID.WorkBenches };
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        }
    }
}