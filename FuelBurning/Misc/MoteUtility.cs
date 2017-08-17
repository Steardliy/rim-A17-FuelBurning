using RimWorld;
using UnityEngine;
using Verse;

namespace FuelBurning
{
    class MoteUtility
    {
        public static void DrawHeatedMote(float ratio, Vector3 drawPos, IntVec3 pos, Map map)
        {
            if (ratio <= 0)
            {
                return;
            }
            if (ratio < 0.33f)
            {
                MoteMaker.ThrowSmoke(drawPos, map, 0.3f + ratio);
            }
            else if (ratio < 0.66f)
            {
                MoteMaker.ThrowMetaPuff(drawPos, map);
                MoteMaker.ThrowMicroSparks(drawPos, map);
            }
            else
            {
                MoteMaker.ThrowMetaPuff(drawPos, map);
                MoteMaker.ThrowMicroSparks(drawPos, map);
                MoteMaker.ThrowFireGlow(pos, map, ratio - 0.3f);
            }
        }
    }
}
