using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace NoobFizz
{
    class Program
    {
        public const string ChampionName = "Fizz";
        public static Obj_AI_Hero Player => ObjectManager.Player;

        public static Orbwalking.Orbwalker Orbwalker;
        //Menu
        public static Menu Menu;
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static Items.Item tiamat;
        private static Items.Item hydra;
        private static Items.Item cutlass;
        private static Items.Item botrk;
        private static Items.Item hextech;

        private static Obj_AI_Base Target;

       //private static bool IsEUsed => Player.HasBuff("");

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Fizz") return;

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1300);

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            var orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            //Combo Menu
            var combo = new Menu("Combo", "Combo");
            Menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("ComboMode", "ComboMode").SetValue(new StringList(new[] { "R after Dash", "R on Dash" })));
            combo.AddItem(new MenuItem("space", ""));
            combo.AddItem(new MenuItem("Combo", "Combo"));
            combo.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("useE", "Use E").SetValue(false));
            combo.AddItem(new MenuItem("useR", "Use R").SetValue(false));
            //Harass
           /* var harass = new Menu("Harass", "Harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassWQ", "Use W+Q after an AA").SetValue(true));*/
            //LaneClear Menu
            var lc = new Menu("Laneclear", "Laneclear");
            Menu.AddSubMenu(lc);
            lc.AddItem(new MenuItem("laneclearQ", "Use Q to LaneClear").SetValue(true));
            lc.AddItem(new MenuItem("laneclearW", "Use W to LaneClear").SetValue(true));
            lc.AddItem(new MenuItem("laneclearE", "Use E to LaneClear").SetValue(true));
            //Jungle Clear Menu
            var jungle = new Menu("JungleClear", "JungleClear");
            Menu.AddSubMenu(jungle);
            jungle.AddItem(new MenuItem("jungleclearQ", "Use Q to JungleClear").SetValue(true));
            jungle.AddItem(new MenuItem("jungleclearW", "Use W to JungleClear").SetValue(true));
            jungle.AddItem(new MenuItem("jungleclearE", "Use E to JungleClear").SetValue(true));

            var miscMenu = new Menu("Misc", "Misc");
            Menu.AddSubMenu(miscMenu);           
            miscMenu.AddItem(new MenuItem("Killsteal", "Killsteal with Q").SetValue(true));

            hydra = new Items.Item(3074, 185);
            tiamat = new Items.Item(3077, 185);
            cutlass = new Items.Item(3144, 450);
            botrk = new Items.Item(3153, 450);
            hextech = new Items.Item(3146, 700);
            Menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Orbwalking.AfterAttack += AfterAa;
            Game.PrintChat("NoobFizz by 1Shinigamix3");
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Menu.Item("Killsteal").GetValue<bool>())
            {
                Killsteal();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Lane();
                Jungle();
            }
        }
        private static void AfterAa(AttackableUnit unit, AttackableUnit target)
        {
            var useE = (Menu.Item("useE").GetValue<bool>());
            var useR = (Menu.Item("useR").GetValue<bool>());
            var ondash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1);
            var afterdash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0);
            var m = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (ondash)
                {
                    if (useE && E.IsReady() && !R.IsReady()) E.Cast(Game.CursorPos);
                }
                if (afterdash)
                {
                    if (useR && R.IsReady()) UseTr(m);
                }
            }
        }
        //R usage
        public static void UseTr(Obj_AI_Hero target)
        {
            var castPosition = R.GetPrediction(target).CastPosition;
            castPosition = Player.ServerPosition.Extend(castPosition, R.Range);

            R.Cast(castPosition);
        }
        //Lane&JungleClear
        private static void Lane()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Menu.Item("laneclearW").GetValue<bool>() && Q.IsReady() && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.Cast();
                    }
                }
            }
            if (Menu.Item("laneclearQ").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
            if (Menu.Item("laneclearE").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }
        }
        private static void Jungle()
        {
            var allMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (Menu.Item("jungleclearW").GetValue<bool>() && W.IsReady() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.Cast();
                    }
                }
            }
            if (Menu.Item("jungleclearQ").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.CastOnUnit(minion);
                    }
                }
            }
            if (Menu.Item("jungleclearE").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.Cast(Game.CursorPos);
                    }
                }
            }
        }
        private static void Combo()
        {
            var useQ = (Menu.Item("useQ").GetValue<bool>());
            var useW = (Menu.Item("useW").GetValue<bool>());
            var useE = (Menu.Item("useE").GetValue<bool>());
            var useR = (Menu.Item("useR").GetValue<bool>());
            var ondash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1);
            var afterdash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0);
            var m = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (m != null && Player.Distance(m) <= botrk.Range)
            {
                botrk.Cast(m);
            }
            if (m != null && Player.Distance(m) <= cutlass.Range)
            {
                cutlass.Cast(m);
            }
            if (m != null && Player.Distance(m) <= hextech.Range)
            {
                hextech.Cast(m);
            }
            if (ondash)
            {
                if (useW && Q.IsReady()) if (Player.Distance(m.Position) < Q.Range) W.Cast();
                if (useQ) if (Player.Distance(m.Position) > 175) Q.CastOnBestTarget();
                if (useR)
                    UseTr(m);
                if (hydra.IsOwned() && Player.Distance(m) < hydra.Range && hydra.IsReady() && !E.IsReady()) hydra.Cast();
                if (tiamat.IsOwned() && Player.Distance(m) < tiamat.Range && tiamat.IsReady() && !E.IsReady()) tiamat.Cast();
            }
            if (afterdash)
            {
                if (useW && Q.IsReady()) if (Player.Distance(m.Position) < Q.Range) W.Cast();
                if (useQ) if (Player.Distance(m.Position) > 175) Q.CastOnBestTarget();
                if (useE && !R.IsReady()) E.Cast(Game.CursorPos);
                if (hydra.IsOwned() && Player.Distance(m) < hydra.Range && hydra.IsReady() && !E.IsReady()) hydra.Cast();
                if (tiamat.IsOwned() && Player.Distance(m) < tiamat.Range && tiamat.IsReady() && !E.IsReady()) tiamat.Cast();
            }
        }
        private static void Killsteal()
        {
            var m = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (m != null && m.Health < Q.GetDamage(m) && Q.IsReady())
            {
                Q.Cast(m);
            }
        }
    }
}
