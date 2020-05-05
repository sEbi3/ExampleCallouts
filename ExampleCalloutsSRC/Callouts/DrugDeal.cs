using System;
using Rage;
using System.Drawing;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using System.Collections.Generic;

namespace ExampleCalloutsSRC.Callouts
{
    [CalloutInfo("DrugDeal", CalloutProbability.High)]
    public class DrugDeal : Callout
    {
        //Strings
        private string[] wepList = new string[] { "WEAPON_PISTOL", "WEAPON_SMG", "WEAPON_MACHINEPISTOL", "WEAPON_PUMPSHOTGUN" };

        //Stuff
        public LHandle pursuit;
        public Blip Blip;
        public Blip Blip2;

        //Vector3
        public Vector3 SpawnPoint;
        public Vector3 Location1;
        public Vector3 Location2;

        //Peds
        public Ped Dealer;
        public Ped Victim;
        public Ped playerPed;

        //Ints
        private int calloutm = 0;
        private int scenario = 0;

        //Bools
        private bool pursuitS = false;
        private bool isArmed = false;
        private bool hasBegunAttacking = false;
        private bool PursuitCreated = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            this.Location1 = new Vector3(13.29765f, -1033.113f, 29.21461f); //Set the cords you want
            this.Location2 = new Vector3(75.73988f, -855.1366f, 30.75766f); //Set the cords you want

            Random random = new Random();
            List<string> list = new List<string>
            {
                "Location1",
                "Location2",
            };
            int num = random.Next(0, 2);
            if (list[num] == "Location1")
            {
                this.SpawnPoint = this.Location1;
            }
            if (list[num] == "Location2")
            {
                this.SpawnPoint = this.Location2;
            }

            playerPed = Game.LocalPlayer.Character;
            scenario = Common.rand.Next(0, 100);

            //Dealer Stuff
            Dealer = new Ped("s_m_y_dealer_01", SpawnPoint, 0f);
            Dealer.BlockPermanentEvents = true;
            Dealer.IsPersistent = true;

            //Victim Stuff
            Victim = new Ped(Dealer.GetOffsetPosition(new Vector3(0, 1.8f, 0)));
            Victim.BlockPermanentEvents = true;
            Victim.IsPersistent = true;

            if (!Dealer.Exists()) return false;
            if (!Victim.Exists()) return false;

            this.ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 15f);
            this.AddMinimumDistanceCheck(5f, SpawnPoint);

            //Creating 2 cases for 2 scenarios
            switch (Common.rand.Next(1, 2))
            {
                case 1:
                    calloutm = 1;
                    break;
                case 2:
                    calloutm = 2;
                    break;
            }
            this.CalloutMessage = "";
            this.CalloutPosition = SpawnPoint;

            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS A_DRUG_DEAL_IN_PROGRESS IN_OR_ON_POSITION");
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Blip = Dealer.AttachBlip();
            Blip2 = Victim.AttachBlip();
            Blip.EnableRoute(Color.Yellow);

            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~DrugDeal", "~b~Dispatch: ~w~Try to arrest the Dealer and the buyer!. Respond with ~y~Code 2");
            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            if (Dealer.Exists()) Dealer.Delete();
            if (Victim.Exists()) Victim.Delete();
            if (Blip.Exists()) Blip.Delete();
            if (Blip2.Exists()) Blip2.Delete();
            base.OnCalloutNotAccepted();
        }
        public override void Process()
        {
            base.Process();

            GameFiber.StartNew(delegate
            {
                if (playerPed.IsDead) this.End();

                if (Dealer.DistanceTo(playerPed.GetOffsetPosition(Vector3.RelativeFront)) < 75f && !isArmed)
                {
                    Dealer.Face(Victim);
                    Victim.Face(Dealer);
                    Dealer.Inventory.GiveNewWeapon(new WeaponAsset(wepList[Common.rand.Next((int)wepList.Length)]), 500, true);
                    isArmed = true;
                }
                if (Dealer.Exists() && Dealer.DistanceTo(playerPed.GetOffsetPosition(Vector3.RelativeFront)) < 60f && !hasBegunAttacking)
                {
                    if (scenario > 40)
                    {
                        new RelationshipGroup("AG");
                        new RelationshipGroup("Vi");
                        Dealer.RelationshipGroup = "AG";
                        Victim.RelationshipGroup = "AG";
                        playerPed.RelationshipGroup = "VI";
                        Game.SetRelationshipBetweenRelationshipGroups("AG", "VI", Relationship.Hate);
                        Dealer.KeepTasks = true;
                        Dealer.Tasks.FightAgainst(playerPed);
                        Game.DisplayNotification("Arrest the ~o~buyer~w~, who is surrendering!");
                        Victim.Tasks.PutHandsUp(-1, Game.LocalPlayer.Character);
                        hasBegunAttacking = true;
                        GameFiber.Wait(2000);
                    }
                    else
                    {
                        if (!pursuitS)
                        {
                            pursuit = Functions.CreatePursuit();
                            Functions.AddPedToPursuit(pursuit, Dealer);
                            Functions.AddPedToPursuit(pursuit, Victim);
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            pursuitS = true;
                        }
                    }
                }
                if (Dealer.IsDead && Victim.IsDead) End();
                if (Functions.IsPedArrested(Dealer)) End();
                if (!Dealer.Exists()) End();
                if (Dealer == null) End();
                if (pursuit != null && !Functions.IsPursuitStillRunning(pursuit))
                {
                    End();
                }
            }, "DrugDeal [ExampleCallouts]");
        }
        public override void End()
        {
            if (Blip.Exists()) Blip.Delete();
            if (Blip2.Exists()) Blip2.Delete();
            Victim.Dismiss();
            Dealer.Dismiss();
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~DrugDeal", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
            Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH WE_ARE_CODE FOUR NO_FURTHER_UNITS_REQUIRED");
            base.End();
        }
    }
}