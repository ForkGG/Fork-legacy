using System;

namespace fork.Logic.Model
{
    [Serializable]
    public class SimpleTime
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public SimpleTime(int hours, int minutes)
        {
            if (hours>23||hours<0||minutes>59||minutes<0)
            {
                throw new ArgumentException("Illegal time");
            }
            Hours = hours;
            Minutes = minutes;
        }

        public override string ToString()
        {
            string mins = Minutes < 10 ? "0" + Minutes : Minutes.ToString();
            string hours = Hours < 10 ? "0" + Hours : Hours.ToString();
            return hours+":"+mins;
        }
        
        public SimpleTime(){}
    }
}