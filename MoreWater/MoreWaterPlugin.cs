using System;
using System.Collections.Generic;
using BepInEx;
using MonoMod.RuntimeDetour;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MoreWater
{
    [BepInPlugin("slime-cubed.morewater", "More Water", "1.0.0")]
    public class MoreWaterPlugin : BaseUnityPlugin
    {
        public static SScreenOptions.GuiOptionDropDown waterPowerDropdown = new SScreenOptions.GuiOptionDropDown(-260, "OPTIONS_WATERPOWER", null, -870, -470, true, 240);
        public static int waterPower = 0;

        public MoreWaterPlugin()
        {
            new Hook(
                typeof(CItem_Weapon).GetMethod(nameof(CItem_Weapon.Use_Local), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                new hook_Weapon_Use(Weapon_Use)
            );

            new Hook(
                typeof(SScreenOptions).GetMethod(nameof(SScreenOptions.OnInit), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                new Action<Action<SScreenOptions>, SScreenOptions>(Options_OnInit)
            );

            new Hook(
                typeof(SScreenOptions).GetMethod(nameof(SScreenOptions.OnActivate), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                new Action<Action<SScreenOptions>, SScreenOptions>(Options_OnActivate)
            );

            new Hook(
                typeof(SScreenOptions).GetMethod(nameof(SScreenOptions.OnControlChanged), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                new Action<Action<SScreenOptions, SScreenOptions.GuiOption>, SScreenOptions, SScreenOptions.GuiOption>(Options_OnControlChanged)
            );

            SSingleton<SLoc>.Inst.m_dico["OPTIONS_WATERPOWER"] = new SLoc.CSentence("OPTIONS_WATERPOWER", "Water Pistol Power");
        }

        private static void Options_OnControlChanged(Action<SScreenOptions, SScreenOptions.GuiOption> orig, SScreenOptions self, SScreenOptions.GuiOption control)
        {
            if(control == waterPowerDropdown)
            {
                if(int.TryParse(waterPowerDropdown.m_dropDown.m_texts[waterPowerDropdown.m_dropDown.m_selection], out int newWaterPower))
                {
                    waterPower = newWaterPower;
                }
            }

            orig(self, control);
        }

        private static void Options_OnActivate(Action<SScreenOptions> orig, SScreenOptions self)
        {
            waterPowerDropdown.m_dropDown.m_texts = new List<string>
            {
                "0",
                "64",
                "256",
                "1024",
                "4096",
                "16384",
                "65536"
            };
            waterPowerDropdown.m_dropDown.m_selection = Mathf.Max(waterPowerDropdown.m_dropDown.m_texts.IndexOf(waterPower.ToString()), 0);
            waterPowerDropdown.m_dropDown.m_text = waterPowerDropdown.m_dropDown.m_texts[waterPowerDropdown.m_dropDown.m_selection];

            orig(self);
        }

        private static void Options_OnInit(Action<SScreenOptions> orig, SScreenOptions self)
        {
            orig(self);

            self.Options.Add(waterPowerDropdown);
            waterPowerDropdown.OnInit();
        }

        private delegate void orig_Weapon_Use(CItem_Weapon self, CPlayer player, Vector2 worldPos, bool isShift);
        private delegate void hook_Weapon_Use(orig_Weapon_Use orig, CItem_Weapon self, CPlayer player, Vector2 worldPos, bool isShift);
        private static void Weapon_Use(orig_Weapon_Use orig, CItem_Weapon self, CPlayer player, Vector2 worldPos, bool isShift)
        {
            orig(self, player, worldPos, isShift);

            if (self == GItems.ultimateWaterPistol
                && SWorld.GridRectCam.Contains(worldPos)
                && isShift)
            {
                int2 intPos = new int2(worldPos);
                SWorld.Grid[intPos.x, intPos.y].m_water += waterPower;
                SWorld.Grid[intPos.x, intPos.y].SetFlag(CCell.Flag_IsLava, false);
            }
            if (self == GItems.ultimateLavaPistol
                && SWorld.GridRectCam.Contains(worldPos)
                && isShift)
            {
                int2 intPos = new int2(worldPos);
                SWorld.Grid[intPos.x, intPos.y].m_water += waterPower;
                SWorld.Grid[intPos.x, intPos.y].SetFlag(CCell.Flag_IsLava, true);
            }
        }
    }
}
