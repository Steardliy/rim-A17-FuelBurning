using RimWorld;
using UnityEngine;
using Verse;

namespace FuelBurning
{
    class Designator_CleanFuel : Designator_Deconstruct
    {
        public Designator_CleanFuel()
        {
            this.defaultLabel = "DesignatorCleanFuel".Translate();
            this.defaultDesc = "DesignatorCleanFuelDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/CleanFuel", true);
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.DesignateDeconstruct;
            this.hotKey = null;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (base.CanDesignateThing(t).Accepted)
            {
                if(t is Building_SprinkledFuel)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
