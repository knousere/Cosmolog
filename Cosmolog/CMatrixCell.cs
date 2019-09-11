using System;
// (c)Richard E. Knouse 2015-1019

namespace Cosmolog
{
    class CMatrixCell : IComparable<CMatrixCell>
    {
        #region constructors
        public CMatrixCell()
        {
            Init();
        }

        // Construct the cell as the product or quotient of quantities x and y
        public CMatrixCell(CQuantity quantityX, CQuantity quantityY, bool bIsProduct)
        {
            _isProduct = bIsProduct;
            _ptrX = new CFactor(quantityX);
            _ptrY = new CFactor(quantityY);
            if (_isProduct)
                _number = quantityX.Number.Product(quantityY.Number);
            else
                _number = quantityX.Number.Quotient(quantityY.Number);
            _isExact = (quantityX.IsExact && quantityY.IsExact);
        }

        private void Init()
        {
            _isAssigned = false;
            _isExact = false;
        }
        #endregion
        #region members
        bool _isProduct;
        bool _isAssigned;
        CQtyPointer _ptrQty;   // the quantity that this cell resolves to
        CFactor _ptrX;
        CFactor _ptrY;
        CNumber _number;
        bool _isExact;
        #endregion
        #region properties
        public CFactor PtrX
        {
            get { return _ptrX; }
            set { _ptrX = value; }
        }
        public CFactor PtrY
        {
            get { return _ptrY; }
            set { _ptrY = value; }
        }
        public CQtyPointer PtrQty
        {
            get { return _ptrQty; }
            set { _ptrQty = value; _isAssigned = true; }
        }
        public bool IsProduct
        {
            get { return _isProduct; }
        }
        public bool IsExact
        {
            get { return _isExact; }
        }
        public bool IsAssigned
        {
            get { return _isAssigned; }
        }
        public CNumber Number
        {
            get { return _number; }
        }
        #endregion
        #region methods
        public bool IsEquivalent(CMatrixCell thatCell)
        {
            return this._number.IsEquivalent(thatCell.Number);
        }
        // true if this cell resolves to a quantity that is not just a factor of 1
        public bool IsMatrixQuantity()
        {
            bool bReturn = true;
            CQuantity qtyX = this.PtrX.Qty;
            CQuantity qtyY = this.PtrY.Qty;
            if (this.IsProduct)
            {
                if (qtyX.Label == "1" || qtyY.Label == "1")
                    bReturn = false;
            }
            else
            {
                if (qtyY.Label == "1")
                    bReturn = false;
            }
            return bReturn;
        }
        #endregion
        public string DumpToString()
        {
            string strDump;
            if (_ptrQty == null)
                strDump = "null\t";
            else
                strDump = _ptrQty.ToString()+"\t";

            strDump += this.ToString() + "\t";
            if (_number == null)
                strDump += "\t";
            else
                strDump += _number.ToString() + "\t" + _number.LogToString();
            return strDump;
        }
        #region wrappers
        public int CompareTo(CMatrixCell otherMatrixCell)
        {
            return _number.CompareTo(otherMatrixCell.Number);
        }
        public override string ToString()
        {
            string strReturn;
            strReturn = (_ptrX == null? "null": _ptrX.ToString());
            strReturn += _isProduct ? " * " : " / ";
            strReturn += (_ptrY == null? "null": _ptrY.ToString());
            return strReturn;
        }
        #endregion
    }
}
