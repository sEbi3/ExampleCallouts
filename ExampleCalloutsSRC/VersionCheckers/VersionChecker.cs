using System;
using Rage;
using System.Net;

namespace ExampleCalloutsSRC.VersionCheckers
{
    public class VersionChecker
    {
        public static bool isUpdateAvailable()
        {
            string curVersion = Settings.CalloutVersion;

            Uri latestVersionUri = new Uri("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=20730&textOnly=1"); //Use instead of "20730" your file number on lcpdfr.com
            WebClient webClient = new WebClient();
            string receivedData = string.Empty;

            try
            {
                receivedData = webClient.DownloadString("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=20730&textOnly=1").Trim(); //Use instead of "20730" your file number on lcpdfr.com
            }
            catch (WebException)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~ExampleCallouts Warning", "~y~Failed to check for a update", "Please check if you are ~o~online~w~, or try to reload the plugin.");

                Game.Console.Print();
                Game.Console.Print("================================================ ExampleCallouts WARNING =====================================================");
                Game.Console.Print();
                Game.Console.Print("Failed to check for a update.");
                Game.Console.Print("Please check if you are online, or try to reload the plugin.");
                Game.Console.Print();
                Game.Console.Print("================================================ ExampleCallouts WARNING =====================================================");
                Game.Console.Print();
            }
            if (receivedData != Settings.CalloutVersion)
            {
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~ExampleCallouts Warning", "~r~A new Update is available!", "Current Version: ~r~" + curVersion + "~w~<br>New Version: ~g~" + receivedData);

                Game.Console.Print();
                Game.Console.Print("================================================ ExampleCallouts WARNING =====================================================");
                Game.Console.Print();
                Game.Console.Print("A new version of ExampleCallouts is available! Update the Version, or play on your own risk.");
                Game.Console.Print("Current Version:  " + curVersion);
                Game.Console.Print("New Version:  " + receivedData);
                Game.Console.Print();
                Game.Console.Print("================================================ ExampleCallouts WARNING =====================================================");
                Game.Console.Print();
                return true;
            }
            else
            {
                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts Information", "~g~You are playing on the newest version!", "Installed Version: ~g~" + curVersion + "");
                return false;
            }
        }
    }
}
