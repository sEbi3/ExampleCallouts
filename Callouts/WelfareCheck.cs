using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace ExampleCalloutsSRC.Callouts
{
    [CalloutInfo("WelfareCheck", CalloutProbability.High)]
    public class WelfareCheck : Callout
    {
        //Peds
        private Ped subject;
        private Ped playerPed;

        private string[] MaleSuspects = new string[] { "ig_andreas", "g_m_m_armlieut_01", "a_m_m_bevhills_01", "a_m_y_business_02", "s_m_m_gaffer_01" };
        private string[] FemaleSuspects = new string[] { "a_f_y_golfer_01", "a_f_y_bevhills_01", "a_f_y_bevhills_04", "a_f_y_fitness_02" };

        //Vector3
        private Vector3 SpawnPoint;
        private Vector3 Location1;
        private Vector3 Location2;
        private Vector3 Location3;
        private Vector3 Location4;
        private Vector3 searcharea;

        private Blip Blip;
        private LHandle pursuit;

        //Stuff
        private int storyLine = 1;
        private int callOutMessage = 0;

        //Scenes
        private bool Scene1 = false;
        private bool Scene2 = false;
        private bool Scene3 = false;

        //Bools
        private bool wasClose = false;
        private bool alreadySubtitleIntrod = false;
        private bool hasTalkedBack = false;
        private bool notificationDisplayed = false;
        private bool getAmbulance = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            //Locations
            this.Location1 = new Vector3(917.1311f, -651.3591f, 57.86318f);
            this.Location2 = new Vector3(-1905.715f, 365.4793f, 93.58082f);
            this.Location3 = new Vector3(1661.571f, 4767.511f, 42.00745f);
            this.Location4 = new Vector3(1878.274f, 3922.46f, 33.06999f);

            Random random = new Random();
            List<string> list = new List<string>
            {
                "Location1",
                "Location2",
                "Location3",
                "Location4",
            };
            int num = random.Next(0, 4);
            if (list[num] == "Location1")
            {
                this.SpawnPoint = this.Location1;
            }
            if (list[num] == "Location2")
            {
                this.SpawnPoint = this.Location2;
            }
            if (list[num] == "Location3")
            {
                this.SpawnPoint = this.Location3;
            }
            if (list[num] == "Location4")
            {
                this.SpawnPoint = this.Location4;
            }

            //Creating cases for 3 different scenarios
            switch (Common.rand.Next(1, 3))
            {
                case 1:
                    subject = new Ped(MaleSuspects[Common.rand.Next((int)MaleSuspects.Length)], SpawnPoint, 0f);
                    subject.Kill();
                    Scene1 = true;
                    break;
                case 2:
                    subject = new Ped(FemaleSuspects[Common.rand.Next((int)FemaleSuspects.Length)], SpawnPoint, 0f);
                    Scene3 = true;
                    break;
                case 3:
                    subject = new Ped(FemaleSuspects[Common.rand.Next((int)FemaleSuspects.Length)], SpawnPoint, 0f);
                    subject.Dismiss();
                    Scene2 = true;
                    break;
            }
            if (!subject.Exists()) return false;

            playerPed = Game.LocalPlayer.Character;

            this.ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 100f);
            this.AddMinimumDistanceCheck(10f, subject.Position);

            switch (Common.rand.Next(1, 3))
            {
                case 1:
                    this.CalloutMessage = "Welfare check";
                    callOutMessage = 1;
                    break;
                case 2:
                    this.CalloutMessage = "Welfare check";
                    callOutMessage = 2;
                    break;
                case 3:
                    this.CalloutMessage = "Welfare check";
                    callOutMessage = 3;
                    break;
            }
            this.CalloutPosition = SpawnPoint;

            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            searcharea = SpawnPoint.Around2D(1f, 2f);
            Blip = new Blip(searcharea, 40f);
            Blip.EnableRoute(Color.Yellow);
            Blip.Color = Color.Yellow;
            Blip.Alpha = 5f;

            Functions.PlayScannerAudioUsingPosition("UNITS WE_HAVE CRIME_CIVILIAN_NEEDING_ASSISTANCE_02", SpawnPoint);
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~WelfareCheck", "~b~Dispatch:~w~ Someone called the police for a welfare check. Search the ~y~yellow area~w~ for the person. Respond with ~y~Code 2");
            GameFiber.Wait(2000);

            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (subject.Exists()) subject.Delete();
            if (Blip.Exists()) Blip.Delete();
        }
        public override void Process()
        {
            if (playerPed.IsDead) this.End();

            base.Process();

            GameFiber.StartNew(delegate
            {
                if (SpawnPoint.DistanceTo(Game.LocalPlayer.Character) < 25f)
                {
                    if (Scene1 == true && Scene3 == false && Scene2 == false && subject.DistanceTo(Game.LocalPlayer.Character) < 8f && Game.LocalPlayer.Character.IsOnFoot && !notificationDisplayed && !getAmbulance)
                    {
                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~Dispatch", "We are going to call a ~y~ambulance~w~ to you're current location, Officer. Press the ~y~END~w~ key to end the welfare check callout.");
                        notificationDisplayed = true;
                        GameFiber.Wait(1000);
                        Game.DisplayHelp("Press the ~y~END~w~ key to end the wellfare check callout. The callout is ~g~ CODE 4~w~.");
                        UltimateBackup.API.Functions.callAmbulance();
                        getAmbulance = true;
                    }

                    if (Scene2 == true && Scene3 == false && Scene1 == false && SpawnPoint.DistanceTo(Game.LocalPlayer.Character) < 8f && Game.LocalPlayer.Character.IsOnFoot && !notificationDisplayed)
                    {
                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~Dispatch", "No one is at home.<br>You are ~g~code 4~w~.");
                        notificationDisplayed = true;
                        GameFiber.Wait(1000);
                        Game.DisplayHelp("Press the ~y~END~w~ key to end the welfare check callout. The callout is ~g~ CODE 4~w~.");
                    }

                    if (Scene3 == true && Scene1 == false && Scene2 == false && subject.DistanceTo(Game.LocalPlayer.Character) < 25f && Game.LocalPlayer.Character.IsOnFoot && alreadySubtitleIntrod == false && pursuit == null)
                    {
                        Game.DisplaySubtitle("Press ~y~Y ~w~to speak with the suspect.", 5000);
                        alreadySubtitleIntrod = true;
                        wasClose = true;
                    }

                    if (Scene3 == true && Scene1 == false && Scene2 == false && subject.DistanceTo(Game.LocalPlayer.Character) < 2f && Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    {
                        subject.Face(Game.LocalPlayer.Character);

                        switch (storyLine)
                        {
                            case 1:
                                Game.DisplaySubtitle("~y~Suspect: ~w~Hello Officer, how can I help you? Is everything alright? (1/5)", 5000);
                                storyLine++;
                                break;
                            case 2:
                                Game.DisplaySubtitle("~b~You: ~w~Hello, we got a call for a welfare check, because you didn't response to the call. (2/5)", 5000);
                                storyLine++;
                                break;
                            case 3:
                                Game.DisplaySubtitle("~y~Suspect: ~w~Oh no, I forgot that. (3/5)", 5000);
                                storyLine++;
                                break;
                            case 4:
                                if (callOutMessage == 1)
                                    Game.DisplaySubtitle("~y~Suspect: ~w~I lost my phone today. I can't find it. (4/5)", 5000);
                                if (callOutMessage == 2)
                                    Game.DisplaySubtitle("~y~Suspect: ~w~The battery of my phone was empty. (4/5)", 5000);
                                if (callOutMessage == 3)
                                    Game.DisplaySubtitle("~y~Suspect: ~w~Okay, I'll definitely call back now. Thank you! (4/5)", 5000);
                                storyLine++;
                                break;
                            case 5:
                                if (callOutMessage == 1)
                                    Game.DisplaySubtitle("~b~You: ~w~Hm, that is annoying. We will let the caller know, that everything is alright. (5/5)", 5000);
                                Game.DisplayHelp("Press the ~y~END~w~ key to end the ~o~welfare check~w~ callout. The callout is ~g~ CODE 4~w~.");
                                if (callOutMessage == 2)
                                    Game.DisplaySubtitle("~b~You: ~w~Okay, everything is alright. If the battery is full now, you can call back. (5/5)", 5000);
                                Game.DisplayHelp("Press the ~y~END~w~ key to end the ~o~welfare check~w~ callout. The callout is ~g~ CODE 4~w~.");
                                if (callOutMessage == 3)
                                    Game.DisplaySubtitle("~b~You: ~w~Alright, no problem! Have a nice day. (5/5)", 5000);
                                Game.DisplayHelp("Press the ~y~END~w~ key to end the ~o~welfare check~w~ callout. The callout is ~g~ CODE 4~w~.");
                                storyLine++;
                                break;
                            case 6:
                                if (callOutMessage == 1)
                                    End();
                                if (callOutMessage == 2)
                                    End();
                                if (callOutMessage == 3)
                                    End();
                                storyLine++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }, "WellfareCheck [ExampleCallouts]");
        }
        public override void End()
        {
            if (subject.Exists()) subject.Dismiss();
            if (Blip.Exists()) Blip.Delete();
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~WelfareCheck", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
            Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH WE_ARE_CODE FOUR NO_FURTHER_UNITS_REQUIRED");
            base.End();
        }
    }
}
