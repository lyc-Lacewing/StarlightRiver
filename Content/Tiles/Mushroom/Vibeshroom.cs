﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Mushroom
{
	class Vibeshroom : ModTile
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Mushroom/" + Name;

        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 1, 0);
            TileObjectData.newTile.RandomStyleRange = 5;
            TileObjectData.newTile.StyleHorizontal = true;

            ItemDrop = ItemType<VibeshroomItem>();

            QuickBlock.QuickSetFurniture(this, 1, 1, 61, SoundID.Grass, false, Color.Green);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            (r, g, b) = (0.1f, 0.275f, 0.175f);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tile = Framing.GetTileSafely(i, j);
            var tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Mushroom/VibeshroomGlow").Value;
            var pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2((float)Math.Sin(StarlightWorld.rottime + i) * 1.5f, (float)Math.Cos(StarlightWorld.rottime * 2 + i));

            spriteBatch.Draw(tex, pos, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White);
        }
    }

    class VibeshroomItem : QuickTileItem
    {
        public VibeshroomItem() : base("Vibeshroom", "Vibin'", "Vibeshroom", 1, "StarlightRiver/Assets/Tiles/Mushroom/") { }
    }
}
