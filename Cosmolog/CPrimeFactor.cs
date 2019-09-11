namespace Cosmolog
{
    class CPrimeFactor
    {
        #region constructors
        public CPrimeFactor(int intPrime)
        {
            _prime = intPrime;
            _count = 0;
        }
        public CPrimeFactor(int intPrime, int intCount)
        {
            _prime = intPrime;
            _count = intCount;
        }
        #endregion
        #region members
        protected int _prime;
        protected int _count;
        #endregion
        #region properties
        public int Prime
        {
            get { return _prime; }
        }
        public int Count
        {
            get { return _count; }
        }
        #endregion
        #region methods
        public void SetCount(int intThatCount)
        {
            _count = intThatCount;
        }
        public void Increment()
        {
            ++_count;
        }
        public void Decrement()
        {
            --_count;
            if (_count < 0)
            {
                // error should never go less than zero
            }
        }
        #endregion
    }
}
