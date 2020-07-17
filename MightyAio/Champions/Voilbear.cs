﻿using System;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace MightyAio.Champions
{
    internal class Voilbear
    {
        #region Basics

        private static Spell _q, _w, _e, _r;
        private static  AIHeroClient Player => ObjectManager.Player;
        private static float range = ObjectManager.Player.GetRealAutoAttackRange();
        

        #endregion
        public Voilbear()
        {
            _q= new Spell(SpellSlot.Q,range);
            _w= new Spell(SpellSlot.W,range);
            _e= new Spell(SpellSlot.E,1200);
            _r= new Spell(SpellSlot.Q,700);
            Game.OnUpdate += GameOnOnUpdate;
            Orbwalker.OnAction += OrbwalkerOnOnAction;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnOnProcessSpellCast;
        }

        private void AIBaseClientOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == " ")
            {
                Orbwalker.ResetAutoAttackTimer();
            }
        }

        private static void OrbwalkerOnOnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.AfterAttack)
            {
                
            }
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    break;
                case OrbwalkerMode.Harass:
                    break;
                case OrbwalkerMode.LaneClear:
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}