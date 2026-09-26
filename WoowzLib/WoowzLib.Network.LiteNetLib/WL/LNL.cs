using WLO.Network;

namespace WL;

public struct LNL{
    public static LiteNetLib.DeliveryMethod ConvertDeliveryMethod(DeliveryMethod DeliveryMethod) => DeliveryMethod switch{
        DeliveryMethod.ReliableOrdered => LiteNetLib.DeliveryMethod.ReliableOrdered,
        DeliveryMethod.Reliable => LiteNetLib.DeliveryMethod.ReliableUnordered,
        DeliveryMethod.Unreliable => LiteNetLib.DeliveryMethod.Unreliable
    };
}