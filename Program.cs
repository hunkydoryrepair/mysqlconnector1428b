using MySqlConnector;
using System.Threading;
using System.Threading.Tasks;
using System;


class MySyncContext : SynchronizationContext
{
    public MySyncContext()
    {
    }
    public override bool Equals(object obj)
    {
        var other = obj as MySyncContext;
        return (other != null); // we only create one.
    }

    public override SynchronizationContext CreateCopy()
    {
        // Console.WriteLine("Copy Context");
        return new MySyncContext();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        SynchronizationContext.SetSynchronizationContext(new MySyncContext());
        Console.WriteLine("Override Context {0}: Thread {1}", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
        using (var conn = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=plposdev;UID=devpos;Pwd=happy123;Charset=utf8mb4;ConnectionReset=true;maxpoolsize=1"))
        {
            await conn.OpenAsync();
            Console.WriteLine("After OpenAsync: {0}, Thread {1}", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
            if (SynchronizationContext.Current == null)
            {
                Console.WriteLine("**** Synchronization Context should still be MySyncContext! ****");
            }
        }

        // don't synchronize context.
        await Task.Run(async () =>
        {
            // In our threadpool thread, we want a different context.
            Console.WriteLine("New Threadpool Thread Context {0}, Thread {1}", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
            using (var conn = new MySqlConnection("Server=127.0.0.1;Port=3306;Database=plposdev;UID=devpos;Pwd=happy123;Charset=utf8mb4;ConnectionReset=true;maxpoolsize=1"))
            {
                await conn.OpenAsync();
                Console.WriteLine("After OpenAsync: {0}, Thread: {1}", SynchronizationContext.Current?.GetType().Name ?? "null", Thread.CurrentThread.ManagedThreadId);
            }
            if (SynchronizationContext.Current != null)
            {
                Console.WriteLine("**** Synchronization Context should still be NULL ****");
            }
        });
    }
}
