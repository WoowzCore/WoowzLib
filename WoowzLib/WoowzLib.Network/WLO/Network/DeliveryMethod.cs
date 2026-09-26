namespace WLO.Network;

public enum DeliveryMethod{
   /// Отправляет сообщение 100%, и сохраняет порядок между сообщениями, если 1 где-то потеряется, весь поток зависнет...
   ReliableOrdered,
   /// Отправляет сообщение 100%
   Reliable,
   /// Отправляет сообщение как повезёт
   Unreliable,
}