using System.Drawing;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Reflection;
using ExampleCalloutsSRC.Callouts;

namespace ExampleCalloutsSRC
{
    public class Main : Plugin
    {
        public Main()
        {
        }
        public override void Finally()
        {
            Game.DisplayHelp("ExampleCallouts loaded without problems.");
        }
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
            Game.LogTrivial("ExampleCallouts plugin has loaded!");
        }
        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                string version = Assembly.GetExecutingAssembly()
                    .GetName()
                    .Version
                    .ToString();

                Game.DisplayNotification(
                    "3dtextures",
                    "mpgroundlogo_cops", //Find all logos in OpenIV
                    "ExampleCallouts",
                    "~y~v.0.0.0.1 ~o~ by sEbi3",
                    "~b~successfully loaded!");

                Functions.RegisterCallout(typeof(DrugDeal));
            }
        }
    }
}
