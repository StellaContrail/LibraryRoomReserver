using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Parser.Html;
using System.Collections;
using Codeplex.Data;
using System.IO;
using System.Text.RegularExpressions;
using AngleSharp.Dom.Html;

namespace LibraryroomReserverGUI
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();
        string rawJson;
        ReservationData data = new ReservationData();
        private void Form1_Load(object sender, EventArgs e)
        {
            // Load Config.json into settings
            using (StreamReader stream = new StreamReader($"{Environment.CurrentDirectory}\\Config\\Config.json", Encoding.UTF8))
            {
                rawJson = stream.ReadToEnd();
            }
            data = (ReservationData)DynamicJson.Parse(rawJson);

            // Navigate to login form
            WebBrowser.Navigate(new Uri("https://opac.lib.niigata-u.ac.jp/portal/portal/portalBoothStatus/?lang=ja&roomGroupId=01"));
        }

        int pageStep = 0;
        LibraryPageHandler handler = new LibraryPageHandler();
        HtmlParser parser = new HtmlParser();

        struct Error
        {
            public string Date;
            public string Time;
            public string Reason;
        }

        Dictionary<string, int> availableDates = new Dictionary<string, int>();
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // WebBrowserの複数呼び出しを阻止
            if (WebBrowser.ReadyState != WebBrowserReadyState.Complete)
            {
                return;
            }
            StatusLabel.Text = "Document completely loaded";
            HtmlDocument doc = WebBrowser.Document;
            IHtmlDocument docText = parser.Parse(WebBrowser.DocumentText);

            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/portal/portalBoothStatus/?lang=ja&roomGroupId=01"))
            {
                handler.LimitSpan(doc);
                handler.InvokeScript(doc, "doFwdChange", "/portal/portal/portalBoothStatus/doViewPeriod");
            }

            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/portal/portalBoothStatus/doViewPeriod"))
            {
                availableDates = FetchAvailableRoomAndDates(docText);

                WebBrowser.Navigate(new Uri("https://opac.lib.niigata-u.ac.jp/portal/"));
            }

            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/"))
            {
                handler.Login(doc, data.id, data.pass);
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/portal/portalLogin/doLogin"))
            {
                handler.InvokeScript(doc, "doSelectMainMenu", "/portal/admin/selectMenu/doSelectPublicUseMainMenu", 11, "");
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/admin/selectMenu/doSelectPublicUseMainMenu?selectedMenuId=11&selectMenu=1"))
            {
                handler.InvokeScript(doc, "doFwdNewReserveProc", "/portal/myl/scMylBO001/doForwardNewRequest");
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/myl/scMylBO001/doForwardNewRequest"))
            {
                KeyValuePair<string, int> availableDate = availableDates.First();
                handler.ApplyReservationData(doc, data, availableDate.Value, availableDate.Key);
                handler.InvokeScript(doc, "doSearch", "/portal/myl/scMylBO002/doForwardSearch");
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/myl/scMylBO002/doForwardSearch"))
            {
                handler.ApplyReservationData(doc, data);
                handler.InvokeScript(doc, "doFwdProc", "/portal/myl/scMylBO002/doForwardConfirm");
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/myl/scMylBO002/doForwardConfirm"))
            {
                handler.InvokeScript(doc, "doFwdProc", "/portal/myl/scMylBO003/doForwardConfirm");
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/myl/scMylBO003/doForwardConfirm"))
            {
                // Dictionaryにまだアイテムが有るなら
                handler.InvokeScript(doc, "doForwardNew", "/portal/myl/scMylBO004/doForwardNew");
            }
            if (IsPageURL("https://opac.lib.niigata-u.ac.jp/portal/myl/scMylBO004/doForwardNew"))
            {
                /*
                    handler.ApplyReservationData(doc, data, availableDate.Value, availableDate.Key);
                    handler.InvokeScript(doc, "doSearch", "/portal/myl/scMylBO002/doForwardSearch");
                */
            }

            #region OldCodes
            /*
            switch (pageStep)
            {
                case 0:
                    handler.Login(doc, data.id, data.pass);
                    break;
                case 1:
                    handler.InvokeScript(doc, "doSelectMainMenu", "/portal/admin/selectMenu/doSelectPublicUseMainMenu", 11, "");
                    break;
                case 2:
                    handler.InvokeScript(doc, "doFwdNewReserveProc", "/portal/myl/scMylBO001/doForwardNewRequest");
                    break;
                    
                case 3:
                    handler.ApplyReservationData(doc, data);
                    handler.InvokeScript(doc, "doFwdProc", "/portal/myl/scMylBO011/doForwardConfirm");
                    break;
                case 4:
                    var errors = FetchErrors(WebBrowser.DocumentText);
                    if (errors != null)
                    {
                        foreach (Error error in errors)
                        {
                            MessageBox.Show($"Error：{error.Time} {error.Date}\n{error.Reason}");
                        }
                        // エラーが発生した日の取得可能時間帯を知りたい
                        Logout(doc, WebBrowser.DocumentText);
                        

                        //Application.Exit();
                    }
                    else
                    {
                        //handler.CheckAvailableReservations(doc);
                        //handler.InvokeScript(doc, "doFwdProc", "/portal/myl/scMylBO012/doForwardConfirmate");
                    }
                    break;
                case 5:
                    //handler.InvokeScript(doc, "doFwdFixEdit", "/portal/myl/scMylBO013/doFwdFixEdit");
                    break;
                case 6:
                    MessageBox.Show("Successfully Reserved.");
                    Logout(doc, WebBrowser.DocumentText);
                    Application.Exit();
                    break;
                    
            }*/
#endregion
                pageStep++;
        }

        Dictionary<string, int> FetchAvailableRoomAndDates(IHtmlDocument docText)
        {
            Dictionary<string, int> availableDatesAndRoom = new Dictionary<string, int>();
            Dictionary<string, int> unavailableDatesAndRoom = new Dictionary<string, int>();
            for (int i = 4; i <= 5; i++)
            {
                foreach (AngleSharp.Dom.IElement otherReservationsAtNumRoom in FetchAllOtherReservationsAt($"3F グループ学習室{i}(中央館)", docText))
                {
                    string source = otherReservationsAtNumRoom.InnerHtml;
                    source = Regex.Replace(source, "</?b>", "");
                    source = Regex.Replace(source, "<font .*><a .*>.*</a></font>", "");
                    source = source.Replace("\n", "").Replace("<br>", "\n");
                    IEnumerable<string> temp = source.Split('\n').Select(x => x.Trim());
                    string date = temp.First().Split(' ')[0];
                    char day = temp.First().Split(' ')[1][0];
                    string[] otherReservations = temp.Skip(2).ToArray();
                    if (data.Translate(day)[typeof(bool)])
                    {
                        string conflictedBy = "";
                        string otherReservationsOutput = "";
                        if (IsTimeConflictedWithOthersOn(day, date, otherReservations, ref conflictedBy, ref otherReservationsOutput))
                        {
                            if (!unavailableDatesAndRoom.ContainsKey(date) && !availableDatesAndRoom.ContainsKey(date))
                            {
                                unavailableDatesAndRoom.Add(date, i);
                            }
                        }
                        else
                        {
                            if (!availableDatesAndRoom.ContainsKey(date))
                            {
                                availableDatesAndRoom.Add(date, i);
                            }
                            if (unavailableDatesAndRoom.ContainsKey(date))
                            {
                                unavailableDatesAndRoom.Remove(date);
                            }
                        }
                    }
                }

                if (unavailableDatesAndRoom.Count > 0)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            unavailableDatesAndRoom.OrderBy(x => x.Key);
            availableDatesAndRoom.OrderBy(x => x.Key);

            return availableDatesAndRoom;
        }
        IEnumerable<AngleSharp.Dom.IElement> FetchAllOtherReservationsAt(string target, IHtmlDocument docText)
        {
            // ID cannot start with number. Who the fuck writes the website html codes!?!?!?
            return docText.QuerySelectorAll("tr.tmMinu td[id='1']")
                    .Where(reserves => reserves.QuerySelector("label#sc_xxx_00x_00x").TextContent == target)
                    .Select(reserves => reserves.QuerySelector("div.portalPopup.portalPopupNon"));
        }

        bool IsTimeConflictedWithOthersOn(char day, string date, string[] otherReservations, ref string conflictedBy, ref string otherReservationsOutput)
        {
            bool isTimeConflicted = false;

            for (int i = 0; i < otherReservations.Length; i++)
            {
                string time = otherReservations[i].Substring(0, 11);
                DateTime timeFrom = new DateTime(1, 1, 1, Convert.ToInt32(time.Substring(0, 2)), Convert.ToInt32(time.Substring(3, 2)), 0);
                DateTime timeTo = new DateTime(1, 1, 1, Convert.ToInt32(time.Substring(6, 2)), Convert.ToInt32(time.Substring(9, 2)), 0);
                DateTime timeSetFrom = new DateTime(1, 1, 1, Convert.ToInt32(data.fromTime.Substring(0, 2)), Convert.ToInt32(data.fromTime.Substring(3, 2)), 0);
                DateTime timeSetTo = new DateTime(1, 1, 1, Convert.ToInt32(data.toTime.Substring(0, 2)), Convert.ToInt32(data.toTime.Substring(3, 2)), 0);

                if (timeTo > timeSetFrom && timeSetTo > timeFrom)
                {
                    conflictedBy += timeFrom.ToShortTimeString() + "-" + timeTo.ToShortTimeString() + "  "
                        + otherReservations[i].Substring(14, otherReservations[i].Length - 14).Trim() + "\n";
                    isTimeConflicted = true;
                }

                otherReservationsOutput += timeFrom.ToShortTimeString() + "-" + timeTo.ToShortTimeString() + "  "
                        + otherReservations[i].Substring(14, otherReservations[i].Length - 14).Trim() + "\n";
            }

            otherReservationsOutput = otherReservations.Length > 0 ? otherReservationsOutput.Remove(otherReservationsOutput.Length - 1, 1) : "";
            conflictedBy = isTimeConflicted ? conflictedBy.Remove(conflictedBy.Length - 1, 1) : "";
            return isTimeConflicted;
        }

        bool IsPageURL(string url) => WebBrowser.Url == new Uri(url);

        IEnumerable<Error> FetchErrors(string source)
        {
            IHtmlDocument document = parser.Parse(source);
            var errorsTable = document.QuerySelectorAll("#trImpossibleDate > tbody > tr");
            if (errorsTable.Length > 0)
            {
                var errorList = errorsTable.Select(errors =>
                {
                    var items = errors.GetElementsByTagName("td");
                    string date = items[0].TextContent;
                    string time = items[1].TextContent;
                    string reason = items[2].TextContent;

                    return new Error { Date = date, Time = time, Reason = reason };
                });
                return errorList;
            }
            else
            {
                return null;
            }
        }

        void Logout(HtmlDocument doc, string source)
        {
            AngleSharp.Dom.Html.IHtmlDocument document = parser.Parse(source);
            var formId = document.QuerySelector("form").Id;
            doc.InvokeScript("doForward", new Object[] { formId, (Object)"/portal/" });
        }

        private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e) => StatusLabel.Text = "Navigating";

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e) => StatusLabel.Text = "Navigated";
    }

    public class _Day
    {
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }

    public class ReservationData
    {
        public string id { get; set; }
        public string pass { get; set; }
        public _Day day { get; set; }
        public string fromTime { get; set; }
        public string toTime { get; set; }
        public string reason { get; set; }
        public int numberOfPeople { get; set; }
    }
}
