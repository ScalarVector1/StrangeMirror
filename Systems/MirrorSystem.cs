using FAudioINTERNAL;
using Microsoft.Xna.Framework.Audio;
using MonoMod.Cil;
using StrangeMirror.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StrangeMirror.Systems
{
	internal class MirrorSystem : ModSystem
	{
		public static bool mirrored;

		public override void Load()
		{
			IL_Main.DoDraw += ScreenspaceHook;
			On_ActiveSound.Update += PitchMusic;
		}

		private void PitchMusic(On_ActiveSound.orig_Update orig, ActiveSound self)
		{
			orig(self);

			if (mirrored)
			{
				self.Pitch -= 0.25f;
			}
		}

		public override void PostUpdateEverything()
		{
			if (mirrored && Main.GameUpdateCount % 60 == 0)
				SoundEngine.PlaySound(SoundID.Zombie94.WithPitchOffset(Main.rand.NextFloat(-0.9f, -1f)).WithVolumeScale(Main.rand.NextFloat(0.05f)));

			if (mirrored && Main.GameUpdateCount % 80 == 0)
				SoundEngine.PlaySound(SoundID.QueenSlime.WithPitchOffset(-1f).WithVolumeScale(0.15f));

			if (mirrored && Main.GameUpdateCount % 250 == 0)
			{
				var rand = Main.rand.Next(80);

				if (rand <= 5)
					SoundEngine.PlaySound(SoundID.Tink.WithPitchOffset(Main.rand.NextFloat(-0.5f, 0f)).WithVolumeScale(Main.rand.NextFloat(0.5f, 1f)));

				else if (rand <= 10)
					SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(-0.5f, 0f)).WithVolumeScale(Main.rand.NextFloat(0.5f, 1f)));

				else if (rand == 25)
					SoundEngine.PlaySound(SoundID.PlayerHit.WithPitchOffset(Main.rand.NextFloat(-0.5f, 0f)).WithVolumeScale(Main.rand.NextFloat(0.5f, 1f)));
			}
		}

		private void ScreenspaceHook(ILContext il)
		{
			ILCursor c = new(il);

			c.TryGotoNext(n => n.MatchLdcI4(36), n => n.MatchCall(typeof(TimeLogger), "DetailedDrawTime"));
			c.EmitDelegate(DrawScreenspace);
		}

		public unsafe void DrawScreenspace()
		{
			if (mirrored)
			{
				Effect shader = Filters.Scene["MirrorEffect"].GetShader().Shader;

				shader.Parameters["background"].SetValue(Main.screenTarget);
				shader.Parameters["tint"].SetValue(new Vector3(0.55f, 0.5f, 0.15f));
				shader.Parameters["time"].SetValue(Main.GameUpdateCount * 0.015f);
				shader.Parameters["freq"].SetValue(20f);
				shader.Parameters["amp"].SetValue(0.01f);

				Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, shader);
				Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
				Main.spriteBatch.End();

				if (Main.audioSystem is LegacyAudioSystem audio)
				{
					IEnumerable<IAudioTrack> tracks = audio.AudioTracks.Where(n => n?.IsPlaying ?? false);
					foreach (IAudioTrack item in tracks)
					{
						if (item is ASoundEffectBasedAudioTrack)
						{
							item?.SetVariable("Pitch", -0.5f);
						}
						else if (item is CueAudioTrack)
						{
							var cue = typeof(CueAudioTrack).GetField("_cue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(item) as Cue;
							nint handle = (IntPtr)typeof(Cue).GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cue);

							var cuePtr = (FACTCue*)(void*)handle;

							FAudio.FAudioFilterParameters parameters = new()
							{
								Type = FAudio.FAudioFilterType.FAudioLowPassFilter,
								Frequency = 1f, // <------------------ frequency
								OneOverQ = 1
							};

							if (cuePtr->simpleWave != null)
							{
								FAudioVoice* voice = cuePtr->simpleWave->voice;
								FAudio.FAudioVoice_SetFilterParameters((nint)voice, ref parameters, 0u);
							}
							else if (cuePtr->playingSound != null)
							{
								FACTSound* factSound = cuePtr->playingSound->sound;
								int count = factSound->trackCount;
								for (int i = 0; i < count; i++)
								{
									ref FACTSoundInstance* sound = ref cuePtr->playingSound;
									ref FACTTrackInstance trackz = ref sound->tracks[i];
									ref FACTTrackInstance._wave wave1 = ref trackz.activeWave;
									FACTWave* wave2 = wave1.wave;
									wave1.basePitch = -600;

									if (wave2 != null)
									{
										FAudioVoice* voice = wave2->voice;
										Marshal.ThrowExceptionForHR((int)FAudio.FAudioVoice_SetFilterParameters((nint)voice, ref parameters, 0u));
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (Main.audioSystem is LegacyAudioSystem audio)
				{
					IEnumerable<IAudioTrack> tracks = audio.AudioTracks.Where(n => n?.IsPlaying ?? false);
					foreach (IAudioTrack item in tracks)
					{
						if (item is ASoundEffectBasedAudioTrack)
						{
							item?.SetVariable("Pitch", 0f);
						}
						else if (item is CueAudioTrack)
						{
							var cue = typeof(CueAudioTrack).GetField("_cue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(item) as Cue;
							nint handle = (IntPtr)typeof(Cue).GetField("handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cue);

							var cuePtr = (FACTCue*)(void*)handle;

							FAudio.FAudioFilterParameters parameters = new()
							{
								Type = FAudio.FAudioFilterType.FAudioNotchFilter,
								Frequency = 1f, // <------------------ frequency
								OneOverQ = 1
							};

							if (cuePtr->simpleWave != null)
							{
								FAudioVoice* voice = cuePtr->simpleWave->voice;
								FAudio.FAudioVoice_SetFilterParameters((nint)voice, ref parameters, 0u);
							}
							else if (cuePtr->playingSound != null)
							{
								FACTSound* factSound = cuePtr->playingSound->sound;
								int count = factSound->trackCount;
								for (int i = 0; i < count; i++)
								{
									ref FACTSoundInstance* sound = ref cuePtr->playingSound;
									ref FACTTrackInstance trackz = ref sound->tracks[i];
									ref FACTTrackInstance._wave wave1 = ref trackz.activeWave;
									FACTWave* wave2 = wave1.wave;
									wave1.basePitch = 0;

									if (wave2 != null)
									{
										FAudioVoice* voice = wave2->voice;
										Marshal.ThrowExceptionForHR((int)FAudio.FAudioVoice_SetFilterParameters((nint)voice, ref parameters, 0u));
									}
								}
							}
						}
					}
				}
			}
		}
	}

	internal class MirrorPlayer : ModPlayer
	{
		public bool Mirrored => MirrorSystem.mirrored;

		public override void SetControls()
		{
			if (Mirrored)
			{
				(Player.controlLeft, Player.controlRight) = (Player.controlRight, Player.controlLeft);
				Main.mouseX = Main.screenWidth - Main.mouseX;
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (Mirrored)
			{
				foreach (Item item in Player.inventory)
				{
					if (item?.GetGlobalItem<MirroredItem>()?.mirrored ?? false)
					{
						Main.NewText(item.stack + " " + item.Name + " shattered...");
						item.TurnToAir();
					}
				}
			}
		}
	}
}
