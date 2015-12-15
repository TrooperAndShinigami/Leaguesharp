using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace NoobDiana
{
    class Program
    {
        public const string ChampionName = "Diana";
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker;
        //Menu
        public static Menu Menu;
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Diana") return;

            Q = new Spell(SpellSlot.Q, 830f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0.5f, 195f, 1600f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 200f, TargetSelector.DamageType.Magical);

            E = new Spell(SpellSlot.E, 350f, TargetSelector.DamageType.Magical);

            R = new Spell(SpellSlot.R, 825f, TargetSelector.DamageType.Magical);

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            var orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            var spellMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            spellMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            spellMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            spellMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            spellMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            spellMenu.AddItem(new MenuItem("useR2", "Use second R").SetValue(true));
            spellMenu.AddItem(new MenuItem("space", ""));
            spellMenu.AddItem(new MenuItem("ComboMode", "ComboMode").SetValue(new StringList(new[] { "Normal", "Misaya" })));

            var lc = new Menu("Laneclear", "Laneclear");
            Menu.AddSubMenu(lc);
            lc.AddItem(new MenuItem("laneclearQ", "Use Q to LaneClear").SetValue(true));
            lc.AddItem(new MenuItem("laneclearW", "Use W to LaneClear").SetValue(true));

            var jc = new Menu("JungleClear", "JungleClear");
            Menu.AddSubMenu(jc);
            jc.AddItem(new MenuItem("JungleClearQ", "Use Q to JungleClear").SetValue(true));
            jc.AddItem(new MenuItem("JungleClearW", "Use W to JungleClear").SetValue(true));
            jc.AddItem(new MenuItem("JungleClearR", "Use R to JungleClear").SetValue(true));

            var harass = new Menu("Harass", "Harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassQ", "Use Q to Harass").SetValue(true));

            var miscMenu = new Menu("Misc", "Misc");
            Menu.AddSubMenu(miscMenu);
            miscMenu.AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(true));
            miscMenu.AddItem(new MenuItem("KillstealR", "Killsteal with R").SetValue(true));
            miscMenu.AddItem(new MenuItem("drawR", "Draw R range").SetValue(true));

            Menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("NoobDiana by 1Shinigamix3");
        }
        private static void OnDraw(EventArgs args)
        {
            if (Menu.Item("drawR").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Blue, 3);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }         
            Killsteal();
            Combo();
            Harass();
            Farm();
        }
        private static void Killsteal()
        {
            var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var targetR = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (Menu.Item("KillstealQ").GetValue<bool>() && targetQ != null && targetQ.Health < Q.GetDamage(targetQ) && Q.IsReady())
            {
                    Q.Cast(targetQ);
            }
            if (Menu.Item("KillstealR").GetValue<bool>() && targetR != null && targetR.Health < R.GetDamage(targetR) && R.IsReady())
            {
                    R.Cast(targetR);       
            }
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                return;
            if (Menu.Item("harassQ").GetValue<bool>() && Q.IsReady()) Q.Cast(target.ServerPosition);
        }
        private static void Combo()
        {
            var misaya = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1);
            var normal = (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0);
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            /*
            if (Menu.Item("useR").GetValue<bool>() && R.IsReady() && Q.IsReady()) R.CastOnUnit(target);
            if (Menu.Item("useQ").GetValue<bool>() && Q.IsReady() && Q.IsInRange(target)) Q.Cast(target.Position);                      
            if (Menu.Item("useW").GetValue<bool>() && W.IsInRange(target) && W.IsReady()) W.Cast();
            if (Menu.Item("useE").GetValue<bool>() && E.IsInRange(target) && E.IsReady()) E.Cast();
            if (Menu.Item("useR2").GetValue<bool>() && !Q.IsReady()) R.CastOnUnit(target);
            */
            if (misaya)
            {
                if (R.IsReady())
                {
                    if (Menu.Item("useR").GetValue<bool>() && Q.IsReady()) R.CastOnUnit(target);
                    if (Menu.Item("useQ").GetValue<bool>() && Q.IsReady() && Q.IsInRange(target)) Q.Cast(target.Position);
                    if (Menu.Item("useW").GetValue<bool>() && W.IsInRange(target) && W.IsReady()) W.Cast();
                    if (Menu.Item("useE").GetValue<bool>() && E.IsInRange(target) && E.IsReady()) E.Cast();
                    if (Menu.Item("useR2").GetValue<bool>() && !Q.IsReady()) R.CastOnUnit(target);
                }
                if (!R.IsReady())
                {
                    if (Menu.Item("useQ").GetValue<bool>() && Q.IsReady() && Q.IsInRange(target)) Q.Cast(target.Position);
                    if (Menu.Item("useW").GetValue<bool>() && W.IsInRange(target) && W.IsReady()) W.Cast();
                    if (Menu.Item("useE").GetValue<bool>() && E.IsInRange(target) && E.IsReady()) E.Cast();
                }
            }
            if (normal)
            {
                if (R.IsReady())
                {
                    if (Menu.Item("useQ").GetValue<bool>() && Q.IsReady()) Q.Cast(target.ServerPosition);
                    if (Menu.Item("useR").GetValue<bool>() && Q.IsReady()) R.CastOnUnit(target);
                    if (Menu.Item("useW").GetValue<bool>() && W.IsInRange(target) && W.IsReady()) W.Cast();
                    if (Menu.Item("useE").GetValue<bool>() && E.IsInRange(target) && E.IsReady()) E.Cast();
                    if (Menu.Item("useR2").GetValue<bool>() && !Q.IsReady()) R.CastOnUnit(target);
                }
                if (!R.IsReady())
                {
                    if (Menu.Item("useQ").GetValue<bool>() && Q.IsReady() && Q.IsInRange(target)) Q.Cast(target.Position);
                    if (Menu.Item("useW").GetValue<bool>() && W.IsInRange(target) && W.IsReady()) W.Cast();
                    if (Menu.Item("useE").GetValue<bool>() && E.IsInRange(target) && E.IsReady()) E.Cast();
                }
            }        
        }
        private static void Farm()
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
                return;
            var allLaneMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            //Lane
            if (Menu.Item("laneclearQ").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allLaneMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        Q.Cast(minion);
                    }
                }
            }
            if (Menu.Item("laneclearW").GetValue<bool>() && W.IsReady())
            {
                foreach (var minion in allLaneMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        W.Cast(minion);
                    }
                }
            }
        }
    }
}