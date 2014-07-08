using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Driver
    {
        public Driver()
        { 
            this.Car = new Car();
        }

        public int CarIndex { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Initials { get; set; }
        public int UserId { get; set; }
        public int IRating { get; set; }
        public int SafetyRating { get; set; }
        public LicenceLevel Licence { get; set; }
        public string ClubName { get; set; }
        public string Division { get; set; }
        public Car Car { get; set; }
        public bool Spectator { get; set; }
        public int NumberPlateInt
        {
            get
            {
                if (Car.CarNumber != null)
                    return Int32.Parse(Car.CarNumber);
                else return 0;
            }
        }

        public int NumberPlatePadded
        {
            get
            {
                if (Car.CarNumber != null) 
                    return Utils.PadCarNum(Car.CarNumber);
                else 
                    return -1;
            }
        }

        public string SrString
        {
            get
            {
                if (SafetyRating == -1)
                    return "Unknown";

                return Licence.ToString() + ((double)SafetyRating / 100).ToString("0.00");
            }
        }

        public override string ToString()
        {
            string lastName = FullName;
            int i = lastName.LastIndexOf(' ');
            lastName = lastName.Substring(i);

            return "#" + Car.CarNumber + " " + lastName;
        }

        public string LastUpperName
        {
            get
            {
                string lastName = FullName;
                int i = lastName.LastIndexOf(' ');
                return lastName.Substring(i).ToUpper();
            }
        }
    }
}
