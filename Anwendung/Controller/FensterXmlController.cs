using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Controller
{
    /// <summary>
    /// Stellt einen Dienst zum
    /// Serialisieren im Xml Format bereit
    /// </summary>
    internal class FensterXmlController 
        : Anwendung.Controller.XmlController<Daten.FensterInfos>
    {
    }
}