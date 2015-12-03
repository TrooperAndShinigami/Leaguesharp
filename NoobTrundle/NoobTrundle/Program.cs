using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace NoobTrundle
{
    class Program
    {
        public const string ChampionName = "Trundle";

        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static Orbwalking.Orbwalker orbwalker;
        static Menu menu;
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Obj_AI_Hero Target = null;

        private static Items.Item tiamat = new Items.Item(3077, 185);
        private static Items.Item hydra = new Items.Item(3074, 185);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Trundle")
            {
                return;
            }
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 700);

            menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var combo = new Menu("Combo", "Combo");
            menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("useR", "Use R").SetValue(true));

            var harass = new Menu("Harass", "Harass");
            menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("AutoHarassQ", "Auto Q if enemy is in Range").SetValue(false));

            var lc = new Menu("Laneclear", "Laneclear");
            menu.AddSubMenu(lc);
            lc.AddItem(new MenuItem("laneclearQ", "Use Q to LaneClear").SetValue(true));

            var ks = new Menu("KillSteal", "KillSteal");
            menu.AddSubMenu(ks);
            ks.AddItem(new MenuItem("useQKS", "Auto Q to KillSteal").SetValue(true));

            hydra = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 250);
            tiamat = new Items.Item((int)ItemId.Tiamat_Melee_Only, 250);
            menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Orbwalking.OnAttack += OnAa;
            Orbwalking.AfterAttack += AfterAa;
            Game.PrintChat("NoobTrundle by 1Shinigamix3");
        }
        private static void OnUpdate(EventArgs args)
        {
            Obj_AI_Hero m = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
            if (menu.Item("useQKS").GetValue<bool>())
                if (m.Health < Q.GetDamage(m))
                {
                    Q.CastOnUnit(m);
                }
            if (menu.Item("AutoHarassQ").GetValue<bool>())
            {
                Q.CastOnUnit(m);
            }
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (W.IsReady() && m.IsValidTarget(W.Range) &&
               menu.Item("useW").GetValue<bool>())
                    W.Cast();

                if (E.IsReady() && m.IsValidTarget(E.Range) &&
                menu.Item("useE").GetValue<bool>())
                    E.CastIfHitchanceEquals(m, HitChance.High);
            }
        }
        private static
            void OnAa(AttackableUnit unit, AttackableUnit target)
        {
           
        }
        private static void AfterAa(AttackableUnit unit, AttackableUnit target)
        {
            Obj_AI_Hero m = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Q.IsReady() && menu.Item("useQ").GetValue<bool>())
                    Q.CastOnUnit(m);

                if (hydra.IsOwned() && Player.Distance(m) < hydra.Range && hydra.IsReady() && !W.IsReady())
                    hydra.Cast(m);
                if (tiamat.IsOwned() && Player.Distance(m) < tiamat.Range && tiamat.IsReady() && !W.IsReady())
                    tiamat.Cast(m);

                if (R.IsReady() && m.IsValidTarget(R.Range) && menu.Item("useR").GetValue<bool>())
                {
                    R.CastOnUnit(m);
                }
            }
            if (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Obj_AI_Base minion = MinionManager.GetMinions(600, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault();
                if (menu.Item("laneclearQ").GetValue<bool>())
                    Q.Cast(minion);
                if (hydra.IsOwned() && Player.Distance(minion) < hydra.Range && hydra.IsReady() && !W.IsReady())
                    hydra.Cast(minion);
                if (tiamat.IsOwned() && Player.Distance(minion) < tiamat.Range && tiamat.IsReady() && !W.IsReady())
                    tiamat.Cast(minion);
            }
        }
    }
}
