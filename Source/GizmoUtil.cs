using RimWorld;
using SaveStorageSettings.Dialog;
using System.Collections.Generic;
using Verse;

namespace SaveStorageSettings
{
    public static class GizmoUtil
    {
        public static IEnumerable<Gizmo> YieldSaveLoadZoneGizmos(string storageTypeName, ThingFilter thingFilter, int groupKey = 987767552)
        {
            yield return new Command_Action
            {
                icon = HarmonyPatches.SaveTexture,
                defaultLabel = "SaveStorageSettings.SaveZoneSettings".Translate(),
                defaultDesc = "SaveStorageSettings.SaveZoneSettingsDesc".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(new SaveFilterDialog(storageTypeName, thingFilter));
                },
                groupKey = groupKey
            };

            yield return new Command_Action
            {
                icon = HarmonyPatches.LoadTexture,
                defaultLabel = "SaveStorageSettings.LoadZoneSettings".Translate(),
                defaultDesc = "SaveStorageSettings.LoadZoneSettingsDesc".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate
                {
                    Find.WindowStack.Add(new LoadFilterDialog(storageTypeName, thingFilter));
                },
                groupKey = groupKey + 1
            };
        }
    }
}
