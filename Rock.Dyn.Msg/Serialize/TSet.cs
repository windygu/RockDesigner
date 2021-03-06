using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Dyn.Msg
{
    public struct TSet
    {
        private TType elementType;
        private int count;

        public TSet(TType elementType, int count)
            : this()
        {
            this.elementType = elementType;
            this.count = count;
        }

        public TType ElementType
        {
            get { return elementType; }
            set { elementType = value; }
        }

        public int Count
        {
            get { return count; }
            set { count = value; }
        }
    }
}
