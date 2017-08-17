using System.Collections.Generic;
using Verse;

namespace FuelBurning
{
    class IgnitionDef : Def
    {
        public float spreadPower = 1f;
        public float firstFireSize = 1f;
        public float maxHeatCapacity = 30f;
        public float easeCooling = 0.001f;
        public float baseIgnitionChance = 0.5f;

        public bool fireRemains = true;

        public List<DamageMultiplier> multiplierDirect = new List<DamageMultiplier>();
        public List<DamageMultiplier> multiplierSparks = new List<DamageMultiplier>();
    }
}
