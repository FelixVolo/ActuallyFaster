using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using EntityStates.Scrapper;
using EntityStates.Duplicator;
//using UnityEngine.Networking;
//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using UnityEngine;

namespace ActuallyFaster
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //We will be using 2 modules from R2API: ItemAPI to add our item and LanguageAPI to add our language tokens.
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    
    
    public class ActuallyFaster : BaseUnityPlugin
	{
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Felda";
        public const string PluginName = "ActuallyFaster";
        public const string PluginVersion = "1.0.8";
        public static ConfigEntry<bool> scrapper { get; set; }
        public static ConfigEntry<bool> printer { get; set; }
        public static ConfigEntry<bool> chanceShrine { get; set; }
        public static ConfigEntry<bool> cauldron { get; set; }
        public static ConfigEntry<bool> cleansingPool { get; set; }
        public static ConfigEntry<bool> chestDrop { get; set; }

        //We need our item definition to persist through our functions, and therefore make it a class field.

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            scrapper = base.Config.Bind<bool>("Actually Faster", "Scrapper brrrrrt", true, "Should the scrapper go brrrrrt?");
            printer = base.Config.Bind<bool>("Actually Faster", "Printer brrrrrt", true, "Should the printer go brrrrrt?");
            chanceShrine = base.Config.Bind<bool>("Actually Faster", "Chance Shrine brrrrrt", true, "Should the chance shrine go brrrrrt?");
            cauldron = base.Config.Bind<bool>("Actually Faster", "Cauldron brrrrrt", true, "Should the cauldrons go brrrrrt?");
            cleansingPool = base.Config.Bind<bool>("Actually Faster", "Cleansing Pool brrrrrt", true, "Should the cleansing pool go brrrrrt?");
            chestDrop = base.Config.Bind<bool>("Actually Faster", "Chest drop brrrrrt", true, "Should the chests go brrrrrt?");
            if (scrapper.Value)
            {
                On.RoR2.Stage.Start += delegate (On.RoR2.Stage.orig_Start orig, Stage self)
                {
                    typeof(WaitToBeginScrapping).SetFieldValue("duration", 0f);
                    typeof(Scrapping).SetFieldValue("duration", 0f);
                    typeof(ScrappingToIdle).SetFieldValue("duration", 0f);
                    orig(self);
                };
            }
            if (printer.Value)
            {
                On.RoR2.Stage.Start += delegate (On.RoR2.Stage.orig_Start orig, Stage self)
                {
                    typeof(Duplicating).SetFieldValue("initialDelayDuration", 0.1f);
                    typeof(Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", 0.0f);
                    orig(self);
                };
                On.EntityStates.Duplicator.Duplicating.DropDroplet += (orig, self) =>
                {
                    self.outer.GetComponent<PurchaseInteraction>().Networkavailable = true;
                    orig(self);
                };
            }
            if (chestDrop.Value)
            {
                On.RoR2.ChestBehavior.ItemDrop += (orig, self) =>
                {
                    self.SetFieldValue("dropUpVelocityStrength", 0.0f);
                    self.SetFieldValue("dropForwardVelocityStrength", 10.0f);
                    orig(self);
                };
            }
            if (chanceShrine.Value || cauldron.Value || cleansingPool.Value)
            {
                On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
                {
                    orig(self, activator);
                    if(self == null)
                    {
                        return;
                    }
                    if (chanceShrine.Value)
                    {
                        ShrineChanceBehavior behavior = self.GetComponent<ShrineChanceBehavior>();
                        if(behavior != null)
                        {
                            behavior.refreshTimer = 0.1f;
                        }
                    }
    
                    if ((self.costType == CostTypeIndex.WhiteItem || self.costType == CostTypeIndex.GreenItem || self.costType == CostTypeIndex.RedItem) && cauldron.Value && (self.Networkcost > 0) && !self.isShrine)
                    {
                        self.available = true;
                    }

                    if (self.costType == CostTypeIndex.LunarItemOrEquipment && self.Networkcost > 0 && cleansingPool.Value)
                    {
                        self.available = true;
                    }
                };
            }
        }
    }
}
