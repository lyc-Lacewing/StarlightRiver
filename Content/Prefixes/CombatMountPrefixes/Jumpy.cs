﻿using StarlightRiver.Content.Abilities;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System.Collections.Generic;

namespace StarlightRiver.Prefixes.CombatMountPrefixes
{
	public class Jumpy : CombatMountPrefix
	{
		public override void ApplyToMount(CombatMount mount)
		{
			mount.primarySpeedMultiplier -= 0.15f;
			mount.moveSpeedMultiplier += 0.1f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			TooltipLine newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip", "+15% Attack Speed");
			newline.IsModifier = true;

			tooltips.Add(newline);

			newline = new TooltipLine(StarlightRiver.Instance, "PrefixTip2", "+10% Movement Speed");
			newline.IsModifier = true;

			tooltips.Add(newline);
		}
	}
}
