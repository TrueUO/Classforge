using System;
using Server.Targeting;

namespace Server.Items
{
    public class AcidSac : Item
    {
        [Constructable]
        public AcidSac()
            : base(0x0C67)
        {
            Stackable = true;
            Weight = 1.0;
            Hue = 648;
        }

        public AcidSac(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1111654;// acid sac
        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1111656); // What do you wish to use the acid on?
                from.Target = new InternalTarget(this);
            }
            else
            {
                from.SendLocalizedMessage(1080063); // This must be in your backpack to use it.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        private class InternalTarget : Target
        {
            private readonly Item m_Item;
            private Item wall;
            private Item wallandvine;
            public InternalTarget(Item item)
                : base(2, false, TargetFlags.None)
            {
                m_Item = item;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Item.Deleted)
                {
                    return;
                }

                if (targeted is AddonComponent component)
                {
                    if (component is MagicVinesComponent || component is StoneWallComponent || component is DungeonWallComponent)
                    {
                        int Xs = component.X;

                        if (component is MagicVinesComponent)
                        {
                            Xs += -1;
                        }

                        if (component.Addon is StoneWallAndVineAddon)
                        {
                            wall = new SecretStoneWallNS();
                            wallandvine = new StoneWallAndVineAddon();
                        }
                        else if (component.Addon is DungeonWallAndVineAddon)
                        {
                            wall = new SecretDungeonWallNS();
                            wallandvine = new DungeonWallAndVineAddon();
                        }

                        wall.MoveToWorld(new Point3D(Xs, component.Y, component.Z), component.Map);

                        component.Delete();

                        m_Item.Consume();

                        wall.PublicOverheadMessage(0, 1358, 1111662); // The acid quickly burns through the writhing wallvines, revealing the strange wall.

                        Timer.DelayCall(TimeSpan.FromSeconds(15.0), delegate
                        {
                            wallandvine.MoveToWorld(wall.Location, wall.Map);

                            wall.Delete();
                            wallandvine.PublicOverheadMessage(0, 1358, 1111663); // The vines recover from the acid and, spreading like tentacles, reclaim their grip over the wall.
                        });
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1111657); // The acid swiftly burn through it.
                    m_Item.Consume();
                }
            }
        }
    }

    public class LuckyCoin : Item, ICommodity
    {
        [Constructable]
        public LuckyCoin()
            : this(1)
        {
        }

        [Constructable]
        public LuckyCoin(int amount)
            : base(0xF87)
        {
            Stackable = true;
            Amount = amount;

            Hue = 1174;
        }

        public LuckyCoin(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override int LabelNumber => 1113366;// lucky coin

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack) && Amount >= 1)
            {
                from.SendLocalizedMessage(1113367); // Make a wish then toss me into sacred waters!!
                from.Target = new InternalTarget(this);
            }
        }

        private class InternalTarget : Target
        {
            private readonly LuckyCoin m_Coin;

            public InternalTarget(LuckyCoin coin)
                : base(3, false, TargetFlags.None)
            {
                m_Coin = coin;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is AddonComponent c && c.Addon is FountainOfFortune)
                {
                    if (c.Addon is FountainOfFortune fortune)
                    {
                        fortune.OnTarget(from, m_Coin);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1113369); // That is not sacred waters. Try looking in the Underworld.
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class FloorTrapComponent : Item
    {
        public override int LabelNumber => 1095001;  // Floor Trap Components

        [Constructable]
        public FloorTrapComponent()
            : base(Utility.RandomMinMax(3117, 3120))
        {
        }

        public FloorTrapComponent(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
