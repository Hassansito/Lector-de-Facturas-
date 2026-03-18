using System.Collections.Concurrent;

public interface IFileQueue
{
    void Enqueue(string path);
    string Dequeue();
    int Count();
}

public class FileQueue : IFileQueue
{
    private readonly ConcurrentQueue<string> queue = new();

    public void Enqueue(string file)
    {
        queue.Enqueue(file);
    }

    public string Dequeue()
    {
        queue.TryDequeue(out var file);
        return file;
    }

    public int Count()
    {
        return queue.Count;
    }
}