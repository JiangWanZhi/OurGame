using System;
using UnityEngine;

public static class TimeUtility
{
    public static readonly DateTime baeTime = new DateTime(1970, 1, 1);

    public const DayOfWeek TheFirstDayOfWeek = DayOfWeek.Monday;


    private static TimeSpan oneDay = TimeSpan.FromDays(1);
    private static string displayName = "(GMT-09:00) Alaskan Standard Time";
    private static string standardName = "Mawson Time";
    private static TimeSpan offset = TimeSpan.FromHours(-8);
    private static TimeZoneInfo defaultTimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(standardName, offset, displayName, standardName);

    public static TimeSpan OneDay
    {
        get { return oneDay; }
        set { oneDay = value; }
    }

    public static void ResetOneDay()
    {
        var newOneDay = TimeSpan.FromDays(1);
        oneDay = newOneDay;
    }

    /// <summary>
    /// 默认时区是否同一天
    /// </summary>
    /// <param name="lhs">UTC时间</param>
    /// <param name="rhs">UTC时间</param>
    /// <returns></returns>
    public static bool IsSameDay(DateTime lhs, DateTime rhs)
    {
        lhs = ConvertTime_Utc2DefaultZoneLocal(lhs);
        rhs = ConvertTime_Utc2DefaultZoneLocal(rhs);

        var ticksPerDay = oneDay.Ticks;

        var min = lhs.Ticks / ticksPerDay * ticksPerDay;
        var max = min + ticksPerDay;

        if (rhs.Ticks >= min && rhs.Ticks < max)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 默认时区是否为下一天
    /// </summary>
    /// <param name="lhs">UTC时间前一天</param>
    /// <param name="rhs">UTC时间后一天</param>
    /// <returns></returns>
    public static bool IsNextDay(DateTime lhs, DateTime rhs)
    {
        lhs = ConvertTime_Utc2DefaultZoneLocal(lhs);
        rhs = ConvertTime_Utc2DefaultZoneLocal(rhs);

        var ticksPerDay = oneDay.Ticks;

        var min = lhs.Ticks / ticksPerDay * ticksPerDay + ticksPerDay;
        var max = min + ticksPerDay;

        if (rhs.Ticks >= min && rhs.Ticks < max)
        {
            return true;
        }

        return false;
    }

    private static int GetDayOfWeek(this DateTime date, DayOfWeek theFirstDayOfWeek = TheFirstDayOfWeek)
    {
        return ((int)date.DayOfWeek - (int)theFirstDayOfWeek + 7) % 7;
    }

    public static bool IsInSameWeek(this DateTime firstDate, DateTime secondDate,
        DayOfWeek theFirstDayOfWeek = TheFirstDayOfWeek)
    {
        var daySpan = (secondDate.Date - firstDate.Date).TotalDays;
        if (daySpan >= 7d || daySpan <= -7d)
        {
            return false;
        }

        var fisrtDayOfWeek = firstDate.GetDayOfWeek(theFirstDayOfWeek);
        var secondDayOfWeek = secondDate.GetDayOfWeek(theFirstDayOfWeek);

        return (daySpan >= 0) ^ (fisrtDayOfWeek >= secondDayOfWeek);
    }

    public static DateTime Now()
    {
        return UtcNow();
    }

    public static DateTime LocalNow()
    {
        return Now().AddHours((DateTime.Now - DateTime.UtcNow).Hours);
    }

    public static DateTime UtcNow()
    {
        //return DateTime.UtcNow;
        return baeTime.AddSeconds(TimeService.Instance.ServerTimestamp);
    }

    public static DateTime SignNow()
    {
        var date = UtcNow();

        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
    }

    public static uint UnixNow()
    {
        //uint unixTimeSeconds = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        //return unixTimeSeconds;
        return (uint)TimeService.Instance.ServerTimestamp;
    }

    //public static int DaysSpan(DateTime start, DateTime end)
    //{
    //    return new TimeSpan(end.Ticks).Days - new TimeSpan(start.Ticks).Days;
    //}

    //public static DateTime CeilToMidNight(DateTime dateTime)
    //{
    //    // ceiling
    //    var ticksPerDay = oneDay.Ticks;
    //    var ticks = (dateTime.Ticks + (ticksPerDay - 1)) / ticksPerDay * ticksPerDay;

    //    var midNight = new DateTime(ticks);
    //    return midNight;
    //}

    //public static DateTime CeilToDayOfWeek(DateTime dateTime, DayOfWeek dayOfWeek)
    //{
    //    var startTime = CeilToMidNight(dateTime);

    //    var startDay = startTime.DayOfWeek;
    //    int days = 0;

    //    if (startDay > dayOfWeek)
    //    {
    //        days = dayOfWeek - startDay + 7;
    //    }
    //    else
    //    {
    //        days = dayOfWeek - startDay;
    //    }

    //    double t = days * oneDay.TotalMinutes;
    //    var target = startTime + TimeSpan.FromMinutes(t);
    //    return target;
    //}

    //public static DateTime TruncateMillisecond(DateTime date)
    //{
    //    var result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
    //    return result;
    //}

    /// <summary>
    /// 方法内已经转换了时区
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string ToShortString(DateTime date)
    {
        date = ConvertTime_Utc2DefaultZoneLocal(date);

        return date.ToString("dd/MM/yy");
    }


    public static DateTime ConvertTime_Utc2DefaultZoneLocal(DateTime time)
    {
        time = DateTime.SpecifyKind(time, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(time, defaultTimeZoneInfo);
    }

    public static DateTime ConvertTime_Local2DefaultZoneLocal(DateTime time)
    {
        time = DateTime.SpecifyKind(time, DateTimeKind.Local);
        return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Local, defaultTimeZoneInfo);
    }

    public static DateTime ConvertTime_DefaultZoneLocal2Utc(DateTime time)
    {
        time = new DateTime(time.Ticks);
        return TimeZoneInfo.ConvertTimeToUtc(time, defaultTimeZoneInfo);
    }

    public static DateTime ConvertTime_DefaultZoneLocal2Local(DateTime time)
    {
        return ConvertTime_DefaultZoneLocal2Utc(time).ToLocalTime();
    }

    /// <summary>
    /// 获取当天还剩下多少时间
    /// </summary>
    /// <returns></returns>
    public static TimeSpan GetTodayTime()
    {
        //var ZeroTime = new TimeSpan(24,0,0);
        //return ZeroTime - UtcNow().TimeOfDay;

        var now = TimeUtility.ConvertTime_Utc2DefaultZoneLocal(TimeUtility.Now());
        var nextRefreshTime = now.AddDays(1);
        nextRefreshTime = new DateTime(nextRefreshTime.Year, nextRefreshTime.Month, nextRefreshTime.Day, 0, 0, 0);

        return nextRefreshTime - now;
    }

    public static DateTime GetDateTime(double timeStamp)
    {
        return new DateTime(1970, 1, 1).AddSeconds(timeStamp);
        //return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(timeStamp);
    }
}
