using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FuelBurning
{
    public enum SparksFlyResult : byte
    {
        Undefine,
        ZeroValue,
        GrowFire,
        TryBurning
    }

    class FlammableLinkComp : ThingComp
    {
        private const float CorrectIgnitionChance = 0.7f;
        private const float CorrectWeatherFactorCooling = -2.0f;
        private const float CorrectWeatherFactorHeating = 0.3f;
        
        private CompProperties_FlammableLink compDef;
        private bool burningNowInt;
        public bool BurningNow
        {
            get { return this.burningNowInt; }
            protected set { this.burningNowInt = value; }
        }
        private float amountOfHeatInt;
        public float AmountOfHeat
        {
            get { return this.amountOfHeatInt; }
            protected set { this.amountOfHeatInt = value; }
        }
        public float HeatRatio
        {
            get { return AmountOfHeat / compDef.ignitionDef.maxHeatCapacity; }
        }

        public override void CompTick()
        {
            if (FireUtility.ContainsStaticFire(base.parent.Position, base.parent.Map))
            {
                this.BurningNow = true;
                for (int i = 0; i < 4; i++)
                {
                    IntVec3 vec = base.parent.Position + GenAdj.CardinalDirectionsAround[i];
                    List<Thing> things = base.parent.Map.thingGrid.ThingsListAt(vec);
                    for (int j = 0; j < things.Count; j++)
                    {
                        FlammableLinkComp comp = things[j].TryGetComp<FlammableLinkComp>();
                        if (comp != null && this.IsConnected(comp))
                        {
                            comp.TrySparksFly(compDef.ignitionDef.spreadPower);
                        }
                    }
                }
            }
            else
            {
                this.BurningNow = false;
                this.AmountOfHeat -= this.WeatherFactor(this.compDef.ignitionDef.easeCooling, CorrectWeatherFactorCooling);
                if (this.AmountOfHeat < 0)
                {
                    this.AmountOfHeat = 0;
                }
            }
        }
        public SparksFlyResult TrySparksFly(float sp)
        {
            if(sp == 0)
            {
                return SparksFlyResult.ZeroValue;
            }
            this.AmountOfHeat += this.WeatherFactor(sp, CorrectWeatherFactorHeating);
            if (this.BurningNow)
            {
                Fire curFire = base.parent.Map.thingGrid.ThingAt<Fire>(base.parent.Position);
                if(curFire != null)
                {
                    curFire.fireSize = compDef.ignitionDef.firstFireSize;
                    return SparksFlyResult.GrowFire;
                }
            }
            else if (IgnitionChance())
            {
                this.AmountOfHeat = this.compDef.ignitionDef.maxHeatCapacity;
                this.TryBurning(compDef.ignitionDef.firstFireSize);
                return SparksFlyResult.TryBurning;
            }
            return SparksFlyResult.Undefine;
        }
        protected virtual bool IgnitionChance()
        {
            float max = this.compDef.ignitionDef.maxHeatCapacity;
            float now = this.AmountOfHeat;
            float chance = this.compDef.ignitionDef.baseIgnitionChance + ((now / max) - CorrectIgnitionChance);
            return Rand.Chance(chance);
        }
        protected virtual void TryBurning(float fireSize)
        {
            if(FireUtility.ContainsStaticFire(base.parent.Position, base.parent.Map))
            {
                return;
            }

            Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
            fire.fireSize = fireSize;
            GenSpawn.Spawn(fire, base.parent.Position, base.parent.Map, Rot4.North, false);
            return;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                this.AmountOfHeat = 0f;
                this.BurningNow = false;
            }
            this.compDef = base.props as CompProperties_FlammableLink;
            if(this.compDef == null)
            {
                Log.Error("CompProperties_FlammableLink was not found");
                this.compDef = new CompProperties_FlammableLink();
            }
        }

        public float HeatedByHitOf(DamageInfo info)
        {
            foreach(var dDef in this.compDef.ignitionDef.multiplierDirect)
            {
                if(info.Def == dDef.damageDef)
                {
                    return info.Amount * dDef.multiplier;
                }
            }
            return 0f;
        }

        public float HeatedBySparksOf(DamageInfo info)
        {
            foreach (var dDef in this.compDef.ignitionDef.multiplierSparks)
            {
                if (info.Def == dDef.damageDef)
                {
                    return info.Amount * dDef.multiplier;
                }
            }
            return 0f;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if ((!this.compDef.ignitionDef.fireRemains) && mode == DestroyMode.KillFinalize)
            {
                List<Thing> list = previousMap.thingGrid.ThingsListAt(parent.Position);
                for (int i = 0; i < list.Count; i++)
                {
                    Fire fire = list[i] as Fire;
                    if (fire != null && fire.parent == null && !fire.Destroyed)
                    {
                        fire.Destroy(DestroyMode.KillFinalize);
                    }
                }
            }
        }
        protected float WeatherFactor(float sp, float coeff)
        {
            return sp - (sp * base.parent.Map.weatherManager.curWeather.rainRate * coeff);
        }
        protected bool IsConnected(FlammableLinkComp comp)
        {
            return comp.compDef.connectionID == this.compDef.connectionID;
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look<bool>(ref this.burningNowInt, "burningNowInt", false);
            Scribe_Values.Look<float>(ref this.amountOfHeatInt, "amountOfHeatInt", 0f);
            base.PostExposeData();
        }
    }
}
