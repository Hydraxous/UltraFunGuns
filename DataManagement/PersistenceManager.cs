using System;
using System.Collections.Generic;
using System.Text;

namespace UltraFunGuns.Datas
{
    public static class PersistenceManager
    {
        public static T LoadPersistable<T>()
        {
            if(!typeof(T).IsSerializable)
            {
                return default(T);
            }


            return default(T);


        }
    }
}
