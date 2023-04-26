using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct Vec3 : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
        }

        public void Deserialize(NetDataReader reader)
        {
            x = reader.GetFloat();
            y = reader.GetFloat();
            z = reader.GetFloat();
        }
    }
}
