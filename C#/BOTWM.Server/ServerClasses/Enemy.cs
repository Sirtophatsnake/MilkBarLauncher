using BOTWM.Server.DTO;

namespace BOTWM.Server.ServerClasses
{
    public class Enemy
    {
        const int CLEARMINUTES = 60;

        public Mutex EMutex = new Mutex();
        public bool isEnemySync;
        private DateTime LastClear;

        public Dictionary<int, EnemyData> EnemyList;
        public List<Dictionary<int, EnemyData>> Queue;

        public Enemy(int playerLimit, bool enemySync)
        {
            EnemyList = new Dictionary<int, EnemyData>();
            Queue = new List<Dictionary<int, EnemyData>>();
            for (int i = 0; i < playerLimit; i++)
                Queue.Add(new Dictionary<int, EnemyData>());
            UpdateServiceStatus(enemySync);
            LastClear = DateTime.Now;
        }

        public void UpdateServiceStatus(bool newStatus)
        {
            isEnemySync = newStatus;
        }

        public void Update(EnemyDTO userData)
        {
            double TimeSinceLastClear = DateTime.Now.Subtract(LastClear).TotalMinutes;

            if (!isEnemySync || TimeSinceLastClear > CLEARMINUTES)
            {
                ClearEnemyData();
                return;
            }

            EMutex.WaitOne(100);

            foreach (EnemyData item in userData.Health)
                UpdateEntry(item);

            EMutex.ReleaseMutex();
        }

        public void ClearEnemyData()
        {
            EMutex.WaitOne(100);

            EnemyList.Clear();
            for (int i = 0; i < Queue.Count; i++)
                Queue[i].Clear();

            LastClear = DateTime.Now;

            EMutex.ReleaseMutex();
        }

        public void FillQueue(int playerNumber)
        {
            EMutex.WaitOne(100);

            Queue[playerNumber].Clear();

            foreach (KeyValuePair<int, EnemyData> kvp in EnemyList)
                Queue[playerNumber][kvp.Key] = kvp.Value;

            EMutex.ReleaseMutex();
        }

        public List<EnemyData> GetQueue(int playerNumber)
        {
            List<EnemyData> Data = new List<EnemyData>();

            EMutex.WaitOne(100);

            foreach(KeyValuePair<int, EnemyData> kvp in Queue[playerNumber])
                Data.Add(kvp.Value);

            Queue[playerNumber].Clear();

            EMutex.ReleaseMutex();

            return Data;
        }

        private void UpdateEntry(EnemyData item)
        {
            bool healthChanged = !EnemyList.ContainsKey(item.Hash) || EnemyList[item.Hash].Health > item.Health;
            bool positionChanged = !EnemyList.ContainsKey(item.Hash) || EnemyList[item.Hash].Position.GetDistance(item.Position) > 0.05f;

            if (!healthChanged && !positionChanged)
                return;

            if (EnemyList.ContainsKey(item.Hash) && !healthChanged)
                item.Health = EnemyList[item.Hash].Health;

            EnemyList[item.Hash] = item;

            for (int i = 0; i < Queue.Count; i++)
                Queue[i][item.Hash] = item;
        }
    }
}