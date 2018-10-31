using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace TextDiffViewer
{
    /// <summary>
    /// Summary description for AddLog
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
     [System.Web.Script.Services.ScriptService]
    public class AddLog : System.Web.Services.WebService
    {
        /// <summary>
        /// Web method called when a page load and renders completely. Called from clientside using ajax call
        /// </summary>
        /// <param name="param1">total page load time sent from client side</param>
        [WebMethod]
        public void Save(string param1)
        {
            Utility.Log(HttpContext.Current.Server.MapPath("log.txt"), "Time Spent in Loading Page: " + param1 );
            Console.Write("success");
        }

    }
}
