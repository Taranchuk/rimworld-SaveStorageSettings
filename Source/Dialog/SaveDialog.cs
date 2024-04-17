﻿using RimWorld;
using System.IO;
using Verse;

namespace SaveStorageSettings.Dialog
{
    class SaveFilterDialog : FileListDialog
    {
        private readonly ThingFilter ThingFilter;

        internal SaveFilterDialog(string storageTypeName, ThingFilter thingFilter) : base(storageTypeName)
        {
            this.ThingFilter = thingFilter;
            this.interactButLabel = "OverwriteButton".Translate();
        }

        protected override bool ShouldDoTypeInField
        {
            get
            {
                return true;
            }
        }

        protected override void DoFileInteraction(FileInfo fi)
        {
            IOUtil.SaveStorageSettings(this.ThingFilter, fi);
            base.Close();
        }
    }

    class SaveCraftingDialog : FileListDialog
    {
        private readonly BillStack BillStack;

        internal SaveCraftingDialog(string type, BillStack billStack) : base(type)
        {
            this.BillStack = billStack;
            this.interactButLabel = "OverwriteButton".Translate();
        }

        protected override bool ShouldDoTypeInField
        {
            get
            {
                return true;
            }
        }

        protected override void DoFileInteraction(FileInfo fi)
        {
            IOUtil.SaveCraftingSettings(this.BillStack, fi);
            base.Close();
        }
    }

    class SaveOperationDialog : FileListDialog
    {
        private readonly Pawn Pawn;

        internal SaveOperationDialog(string type, Pawn pawn) : base(type)
        {
            this.Pawn = pawn;
            this.interactButLabel = "OverwriteButton".Translate();
        }

        protected override bool ShouldDoTypeInField
        {
            get
            {
                return true;
            }
        }

        protected override void DoFileInteraction(FileInfo fi)
        {
            IOUtil.SaveOperationSettings(this.Pawn, fi);
            base.Close();
        }
    }

    class SavePolicyDialog : FileListDialog
    {
        private readonly DrugPolicy DrugPolicy;

        internal SavePolicyDialog(string type, DrugPolicy drugPolicy) : base(type)
        {
            this.DrugPolicy = drugPolicy;
            this.interactButLabel = "OverwriteButton".Translate();
        }

        protected override bool ShouldDoTypeInField
        {
            get
            {
                return true;
            }
        }

        protected override void DoFileInteraction(FileInfo fi)
        {
            IOUtil.SaveDrugPolicySettings(this.DrugPolicy, fi);
            base.Close();
        }
    }

    class SaveFoodRestrictionDialog : FileListDialog
    {
        private readonly FoodPolicy FoodPolicy;

        internal SaveFoodRestrictionDialog(string type, FoodPolicy foodRestriction) : base(type)
        {
            this.FoodPolicy = foodRestriction;
            this.interactButLabel = "OverwriteButton".Translate();
        }

        protected override bool ShouldDoTypeInField
        {
            get
            {
                return true;
            }
        }

        protected override void DoFileInteraction(FileInfo fi)
        {
            IOUtil.SaveFoodPolicySettings(this.FoodPolicy, fi);
            base.Close();
        }
    }

    class SaveReadingPolicyDialog : FileListDialog
    {
        private readonly ReadingPolicy ReadingPolicy;

        internal SaveReadingPolicyDialog(string type, ReadingPolicy readingPolicy) : base(type)
        {
            this.ReadingPolicy = readingPolicy;
            this.interactButLabel = "OverwriteButton".Translate();
        }

        protected override bool ShouldDoTypeInField
        {
            get
            {
                return true;
            }
        }

        protected override void DoFileInteraction(FileInfo fi)
        {
            IOUtil.SaveReadingPolicySettings(this.ReadingPolicy, fi);
            base.Close();
        }
    }

}

