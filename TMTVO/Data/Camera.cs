using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMTVO.Data
{
    public class Camera
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public Camera()
        {
            Name = "";
            Id = -1;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
