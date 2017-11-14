using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryroomReserverGUI
{
    class LibraryPageHandler : PageHandler
    {
        public void Login(HtmlDocument doc, string id, string pass)
        {
            ChangeText(doc, "portalId", id);
            ChangeText(doc, "portalPasswd", pass);
            doc.InvokeScript("doLogin", new Object[] { (Object)"/portal/portal/portalLogin/doLogin" });
        }

        public void ApplyReservationData(HtmlDocument doc, ReservationData data)
        {
            SelectMenu(doc, "selReserveTimeFromH", data.fromTime.Substring(0, 2));
            SelectMenu(doc, "selReserveTimeFromM", data.fromTime.Substring(3, 2));
            SelectMenu(doc, "selReserveTimeToH", data.toTime.Substring(0, 2));
            SelectMenu(doc, "selReserveTimeToM", data.toTime.Substring(3, 2));
            ChangeText(doc, "reserveName", data.reason);
            ChangeText(doc, "personCount", data.numberOfPeople.ToString());
        }

        public void ApplyReservationData(HtmlDocument doc,  ReservationData data, int room, string date)
        {
            SelectMenu(doc, "selSearchRoom", $"001{room - 3}");
            ChangeText(doc, "searchDate", $"{DateTime.Now.ToString("yyyy")}/{date.Substring(0, 2)}/{date.Substring(3, 2)}");
        }

        public void CheckAvailableReservations(HtmlDocument doc) => doc.GetElementById("itemCheckA000").InvokeMember("Click");

        public void InvokeScript(HtmlDocument doc, string method, params object[] args) => doc.InvokeScript(method, args);

        public void LimitSpan(HtmlDocument doc)
        {
            DateTime present = DateTime.Now;
            ChangeText(doc, "searchPeriodFrom", present.ToShortDateString());
            present = present.AddDays(7);
            ChangeText(doc, "searchPeriodTo", present.ToShortDateString());
        }
    }
}
