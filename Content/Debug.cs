using StrangeMirror.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StrangeMirror.Content
{
	internal class Debug : ModItem
	{
		public override string Texture => "StrangeMirror/icon";

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 40;
			Item.useTime = 18;

			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
			Item.accessory = true;
		}

		public override bool? UseItem(Player player)
		{
			ModContent.GetInstance<ShaderLoader>().Load();

			MirrorSystem.mirrored = !MirrorSystem.mirrored;
			return true;
		}
	}
}
