using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP01_Stepanets
{
    internal class Core
    {
        private static DB_Stepanets_SalonBeautyEntities _context;

        public static DB_Stepanets_SalonBeautyEntities GetContext()
        {
            if (_context == null)
            {
                _context = new DB_Stepanets_SalonBeautyEntities();
            }
            return _context;
        }
    }
}
