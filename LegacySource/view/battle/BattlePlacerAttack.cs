using System;
using System.Collections.Generic;
using game.battle.div;
using game.battle.task;
using game.battle.traj;
using game.entity;
using game.graphics;
using game.input;
using game.map;
using game.render;
using game.ui;
using game.util;

namespace game.battle
{
    public class BattleAttack : BattleMode
    {
        private const int More = 10;

        private readonly Game game;
        private readonly BattleView battleView;
        private readonly BattleSelection selection;
        private readonly BattleTaskFactory taskFactory;
        private readonly BattleTrajectory trajectory;

        private bool melee;
        private bool ranged;
        private bool artillery;

        public BattleAttack(Game game, BattleView battleView, BattleSelection selection, BattleTaskFactory taskFactory, BattleTrajectory trajectory)
        {
            this.game = game;
            this.battleView = battleView;
            this.selection = selection;
            this.taskFactory = taskFactory;
            this.trajectory = trajectory;
        }

        public override void Start()
        {
            melee = true;
            ranged = true;
            artillery = true;
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (melee)
                {
                    AttackMelee();
                }
                else if (ranged)
                {
                    AttackRanged();
                }
                else if (artillery)
                {
                    AttackArtillery();
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleAttackMode();
            }
        }

        private void AttackMelee()
        {
            var target = GetTarget();
            if (target != null)
            {
                var task = taskFactory.CreateMeleeAttackTask(selection.SelectedUnits, target);
                selection.SelectedUnits.ForEach(unit => unit.SetTask(task));
            }
        }

        private void AttackRanged()
        {
            var target = GetTarget();
            if (target != null)
            {
                var task = taskFactory.CreateRangedAttackTask(selection.SelectedUnits, target);
                selection.SelectedUnits.ForEach(unit => unit.SetTask(task));
            }
        }

        private void AttackArtillery()
        {
            var target = GetTarget();
            if (target != null)
            {
                var task = taskFactory.CreateArtilleryAttackTask(selection.SelectedUnits, target);
                selection.SelectedUnits.ForEach(unit => unit.SetTask(task));
            }
        }

        private void CycleAttackMode()
        {
            if (melee)
            {
                melee = false;
                ranged = true;
            }
            else if (ranged)
            {
                ranged = false;
                artillery = true;
            }
            else if (artillery)
            {
                artillery = false;
                melee = true;
            }
        }

        private Entity GetTarget()
        {
            // Implement target selection logic
            return null;
        }
    }
}