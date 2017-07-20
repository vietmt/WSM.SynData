using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFA.SynAll
{
    public class Attendance
    {
        #region Properties
        public int EnrollNumber;
        public DateTime date;
        public bool pushed;
        #endregion
        #region Methods
        public Attendance(int iEnrollNumber, DateTime dtDate)
        {
            EnrollNumber = iEnrollNumber;
            date = dtDate;
            pushed = false;
        }
        #endregion
    }
}
