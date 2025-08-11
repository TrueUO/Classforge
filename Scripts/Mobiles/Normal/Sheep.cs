using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a sheep corpse")]
    public class Sheep : BaseCreature, ICarvable
    {
        public override int XpToGive => 1;

        private DateTime _NextWoolTime;

        [Constructable]
        public Sheep()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a sheep";
            Body = 0xCF;
            BaseSoundID = 0xD6;

            SetStr(19);
            SetDex(25);
            SetInt(5);

            SetHits(12);
            SetMana(0);

            SetDamage(1, 2);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 5, 10);

            SetSkill(SkillName.MagicResist, 5.0);
            SetSkill(SkillName.Tactics, 6.0);
            SetSkill(SkillName.Wrestling, 5.0);

            Fame = 300;
            Karma = 0;

            //Tamable = true;
            //ControlSlots = 1;
            //MinTameSkill = 11.1;
        }

        public Sheep(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextWoolTime
        {
            get => _NextWoolTime;
            set
            {
                _NextWoolTime = value;

                Body = DateTime.UtcNow >= _NextWoolTime ? 0xCF : 0xDF;
            }
        }

        public override int Meat => 3;
        public override MeatType MeatType => MeatType.LambLeg;
        public override FoodType FavoriteFood => FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
        public override int Wool => Body == 0xCF ? 1 : 0;

        public bool Carve(Mobile from, Item item)
        {
            if (DateTime.UtcNow < _NextWoolTime)
            {
                // This sheep is not yet ready to be shorn.
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500449, from.NetState);
                return false;
            }

            from.SendLocalizedMessage(500452); // You place the gathered wool into your backpack.
            from.AddToBackpack(new Wool(1));

            NextWoolTime = DateTime.UtcNow + TimeSpan.FromHours(2.0); // TODO: Proper time delay

            return true;
        }

        public override void OnThink()
        {
            base.OnThink();
            Body = DateTime.UtcNow >= _NextWoolTime ? 0xCF : 0xDF;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.WriteDeltaTime(_NextWoolTime);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            NextWoolTime = reader.ReadDeltaTime();
        }
    }
}
