using BOTWM.Server.DataTypes;

namespace BOTWM.Server.DTO
{

    public class EnemyData
    {

        public int Hash;
        public int Health;
        public Vec3f Position;

        public EnemyData()
        {
            Position = new Vec3f();
        }

        public EnemyData(int hash, int health, Vec3f position)
        {
            Hash = hash;
            Health = health;
            Position = position;
        }

    }

    public class EnemyDTO
    {
        public List<EnemyData> Health;
    }
}