using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace NoobLeBlanc
{
    class Program
    {
        public const string ChampionName = "Leblanc";

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static Orbwalking.Orbwalker orbwalker;
        static Menu menu;
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Obj_AI_Hero Target = null;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Leblanc")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 720);
            W = new Spell(SpellSlot.W, 670);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 720);

            menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            //Combo Menu
            var combo = new Menu("Combo", "Combo");
            menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("useR", "Use R").SetValue(true));

            //Lane Clear
            var lc = new Menu("Laneclear", "Laneclear");
            menu.AddSubMenu(lc);
            lc.AddItem(new MenuItem("laneclearW", "Use W to LaneClear").SetValue(true));

            //Jungle Clear
            var jungle = new Menu("JungleClear", "JungleClear");
            menu.AddSubMenu(jungle);
            jungle.AddItem(new MenuItem("jungleclearQ", "Use Q to JungleClear").SetValue(true));
            jungle.AddItem(new MenuItem("jungleclearW", "Use W to JungleClear").SetValue(true));
            jungle.AddItem(new MenuItem("jungleclearE", "Use E to JungleClear").SetValue(true));

            menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Game.PrintChat("NoobLeBlanc by 1Shinigamix3");
        }
        private static void OnUpdate(EventArgs args)
        {
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo(TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical));
            }
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Lane();
                Jungle();
            }
        }
        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (menu.Item("useR").GetValue<bool>() && R.IsReady() && args.Target.IsEnemy && args.Target.IsValid<Obj_AI_Hero>())
            {
                R.Cast();
            }
        }
        private static void Combo(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }
            if (Player.Distance(target) <= Q.Range && Q.IsReady() && (menu.Item("useQ").GetValue<bool>()))
            {
                Q.Cast(target);
            }
            if (Player.Distance(target) <= E.Range && E.IsReady() && (menu.Item("useR").GetValue<bool>()))
            {
                R.Cast(target);
            }
            if (Player.Distance(target) <= E.Range && E.IsReady() && (menu.Item("useW").GetValue<bool>()))
            {
                W.CastIfHitchanceEquals(target, HitChance.High);
            }
            if (Player.Distance(target) <= E.Range && E.IsReady() && (menu.Item("useE").GetValue<bool>()))
            {
                E.CastIfHitchanceEquals(target, HitChance.High);
            }
        }
        private static void Lane()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (menu.Item("laneclearW").GetValue<bool>() && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }          
        }
        private static void Jungle()
        {
            var allMinions = MinionManager.GetMinions(
            ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (menu.Item("jungleclearE").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.Cast(minion);
                    }
                }
            }
            if (menu.Item("jungleclearQ").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.Cast(minion);
                    }
                }
            }

            if (menu.Item("jungleclearW").GetValue<bool>() && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.CastOnUnit(minion);
                    }
                }
            }
            

        }
    }
}
