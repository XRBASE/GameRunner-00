using System;

namespace MathBuddy.Time
{
    public static class Time
    {

        public static string DateToString(DateTime t)
        {
            //sample structure:             "2021-09-27T12:57:02.728+00:00";
            //TODO: timezones?
            t = TimeZoneInfo.ConvertTime(t, TimeZoneInfo.Local, TimeZoneInfo.Utc);
            return $"{t.Year}-{t.Month}-{t.Day}T{t.Hour}:{t.Minute}:{t.Second}.{t.Millisecond}+00:00";
        }
        
        public static DateTime DateFromString(string data)
        {
            //sample structure:             "2021-09-27T12:57:02.728+00:00";

            // split in two [0] =           "2021-09-27"
            //              [1] =           "12:57:02.728+00:00"
            string[] dateTime = data.Split('T');
        
            //split date part   [0] =         "2021"
            //                  [1] =         "09"
            //                  [2] =         "27"
            string[] date = dateTime[0].Split('-');
            //retrieve date from data
            int year = int.Parse(date[0]);
            int month = int.Parse(date[1]);
            int day = int.Parse(date[2]);
        
            //now do time part
            //check if plus timezone or minus
            bool positiveTimeZone = dateTime[1].Contains('+');
            
            // split clock and zone:    [0] 12:57:02.728
            //                          [1] 00:00
            string[] time = (positiveTimeZone)? dateTime[1].Split('+') : dateTime[1].Split('-');

            int z_hour = 0;
            int z_min = 0;
            if (time.Length > 1) {
                // zone:    [0] 00
                //          [1] 00
                string[] zone = time[1].Split(':');
            
                //get values
                z_hour = int.Parse(zone[0]);
                z_min = int.Parse(zone[1]);
                
                //change sign if negative
                if (!positiveTimeZone) {
                    z_hour *= -1;
                    z_min *= -1;
                }
            }
            
            //split clock data  [0] 12
            //                  [1] 57
            //                  [2] 02.728
            string[] clock = time[0].Split(':');
            
            //get values, including timezone
            int hours = int.Parse(clock[0]) + z_hour;
            int minutes = int.Parse(clock[1]) + z_min;
            
            //split seconds     [0] 02
            //                  [1] 728
            string[] secs = clock[2].Split('.');
            int seconds = int.Parse(secs[0]);
            int milliseconds = int.Parse(secs[1].Substring(0, 3));
            
            //return datetime
            //todo: convert back to my timezone
            
            //time in greenwich timezone
            DateTime resultGW = new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
            resultGW = TimeZoneInfo.ConvertTime(resultGW, TimeZoneInfo.Utc, TimeZoneInfo.Local);
            return resultGW;
        }

        public struct Timer
        {
            public float duration;
            private float _elapsed;
            
            public Timer(float duration)
            {
                _elapsed = 0f;
                this.duration = duration;
            }

            public bool Update(float deltaTime)
            {
                _elapsed += deltaTime;
                return _elapsed >= duration;
            }

            public void Reset()
            {
                _elapsed = 0f;
            }
        }
    }
}