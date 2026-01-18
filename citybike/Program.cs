using CsvHelper;
using System.Globalization;

namespace citybike;

class Program
{
    static List<Trip> LoadTrips(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "yyyy-MM-dd HH:mm:ss.FFF", "yyyy-MM-dd HH:mm:ss" };

        return csv.GetRecords<Trip>().ToList();
    }

    static string GetTimeBlock(int hour)
    {
        if(hour >= 2 && hour < 6) return "02-06";
        if(hour >= 6 && hour < 10) return "06-10";
        if(hour >= 10 && hour < 14) return "10-14";
        if(hour >= 14 && hour < 18) return "14-18";
        if(hour >= 18 && hour < 22) return "18-22";
        return "22-02";
    }

    static void Main()
    {
        var trips = LoadTrips("../data/JC-202509-citibike-tripdata.csv");
        Console.WriteLine($"Loaded {trips.Count} trips");
        
        Console.WriteLine("\nTop 10 station pairs by count:");
        var topStations = trips
            .Where(t => t.start_station_name != null && t.end_station_name != null)
            .GroupBy(t => (t.start_station_name, t.end_station_name))
            .OrderByDescending(g => g.Count())
            .Take(10);
        
        int rank = 1;
        foreach(var g in topStations)
        {
            Console.WriteLine($"{rank, 2}. {g.Key.start_station_name} -> {g.Key.end_station_name} | {g.Count()} rides");
            rank++;
        }
        
        Console.WriteLine($"\nOvernight rides by weekdays (when started):");
        var overNightRidesByWeekdays = trips
            .Where(t => t.started_at.Date.AddDays(1) == t.ended_at.Date)
            .GroupBy(t => t.started_at.DayOfWeek)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .OrderByDescending(d => d.Count);
        
        foreach(var d in overNightRidesByWeekdays)
        {
            Console.WriteLine($"{d.Day} | {d.Count} rides");
        }

        Console.WriteLine($"\nTotal time in given dataset spent riding at night grouped by member vs casual (night is 10PM-4AM):");
        var totalTimeAtNightMemberType = trips
            .Where(t => t.started_at.Date.AddDays(1) == t.ended_at.Date && t.started_at.Hour >= 22 && t.ended_at.Hour < 4)
            .GroupBy(t => t.member_casual)
            .Select(g => new { 
                MemberType = g.Key, 
                TotalMinutes = g.Sum(t => (t.ended_at - t.started_at).TotalMinutes),
                Count = g.Count(),
            });
        
        foreach(var m in totalTimeAtNightMemberType)
        {
            Console.WriteLine($"{m.MemberType} | {m.TotalMinutes / 60.0:F1} hours | {m.Count} rides");
        }

        Console.WriteLine($"\nAverage duration by time block");
        var avgDurationByTimeBlock = trips
            .Where(t => (t.ended_at - t.started_at).TotalMinutes > 0)
            .GroupBy(t => GetTimeBlock(t.started_at.Hour))
            .Select(g => new
            {
                TimeBlock = g.Key,
                AvgMinutes = g.Average(t => (t.ended_at - t.started_at).TotalMinutes),
                Count = g.Count()
            })
            .OrderByDescending(b => b.AvgMinutes);
        
        foreach(var b in avgDurationByTimeBlock)
        {
            Console.WriteLine($"{b.TimeBlock} | {b.AvgMinutes:F1} min | {b.Count} rides");
        }

        
        var leastPopularStartStation = trips
            .Where(t => t.start_station_name != null)
            .GroupBy(t => t.start_station_name)
            .OrderBy(g => g.Count())
            .First();
        Console.WriteLine($"\nAll destination from least popular station ({leastPopularStartStation.Key})");
        var topDestinationFromLeastPopular = trips
            .Where(t => t.start_station_name == leastPopularStartStation.Key && t.end_station_name != null)
            .GroupBy(t => t.end_station_name)
            .Select(g => new
            {
                Destination = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(s => s.Count);
        
        rank = 1;
        foreach(var s in topDestinationFromLeastPopular)
        {
            Console.WriteLine($"{rank, 2}. {s.Destination} | {s.Count} rides");
            rank++;
        }
    }
}