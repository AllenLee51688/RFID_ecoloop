using EcoloopSystem.Server.Models;

namespace EcoloopSystem.Server.Data
{
    public static class DbInitializer
    {
        public static void Initialize(EcoloopContext context)
        {
            context.Database.EnsureCreated();

            // 初始化測試餐具資料
            if (!context.Tablewares.Any())
            {
                var tablewares = new Tableware[]
                {
                    new Tableware{TagId="TW001A2B3C", Type=TablewareType.Bowl, Status=TablewareStatus.Available},
                    new Tableware{TagId="TW002D4E5F", Type=TablewareType.Cup, Status=TablewareStatus.Available},
                    new Tableware{TagId="TW003G7H8I", Type=TablewareType.Chopsticks, Status=TablewareStatus.Available},
                    new Tableware{TagId="TW004J0K1L", Type=TablewareType.Bowl, Status=TablewareStatus.Available},
                    new Tableware{TagId="TW005M2N3O", Type=TablewareType.Cup, Status=TablewareStatus.Available},
                };

                foreach (var t in tablewares)
                {
                    context.Tablewares.Add(t);
                }
                context.SaveChanges();
                Console.WriteLine("已初始化 5 個測試餐具資料");
            }
        }
    }
}
