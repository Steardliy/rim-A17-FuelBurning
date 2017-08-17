using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FuelBurning
{
    class PlaceWorker_SprinkledFuel : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            ThingDef checkThingDef = checkingDef as ThingDef;
            CompProperties_FlammableLink prop = checkThingDef.GetCompProperties<CompProperties_FlammableLink>();
            if (prop == null)
            {
                return true;
            }

            List<Thing> things = loc.GetThingList(base.Map);
            for(int i = 0; i < things.Count; i++)
            {
                if (things[i].def == ThingDefOf.Wall)
                {
                    return false;
                }
                CompProperties_FlammableLink prop2 = things[i].def.GetCompProperties<CompProperties_FlammableLink>();
                if (prop2 != null && prop.connectionID == prop2.connectionID)
                {
                    return false;
                }

                BuildableDef curBuildDef = GenConstruct.BuiltDefOf(things[i].def);
                if (curBuildDef != null && curBuildDef is ThingDef)
                {
                    ThingDef curDef = (ThingDef)curBuildDef;
                    if(curDef == ThingDefOf.Wall)
                    {
                        return false;
                    }
                    prop2 = curDef.GetCompProperties<CompProperties_FlammableLink>();
                    if (prop2 != null && prop.connectionID == prop2.connectionID)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
