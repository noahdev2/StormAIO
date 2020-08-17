using System;
using System.Linq;
using System.Security.AccessControl;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using SharpDX;
using static StormAIO.MainMenu;

namespace StormAIO.utilities
{
    public class Rundown
    {
        public static Menu RundownMenu;

        #region MenuHelper
        
        private static bool rundownTop => RundownMenu["rundownTab"].GetValue<MenuBool>("rundownTop");
        private static bool rundownMid => RundownMenu["rundownTab"].GetValue<MenuBool>("rundownMid");
        private static bool rundownBot => RundownMenu["rundownTab"].GetValue<MenuBool>("rundownBot");
        private static bool followBool => RundownMenu.GetValue<MenuBool>("selector");
        #endregion
        public Rundown()
        {
            RundownMenu = new Menu("rundownMenu", "Rundown™")
            {
                new Menu("rundownTab", "Run it down?")
                {
                    new MenuBool("rundownTop", "Top").SetValue(false),
                    new MenuBool("rundownMid", "Mid").SetValue(false),
                    new MenuBool("rundownBot", "Bot").SetValue(false),
                },
                new MenuBool("selector", "Adjust the weight of the champs to follow").SetValue(false)
            };
            var allies = from hero in ObjectManager.Get<AIHeroClient>()
                where hero.IsAlly
                select hero;
            foreach (var ally in allies.Where(x => !x.IsMe))
                RundownMenu.Add(new MenuSlider(ally.CharacterName, "Follow weight: " + ally.CharacterName, 1, 1, 5));


            MainMenu.UtilitiesMenu.Add(RundownMenu);
            
            Game.OnUpdate += GameOnUpdate;
            //AIBaseClient.OnBuffGain += AiHeroClientOnOnBuffGain;
            Inting();
            InitSpell();
        }

        #region Other

        private static Spell Q, W, E;
        private static readonly Vector3 RedTeam = new Vector3(604f, 612f, 183.5748f);
        private static readonly Vector3 BlueTeam = new Vector3(14102f, 14194f, 171.9777f);
        private static readonly Vector3 Bot = new Vector3(12743.6f, 2305.73f, 51.5507f);
        private static readonly Vector3 Top = new Vector3(2076, 12356, 52.8381f);
        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q,1000f);
            Q.SetSkillshot(0.25f, 20f, 1500f, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W,1000f);
            W.SetSkillshot(0.25f, 20f, 1500f, false, SkillshotType.Line);
            E = new Spell(SpellSlot.E,600f);
        }
        private static bool TopSpotArrived { get; set; }
        private static bool RunMidSpot { get; set; }
        private static bool BotSpotArrived { get; set; }

        private static Vector3 _runitdownMid;
        private static AIHeroClient Player => ObjectManager.Player;

        #endregion
        
        #region Args
        
        private static void GameOnUpdate(EventArgs args)
        {

            Troll();
            Follow();
            
            if (rundownTop)
            {
                if (RunMidSpot || Player.IsDead)
                {
                    TopSpotArrived = false;
                    RunMidSpot = false;
                }
                
                if (Player.Position.Distance(Top) > 600 && !TopSpotArrived)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Top);
                }
                else
                {
                    TopSpotArrived = true;
                }

                if (TopSpotArrived)
                {
                    if (!RunMidSpot)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, _runitdownMid);
                    }

                    if (Player.Position.Distance(_runitdownMid) < 400 || Player.IsDead)
                    {
                        RunMidSpot = true;
                    }
                }
            }
            else
            {
                TopSpotArrived = false;
                RunMidSpot = false;
            }
            if (rundownMid) Player.IssueOrder(GameObjectOrder.MoveTo, _runitdownMid);
            if (rundownBot)
            {
                if (RunMidSpot || Player.IsDead)
                {
                    BotSpotArrived = false;
                    RunMidSpot = false;
                }
                
                if (Player.Position.Distance(Bot) > 600 && !BotSpotArrived)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Bot);
                }
                else
                {
                    BotSpotArrived = true;
                }

                if (BotSpotArrived)
                {
                    if (!RunMidSpot)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, _runitdownMid);
                    }

                    if (Player.Position.Distance(_runitdownMid) < 400 || Player.IsDead)
                    {
                        RunMidSpot = true;
                    }
                }
            }
            else
            {
                BotSpotArrived = false;
                RunMidSpot = false;
            }
        }

        private void AiHeroClientOnOnBuffGain(AIBaseClient sender, AIBaseClientBuffGainEventArgs args)
        {
            if (args.Buff.Name.Contains("ASSETS"))
            {
                return;
            }
            if (sender.IsMe)
            {
                Game.Print("My Buff Name: " + args.Buff.Name);
                Game.Print("My Buff Count: " + args.Buff.Count);
                Game.Print("y Buff Type: " + args.Buff.Type);
                return;
            }
            Game.Print("Enemy Buff Name: " + args.Buff.Name);
            Game.Print("Enemy Buff Count: " + args.Buff.Count);
            Game.Print("Enemy Buff Type: " + args.Buff.Type);
        }
        
        #endregion

        #region Functions

        private static void Follow()
        {
            if (!followBool) return;
            var validAllies = GameObjects.AllyHeroes.Where(x => x != null && x.IsValid && !x.IsMe && !x.IsDead).ToList();
            if (!validAllies.Any()) return;
            foreach (var ally in validAllies.OrderByDescending(x => RundownMenu.GetValue<MenuSlider>(x.CharacterName).Value))
            {
                Orbwalker.Move(ally.Position);
                return;
            }
        }
        private static void Troll()
        {
            if (!Player.CharacterName.Equals("Anivia") && !Player.CharacterName.Equals("Trundle")) return;
            var validAllies = GameObjects.AllyHeroes.Where(x => x != null && x.IsValid && !x.IsMe);

            foreach (var ally in validAllies.Where(x => (x.HasBuff("recall") || x.HasBuff("SummonerTeleport"))))
            {
                if (ally.Position.Distance(Player) < 2000)
                {
                    if (W.IsReady())
                    {
                        W.Cast(ally.Position);
                    }
                }
            }
        }
        private static void Inting()
        {
            _runitdownMid = Player.Position.Distance(RedTeam) < Player.Position.Distance(BlueTeam) ? BlueTeam : RedTeam;
        }

        #endregion
    }
}