using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Services.ClassSystem;

namespace Server.Items
{
    public class ClassSelectionItem : Item
    {
        [Constructable]
        public ClassSelectionItem()
            : base(45554)
        {
            Name = "Rune Obelisk";
            Movable = false;
        }

        public ClassSelectionItem(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add("Character Class Selection");
        }

        public override double DefaultWeight => 0.0;

        public override void OnDoubleClick(Mobile from)
        {
            if (from is PlayerMobile pm)
            {
                from.CloseGump(typeof(ClassSelectionGump));
                from.SendGump(new ClassSelectionGump(pm));
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

        public class ClassSelectionGump : Gump
        {
            private readonly PlayerMobile _Player;

            public ClassSelectionGump(PlayerMobile pm) : base(150, 100)
            {
                _Player = pm;

                AddPage(0);
                AddBackground(0, 0, 300, 300, 5054);
                AddLabel(100, 20, 1152, "Choose Your Path");

                int y = 60;
                int buttonId = 1;

                foreach (CharacterClass cls in Enum.GetValues(typeof(CharacterClass)))
                {
                    if (cls == CharacterClass.None)
                    {
                        continue;
                    }

                    AddButton(30, y, 4005, 4006, buttonId++, GumpButtonType.Reply, 0);
                    AddLabel(70, y, 1152, ClassSystemHelper.GetClassName(cls));

                    y += 30;
                }
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (_Player == null || info.ButtonID == 0)
                {
                    return;
                }

                // Delete the starting gear players wear.
                Item startingMonkRobe = _Player.FindItemOnLayer(Layer.OuterTorso);
                startingMonkRobe?.Delete();

                Item startingSandals = _Player.FindItemOnLayer(Layer.Shoes);
                startingSandals?.Delete();

                // Add starting items and move player based on class selected.
                CharacterClass selected = (CharacterClass)info.ButtonID;

                _Player.CharacterClass = selected;

                switch (selected)
                {
                    case CharacterClass.Cleric:
                    {
                        //_Player.MoveToWorld(new Point3D(4447, 1173, 0), Map.Trammel); // Moonglow
                        ClericClass.OnClassSelected(_Player);
                        break;
                    }
                    case CharacterClass.Mage:
                    {
                        MageClass.OnClassSelected(_Player);
                        break;
                    }
                    case CharacterClass.Ranger:
                    {
                        //_Player.MoveToWorld(new Point3D(3032, 3384, 15), Map.Trammel); // Serpent's Hold
                        RangerClass.OnClassSelected(_Player);
                        break;
                    }
                    case CharacterClass.Rogue:
                    {
                        RogueClass.OnClassSelected(_Player);
                        break;
                    }
                    case CharacterClass.Warrior:
                    {
                        WarriorClass.OnClassSelected(_Player);
                        break;
                    }
                }

                _Player.MoveToWorld(new Point3D(633, 861, 0), Map.Trammel); // Yew

                _Player.SendMessage(52, $"You are now a {ClassSystemHelper.GetClassName(selected)}.");
            }
        }
    }
}
