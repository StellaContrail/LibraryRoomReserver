using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryroomReserverGUI
{
    class PageHandler
    {

        protected void Check(HtmlDocument doc, string id) => doc.ID(id).NullCheck().SetAttribute("checked", "checked");

        protected void ChangeText(HtmlDocument doc, string id, string innerText) => doc.ID(id).NullCheck().InnerText = innerText;

        protected void SelectMenu(HtmlDocument doc, string id, string value) => doc.ID(id).NullCheck().SetAttribute("value", value);
    }

    static class ExtendedMethods
    {
        static public HtmlElement ID(this HtmlDocument doc, string id) => doc.GetElementById(id);

        static public HtmlElement NullCheck(this HtmlElement element)
        {
            if (element == null)
            {
                MessageBox.Show("Failed to get DOM. Chances that page has been changed.");
                Application.Exit();
                return null;
            }
            else
            {
                return element;
            }
        }

        // Translate("月")[typeof(bool)] => data.day.Monday
        // int <-> DayOfWeek <-> string <-> data.day
        // output[typeof(int)]は月 -> 1, 火 -> 2, 水 -> 3, ..., 日 -> 7
        // これはHTMLの記法に基づいたもの
        // DayOfWeekは日 -> 0, 月 -> 1なので気をつけること
        // int numberはHTMLの記法のほうを基準とした
        // -> 外部からはHTML中心で内部ではDayOfWeek中心
        static public Dictionary<System.Type, dynamic> Translate<Type>(this ReservationData data, Type original)
        {
            DayOfWeek DayOfWeekfromChar(char day) => (DayOfWeek)Enum.ToObject(typeof(DayOfWeek), "日月火水木金土".IndexOf(day));
            // IntはDayOfWeek基準
            string ENfromInt(int dayInt) => "Sun   Mon   Tues  WednesThurs Fri   Satur ".Substring(dayInt * 6, 6).TrimEnd() + "day";
            char JPfromEN(string source)
            {
                int index = "SuMoTuWeThFrSa".IndexOf(source.Remove(2, source.Length - 2)) / 2;
                return "日月火水木金土"[index];
            }
            bool FetchDayBoolByName(string name) => (bool)typeof(_Day).GetProperty(name).GetValue(data.day);

            Dictionary<System.Type, dynamic> output = new Dictionary<System.Type, dynamic>();
            switch (original)
            {
                case int number:
                    if (0 < number && number < 8)
                    {
                        output[typeof(int)] = number;
                        string dayNameEN = ENfromInt(number == 7 ? 0 : number);
                        output[typeof(char)] = JPfromEN(dayNameEN);
                        output[typeof(DayOfWeek)] = DayOfWeekfromChar(output[typeof(char)]);
                        output[typeof(bool)] = FetchDayBoolByName(dayNameEN);
                    }
                    return output;
                case char dayName:
                    output[typeof(char)] = dayName;
                    output[typeof(DayOfWeek)] = DayOfWeekfromChar(dayName);
                    output[typeof(int)] = (int)output[typeof(DayOfWeek)] + 1;
                    output[typeof(bool)] = FetchDayBoolByName(ENfromInt((int)output[typeof(DayOfWeek)]));
                    return output;
                default:
                    return output;
            }
        }
    }
}
