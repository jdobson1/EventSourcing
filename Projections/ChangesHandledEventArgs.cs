using System;
using System.Collections.Generic;

namespace Projections
{
    public class ChangesHandledEventArgs : EventArgs
    {
        public UpdatedView View { get; set; }
    }
}