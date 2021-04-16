﻿using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace ProjectAncients
{
    [Menu("Return of the Ancients", SaveOn = MenuAttribute.SaveEvents.ChangeValue, LoadOn = MenuAttribute.LoadEvents.MenuOpened | MenuAttribute.LoadEvents.MenuRegistered)]
    public class Config : ConfigFile
    {
        [Toggle("Override loading screen", Tooltip = "Whether to use the custom Return of the Ancients loading screen. You may want to disable this to use any custom loading screen mods.")]
        public bool OverrideLoadingScreen = true;
    }
}
