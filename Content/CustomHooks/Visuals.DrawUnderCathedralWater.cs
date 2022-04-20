﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class DrawUnderCathedralWater : HookGroup
    {
        //Rare method to hook but not the best finding logic, but its really just some draws so nothing should go terribly wrong.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            IL.Terraria.Main.DoDraw_WallsTilesNPCs += DrawWater;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DoDraw_WallsTilesNPCs -= DrawWater;
        }

        private void DrawWater(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchLdfld<Main>("DrawCacheNPCsBehindNonSolidTiles"));
            //c.Index--;

            c.EmitDelegate<DrawWaterDelegate>(DrawWater);
        }

        private delegate void DrawWaterDelegate();

        public static void DrawWater()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            NPC NPC = Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor);

            if(NPC != null && NPC.active)
            {
                if(ReflectionTarget.canUseTarget)
                    (NPC.ModNPC as ArenaActor).DrawBigWindow(Main.spriteBatch);

                int boss = -1;
                List<NPC> drawCache = new List<NPC>();

                for (int k = 0; k < Main.maxNPCs; k++) //draw NPCs and find boss
                {
                    var NPC2 = Main.npc[k];

                    if (NPC2.active && NPC2.ModNPC is IUnderwater)
                    {
                        if (NPC2.type == ModContent.NPCType<SquidBoss>())
                            boss = k;
                        else
                            drawCache.Add(NPC2);
                            
                    }
                }

                drawCache.ForEach(n => (n.ModNPC as IUnderwater).DrawUnderWater(Main.spriteBatch, 0));
               
                foreach (Projectile proj in Main.projectile.Where(n => n.active && n.ModProjectile is IUnderwater)) //draw all Projectiles
                    (proj.ModProjectile as IUnderwater).DrawUnderWater(Main.spriteBatch, 0);

                if (boss != -1 && Main.npc[boss].ModNPC is IUnderwater)
                   (Main.npc[boss].ModNPC as IUnderwater).DrawUnderWater(Main.spriteBatch, 0); //draw boss ontop if extant

                drawCache.ForEach(n => (n.ModNPC as IUnderwater).DrawUnderWater(Main.spriteBatch, 1)); //draw layer for NPCs over bosses, used for the front part of tentacles

                var effect = Filters.Scene["Waves"].GetShader().Shader;

                if (effect is null)
                    return;

                effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
                effect.Parameters["power"].SetValue(0.01f);
                effect.Parameters["offset"].SetValue(new Vector2(Main.screenPosition.X / Main.screenWidth, 0));
                effect.Parameters["sampleTexture"].SetValue(AuroraWaterSystem.auroraBackTarget);
                effect.Parameters["speed"].SetValue(50f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

                Main.spriteBatch.Draw(CathedralTarget.CatherdalWaterTarget, Vector2.Zero, Color.White);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }
        }
    }
}