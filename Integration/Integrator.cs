using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.Integration
{
    interface Integrator
    {
        //Here we create an interface which acts as a contract for all
        //subsequent integration methods
        //They all must contain an integrate method which returns a double.
        double Integrate();
    }
}
