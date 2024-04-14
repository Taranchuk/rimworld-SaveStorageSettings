using HarmonyLib;
using RimWorld;
using SaveStorageSettings.Dialog;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using System;
using Verse.Sound;
using System.Linq;

namespace SaveStorageSettings
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        public static readonly Texture2D DeleteXTexture;
        public static readonly Texture2D SaveTexture;
        public static readonly Texture2D LoadTexture;
        public static readonly Texture2D AppendTexture;

        static HarmonyPatches()
        {
            var harmony = new Harmony("com.savestoragesettings.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            DeleteXTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
            SaveTexture = ContentFinder<Texture2D>.Get("UI/save", true);
            LoadTexture = ContentFinder<Texture2D>.Get("UI/load", true);
            AppendTexture = ContentFinder<Texture2D>.Get("UI/append", true);
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Patch_Pawn_GetGizmos
    {
        static FieldInfo OnOperationTab = null;
        static Patch_Pawn_GetGizmos()
        {
            OnOperationTab = typeof(HealthCardUtility).GetField("onOperationTab", BindingFlags.Static | BindingFlags.NonPublic);
        }
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }

            if (!(bool)OnOperationTab.GetValue(null))
                yield break;

            if (!__instance.IsColonist && !__instance.IsPrisoner)
                yield break;

            string type = "OperationHuman";
            if (__instance.RaceProps.Animal)
                type = "OperationAnimal";

            yield return new Command_Action
            {
                icon = HarmonyPatches.SaveTexture,
                defaultLabel = "SaveStorageSettings.SaveOperations".Translate(),
                defaultDesc = "SaveStorageSettings.SaveOperations".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate {
                    Find.WindowStack.Add(new SaveOperationDialog(type, __instance));
                },
                groupKey = 987764552
            };

            yield return new Command_Action
            {
                icon = HarmonyPatches.AppendTexture,
                defaultLabel = "SaveStorageSettings.LoadOperations".Translate(),
                defaultDesc = "SaveStorageSettings.LoadOperations".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate
                {
                    Find.WindowStack.Add(new LoadOperationDialog(__instance, type));
                },
                groupKey = 987764553
            };
        }
    }

    [HarmonyPatch(typeof(Building), "GetGizmos")]
    static class Patch_Building_GetGizmos
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building __instance)
        {
            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }

            if (__instance.def.IsWorkTable)
            {
                string type = GetType(__instance.def.defName);
                if (type == null)
                    yield break;

                yield return new Command_Action
                {
                    icon = HarmonyPatches.SaveTexture,
                    defaultLabel = "SaveStorageSettings.SaveBills".Translate(),
                    defaultDesc = "SaveStorageSettings.SaveBillsDesc".Translate(),
                    activateSound = SoundDef.Named("Click"),
                    action = delegate {
                        Find.WindowStack.Add(new SaveCraftingDialog(type, ((Building_WorkTable)__instance).billStack));
                    },
                    groupKey = 987767552
                };

                yield return new Command_Action
                {
                    icon = HarmonyPatches.AppendTexture,
                    defaultLabel = "SaveStorageSettings.AppendBills".Translate(),
                    defaultDesc = "SaveStorageSettings.AppendBillsDesc".Translate(),
                    activateSound = SoundDef.Named("Click"),
                    action = delegate {
                        Find.WindowStack.Add(new LoadCraftingDialog(type, ((Building_WorkTable)__instance).billStack, LoadCraftingDialog.LoadType.Append));
                    },
                    groupKey = 987767553
                };

                yield return new Command_Action
                {
                    icon = HarmonyPatches.LoadTexture,
                    defaultLabel = "SaveStorageSettings.LoadBills".Translate(),
                    defaultDesc = "SaveStorageSettings.LoadBillsDesc".Translate(),
                    activateSound = SoundDef.Named("Click"),
                    action = delegate
                    {
                        Find.WindowStack.Add(new LoadCraftingDialog(type, ((Building_WorkTable)__instance).billStack, LoadCraftingDialog.LoadType.Replace));
                    },
                    groupKey = 987767554
                };
            }
        }

        private static string GetType(string defName)
        {
            switch (defName)
            {
                case "ButcherSpot":
                case "TableButcher":
                    return "Butcher";
                case "HandTailoringBench":
                case "ElectricTailoringBench":
                    return "TailoringBench";
                case "FueledSmithy":
                case "ElectricSmithy":
                    return "Smithy";
                case "FueledStove":
                case "ElectricStove":
                    return "Stove";
                case "SimpleResearchBench":
                case "HiTechResearchBench":
                    return null;
            }
            return defName;
        }
    }

    [HarmonyPatch(typeof(Building_Storage), "GetGizmos")]
    static class Patch_BuildingStorage_GetGizmos
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building __instance)
        {
            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }

            if (__instance is Building_Storage)
            {
                string type = !(__instance.TryGetComp<CompSaveStorageSettings>()?.props is CompProperties_SaveStorageSettings s) ? GetType(__instance.def.defName) : s.name;
                var SLGizmos = GizmoUtil.YieldSaveLoadZoneGizmos(type, ((Building_Storage)__instance).GetStoreSettings().filter);
                foreach (var aGizmo in SLGizmos)
                {
                    yield return aGizmo;
                }
            }
        }

        private static string GetType(string defName)
        {
            string s = defName.ToLower();
            if (s.Contains("shelf"))
            {
                return "shelf";
            }
            if (s.Contains("clothing"))
            {
                return "Apparel_Management";
            }
            if (s.Contains("hopper"))
            {
                return "Hopper";
            }
            return "Zone_Stockpile";
        }
    }

    [HarmonyPatch(typeof(Building_Bookcase), "GetGizmos")]
    static class Patch_BuildingBookcase_GetGizmos
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_Bookcase __instance)
        {
            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }

            var SLGizmos = GizmoUtil.YieldSaveLoadZoneGizmos("BookCase", __instance.GetStoreSettings().filter);
            foreach (var aGizmo in SLGizmos)
            {
                yield return aGizmo;
            }
        }
    }

    [HarmonyPatch(typeof(CompBiosculpterPod), "CompGetGizmosExtra")]
    static class Patch_CompBiosculpterPod_CompGetGizmosExtra
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompBiosculpterPod __instance)
        {
            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }

            var SLGizmos = GizmoUtil.YieldSaveLoadZoneGizmos("CompBiosculpterPod", __instance.GetStoreSettings().filter);
            foreach (var aGizmo in SLGizmos)
            {
                yield return aGizmo;
            }
        }
    }

    [HarmonyPatch(typeof(Zone_Stockpile), "GetGizmos")]
    static class Patch_Zone_Stockpile_GetGizmos
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Zone_Stockpile __instance)
        {
            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }
            var SLGizmos = GizmoUtil.YieldSaveLoadZoneGizmos("Zone_Stockpile", __instance.GetStoreSettings().filter);
            foreach (var aGizmo in SLGizmos)
            {
                yield return aGizmo;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_ManageApparelPolicies), "DoContentsRect")]
    static class Patch_Dialog_ManageApparelPolicies_DoContentsRect
    {
        static void Postfix(Dialog_ManageApparelPolicies __instance, Rect rect)
        {
            float x = 500;
            if (Widgets.ButtonText(new Rect(x, 0f, 150f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true))
            {
                ApparelPolicy policy = Current.Game.outfitDatabase.MakeNewOutfit();
                SetApparelPolicy(__instance, policy);

                Find.WindowStack.Add(new LoadFilterDialog("Apparel_Management", policy.filter));
            }

            ApparelPolicy selectedPolicy = GetSelectedPolicy(__instance);
            if (selectedPolicy != null)
            {
                if (Widgets.ButtonText(new Rect(x, 50f, 72f, 35f), "SaveStorageSettings.LoadOutfit".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new LoadFilterDialog("Apparel_Management", selectedPolicy.filter));
                }
                if (Widgets.ButtonText(new Rect(x + 77, 50f, 72f, 35f), "SaveStorageSettings.SaveOutfit".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new SaveFilterDialog("Apparel_Management", selectedPolicy.filter));
                }
            }
        }

        private static ApparelPolicy GetSelectedPolicy(Dialog_ManageApparelPolicies dialog)
        {
            return (ApparelPolicy)typeof(Dialog_ManageApparelPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }

        private static void SetApparelPolicy(Dialog_ManageApparelPolicies dialog, ApparelPolicy policy)
        {
            typeof(Dialog_ManageApparelPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, policy, null);
        }
    }

    [HarmonyPatch(typeof(Dialog_ManageDrugPolicies), "DoContentsRect")]
    static class Patch_Dialog_ManageDrugPolicies_DoContentsRect
    {
        static void Postfix(Dialog_ManageDrugPolicies __instance, Rect rect)
        {
            float x = 500;
            if (Widgets.ButtonText(new Rect(x, 0, 150f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true))
            {
                DrugPolicy policy = Current.Game.drugPolicyDatabase.MakeNewDrugPolicy();
                SetDrugPolicy(__instance, policy);

                Find.WindowStack.Add(new LoadDrugPolicyDialog("DrugPolicy", policy));
            }

            DrugPolicy selectedPolicy = GetDrugPolicy(__instance);
            if (selectedPolicy != null)
            {
                if (Widgets.ButtonText(new Rect(x, 50f, 72f, 35f), "LoadGameButton".Translate(), true, false, true))
                {
                    string label = selectedPolicy.label;
                    Find.WindowStack.Add(new LoadDrugPolicyDialog("DrugPolicy", selectedPolicy));
                    selectedPolicy.label = label;
                }
                if (Widgets.ButtonText(new Rect(x + 77, 50f, 72f, 35f), "SaveGameButton".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new SavePolicyDialog("DrugPolicy", selectedPolicy));
                }
            }
        }

        private static DrugPolicy GetDrugPolicy(Dialog_ManageDrugPolicies dialog)
        {
            return (DrugPolicy)typeof(Dialog_ManageDrugPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }

        private static void SetDrugPolicy(Dialog_ManageDrugPolicies dialog, DrugPolicy selectedPolicy)
        {
            typeof(Dialog_ManageDrugPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, selectedPolicy, null);
        }
    }

    [HarmonyPatch(typeof(Dialog_ManageFoodPolicies), "DoContentsRect")]
    static class Patch_Dialog_ManageFoodRestrictions_DoContentsRect
    {
        static void Postfix(Dialog_ManageFoodPolicies __instance, Rect rect)
        {
            float x = 500;
            if (Widgets.ButtonText(new Rect(x, 0, 149f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true))
            {
                FoodPolicy policy = Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
                SetFoodPolicy(__instance, policy);

                Find.WindowStack.Add(new LoadFoodRestrictionDialog("FoodRestriction", policy));
            }

            FoodPolicy selected = GetFoodPolicy(__instance);
            if (selected != null)
            {
                if (Widgets.ButtonText(new Rect(x, 50f, 72, 35f), "LoadGameButton".Translate(), true, false, true))
                {
                    string label = selected.label;
                    Find.WindowStack.Add(new LoadFoodRestrictionDialog("FoodRestriction", selected));
                    selected.label = label;
                }
                if (Widgets.ButtonText(new Rect(x + 77, 50f, 72, 35f), "SaveGameButton".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new SaveFoodRestrictionDialog("FoodRestriction", selected));
                }
            }
        }

        private static FoodPolicy GetFoodPolicy(Dialog_ManageFoodPolicies dialog)
        {
            return (FoodPolicy)typeof(Dialog_ManageFoodPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }

        private static void SetFoodPolicy(Dialog_ManageFoodPolicies dialog, FoodPolicy selectedPolicy)
        {
            typeof(Dialog_ManageFoodPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, selectedPolicy, null);
        }
    }


    [HarmonyPatch(typeof(Dialog_ManageReadingPolicies), "DoContentsRect")]
    static class Patch_Dialog_ManageReadingPolicies_DoContentsRect
    {
        static void Postfix(Dialog_ManageReadingPolicies __instance, Rect rect)
        {
            float x = 500;
            if (Widgets.ButtonText(new Rect(x, 0, 149f, 35f), "SaveStorageSettings.LoadAsNew".Translate(), true, false, true))
            {
                ReadingPolicy policy = Current.Game.readingPolicyDatabase.MakeNewReadingPolicy();
                SetReadingPolicy(__instance, policy);
                Find.WindowStack.Add(new LoadReadingPolicyDialog("ReadingPolicy", policy));
            }

            ReadingPolicy selected = GetReadingPolicy(__instance);
            if (selected != null)
            {
                if (Widgets.ButtonText(new Rect(x, 50f, 72, 35f), "LoadGameButton".Translate(), true, false, true))
                {
                    string label = selected.label;
                    Find.WindowStack.Add(new LoadReadingPolicyDialog("ReadingPolicy", selected));
                    selected.label = label;
                }
                if (Widgets.ButtonText(new Rect(x + 77, 50f, 72, 35f), "SaveGameButton".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new SaveReadingPolicyDialog("ReadingPolicy", selected));
                }
            }
        }

        private static ReadingPolicy GetReadingPolicy(Dialog_ManageReadingPolicies dialog)
        {
            return (ReadingPolicy)typeof(Dialog_ManageReadingPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).GetValue(dialog, null);
        }

        private static void SetReadingPolicy(Dialog_ManageReadingPolicies dialog, ReadingPolicy selectedPolicy)
        {
            typeof(Dialog_ManageReadingPolicies).GetProperty("SelectedPolicy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty).SetValue(dialog, selectedPolicy, null);
        }
    }

    // Fix the redundant allowedDefs in the default "Anything" reading policy's defFilter
    [HarmonyPatch(typeof(ReadingPolicyDatabase), "GenerateStartingPolicies")]
    static class Patch_ReadingPolicyDatabase_GenerateStartingPolicies
    {
        static void Postfix(ReadingPolicyDatabase __instance)
        {
            ReadingPolicy allReadingPolicy = __instance.AllReadingPolicies.First(p => p.label == "AllReadingPolicy".Translate());
            allReadingPolicy.defFilter.SetDisallowAll(null, null);
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.HasComp<CompBook>())
                {
                    allReadingPolicy.defFilter.SetAllow(thingDef, true);
                }
            }
        }
    }

}