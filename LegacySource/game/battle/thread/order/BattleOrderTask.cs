using System;
using System.Collections.Generic;
using System.IO;

namespace game.battle.thread.order
{
    public class BattleOrderTask : Copyable<BattleOrderTask>
    {
        private DIVTASK task = DIVTASK.MOVE;
        private int target;
        private bool orderedWhenFighting;

        public enum DIVTASK
        {
            STOP,
            MOVE,
            ATTACK_BUILDING,
            ATTACK_MELEE,
            ATTACK_RANGED,
            CHARGE,
        }

        public static readonly List<DIVTASK> all = new List<DIVTASK>(Enum.GetValues(typeof(DIVTASK)));

        public DIVTASK Task()
        {
            return task;
        }

        private void Set(DIVTASK t, Div div)
        {
            orderedWhenFighting = div.Status().IsFighting();
            this.task = t;
        }

        public void Move(Div div)
        {
            Set(DIVTASK.MOVE, div);
        }

        public BattleOrderTask Stop(Div div)
        {
            Set(DIVTASK.STOP, div);
            return this;
        }

        public void Attack(int tx, int ty, Div div)
        {
            target = tx | (ty << 16);
            Set(DIVTASK.ATTACK_BUILDING, div);
        }

        public void AttackMelee(Div other, Div div)
        {
            target = other.Index();
            Set(DIVTASK.ATTACK_MELEE, div);
        }

        public void AttackRanged(Div other, Div div)
        {
            target = other.Index();
            Set(DIVTASK.ATTACK_RANGED, div);
        }

        public void Charge(Div div)
        {
            Set(DIVTASK.CHARGE, div);
        }

        public void Save(BinaryWriter file)
        {
            file.Write((int)task);
            file.Write(target);
            file.Write(orderedWhenFighting);
        }

        public void Load(BinaryReader file)
        {
            if (false)
            {
                //need an attack ranged building (catapults)
            }
            task = all[file.ReadInt32()];
            target = file.ReadInt32();
            orderedWhenFighting = file.ReadBoolean();
        }

        public void Clear()
        {
            task = DIVTASK.STOP;
            target = 0;
            orderedWhenFighting = false;
        }

        public void Copy(BattleOrderTask toBeCopied)
        {
            task = toBeCopied.task;
            target = toBeCopied.target;
            orderedWhenFighting = toBeCopied.orderedWhenFighting;
        }

        public Div TargetDiv()
        {
            if (task != DIVTASK.ATTACK_MELEE && task != DIVTASK.ATTACK_RANGED)
                return null;
            return GAME.ARMIES().Division((short)target);
        }

        public int TargetTileX()
        {
            if (task != DIVTASK.ATTACK_BUILDING)
                return -1;
            return target & 0x0FFFF;
        }

        public int TargetTileY()
        {
            if (task != DIVTASK.ATTACK_BUILDING)
                return -1;
            return (target >> 16) & 0x0FFFF;
        }

        public bool OrderedWhenFighting()
        {
            return orderedWhenFighting;
        }
    }
}