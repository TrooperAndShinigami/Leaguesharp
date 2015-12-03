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
            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            //Combo Menu
            var combo = new Menu("Combo", "Combo");
            Menu.AddSubMenu(combo);
            combo.AddItem(new MenuItem("ComboMode", "ComboMode").SetValue(new StringList(new[] { "R on Dash", "R After Dash" })));
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
            var useQ = (Menu.Item("useQ").GetValue<bool>());
            var useW = (Menu.Item("useW").GetValue<bool>());
            var useE = (Menu.Item("useE").GetValue<bool>());
            var useR = (Menu.Item("useR").GetValue<bool>());
            var ondash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0);
            var afterdash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1);
            var m = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            //Q Ks
            if (m.Health < Q.GetDamage(m))
            {
                Q.Cast(m);
            }
            //Combo
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }
            if (Player.Distance(m) <= botrk.Range)
            {
                botrk.Cast(m);
            }
            if (Player.Distance(m) <= cutlass.Range)
            {
                cutlass.Cast(m);
            }
            if (Player.Distance(m) <= hextech.Range)
            {
                hextech.Cast(m);
            }
            if (ondash)
            {
                if (useW && Q.IsReady()) W.Cast();
                if (useQ) if (Player.Distance(m.Position) > 125) Q.Cast(m);
                if (useR)
                    UseR(m);
                if (hydra.IsOwned() && Player.Distance(m) < hydra.Range && hydra.IsReady() && !E.IsReady()) hydra.Cast();
                if (tiamat.IsOwned() && Player.Distance(m) < tiamat.Range && tiamat.IsReady() && !E.IsReady()) tiamat.Cast();
            }
            if (!afterdash)
            {
                return;
            }
            {
                if (useW && Q.IsReady()) W.Cast();
                if (useQ) if (Player.Distance(m.Position) > 125) Q.Cast(m);
                if (useE && !R.IsReady()) E.Cast(Game.CursorPos);
                if (hydra.IsOwned() && Player.Distance(m) < hydra.Range && hydra.IsReady() && !E.IsReady()) hydra.Cast();
                if (tiamat.IsOwned() && Player.Distance(m) < tiamat.Range && tiamat.IsReady() && !E.IsReady()) tiamat.Cast();
            }
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
            {
                return;
            }
            Lane();
            Jungle();
        }
        private static void AfterAa(AttackableUnit unit, AttackableUnit target)
        {
            var useE = (Menu.Item("useE").GetValue<bool>());
            var useR = (Menu.Item("useR").GetValue<bool>());
            var ondash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0);
            var afterdash = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1);
            var m = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }
            {
                if (ondash)
                {
                    if (useE && E.IsReady() && !R.IsReady()) E.Cast(Game.CursorPos);
                }
                if (!afterdash)
                {
                    return;
                }
                if (useR && R.IsReady()) UseR(m);
            }
        }
        //R usage
        public static void UseR(Obj_AI_Hero target)
        {
            var castPosition = R.GetPrediction(target).CastPosition;
            castPosition = Player.ServerPosition.Extend(castPosition, R.Range);

            R.Cast(castPosition);
        }
        //Lane&JungleClear
        public static void Lane()
        {
            var useQ = (Menu.Item("laneclearQ").GetValue<bool>());
            var useW = (Menu.Item("laneclearW").GetValue<bool>());
            var useE = (Menu.Item("laneclearE").GetValue<bool>());
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 550);
            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget()))
                {
                    Q.CastOnUnit(minion);
                }
            }
            if (useW && Q.IsReady() && W.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget()))
                {
                    W.Cast(minion);
                }
            }

            /*if (Menu.Item("useE").GetValue<bool>() && R.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.Cast(minion);
                    }
                }
            }*/
        }

        public static void Jungle()
        {
            var useQ = (Menu.Item("jungleclearQ").GetValue<bool>());
            var useW = (Menu.Item("jungleclearW").GetValue<bool>());
            var useE = (Menu.Item("jungleclearE").GetValue<bool>());
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 550, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget()))
                {
                    Q.CastOnUnit(minion);
                }
            }

            if (useW && W.IsReady() && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget()))
                {
                    W.Cast(minion);
                }
            }
            /*if (useQ && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        E.Cast(minion);
                    }
                }
            }*/
        }
    }
}
