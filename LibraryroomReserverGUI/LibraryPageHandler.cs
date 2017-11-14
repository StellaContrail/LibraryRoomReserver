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

            /*
            DateTime present = DateTime.Now;
            DateTime reservationFrom = new DateTime(present.Year, present.Month, present.Day, Convert.ToInt32(data.fromTime.Split(':')[0]), Convert.ToInt32(data.fromTime.Split(':')[1]), 0);
            present = present.AddDays(7);
            DateTime reservationTo = new DateTime(present.Year, present.Month, present.Day, Convert.ToInt32(data.toTime.Split(':')[0]), Convert.ToInt32(data.toTime.Split(':')[1]), 0);
            ChangeText(doc, "reserveDateFrom", reservationFrom.ToShortDateString());
            ChangeText(doc, "reserveDateTo", reservationTo.ToShortDateString());
            SelectMenu(doc, "selReserveTimeFromH", reservationFrom.ToString("HH"));
            SelectMenu(doc, "selReserveTimeFromM", reservationFrom.ToString("mm"));
            SelectMenu(doc, "selReserveTimeToH", reservationTo.ToString("HH"));
            SelectMenu(doc, "selReserveTimeToM", reservationTo.ToString("mm"));
            
            Check(doc, "rdoIntervalEveryweek");
            for (int i = 1; i <= 7; i++)
            {
                if (data.Translate(i)[typeof(bool)])
                {
                    Check(doc, $"itemCheckA00{i}");
                }
            }

            ChangeText(doc, "reserveName", data.reason);

            ChangeText(doc, "personCount", data.numberOfPeople.ToString());
            */

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
