using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RepoDb;

namespace binreader
{
    class Program
    {
        private static string DataDirectory 
        {
            get
            {
                var currentDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
                while (currentDir != null && currentDir.Exists && !currentDir.Name.EndsWith("data"))
                {
                    if (currentDir.EnumerateDirectories().Any(z => z.Name == "data"))
                    {
                        return Path.Combine(currentDir.FullName, "data");
                    }
                    currentDir = currentDir.Parent;
                }
                return currentDir.FullName;
            }
        }
        
        static async Task Main(string[] args)
        {
            var txnFile = Path.Combine(DataDirectory, "txnlog.dat");
            var sqlFile = Path.Combine(DataDirectory, "mydb.sqlite");

            try
            {
                SqLiteBootstrap.Initialize();
                await using var fs = new FileStream(txnFile, FileMode.Open);
                var header = await fs.ReadMps7HeaderAsync();
                //await using var conn = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");
                await using var conn = new SQLiteConnection($"Data Source={sqlFile};Version=3;");
                var createTable = await conn.ExecuteNonQueryAsync(@"
                CREATE TABLE IF NOT EXISTS [Mps7Record]
                (                
	                Id INTEGER PRIMARY KEY AUTOINCREMENT,
	                TransactionCode INTEGER,
	                UnixTimeStamp INTEGER,
	                UserID INTEGER,
	                Amount NUMERIC
	            );");
                int transactionType;
                while ((transactionType = fs.ReadByte()) != -1)
                {
                    var mpsRecord = new Mps7Record
                    {
                        TransactionCode = (byte) transactionType,
                    };
                    var userAndTime = new byte[12];
                    fs.Read(userAndTime);
                    mpsRecord.UnixTimeStamp = (uint) userAndTime[Range.EndAt(4)].ToUInt64();
                    mpsRecord.UserId = userAndTime[Range.StartAt(4)].ToUInt64();
                    if (mpsRecord.TransactionCode == 0x0 || mpsRecord.TransactionCode == 0x01)
                    {
                        var amount = new byte[8];
                        fs.Read(amount);
                        var y = BitConverter.IsLittleEndian;
                        mpsRecord.Amount = BitConverter.ToDouble((BitConverter.IsLittleEndian ?amount.Reverse() : amount).ToArray());
                    }
                    await conn.InsertAsync(mpsRecord);
                }

                var total = (await conn.ExecuteQueryAsync(@"
                        SELECT SUM
	                    (
	                    CASE TransactionCode
		                    WHEN 0 then Amount
		                    WHEN 1 then - Amount
		                    ELSE 0
		                    END
	                    ) as Total
                    FROM Mps7Record
                    WHERE UserId = @userId", new
                    {
                        userId = 4596061716608039 //4596061716608039//2456938384156277127
                    })).Select(z => z as IDictionary<string, object>).FirstOrDefault().FirstOrDefault(v => v.Key == "Total").Value;

                if (total != null)
                {
                    Console.WriteLine($"{Convert.ToDouble(total)}");                    
                }
                //ConsoleTable.From(display).Configure(o => o.NumberAlignment = Alignment.Right).Write(Format.Alternative);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Console.WriteLine($"balance for user 2456938384156277127=0.00");
        }
    }

    public class Total
    {
        public float Result { get; set; }
    }
    
}