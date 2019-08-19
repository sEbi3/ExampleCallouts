using System;
using Rage;
using System.Drawing;
using Rage.Native;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace ExampleCalloutsSRC.Callouts
{
    [CalloutInfo("DrugDeal", CalloutProbability.Medium)]
    public class DrugDeal : Callout
    {
        public LHandle pursuit;
        public Vector3 SpawnPoint;
        public Blip ABlip;
        public Ped Dealer;
        public Ped Victim;
        private bool PursuitCreated = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = new Vector3(13.29765f, -1033.113f, 29.21461f);
            Dealer = new Ped("s_m_y_dealer_01", SpawnPoint, 0f); // Set the Skin of the Dealer.
            Victim = new Ped(Dealer.GetOffsetPosition(new Vector3(0, 1.8f, 0)));

            if (!Dealer.Exists()) return false;
            if (!Victim.Exists()) return false;

            this.ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 15f);
            this.AddMinimumDistanceCheck(5f, SpawnPoint);

            this.CalloutMessage = "Drug Deal in progress";
            this.CalloutPosition = SpawnPoint;

            Functions.PlayScannerAudio("A_DRUG_DEAL_IN_PROGRESS"); // Find more sounds in OpenIV
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            ABlip = Dealer.AttachBlip();
            ABlip.EnableRoute(Color.Yellow);

            if (Dealer.DistanceTo(SpawnPoint) > 10f)
                Dealer.Tasks.PlayAnimation(new AnimationDictionary("missexile2"), "amb@world_human_drug_dealer_hard@male@idle_a", 10f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly);

            Victim.BlockPermanentEvents = true;

            Game.DisplaySubtitle("Stop the ~r~drug deal!~w~", 6500);

            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            if (Dealer.Exists()) Dealer.Delete();
            if (Victim.Exists()) Victim.Delete();
            if (ABlip.Exists()) ABlip.Delete();
            base.OnCalloutNotAccepted();
        }
        public override void Process()
        {
            base.Process();
            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(Dealer.Position) < 12f)
            {
                pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, Dealer);
                Functions.AddPedToPursuit(pursuit, Victim);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                PursuitCreated = true;

                Functions.AddPedToPursuit(this.pursuit, Dealer);
                Functions.RequestBackup(Game.LocalPlayer.Character.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.NooseTeam);
            }
            if (PursuitCreated && !Functions.IsPursuitStillRunning(pursuit))
            {
                End();
            }
        }
        public override void End()
        {
            if (ABlip.Exists()) ABlip.Delete();
            Victim.Dismiss();
            Dealer.Dismiss();
            base.End();
        }
    }
}