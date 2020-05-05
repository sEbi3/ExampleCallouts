using Rage;
using LSPD_First_Response.Mod.API;
using System.Reflection;
using ExampleCalloutsSRC.Callouts;
using ExampleCalloutsSRC.VersionCheckers;

namespace ExampleCalloutsSRC
{
    public class Main : Plugin
    {
        public override void Finally()
        {
            Game.DisplayHelp("ExampleCallouts loaded without problems.");
        }
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
            Game.Console.Print();
            Game.Console.Print("=============================================== ExampleCallouts by sEbi3 ================================================");
            Game.Console.Print();
            Game.Console.Print("ExampleCallouts loaded successfully.");
            Game.Console.Print("Detected Version:  " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Game.Console.Print();
            Game.Console.Print("=============================================== ExampleCallouts by sEbi3 ================================================");
            Game.Console.Print();
        }
        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                string version = Assembly.GetExecutingAssembly()
                    .GetName()
                    .Version
                    .ToString();

                VersionChecker.isUpdateAvailable();
                Game.DisplayNotification(
                    "web_lossantospolicedept", // You can find all logos/images in OpenIV
                    "web_lossantospolicedept", // You can find all logos/images in OpenIV
                    "ExampleCallouts", // Your Callout/Plugin name 
                    "~y~v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ~o~by sEbi3", "~b~successfully loaded!");
            }
        }
        private static void RegisterCallouts()             //Register all your callouts here
        {
            Functions.RegisterCallout(typeof(DrugDeal));
            Functions.RegisterCallout(typeof(CarTrade));
            Functions.RegisterCallout(typeof(WelfareCheck));
        }
    }
}