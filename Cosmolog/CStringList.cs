using System;
using System.Collections.Generic;
// (c)Richard E. Knouse 2015-1019

namespace Cosmolog
{
    class CStringList : List<String>
    {
        #region constructors
        public CStringList() : base()
        {
        }
        #endregion
        #region methods
        public int FindMatch(string strMatch)
        {
            int intFound = -1;
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i] == strMatch)
                {
                    intFound = i;
                    break;
                }
            }
            return intFound;
        }
        public void AddUnique(string strLabel)
        {
            int intFound = FindMatch(strLabel);
            if (intFound == -1)
                Add(strLabel);
        }

        #endregion
    }
}
