using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Facebook;
using HtmlAgilityPack;
using System.Threading;
using mshtml;
using System.Dynamic;

namespace ParserFBObject
{
    public partial class Form1 : Form
    {
        private const string AppID = "669169496459417";
        private WebBrowser webBrowser1;
        private const string ExtendedPermissions = "basic_info, export_stream, friends_checkins, friends_groups, friends_likes, friends_status, manage_pages, public_profile, read_friendlists, read_stream, user_checkins, user_friends, user_groups, user_status";
        private string _accessToken;
        private FacebookClient fb;
        private string groupID;
        delegate void StatusCallback();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoginForm login = new LoginForm(AppID, ExtendedPermissions);
            login.ShowDialog();
            DisplayAppropriateMessage(login.FacebookOAuthResult);
        }

        private void DisplayAppropriateMessage(FacebookOAuthResult facebookOAuthResult)
        {
            if (facebookOAuthResult != null)
            {
                if (facebookOAuthResult.IsSuccess)
                {
                    _accessToken = facebookOAuthResult.AccessToken;
                    fb = new FacebookClient(facebookOAuthResult.AccessToken);

                    dynamic result = fb.Get("/me");
                    var name = result.name;

                    // for .net 3.5
                    //var result = (IDictionary<string, object>)fb.Get("/me");
                    //var name = (string)result["name"];
                    this.Text = "Welcome " + name + "";
                    
                }
                else
                {
                    MessageBox.Show(facebookOAuthResult.ErrorDescription);
                }
            }
            else
            {
                this.Close();
            }
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            if (txtUrl.Text.Trim().Length == 0)
            {
                // do nothing
            }
            // can validate url format = regx - later

            // can su dung command pattern de parse
            // nhieu loai object nhu page, wall, group
            // o day mac dinh la parse group tu targetURL
            string TargetURL = txtUrl.Text.Trim();

            // van de dau tien , lay ID cua group
            retrieveGroupID(TargetURL);
        }

        public void retrieveGroupID(string targetURL)
        {
            // doan nay viet tam thoi

            // xu ly dau / du o vi tri cuoi url
            if (targetURL[targetURL.Length - 1].Equals('/'))
            {
                targetURL = targetURL.Remove(targetURL.Length - 1);
            }

            string groupConstraintURL = @"https://www.facebook.com/groups/";
            string groupName = targetURL.Substring(groupConstraintURL.Length);
            groupID = groupName;
            // group cua FB co the co alias

            for (int i = 0; i < groupName.Length; i++)
            {
                if (!Char.IsDigit(groupName[i]))
                {
                    groupID = "";
                }
            }
            if (groupID.Length == 0)
            {
                HtmlWeb webPage = new HtmlWeb();
                webBrowser1 = new WebBrowser();
                webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
                webBrowser1.Navigate(targetURL);
            }
            else
            {
                txtUrl.Text = groupID;
                DisplayMembers();
            }

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // by pass async call
            
            
            if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                return;
            }
            // by pass iframe with js
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
            {
                return;
            }
            var th = new Thread(() =>
            {
                Thread.Sleep(20000);
                callback();
            });
            // separate thread with main browser thread
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            
        }

        public void callback()
        {
            if (txtUrl.InvokeRequired)
            {
                this.Invoke(new StatusCallback(this.callback));
            }
            else
            {
                // Use DomDocument, not Document to get the up to date data after JS call back on page
                HtmlAgilityPack.HtmlDocument ResultDoc = new HtmlAgilityPack.HtmlDocument();
                IHTMLDocument3 doc = webBrowser1.Document.DomDocument as IHTMLDocument3;
                ResultDoc.LoadHtml(doc.documentElement.innerHTML);

                // get processing result
                HtmlAgilityPack.HtmlNode resultNode = ResultDoc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='targetid']");

                // if data in browser not yet updated , make call back after 5 seconds
                if(resultNode == null)
                {
                    
                    var th = new Thread(() =>
                    {
                        Thread.Sleep(5000);
                        callback();
                    });
                    th.SetApartmentState(ApartmentState.STA);
                    th.Start();
                }
                groupID = resultNode.Attributes["value"].Value.ToString();
                txtUrl.Text = groupID;
                DisplayMembers();
            }
        }
        private void DisplayMembers()
        {
            dynamic MemberListData = fb.Get("/" + groupID+ "/members");
            var friendList = (from f in (IEnumerable<dynamic>)MemberListData.data
                              select new { f.id, f.name }).ToList();
            gvResult.DataSource = friendList;
        }
    }
}
