using ArchitectsLibrary.API;
using ArchitectsLibrary.Interfaces;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectsLibrary.Items
{
    class CyclopsModuleTest : CyclopsUpgrade, ICyclopsOnModulesUpdated
    {
        public CyclopsModuleTest() : base("CyclopsModuleTest", LanguageSystem.Default, LanguageSystem.Default)
        {
        }

        public void OnModuleCountChanged(SubRoot cyclops, int modulesCount)
        {
            ErrorMessage.AddMessage($"Modules count for {ClassID}: {modulesCount}");;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData(new List<Ingredient>() { new Ingredient(TechType.Titanium, 2)});
        }
    }
}
