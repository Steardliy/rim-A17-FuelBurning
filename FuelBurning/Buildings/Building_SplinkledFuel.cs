using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FuelBurning
{
    class Building_SprinkledFuel : Building
    {
        private const float FlameTick = 150f;
        private const float FlameDamage = 3f;
        private const float FlameDamagePerSec = (FlameTick / FlameDamage) / 60f;
        private const int TicksCheckCellInterval = 20;
        private const float AttachSparksHeat = TicksCheckCellInterval * 2f;
        private const float SparksRangeOrigin = 4.55f;
        private const float SparksRangeDestination = 1.5f;


        private float innerHitpointInt;
        public float InnerHitpoint
        {
            get { return this.innerHitpointInt; }
            set { this.innerHitpointInt = value; }
        }
        private Graphic graphicInt;
        public override Graphic Graphic
        {
            get
            {
                if (this.graphicInt == null)
                {
                    this.graphicInt = GraphicDatabase.Get<Graphic_LinkedCornerComplement>(base.def.graphicData.texPath, ShaderDatabase.Transparent);
                }
                return this.graphicInt;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if(this.IsHashIntervalTick(TicksCheckCellInterval))
            {
                FlammableLinkComp comp = base.GetComp<FlammableLinkComp>();
                if (comp.BurningNow)
                {
                    return;
                }
                List<Thing> things = base.Map.thingGrid.ThingsListAt(base.Position);
                for (int i = 0; i < things.Count; i++)
                {
                    this.CheckSparksFromPawn(things[i] as Pawn, comp);
                    this.CheckOverlapBullet(things[i] as Bullet, comp);
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                this.InnerHitpoint = this.GetStatValue(BTF_StatsDefOf.BurningTime, true) * FlameDamagePerSec;
            }
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            FlammableLinkComp comp = this.TryGetComp<FlammableLinkComp>();
            if (comp != null)
            {
                if (FireUtility.ContainsStaticFire(base.Position, base.Map))
                {
                    this.InnerHitpoint -= dinfo.Amount;
                    if (this.InnerHitpoint <= 0)
                    {
                        this.InnerHitpoint = 0;
                        if (!this.Destroyed)
                        {
                            this.Kill(dinfo);
                        }
                    }
                }
                else
                {
                    float heat = comp.HeatedByHitOf(dinfo);
                    comp.TrySparksFly(heat);
                    MoteUtility.DrawHeatedMote(comp.HeatRatio, base.DrawPos, base.Position, base.Map);
                }
#if DEBUG
                Log.Message("pos:" + base.Position + " type:" + dinfo.Def.ToString() + " amount:" + dinfo.Amount + " sp:" + comp.HeatedByHitOf(dinfo) + " heat:" + comp.AmountOfHeat + " hp:" + this.InnerHitpoint);
#endif
            }
        }
        private void CheckSparksFromPawn(Pawn pawn, FlammableLinkComp comp)
        {
            if (pawn == null)
            {
                return;
            }
            if (pawn.HasAttachment(ThingDefOf.Fire))
            {
                if (comp.TrySparksFly(AttachSparksHeat) == SparksFlyResult.Undefine)
                {
                    MoteUtility.DrawHeatedMote(comp.HeatRatio, base.DrawPos, base.Position, base.Map);
                }
            }
        }
        private void CheckOverlapBullet(Bullet bullet, FlammableLinkComp comp)
        {
            if (bullet == null)
            {
                return;
            }

            

            FieldInfo originInfo = bullet.GetType().GetField("origin", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 origin = (Vector3)originInfo.GetValue(bullet);
            Vector3 pos = new Vector3(base.DrawPos.x, base.DrawPos.y, base.DrawPos.z);
            float distanceSqr = (origin - pos).sqrMagnitude;

            if (distanceSqr <= SparksRangeOrigin * SparksRangeOrigin)
            {
                DamageInfo dinfo = new DamageInfo(bullet.def.projectile.damageDef, bullet.def.projectile.damageAmountBase);
                float heat = comp.HeatedBySparksOf(dinfo);
                if (comp.TrySparksFly(heat) == SparksFlyResult.Undefine)
                {
                    MoteUtility.DrawHeatedMote(comp.HeatRatio, base.DrawPos, base.Position, base.Map);
                }
#if DEBUG
                Log.Message("Near Def:" + dinfo.Def.ToString() + " origin:" + origin + " base:" + base.DrawPos + " distanceSqr:" + distanceSqr);
#endif
            }

            FieldInfo destinationInfo = bullet.GetType().GetField("destination", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
            Vector3 destination = (Vector3)destinationInfo.GetValue(bullet);
            pos.y = 0f;
            distanceSqr = (destination - pos).sqrMagnitude;

            if (distanceSqr <= SparksRangeDestination * SparksRangeDestination)
            {
                DamageInfo dinfo = new DamageInfo(bullet.def.projectile.damageDef, bullet.def.projectile.damageAmountBase);
                float heat = comp.HeatedBySparksOf(dinfo);
                if (comp.TrySparksFly(heat) == SparksFlyResult.Undefine)
                {
                    MoteUtility.DrawHeatedMote(comp.HeatRatio, base.DrawPos, base.Position, base.Map);
                }
#if DEBUG
                Log.Message("Near Def:" + dinfo.Def.ToString() + " destination:" + destination + " base:" + base.DrawPos + " distanceSqr:" + distanceSqr);
#endif
            }
        }
        public override void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.innerHitpointInt, "innerHitpointInt", 10);
            base.ExposeData();
        }
    }
}
