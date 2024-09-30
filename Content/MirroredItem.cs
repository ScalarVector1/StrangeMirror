using StrangeMirror.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StrangeMirror.Content
{
	internal class MirroredItem : GlobalItem
	{
		public bool mirrored;

		public override bool InstancePerEntity => true;

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (mirrored)
			{
				tooltips.Add(new(Mod, "MirrorInfo", "Mirrored") { OverrideColor = new Color(1f, 0.9f, 0.8f)});
			}
		}

		public override void OnCreated(Item item, ItemCreationContext context)
		{
			if (MirrorSystem.mirrored)
				mirrored = true;
		}

		public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			if (MirrorSystem.mirrored)
				mirrored = true;
		}

		public override bool CanStack(Item destination, Item source)
		{
			return source.GetGlobalItem<MirroredItem>().mirrored == destination.GetGlobalItem<MirroredItem>().mirrored;
		}

		public override bool CanStackInWorld(Item destination, Item source)
		{
			return source.GetGlobalItem<MirroredItem>().mirrored == destination.GetGlobalItem<MirroredItem>().mirrored;
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (mirrored)
			{
				spriteBatch.Draw(Terraria.GameContent.TextureAssets.Item[item.type].Value, position, frame, Color.Lerp(Color.White, new Color(0.55f, 0.5f, 0.15f), 0.1f), 0f, origin, scale, SpriteEffects.FlipHorizontally, 0);
				return false;
			}

			return true;
		}
	}
}
