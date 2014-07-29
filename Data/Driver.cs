using System;
using System.Collections.Generic;
using System.Globalization;
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
                    return int.Parse(Car.CarNumber);
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
            return "#" + Car.CarNumber + " " + LastUpperName;
        }

        public string LastUpperName
        {
            get
            {
                string lastName = FullName;
                string[] names = lastName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names[names.Length - 1].StartsWith("Jr", true, CultureInfo.CurrentCulture) || names[names.Length - 1].StartsWith("Sr", true, CultureInfo.CurrentCulture))
                    lastName = names[names.Length - 2] + " " + names[names.Length - 1];
                else
                    lastName = names[names.Length - 1];

                return lastName.ToUpper();
            }
        }

        public string ThreeLetterCode
        {
            get
            {
                // TODO
                return this.Initials;
            }
        }
    }
}
