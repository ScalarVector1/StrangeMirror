using System.Linq;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace StrangeMirror.Systems
{
    class ShaderLoader : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ)
                return;

            MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
            var file = (TmodFile)info.Invoke(ModLoader.GetMod("StrangeMirror"), null);

            System.Collections.Generic.IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

            foreach (FileEntry entry in shaders)
            {
                string name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
                string path = entry.Name.Replace(".xnb", "");
                LoadShader(name, path);
            }
        }

        public static void LoadShader(string name, string path)
        {
			var screenRef = new Ref<Effect>(ModLoader.GetMod("StrangeMirror").Assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value);
            Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
            Filters.Scene[name].Load();
        }
    }
}