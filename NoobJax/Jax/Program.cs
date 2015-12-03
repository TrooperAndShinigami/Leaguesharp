using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace NoobJax
{
    class Program
    {
        public const string ChampionName = "Jax";
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

        private static Items.Item Tiamat;
        private static Items.Item Hydra;
        private static Items.Item cutlass;
        private static Items.Item botrk;
        private static Items.Item hextech;
        //private static Items.Item Hextech = new Items.Item(3146, 700);
        private static Obj_AI_Base target;

        private static bool IsEUsed
        {
            get { return Player.HasBuff("JaxCounterStrike"); }
        }
        private static bool IsWUsed
        {
            get { return Player.HasBuff("JaxEmpowerTwo"); }
        }
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Jax") return;

            Q = new Spell(SpellSlot.Q, 680);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);


            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu spellMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            spellMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            spellMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            spellMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            spellMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));

            var lc = new Menu("Laneclear", "Laneclear");
            Menu.AddSubMenu(lc);
            lc.AddItem(new MenuItem("laneclearQ", "Use Q to LaneClear").SetValue(true));
            lc.AddItem(new MenuItem("laneclearW", "Use W to LaneClear").SetValue(true));

            var harass = new Menu("Harass", "Harass");
            Menu.AddSubMenu(harass);
            harass.AddItem(new MenuItem("harassW", "Use W to Cancel AA to Harass").SetValue(true));

            Hydra = new Items.Item(3074, 185);
            Tiamat = new Items.Item(3077, 185);
            cutlass = new Items.Item(3144, 450);
            botrk = new Items.Item(3153, 450);
            hextech = new Items.Item(3146, 700);
            Menu.AddToMainMenu();

            Game.OnUpdate += OnUpdate;
            Orbwalking.OnAttack += OnAa;
            Orbwalking.AfterAttack += AfterAa;
            Game.PrintChat("NoobJax by 1Shinigamix3");
        }
        private static void OnUpdate(EventArgs args)
        {
            Obj_AI_Hero m = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (m.Health < Q.GetDamage(m))
            {
                Q.Cast(m);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
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
                if (Menu.Item("useQ").GetValue<bool>())
                    if (Player.Distance(m.Position) > 125)
                        Q.CastOnBestTarget();

                if (Menu.Item("useR").GetValue<bool>())
                        R.Cast(m);
                if (Hydra.IsOwned() && Player.Distance(m) < Hydra.Range && Hydra.IsReady() && !W.IsReady() && !IsWUsed)
                    Hydra.Cast();
                if (Tiamat.IsOwned() && Player.Distance(m) < Tiamat.Range && Tiamat.IsReady() && !W.IsReady() && !IsWUsed)
                    Tiamat.Cast();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 225).FirstOrDefault();
                if (Hydra.IsOwned() && Player.Distance(minion) < Hydra.Range && Hydra.IsReady() && !W.IsReady())
                    Hydra.Cast(minion);
                if (Tiamat.IsOwned() && Player.Distance(minion) < Tiamat.Range && Tiamat.IsReady() && !W.IsReady())
                    Tiamat.Cast(minion);
            }
        }       
        private static void OnAa(AttackableUnit unit, AttackableUnit target)
        {
            Obj_AI_Hero y = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (E.IsReady())
                {
                    if (E.IsReady() && Q.IsReady() && y.IsValidTarget(Q.Range))
                    {
                        E.Cast();
                    }
                    if (IsEUsed && y.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65))
                    {
                        E.Cast();
                    }
                }                                                     
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Hydra.IsOwned() && Player.Distance(target) < Hydra.Range && Hydra.IsReady() && !W.IsReady()) Hydra.Cast();
                if (Tiamat.IsOwned() && Player.Distance(target) < Tiamat.Range && Tiamat.IsReady() && !W.IsReady()) Tiamat.Cast();
            }
        }
        private static void AfterAa(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Obj_AI_Base minion = MinionManager.GetMinions(Player.Position, 225).FirstOrDefault();
                if (Menu.Item("laneclearW").GetValue<bool>())
                    W.Cast(minion);
                if (Menu.Item("laneclearQ").GetValue<bool>() && !W.IsReady())
                    Q.Cast(minion);
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                if (Menu.Item("useW").GetValue<bool>())
                    W.Cast();
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Menu.Item("harassW").GetValue<bool>()&& W.IsReady()) W.Cast();
            }
        }      

    }
  }
