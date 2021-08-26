using Rage;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using System.Drawing;

namespace ExampleCalloutsSRC.Callouts
{
    [CalloutInfo("CarTrade", CalloutProbability.High)]
    public class CarTrade : Callout
    {
        //Peds
        private Ped Buyer;
        private Ped Seller;
        private Ped Player;

        //Spawnpoints
        private Vector3 SpawnPoint;
        private Vector3 BuyerSpawn;
        private Vector3 CarSpawn = new Vector3(-30.4387f, -1089.152f, 26.42208f);

        //Vehicle
        private Vehicle Car;

        //Blips
        private Blip SpawnBlip;

        //strings
        private string[] CarList = new string[] { "POLICE", "POLICE2", "POLICE3", "SHERIFF", "POLICE4", "SHERIFF2", "FBI", "FBI2", "POLICEB" };
        private string[] SellerList = new string[] { "ig_andreas", "ig_bankman", "ig_barry", "a_m_m_business_01", "a_m_y_business_02" };

        //Stuff
        private LHandle pursuit;
        private bool attack = false;
        private int storyLine = 1;
        private bool startedPursuit = false;
        private bool wasClose = false;
        private bool alreadySubtitleIntrod = false;
        private bool hasTalkedBack = false;
        private int calloutm = 0;

        public override bool OnBeforeCalloutDisplayed()
        {
            //Spawnpoints
            SpawnPoint = new Vector3(-34.79253f, -1096.583f, 26.42235f);
            BuyerSpawn = new Vector3(-33.81899f, -1089.764f, 26.42229f);

            //Seller Ped Stuff
            Seller = new Ped(SellerList[Common.rand.Next((int)SellerList.Length)], SpawnPoint, 0f);
            Seller.Position = SpawnPoint;
            Seller.IsPersistent = true;
            Seller.BlockPermanentEvents = true;

            //Buyer Ped Stuff
            Buyer = new Ped(BuyerSpawn);
            Buyer.Position = BuyerSpawn;
            Buyer.IsPersistent = true;
            Buyer.BlockPermanentEvents = true;

            //RelationshipGroup
            Buyer.RelationshipGroup = RelationshipGroup.AggressiveInvestigate;
            Seller.RelationshipGroup = RelationshipGroup.AggressiveInvestigate;

            Ped Player = Game.LocalPlayer.Character;

            //Car Spawn
            Car = new Vehicle(CarList[Common.rand.Next((int)CarList.Length)], CarSpawn);
            Car.IsStolen = true;

            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(20f, SpawnPoint);

            //Creating a switch for 3 different scenes
            switch (Common.rand.Next(1, 3))
            {
                case 1:
                    attack = true;
                    break;
                case 2:

                    break;
                case 3:

                    break;
            }

            if (!Buyer.Exists()) return false;
            if (!Seller.Exists()) return false;
            if (!Car.Exists()) return false;

            switch (Common.rand.Next(1, 3))
            {
                case 1:
                    this.CalloutMessage = "Illegal police car trade";
                    calloutm = 1;
                    break;
                case 2:
                    this.CalloutMessage = "Illegal police car trade";
                    calloutm = 2;
                    break;
                case 3:
                    this.CalloutMessage = "Illegal police car trade";
                    calloutm = 3;
                    break;
            }
            this.CalloutPosition = SpawnPoint;
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~Illegal police car trade", "~b~Dispatch:~w~ Try to arrest the buyer and seller from the illegal trade. Respond with ~y~Code 2");

            SpawnBlip = Car.AttachBlip();
            SpawnBlip.Color = Color.LightBlue;
            SpawnBlip.IsFriendly = false;
            SpawnBlip.EnableRoute(System.Drawing.Color.Yellow);

            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS CRIME_SUSPECT_RESISTING_ARREST_01 IN_OR_ON_POSITION", SpawnPoint);

            return base.OnCalloutAccepted();
        }
        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (Buyer.Exists()) Buyer.Delete();
            if (SpawnBlip.Exists()) SpawnBlip.Delete();
            if (Seller.Exists()) Seller.Delete();
            if (Car.Exists()) Car.Delete();
        }
        public override void Process()
        {
            base.Process();

            GameFiber.StartNew(delegate
            {
                if (Seller.DistanceTo(Game.LocalPlayer.Character) < 18f)
                {
                    if (attack == true && startedPursuit == false)
                    {
                        pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(pursuit, Seller);
                        Functions.AddPedToPursuit(pursuit, Buyer);
                        Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                        startedPursuit = true;
                    }
                    if (Seller.DistanceTo(Game.LocalPlayer.Character) < 15f && Game.LocalPlayer.Character.IsOnFoot && alreadySubtitleIntrod == false && pursuit == null)
                    {
                        Game.DisplaySubtitle("Press ~y~Y ~w~to speak with the Seller", 5000);
                        Buyer.Face(Car);
                        Seller.Face(Game.LocalPlayer.Character);
                        alreadySubtitleIntrod = true;
                        wasClose = true;
                    }

                    if (attack == false && Seller.DistanceTo(Game.LocalPlayer.Character) < 2f && Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    {
                        Seller.Face(Game.LocalPlayer.Character);

                        switch (storyLine)
                        {
                            case 1:
                                Game.DisplaySubtitle("~y~Seller: ~w~Oh, hello Officer, I didn't hear you. How can I help?", 5000);
                                storyLine++;
                                break;
                            case 2:
                                Game.DisplaySubtitle("~b~You: ~w~Are you the owner?", 5000);
                                storyLine++;
                                break;
                            case 3:
                                Game.DisplaySubtitle("~y~Suspect: ~w~ah...yes I am the owner! Is anything wrong? ", 5000);
                                storyLine++;
                                break;
                            case 4:
                                if (calloutm == 1)
                                    Game.DisplaySubtitle("~b~You: ~w~Why is there a police car?", 5000);
                                if (calloutm == 2)
                                    Game.DisplaySubtitle("~b~You: ~w~Who is the guy next to the police vehicle?", 5000);
                                if (calloutm == 3)
                                    Game.DisplaySubtitle("~b~You: ~w~If you are the owner, I'll need to arrest you.", 5000);
                                storyLine++;
                                break;
                            case 5:
                                if (calloutm == 1)
                                    Game.DisplaySubtitle("~y~Suspect: ~w~I don't know. I came in and the police car was here.", 5000);
                                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~w~ExampleCallouts", "~y~Dispatch", "The plate of the ~b~" + Car.Model.Name + "~w~ is ~o~" + Car.LicensePlate + "~w~. The car was ~r~stolen~w~ from the police station in ~b~Mission Row~w~.");
                                    Game.DisplayHelp("Arrest the owner and the buyer.");
                                if (calloutm == 2)
                                {
                                    Game.DisplaySubtitle("~y~Suspect: ~w~Only a friend!", 5000);
                                    Buyer.Inventory.GiveNewWeapon("WEAPON_PISTOL", 500, true);
                                    Rage.Native.NativeFunction.CallByName<uint>("TASK_COMBAT_PED", Buyer, Game.LocalPlayer.Character, 0, 16);
                                }
                                if (calloutm == 3)
                                {
                                    Game.DisplaySubtitle("~y~Suspect: ~w~No, why?", 5000);
                                    Seller.Inventory.GiveNewWeapon("WEAPON_KNIFE", 500, true);
                                    Rage.Native.NativeFunction.CallByName<uint>("TASK_COMBAT_PED", Seller, Game.LocalPlayer.Character, 0, 16);
                                    Rage.Native.NativeFunction.CallByName<uint>("TASK_COMBAT_PED", Buyer, Game.LocalPlayer.Character, 0, 16);
                                }
                                storyLine++;

                                if (Common.rand.Next(1, 4) == 4)
                                {
                                    this.pursuit = Functions.CreatePursuit();
                                    Functions.AddPedToPursuit(this.pursuit, Seller);
                                    Functions.AddPedToPursuit(this.pursuit, Buyer);
                                    startedPursuit = true;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (Seller.Exists() && Functions.IsPedArrested(Seller) && attack && Seller.DistanceTo(Game.LocalPlayer.Character) < 8f && !hasTalkedBack)
                {
                    Game.DisplaySubtitle("~y~Seller: ~w~I don't know, whats wrong with me...", 4000);
                    hasTalkedBack = true;
                }
                if (Buyer.Exists() && Functions.IsPedArrested(Buyer) && attack && Buyer.DistanceTo(Game.LocalPlayer.Character) < 8f && !hasTalkedBack)
                {
                    Game.DisplaySubtitle("~y~Buyer: ~w~I only wanted to safe me with a police car!", 4000);
                    hasTalkedBack = true;
                }
                if (Seller.DistanceTo(Game.LocalPlayer.Character) >= 200f && wasClose)
                {
                    if (startedPursuit)
                    {
                        Game.DisplaySubtitle("The Seller escaped!", 4300);
                    }
                    this.End();
                }
                if (Buyer.DistanceTo(Game.LocalPlayer.Character) >= 200f && wasClose)
                {
                    if (startedPursuit)
                    {
                        Game.DisplaySubtitle("The Buyer escaped!", 4300);
                    }
                    this.End();
                }
                if (Seller.IsDead || !Seller.Exists() || Functions.IsPedArrested(Seller))
                {
                    this.End();
                }
                if (Buyer.IsDead || !Buyer.Exists() || Functions.IsPedArrested(Buyer))
                {
                    this.End();
                }
                if (this.pursuit != null && !Functions.IsPursuitStillRunning(this.pursuit))
                {
                    this.End();
                }
            }, "CarTrade [ExampleCallouts]");
        }
        public override void End()
        {
            if (Seller.Exists()) Seller.Dismiss();
            if (SpawnBlip.Exists()) SpawnBlip.Delete();
            if (Buyer.Exists()) Buyer.Dismiss();
            if (Car.Exists()) Car.Dismiss();
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~ExampleCallouts", "~y~Illegal police car trade", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
            Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH WE_ARE_CODE FOUR NO_FURTHER_UNITS_REQUIRED");
            base.End();
        }
    }
}