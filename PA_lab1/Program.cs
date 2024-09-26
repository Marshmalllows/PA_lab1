using System.Diagnostics;
using System.Text;

namespace PA_lab1;

internal static class Program
{
    private static long _size;

    private static long _gigabytes;

    private static int _sortedId;
    
    public static void Main()
    {
        GenerateFile();
        
        Console.WriteLine("Sorting...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        PolyphaseSort();
        stopwatch.Stop();
        
        Console.WriteLine($"Done! Time elapsed: {stopwatch.Elapsed.Minutes} minutes {stopwatch.Elapsed.Seconds} seconds\n" +
                          $"Sorted file name: B{_sortedId}.txt");
        Console.ReadKey();
    }

    private static void GenerateFile()
    {
        Console.Write("Enter size of file you want to generate (GB):");
        var inputSize = long.Parse(Console.ReadLine()!);
        _gigabytes = inputSize;
        var targetSize = inputSize * 1024 * 1024 * 1024;
        var currentSize = 0L;
        var random = new Random();
        var sb = new StringBuilder();

        Console.WriteLine("Generating...");
        using var writer = new StreamWriter(@"..\..\..\A.txt", false, Encoding.UTF8);
        while (currentSize < targetSize)
        {
            sb.Clear();
            sb.Append(random.Next());
            sb.Append(Environment.NewLine);
            var line = sb.ToString();
            writer.Write(line);
            _size++;
            currentSize += Encoding.UTF8.GetByteCount(line);
        }
    }

    private static void PolyphaseSort()
    {
        var filesPaths = new string[4];
        filesPaths[0] = @"..\..\..\A.txt";
        filesPaths[1] = @"..\..\..\B1.txt";
        filesPaths[2] = @"..\..\..\B2.txt";
        filesPaths[3] = @"..\..\..\B3.txt";
        var distribution = GetSerialDistribution();

        WriteFiles(filesPaths, distribution.Item2, distribution.Item1);
        Sort(distribution.Item2, distribution.Item1, filesPaths);
    }

    private static void WriteFiles(string[] paths, long serials1, long serials2)
    {
        using var reader = new StreamReader(paths[0]);
        using var writer1 = new StreamWriter(paths[1]);
        using var writer2 = new StreamWriter(paths[2]);
        var serialSize = _size / (serials1 + serials2);
        var serial = new List<int>();
        var position = 0L;

        for (var i = 1L; i < serials1 + serials2; i++)
        {
            for (var j = 0; j < serialSize; j++)
            {
                serial.Add(int.Parse(reader.ReadLine()!));
                position++;
            }

            serial.Sort();

            if (i <= serials1)
            {
                foreach (var number in serial)
                {
                    writer1.WriteLine(number);
                }
            }
            else
            {
                foreach (var number in serial)
                {
                    writer2.WriteLine(number);
                }
            }

            serial.Clear();
        }

        for (var i = position; i < _size; i++)
        {
            serial.Add(int.Parse(reader.ReadLine()!));
        }

        serial.Sort();

        foreach (var number in serial)
        {
            writer2.WriteLine(number);
        }
    }

    private static void Sort(long serials1, long serials2, string[] paths)
    {
        var serials = new [] {serials1, serials2, 0L};
        var indexes = new [] {1, 2, 3};
        var isDone = false;

        while (!isDone)
        {
            for (var i = 0; i < 2; i++)
            {
                for (var j = i; j < 3; j++)
                {
                    if (serials[i] <= serials[j]) continue;
                    (serials[i], serials[j]) = (serials[j], serials[i]);
                    (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
                }
            }

            if (serials[1] == 0)
            {
                isDone = true;
            }
            else
            {

                Merge(paths, indexes);

                serials[0] = serials[1];
                serials[1] = serials[2] - serials[1];
                serials[2] = 0;
            }
        }
    }

    private static void Merge(string[] paths, int[] indexes)
    {
        _sortedId = indexes[0];
        using (var reader1 = new StreamReader(paths[indexes[1]]))
        using (var reader2 = new StreamReader(paths[indexes[2]]))
        using (var writer = new StreamWriter(paths[indexes[0]]))
        {
            var size = File.ReadLines(paths[indexes[1]]).Count();
            var isDone1 = false;
            var isDone2 = false;
            var current1 = 2;
            var current2 = 2;
            var x1 = int.Parse(reader1.ReadLine()!);
            var x2 = int.Parse(reader2.ReadLine()!);
            var y1 = int.Parse(reader1.ReadLine()!);
            var y2 = int.Parse(reader2.ReadLine()!);

            if (x1 <= x2)
            {
                writer.WriteLine(x1);
                if (x1 > y1)
                {
                    isDone1 = true;
                }

                x1 = y1;
                y1 = int.Parse(reader1.ReadLine()!);
                current1++;
            }
            else
            {
                writer.WriteLine(x2);
                if (x2 > y2)
                {
                    isDone2 = true;
                }

                current2++;

                x2 = y2;
                y2 = int.Parse(reader2.ReadLine()!);
            }

            for (var i = current1; i < size; i++)
            {
                if (!(isDone1 || isDone2))
                {
                    if (x1 <= x2)
                    {
                        writer.WriteLine(x1);
                        if (x1 > y1)
                        {
                            isDone1 = true;
                        }

                        x1 = y1;
                        y1 = int.Parse(reader1.ReadLine()!);
                    }
                    else
                    {
                        writer.WriteLine(x2);
                        if (x2 > y2 || y2 == -1)
                        {
                            isDone2 = true;
                        }

                        i--;
                        current2++;
                        x2 = y2;
                        if (!int.TryParse(reader2.ReadLine()!, out y2))
                        {
                            y2 = -1;
                        }
                    }
                }
                else
                {
                    if (isDone1)
                    {
                        writer.WriteLine(x2);
                        if (x2 > y2)
                        {
                            isDone1 = false;
                        }

                        x2 = y2;
                        y2 = int.Parse(reader2.ReadLine()!);
                        i--;
                        current2++;
                    }

                    if (!isDone2) continue;
                    writer.WriteLine(x1);
                    if (x1 > y1)
                    {
                        isDone2 = false;
                    }

                    x1 = y1;
                    y1 = int.Parse(reader1.ReadLine()!);
                }
            }

            var isCompleted = false;

            while (!isCompleted)
            {
                if (!(isDone1 || isDone2))
                {
                    if (x1 <= x2)
                    {
                        writer.WriteLine(x1);
                        if (y1 == -1)
                        {
                            isDone1 = true;
                        }

                        x1 = y1;
                        y1 = -1;
                    }
                    else
                    {
                        writer.WriteLine(x2);
                        if (x2 > y2 || y2 == -1)
                        {
                            isDone2 = true;
                        }
                        
                        x2 = y2;
                        if (!int.TryParse(reader2.ReadLine()!, out y2))
                        {
                            y2 = -1;
                        }
                        
                        current2++;
                    }
                }
                else
                {
                    if (isDone1)
                    {
                        writer.WriteLine(x2);
                        if (x2 > y2 || y2 == -1)
                        {
                            isCompleted = true;
                        }

                        x2 = y2;
                        if (!int.TryParse(reader2.ReadLine()!, out y2))
                        {
                            y2 = -1;
                        }
                        current2++;
                    }
                
                    if (isDone2)
                    {
                        writer.WriteLine(x1);
                        if (y1 == -1)
                        {
                            isCompleted = true;
                        }

                        x1 = y1;
                        y1 = -1;
                    }
                }
            }
            
            reader1.Close();

            using (var writer2 = new StreamWriter(paths[indexes[1]]))
            {
                if (x2 != -1)
                {
                    writer2.WriteLine(x2);
                    writer2.WriteLine(y2);
                    var toWrite = File.ReadLines(paths[indexes[2]]).Count();

                    for (var i = current2; i < toWrite; i++)
                    {
                        writer2.WriteLine(reader2.ReadLine());
                    }
                }
                else
                {
                    writer2.WriteLine();
                }
            }
        }
        
        using (var clearer = new StreamWriter(paths[indexes[2]]))
        { 
            clearer.WriteLine();
        }
    }
    
    private static (long, long) GetSerialDistribution()
    {
        var serials = 10 * _gigabytes;
        var fib1 = 0L;
        var fib2 = 1L;
        var previous = 0L;
        while (serials > fib1 + fib2)
        {
            var temp = fib1 + fib2;
            previous = fib1;
            fib1 = fib2;
            fib2 = temp;
        }
        
        return (previous, fib1);
    }
}