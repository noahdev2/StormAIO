﻿using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Utility;

namespace StormAIO.utilities
{
    public class AutoLeveler
    {
        private static int[] SpellLevels;
        private static AIHeroClient Player => ObjectManager.Player;
        private static bool Urf => Math.Abs(Player.PercentCooldownMod) >= 0.8;
        public AutoLeveler()
        {
            var LevelMenu = MainMenu.Level.GetValue<MenuBool>("autolevel");
            Champ();
            if (!LevelMenu || Urf || SpellLevels == null) return;
            DelayAction.Add(3000, () => MyLevelLogic());
            AIHeroClient.OnLevelUp +=  AIHeroClientOnOnLevelUp;
        }

        #region Args

        private void AIHeroClientOnOnLevelUp(AIHeroClient sender, AIHeroClientLevelUpEventArgs args)
        {
            if (sender.IsMe )
            {
               DelayAction.Add(100,()=> MyLevelLogic()); 
            }
        }

        #endregion

        #region Functions

        private void MyLevelLogic()
        {
            var qLevel = Player.Spellbook.GetSpell(SpellSlot.Q).Level;
            var wLevel = Player.Spellbook.GetSpell(SpellSlot.W).Level;
            var eLevel = Player.Spellbook.GetSpell(SpellSlot.E).Level;
            var rLevel = Player.Spellbook.GetSpell(SpellSlot.R).Level;
            if (qLevel + wLevel + eLevel + rLevel >= Player.Level || Player.Level > 18) return;
            
            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);
            if (wLevel < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);
            if (eLevel < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }
        private static void Champ()
        {
            var champ = Player.CharacterName;
            switch (champ)
            {
                case "Yone":
                    SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
                    break;
                case "Warwick":
                    SpellLevels = new[] {1, 2, 3, 2, 2, 4, 1, 1, 1, 1, 4, 3, 3, 3, 3, 4, 2, 2};
                    break;
            }
        }
        #endregion
    }
}