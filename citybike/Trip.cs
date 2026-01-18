namespace citybike;

/*
"ride_id",
"rideable_type",
"started_at",
"ended_at",
"start_station_name",
"start_station_id",
"end_station_name",
"end_station_id",
"start_lat",
"start_lng",
"end_lat",
"end_lng",
"member_casual"
*/

public sealed class Trip
{
    public string ride_id { get; set; } = "";
    public string rideable_type { get; set; } = "";
    public DateTime started_at { get; set; }
    public DateTime ended_at { get; set; }

    public string? start_station_name { get; set; }
    public string? start_station_id { get; set; }
    public string? end_station_name { get; set; }
    public string? end_station_id { get; set; }

    public double? start_lat { get; set; }
    public double? start_lng { get; set; }
    public double? end_lat { get; set; }
    public double? end_lng { get; set; }

    public string member_casual { get; set; } = "";
};

