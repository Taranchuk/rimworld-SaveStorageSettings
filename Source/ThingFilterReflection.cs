using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace SaveStorageSettings
{
    internal class ThingFilterReflection
    {
        private readonly ThingFilter filter;

        internal ThingFilterReflection(ThingFilter filter)
        {
            this.filter = filter;
        }

        internal IEnumerable<ThingDef> AllStorableThingDefs => from def in DefDatabase<ThingDef>.AllDefs
                                                               where def.EverStorable(true)
                                                               select def;

        internal HashSet<ThingDef> AllowedDefs
        {
            get => (HashSet<ThingDef>)GetPrivateFieldInfo("allowedDefs").GetValue(filter);
            set => GetPrivateFieldInfo("allowedDefs").SetValue(filter, value);
        }

        internal List<SpecialThingFilterDef> DisallowedSpecialFilters
        {
            get => (List<SpecialThingFilterDef>)GetPrivateFieldInfo("disallowedSpecialFilters").GetValue(filter);
            set => GetPrivateFieldInfo("disallowedSpecialFilters").SetValue(filter, value);
        }

        internal QualityRange AllowedQualities => (QualityRange)GetPrivateFieldInfo("allowedQualities").GetValue(filter);

        internal void SettingsChangedCallback()
        {
            Action a = (Action)GetPrivateFieldInfo("settingsChangedCallback").GetValue(filter);
            if (a != null)
            {
                a.Invoke();
            }
        }

        private FieldInfo GetPrivateFieldInfo(string name)
        {
            return typeof(ThingFilter).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /*internal List<string> SpecialFiltersToAllow
        {
            get
            {
                return (List<string>)this.GetPrivateFieldInfo("specialFiltersToAllow").GetValue(this.filter);
            }
            set
            {
                this.GetPrivateFieldInfo("specialFiltersToAllow").SetValue(this.filter, value);
            }
        }

        internal List<string> SpecialFiltersToDisallow
        {
            get
            {
                return (List<string>)this.GetPrivateFieldInfo("specialFiltersToDisallow").GetValue(this.filter);
            }
            set
            {
                this.GetPrivateFieldInfo("specialFiltersToDisallow").SetValue(this.filter, value);
            }
        }*/
    }
}
