using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class ItemIdentification
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.ItemID].Callback = OnUse;
        }

        public static TimeSpan OnUse(Mobile from)
        {
            from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
            from.Target = new InternalTarget();

            return TimeSpan.FromSeconds(1.0);
        }

        [PlayerVendorTarget]
        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(8, false, TargetFlags.None)
            {
                AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                Item item = o as Item;
                Mobile m = o as Mobile;

                if (item == null && m == null)
                {
                    from.SendLocalizedMessage(500353); // You are not certain...
                    return;
                }

                if (!from.CheckTargetSkill(SkillName.ItemID, o, 0, 100))
                {
                    from.PrivateOverheadMessage(MessageType.Emote, 0x3B2, 1041352, from.NetState); // You have no idea how much it might be worth.
                    return;
                }

                if (m != null)
                {
                    from.PrivateOverheadMessage(MessageType.Emote, 0x3B2, 1041349, AffixType.Append, "  " + m.Name, "", from.NetState); // It appears to be:
                    return;
                }

                if (item.Name != null)
                {
                    from.PrivateOverheadMessage(MessageType.Emote, 0x3B2, false, item.Name, from.NetState);
                    item.PrivateOverheadMessage(MessageType.Label, 0x3B2, false, item.Name, from.NetState);
                }
                else
                {
                    from.PrivateOverheadMessage(MessageType.Emote, 0x3B2, item.LabelNumber, from.NetState);
                    item.PrivateOverheadMessage(MessageType.Label, 0x3B2, item.LabelNumber, from.NetState);
                }

                if (item is Meteorite meteorite)
                {
                    if (meteorite.Polished)
                    {
                        from.SendLocalizedMessage(1158697); // The brilliance of the meteorite shimmers in the light as you rotate it in your hands! Brightly hued veins of exotic minerals reflect against the polished surface. You think to yourself you have never seen anything so full of splendor!
                    }
                    else
                    {
                        from.SendLocalizedMessage(1158696); // The rock seems to be otherwordly. Judging by the pitting and charring, it appears to have crash landed here from the sky! The rock feels surprisingly dense given its size. Perhaps if you polished it with an oil cloth you may discover what is inside...
                    }

                    return;
                }

                from.PrivateOverheadMessage(MessageType.Emote, 0x3B2, 1041351, AffixType.Append, "  " + GetPriceFor(item), "", from.NetState); // You guess the value of that item at:
            }

            public static int GetPriceFor(Item item)
            {
                Type type = item.GetType();

                if (GenericBuyInfo.BuyPrices.ContainsKey(type))
                {
                    return GenericBuyInfo.BuyPrices[item.GetType()] * item.Amount;
                }

                if (TypeCostCache == null)
                {
                    TypeCostCache = new Dictionary<Type, int>();
                }

                if (!TypeCostCache.ContainsKey(type))
                {
                    TypeCostCache[type] = Utility.RandomMinMax(2, 7);
                }

                return TypeCostCache[type];
            }

            public static Dictionary<Type, int> TypeCostCache { get; set; }
        }
    }
}
