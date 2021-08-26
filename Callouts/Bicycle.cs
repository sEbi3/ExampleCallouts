using System;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using System.Drawing;
using System.Collections.Generic;

namespace ExampleCalloutsSRC.Callouts
{
    [CalloutInfo("Bicycle on the Freeway", CalloutProbability.High)]
    public class Bicycle : Callout
    {
        //Strings
        private string[] pedList = new string[] { "A_F_Y_Hippie_01", "A_M_Y_Skater_01", "A_M_M_FatLatin_01", "A_M_M_EastSA_01", "A_M_Y_Latino_01", "G_M_Y_FamDNF_01", "G_M_Y_FamCA_01", "G_M_Y_BallaSout_01", "G_M_Y_BallaOrig_01", 
                                                  "G_M_Y_BallaEast_01", "G_M_Y_StrPunk_02", "S_M_Y_Dealer_01", "A_M_M_RurMeth_01", "A_M_Y_MethHead_01", "A_M_M_Skidrow_01", "S_M_Y_Dealer_01", "a_m_y_mexthug_01", "G_M_Y_MexGoon_03", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_01", "G_M_Y_SalvaGoon_01", 
                                                  "G_M_Y_SalvaGoon_02", "G_M_Y_SalvaGoon_03", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_StrPunk_01" }; //Ped list

        private string[] Bicycles = new string[] { "bmx", "Cruiser", "Fixter", "Scorcher", "tribike3", "tribike2", "tribike" }; //Bike list

        //Ped
        private Ped subject;

        //Vehicle
        private Vehicle Bike;

        //Vector3
        private Vector3 SpawnPoint;
        private Vector3 Location1;
        private Vector3 Location2;
        private Vector3 Location3;
        private Vector3 Location4;
        private Vector3 Location5;

        //Stuff
        private Blip Blip;
        private LHandle pursuit;
        private int calloutm = 0;

        //Bools
        private bool IsStolen = false;
        private bool startedPursuit = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            this.Location1 = new Vector3(1720.068f, 1535.201f, 84.72424f);
            this.Location2 = new Vector3(2563.921f, 5393.056f, 44.55834f);
            this.Location3 = new Vector3(-1826.79f, 4697.899f, 56.58701f);
            this.Location4 = new Vector3(-1344.75f, -757.6135f, 11.10569f);
            this.Location5 = new Vector3(1163.919f, 449.0514f, 82.59987f);

            Random random = new Random();
            List<string> list = new List<string>
            {
                "Location1",
                "Location2",
                "Location3",
                "Location4",
                "Location5",
            };
            int num = random.Next(0, 5);
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
            if (list[num] == "Location5")
            {
                this.SpawnPoint = this.Location5;
            }

            subject = new Ped(this.pedList[Common.rand.Next((int)pedList.Length)], SpawnPoint, 0f);
            Bike = new Vehicle(this.Bicycles[Common.rand.Next((int)Bicycles.Length)], SpawnPoint, 0f);

            subject.WarpIntoVehicle(Bike, -1);

            switch (Common.rand.Next(1, 3))
            {
                case 1:
                    IsStolen = true;
                    break;
                case 2:

                    break;
                case 3:

                    break;
            }

            if (!subject.Exists()) return false;
            if (!Bike.Exists()) return false;

            this.ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 100f);
            this.AddMinimumDistanceCheck(10f, subject.Position);

            switch (Common.rand.Next(1, 3))
            {
                case 1:
                    calloutm = 1;
                    break;
                case 2:
                    calloutm = 2;
                    break;
                case 3:
                    calloutm = 3;
                    break;
            }
            this.CalloutMessage = "Bicycle on the Freeway";
            this.CalloutPosition = SpawnPoint;

            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Blip = Bike.AttachBlip();
            Blip.Color = Color.LightBlue;
            Blip.EnableRoute(Color.Yellow);

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~Bicycle on the Freeway", "~b~Dispatch:~w~ Someone called the police, because there is someone with a ~g~bicycle~w~ on the ~o~freeway~w~. Respond with ~y~Code 2");
            GameFiber.Wait(2000);

            subject.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.FollowTraffic);
            subject.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Normal);

            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (subject.Exists()) subject.Delete();
            if (Bike.Exists()) Bike.Delete();
            if (Blip.Exists()) Blip.Delete();
        }
        public override void Process()
        {
            base.Process();

            GameFiber.StartNew(delegate
            {
                if (subject.DistanceTo(Game.LocalPlayer.Character) < 20f)
                {
                    if (IsStolen == true && startedPursuit == false)
                    {
                        if (Blip.Exists()) Blip.Delete();

                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, subject);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        startedPursuit = true;

                        Bike.IsStolen = true;

                        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts Computer", "~y~Dispatch Information", "The bicycle from the suspect is a ~o~" + Bike.Model.Name + "~w~. The bicycle was ~r~stolen~w~.");
                        GameFiber.Wait(2000);
                    }

                    if (subject.DistanceTo(Game.LocalPlayer.Character) < 25f && Game.LocalPlayer.Character.IsOnFoot && pursuit == null)
                    {
                        Game.DisplayNotification("Perform a normal traffic stop with the ~o~suspect~w~.");
                        Game.DisplayNotification("~b~Dispatch~w~ Checking the serial number of the bike.....");
                        GameFiber.Wait(600);
                        Game.DisplayNotification("~b~Dispatch~w~ We checked the serial number of the bike.<br>Model: ~o~" + Bike.Model.Name + "<br>~w~Serial number: ~o~" + Bike.LicensePlate + "");
                        return;
                    }
                }
                if (subject.Exists() && Functions.IsPedArrested(subject) && IsStolen && subject.DistanceTo(Game.LocalPlayer.Character) < 15f)
                {
                    Game.DisplaySubtitle("~y~Suspect: ~w~Please let me go! I bring the bike back.", 4000);
                }
                if (subject.IsDead || !subject.Exists() || Functions.IsPedArrested(subject))
                {
                    this.End();
                }
                if (this.pursuit != null && !Functions.IsPursuitStillRunning(this.pursuit))
                {
                    this.End();
                }
            }, "Bicycle on the Freeway [ExampleCallouts]");
        }
        public override void End()
        {
            if (subject.Exists()) subject.Dismiss();
            if (Bike.Exists()) Bike.Dismiss();
            if (Blip.Exists()) Blip.Delete();
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~Bicycle on the Freeway", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
            Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH WE_ARE_CODE FOUR NO_FURTHER_UNITS_REQUIRED");
            base.End();
        }
    }
}
