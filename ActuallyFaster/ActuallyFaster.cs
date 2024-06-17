using BepInEx;
using BepInEx.Configuration;
//using R2API.Utils;
using RoR2;
using EntityStates.Scrapper;
using EntityStates.Duplicator;
//using BepInEx.Logging;

namespace ActuallyFaster
{
    
    //Plugin Declaration
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class ActuallyFaster : BaseUnityPlugin
	{
        
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Felda";
        public const string PluginName = "ActuallyFaster";
        public const string PluginVersion = "1.0.9";

        //Config entries
        public static ConfigEntry<bool> scrapper { get; set; }
        public static ConfigEntry<bool> printer { get; set; }
        public static ConfigEntry<bool> chanceShrine { get; set; }
        public static ConfigEntry<bool> cauldron { get; set; }
        public static ConfigEntry<bool> cleansingPool { get; set; }
        public static ConfigEntry<bool> chestDrop { get; set; }

        

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake() {

            //Uncomment for debugging
            //base.Logger.Log(LogLevel.Info, "Initializing Actually Faster...");

            scrapper = base.Config.Bind<bool>("Actually Faster", "Scrapper brrrrrt", true, "Should the scrapper go brrrrrt?");
            printer = base.Config.Bind<bool>("Actually Faster", "Printer brrrrrt", true, "Should the printer go brrrrrt?");
            chanceShrine = base.Config.Bind<bool>("Actually Faster", "Chance Shrine brrrrrt", true, "Should the chance shrine go brrrrrt?");
            cauldron = base.Config.Bind<bool>("Actually Faster", "Cauldron brrrrrt", true, "Should the cauldrons go brrrrrt?");
            cleansingPool = base.Config.Bind<bool>("Actually Faster", "Cleansing Pool brrrrrt", true, "Should the cleansing pool go brrrrrt?");
            chestDrop = base.Config.Bind<bool>("Actually Faster", "Chest drop brrrrrt", true, "Should the chests go brrrrrt?");
            
            
            
            // Stopped using reflections to remove dependency on R2API
            if (scrapper.Value)
            {
                On.RoR2.Stage.Start += delegate (On.RoR2.Stage.orig_Start orig, Stage self)
                {
                    WaitToBeginScrapping.duration = 0.0f;
                    Scrapping.duration = 0.0f;
                    ScrappingToIdle.duration = 0.0f;

                    //typeof(WaitToBeginScrapping).SetFieldValue("duration", 0f);
                    //typeof(Scrapping).SetFieldValue("duration", 0f);
                    //typeof(ScrappingToIdle).SetFieldValue("duration", 0f);
                    orig(self);
                };
            }
            if (printer.Value)
            {
                On.RoR2.Stage.Start += delegate (On.RoR2.Stage.orig_Start orig, Stage self)
                {
                    Duplicating.initialDelayDuration = 0.1f;
                    Duplicating.timeBetweenStartAndDropDroplet = 0.0f;

                    //typeof(Duplicating).SetFieldValue("initialDelayDuration", 0.1f);
                    //typeof(Duplicating).SetFieldValue("timeBetweenStartAndDropDroplet", 0.0f);
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

                    self.dropUpVelocityStrength = 0.0f;
                    self.dropForwardVelocityStrength = 0.0f;

                    //self.SetFieldValue("dropUpVelocityStrength", 0.0f);
                    //self.SetFieldValue("dropForwardVelocityStrength", 10.0f);
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
