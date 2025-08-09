using System;
using System.Xml;
using Server.Mobiles;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Ninjitsu;

namespace Server.Engines.Blackthorn
{
    public class BlackthornDungeon : DungeonRegion
    {
        public BlackthornDungeon(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            if (traveller.AccessLevel > AccessLevel.Player)
            {
                return true;
            }

            return type > TravelCheckType.Mark;
        }

        public override void OnDeath(Mobile m)
        {
            if (m is BaseCreature creature && Map == Map.Trammel && InvasionController.TramInstance != null)
            {
                InvasionController.TramInstance.OnDeath(creature);
            }

            if (m is BaseCreature baseCreature && Map == Map.Felucca && InvasionController.FelInstance != null)
            {
                InvasionController.FelInstance.OnDeath(baseCreature);
            }
        }
    }

    public class BlackthornCastle : GuardedRegion
    {
        public static readonly Point3D[] StableLocs = { new Point3D(1510, 1543, 25),
            new Point3D(1516, 1542, 25), new Point3D(1520, 1542, 25), new Point3D(1525, 1542, 25) };

        public BlackthornCastle(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (m.AccessLevel > AccessLevel.Player)
            {
                return base.OnBeginSpellCast(m, s);
            }

            int loc;

            if (s is PaladinSpell)
            {
                loc = 1062075; // You cannot use a Paladin ability here.
            }
            else if (s is NinjaMove || s is NinjaSpell || s is SamuraiSpell || s is SamuraiMove)
            {
                loc = 1062938; // That ability does not seem to work here.
            }
            else
            {
                loc = 502629;
            }

            m.SendLocalizedMessage(loc);
            return false;
        }

        public override void OnLocationChanged(Mobile m, Point3D oldLocation)
        {
            base.OnLocationChanged(m, oldLocation);

            if (m.AccessLevel > AccessLevel.Player)
            {
                return;
            }

            if (m.Mount != null)
            {
                if (m is PlayerMobile mobile)
                {
                    mobile.SetMountBlock(BlockMountType.DismountRecovery, TimeSpan.FromSeconds(30), true);
                }
                else
                {
                    m.Mount.Rider = null;
                }

                m.SendLocalizedMessage(1153052); // Mounts and flying are not permitted in this area.

                if (m.Mount is BaseCreature mount && mount.Controlled)
                {
                    TryAutoStable(mount);
                }
            }

            if (m is BaseCreature creature && creature.Controlled)
            {
                TryAutoStable(creature);
            }
        }

        public void TryAutoStable(BaseCreature pet)
        {
            if (pet == null)
            {
                return;
            }

            Mobile owner = pet.GetMaster();

            if (!pet.Controlled || owner == null)
            {
                return;
            }
            if (pet.Body.IsHuman || pet.IsDeadPet || pet.Allured)
            {
                SendToStables(pet, owner);
            }
            else if (owner.Stabled.Count >= AnimalTrainer.GetMaxStabled(owner))
            {
                SendToStables(pet, owner);
            }
            else if ((pet is PackLlama || pet is PackHorse || pet is Beetle) &&
                     (pet.Backpack != null && pet.Backpack.Items.Count > 0))
            {
                SendToStables(pet, owner);
            }
            else
            {
                pet.ControlTarget = null;
                pet.ControlOrder = LastOrderType.Stay;
                pet.Internalize();

                pet.SetControlMaster(null);
                pet.SummonMaster = null;

                pet.IsStabled = true;
                pet.StabledBy = owner;

                pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy
                owner.Stabled.Add(pet);
                owner.SendLocalizedMessage(1153050, pet.Name); // Pets are not permitted in this location. Your pet named ~1_NAME~ has been sent to the stables.
            }
        }

        public void SendToStables(BaseCreature bc, Mobile m = null)
        {
            Point3D p = StableLocs[Utility.Random(StableLocs.Length)];
            bc.MoveToWorld(p, Map);

            if (m != null)
            {
                m.SendLocalizedMessage(1153053, bc.Name); // Pets are not permitted in this area. Your pet named ~1_NAME~ could not be sent to the stables, so has been teleported outside the event area.
            }
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            if (traveller.AccessLevel > AccessLevel.Player)
            {
                return true;
            }

            return type > TravelCheckType.Mark;
        }
    }
}
