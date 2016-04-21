using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace NoobJaxReloaded
{
    class Program
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        private static readonly Obj_AI_Hero[] AllEnemy = HeroManager.Enemies.ToArray();
        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Items.Item tiamat, hydra, cutlass, botrk, hextech;
        private static bool IsEUsed => Player.HasBuff("JaxCounterStrike");

        private static bool IsWUsed => Player.HasBuff("JaxEmpowerTwo");

        private static Menu _menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// Game Loaded Method
        /// </summary>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Jax") // check if the current champion is Jax
                return; // stop programm

            // Set Spells
            Q = new Spell(SpellSlot.Q, 680);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            // Create Items
            hydra = new Items.Item(3074, 185);
            tiamat = new Items.Item(3077, 185);
            cutlass = new Items.Item(3144, 450);
            botrk = new Items.Item(3153, 450);
            hextech = new Items.Item(3146, 700);

            // create _menu
            _menu = new Menu("Noob" + Player.ChampionName, "Noob"+ Player.ChampionName, true);

            Menu orbwalkerMenu = _menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            // create TargetSelector
            Menu ts = _menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));

            // attach
            TargetSelector.AddToMenu(ts);

            //Combo-_menu
            Menu comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useQ2", "Use Q when enemy is in AA Range").SetValue(false).SetTooltip("Turn this on to use Q when enemy is in AA range"));
            comboMenu.AddItem(new MenuItem("qsetting", "Q range").SetValue(new Slider(680, 680)).SetTooltip("Don't change anything unless you want a shorter Q range usage"));
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("space1", "E options"));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE2", "Use second E").SetValue(true).SetTooltip("Turn this off if you want to use second E manually."));          
            _menu.AddSubMenu(comboMenu);

            //Harrass-_menu
            Menu harassMenu = new Menu("Harass", "Harass");
            harassMenu.AddItem(new MenuItem("harassW", "Use W to Cancel AA to Harass").SetValue(true));
            _menu.AddSubMenu(harassMenu);

            //Laneclear-_menu
            Menu laneclear = new Menu("Laneclear", "Laneclear");
            laneclear.AddItem(new MenuItem("laneclearW", "Use W to LaneClear").SetValue(true));
            _menu.AddSubMenu(laneclear);

            //Jungleclear-_menu
            Menu jungleclear = new Menu("Jungleclear", "Jungleclear");
            jungleclear.AddItem(new MenuItem("jungleclearQ", "Use Q to JungleClear").SetValue(true));
            jungleclear.AddItem(new MenuItem("jungleclearW", "Use W to JungleClear").SetValue(true));
            jungleclear.AddItem(new MenuItem("jungleclearE", "Use E to JungleClear").SetValue(true));
            _menu.AddSubMenu(jungleclear);

            //KS-_menu
            Menu ksMenu = new Menu("Killsteal", "Killsteal");
            ksMenu.AddItem(new MenuItem("Killsteal", "Killsteal with Q").SetValue(true));
            _menu.AddSubMenu(ksMenu);

            //Drawings-_menu
            Menu drawingsMenu = new Menu("Drawings", "Drawings");
            drawingsMenu.AddItem(new MenuItem("drawsetQ", "Draw set Q range").SetValue(false));
            drawingsMenu.AddItem(new MenuItem("drawAa", "Draw Autoattack range").SetValue(false));
            _menu.AddSubMenu(drawingsMenu);

            //Misc-Menü
            Menu miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("usejump", "Use Wardjump").SetValue(true));
            miscMenu.AddItem(new MenuItem("jumpkey", "Wardjump Key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));//Standardkey für Wardjump
            _menu.AddSubMenu(miscMenu);

            _menu.AddToMainMenu();
            OnDoCast();
            Orbwalking.OnAttack += OnAa;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            if (_menu.Item("Killsteal").GetValue<bool>())
            {
                Killsteal();
            }
            if (_menu.Item("jumpkey").GetValue<KeyBind>().Active && _menu.Item("usejump").GetValue<bool>())
            {
                WardJump();
            }
            Combo();
        }

        private static void Combo()
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            Obj_AI_Hero targetQ = TargetSelector.GetTarget(_menu.Item("qsetting").GetValue<Slider>().Value, TargetSelector.DamageType.Physical);
            Obj_AI_Hero target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Magical);

            // IITEMS
            if (target != null && Player.Distance(target) <= botrk.Range)
            {
                botrk.Cast(target);
            }
            if (target != null && Player.Distance(target) <= cutlass.Range)
            {
                cutlass.Cast(target);
            }
            if (target != null && Player.Distance(target) <= hextech.Range)
            {
                hextech.Cast(target);
            }


            // ACTUAL COMBO
            if (targetQ != null && !targetQ.IsZombie)
            {
                if (Q.IsReady() && _menu.Item("useQ").GetValue<bool>())
                {
                    if ((Player.Distance(targetQ.Position) > Orbwalking.GetRealAutoAttackRange(Player)) || _menu.Item("useQ2").GetValue<bool>())
                    {
                        Q.CastOnUnit(targetQ);
                    }
                }
                if (E.IsReady() && (_menu.Item("useE").GetValue<bool>()))
                {
                    if (!IsEUsed && Q.IsReady() && targetQ.IsValidTarget(Q.Range) || !IsEUsed && Player.Distance(targetQ.Position) < 250)
                    {
                        E.Cast();
                    }
                    if (_menu.Item("useE2").GetValue<bool>() && IsEUsed && (Player.Distance(targetQ.Position) > 50))
                    {
                        E.Cast();
                    }
                }
                if ((_menu.Item("useR").GetValue<bool>() && Q.IsReady()) || (_menu.Item("useR").GetValue<bool>() && !Q.IsReady() && Player.Distance(targetQ.Position) < 300)) R.Cast();
            }
        }

        private static void OnDoCast()
        {
            Obj_AI_Base.OnDoCast += (sender, args) =>
            {
                //if (!sender.IsMe || !Orbwalking.IsAutoAttack((args.SData.Name))) return;
                if (sender.IsMe && args.SData.IsAutoAttack())
                {
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    {
                        if (_menu.Item("useW").GetValue<bool>() && W.IsReady()) W.Cast();
                    }

                    // Jungleclear 
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (args.Target is Obj_AI_Minion)
                        {
                            var allJungleMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All,
                        MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                            if (allJungleMinions.Count != 0)
                            {
                                /*if (_menu.Item("JungleClearE").GetValue<bool>() && E.IsReady() && !IsEUsed)
                                {
                                    foreach (var minion in allJungleMinions)
                                    {
                                        if (minion.IsValidTarget())
                                        {
                                            E.Cast(minion);
                                        }
                                    }
                                }*/
                                if (_menu.Item("jungleclearQ").GetValue<bool>() && Q.IsReady())
                                {
                                    foreach (var minion in allJungleMinions)
                                    {
                                        if (minion.IsValidTarget())
                                        {
                                            Q.CastOnUnit(minion);
                                        }
                                    }
                                }
                                if (_menu.Item("jungleclearW").GetValue<bool>() && W.IsReady())
                                {
                                    foreach (var minion in allJungleMinions)
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

                    // Laneclear
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (args.Target is Obj_AI_Minion)
                        {
                            var allLaneMinions = MinionManager.GetMinions(Q.Range);
                            //Lane
                            if (_menu.Item("laneclearW").GetValue<bool>() && W.IsReady())
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
            };
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target != null && args.Target.IsMe && args.SData.IsAutoAttack() && _menu.Item("jungleclearE").GetValue<bool>() && E.IsReady() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && sender.Team == GameObjectTeam.Neutral)
            {
                E.Cast();
             }
        }

        private static void OnDraw(EventArgs args)
        {
            if (_menu.Item("drawsetQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, _menu.Item("qsetting").GetValue<Slider>().Value,
                    Color.Tan);
            }
            if (_menu.Item("drawAa").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player),
                    Color.Blue);
            }
        }
        static int CanKill(Obj_AI_Hero target, bool useq)
        {
            double damage = 0;
            if (!useq)
                return 0;
            if (Q.IsReady())
            {
                damage += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);              
            }            
            if (damage >= target.Health)
            {
                return 1;
            }
            return damage >= target.Health ? 2 : 0;

        }
        private static void Killsteal()
        {
            foreach (Obj_AI_Hero enemy in AllEnemy)
            {
                if (enemy == null || enemy.HasBuffOfType(BuffType.Invulnerability))
                    return;

                if (CanKill(enemy,  _menu.Item("useQ").GetValue<bool>()) == 1 && enemy.IsValidTarget(390))
                {
                    Q.Cast(enemy);
                    return;
                }
            }
        }

        private static void OnAa(AttackableUnit unit, AttackableUnit target)
        {
            Obj_AI_Hero y = TargetSelector.GetTarget(185, TargetSelector.DamageType.Physical);
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {               
                    if (hydra.IsOwned() && Player.Distance(y) < hydra.Range && hydra.IsReady() && !W.IsReady()
                        && !IsWUsed)
                        hydra.Cast();
                    if (tiamat.IsOwned() && Player.Distance(y) < tiamat.Range && tiamat.IsReady() && !W.IsReady()
                        && !IsWUsed)
                        tiamat.Cast();                
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (hydra.IsOwned() && Player.Distance(y) < hydra.Range && hydra.IsReady() && !W.IsReady()
                        && !IsWUsed)
                    hydra.Cast();
                if (tiamat.IsOwned() && Player.Distance(y) < tiamat.Range && tiamat.IsReady() && !W.IsReady()
                        && !IsWUsed)
                    tiamat.Cast();
            }
        }

        public static void WardJump()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (!Q.IsReady())
            {
                return;
            }
            Vector3 wardJumpPosition = (Player.Position.Distance(Game.CursorPos) < 600) ? Game.CursorPos : Player.Position.Extend(Game.CursorPos, 600);
            var lstGameObjects = ObjectManager.Get<Obj_AI_Base>().ToArray();
            Obj_AI_Base entityToWardJump = lstGameObjects.FirstOrDefault(obj =>
                obj.Position.Distance(wardJumpPosition) < 150
                && (obj is Obj_AI_Minion || obj is Obj_AI_Hero)
                && !obj.IsMe && !obj.IsDead
                && obj.Position.Distance(Player.Position) < Q.Range);

            if (entityToWardJump != null)
            {
                Q.Cast(entityToWardJump);
            }
            else
            {
                int wardId = GetWardItem();


                if (wardId != -1 && !wardJumpPosition.IsWall())
                {
                    PutWard(wardJumpPosition.To2D(), (ItemId)wardId);
                    lstGameObjects = ObjectManager.Get<Obj_AI_Base>().ToArray();
                    Q.Cast(
                        lstGameObjects.FirstOrDefault(obj =>
                        obj.Position.Distance(wardJumpPosition) < 150 &&
                        obj is Obj_AI_Minion && obj.Position.Distance(Player.Position) < Q.Range));
                }
            }
        }
        public static int GetWardItem()
        {
            int[] wardItems = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };
            foreach (var id in wardItems.Where(id => Items.HasItem(id) && Items.CanUseItem(id)))
                return id;
            return -1;
        }
        public static void PutWard(Vector2 pos, ItemId warditem)
        {

            foreach (var slot in Player.InventoryItems.Where(slot => slot.Id == warditem))
            {
                ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, pos.To3D());
                return;
            }
        }
    }
}

