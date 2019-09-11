using System;

namespace Cosmolog
{
    // Used to store a quantity label for a physical quantity.
    // Provision is made to segregate main character for its optional subscript.
    class CSymbol : IComparable<CSymbol>
    {
        #region constructors
        public CSymbol()
        {
            Init();
        }
        public CSymbol(string strThatLabel)
        {
            Init(strThatLabel);
        }

        public CSymbol(CSymbol thatSymbol)
        {
            Init(thatSymbol.Label);
            _description = thatSymbol.Description;
        }
        
        public CSymbol(string strThatMain, string strThatSubscript)
        {
            Init(strThatMain);
            if (strThatSubscript != "")
                _strSubscript = strThatSubscript;
            if (_strSubscript == "")
                _label = _main;
            else
                _label = _main + "_" + _strSubscript;
        }

        public CSymbol(string strThatMain, string strThatSubscript, string strThatDescription)
        {
            Init(strThatMain);
            if (strThatSubscript != "")
                _strSubscript = strThatSubscript;
            if (_strSubscript == "")
                _label = _main;
            else
                _label = _main + "_" + _strSubscript;
            _description = strThatDescription;
        }

        public CSymbol(string strThatMain, string strThatSubscript, string strThatDescription, string strThatUnit)
        {
            Init(strThatMain);
            if (strThatSubscript != "")
                _strSubscript = strThatSubscript;
            if (_strSubscript == "")
                _label = _main;
            else
                _label = _main + "_" + _strSubscript;
            _description = strThatDescription;
            _unit = strThatUnit;
        }

        private void Init(string strThatLabel)
        {
            Init();
            _label = strThatLabel;
            int intPos = strThatLabel.IndexOf('_');
            if (intPos < 0)
            {
                _main = strThatLabel;
            }
            else
            {
                _main = strThatLabel.Substring(0, intPos);
                _strSubscript = strThatLabel.Substring(intPos + 1);
            }
        }
        private void Init()
        {
            _label = "";
            _main = "";
            _strSubscript = "";
            _description = "";
            _unit = "";
        }
        #endregion
        #region members
        string _label;
        string _main;
        string _strSubscript;
        string _description;
        string _unit;
        #endregion
        #region properties
        public string Label
        {
            get { return _label; }
        }
        public string SimpleLabel
        {
            get { return _label.Replace("_", ""); }
        }
        public string Main
        {
            get { return _main; }
        }
        public string Subscript
        {
            get { return _strSubscript; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }
        #endregion
        #region methods
        public bool ContainsX()
        {
            return (_label.Length > 0 && _label[0] == 'X');
        }
        // true if suffix is + or - indicating sense of the log of the underlying value
        public bool ContainsSense()
        {
            return (_label.Length > 0 && _label.IndexOfAny(new char[]{'+','-'}) > -1);
        }
        public bool IsMatch(CSymbol thatSymbol)
        {
            return this._label == thatSymbol.Label;
        }
        // symbol.SymbolInverse() = symbol- or symbol+ and vice versa.
        // strSense is the sign of the log of the underlying value.
        public CSymbol SymbolInverse(string strSense)
        {
            string strInverse;
            if (this.ContainsSense())
            {
                // in all cases except π the inverse of a signed quantity is unsigned.
                strInverse = _label.Substring(0, _label.Length - 1);
                if (strInverse[strInverse.Length - 1] == '_')
                    strInverse = _label.Substring(0, _label.Length - 1);
                if (this._main == "π")
                    strInverse = this._main + "_" + strSense;
            }
            else
            {
                if (_strSubscript.Length > 0)
                    strInverse = _label + strSense;
                else
                    strInverse = _label + "_" + strSense;
            }
            CSymbol symbolInverse = new CSymbol(strInverse);
            symbolInverse.Description = "Inverse of " + this.Description;
            // !!! may want to set unit here also
            return symbolInverse;
        }
        #endregion
        #region wrappers
        public override string ToString()
        {
            return _label;
        }
        public int CompareTo(CSymbol otherSymbol)
        {
            // Compare case sensitive
            return String.Compare(_label, otherSymbol.Label, false);
        }
        #endregion
    }
}
