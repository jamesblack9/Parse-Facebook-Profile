using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Facebook;

namespace ParserFBObject
{
    public partial class Form1 : Form
    {
        private const string AppID = "669169496459417";
        private const string ExtendedPermissions = "basic_info, export_stream, friends_checkins, friends_groups, friends_likes, friends_status, manage_pages, public_profile, read_friendlists, read_stream, user_checkins, user_friends, user_groups, user_status";
        private string _accessToken;
        private FacebookClient fb;

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
                    this.Text = "Friends of " + name + " : ";
                    
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

        }

        public string retrieveGroupID(string targetURL)
        {
            // doan nay viet tam thoi
            string groupConstraintURL = @"https://www.facebook.com/groups/";
            string groupName = targetURL.Substring(targetURL.IndexOf(groupConstraintURL), targetURL.Length);
            string groupID = groupName;
            for (int i = 0; i < groupName.Length; i++)
            {
                if (!Char.IsDigit(groupName[i]))
                {
                    groupID = "";
                }
            }
            if (groupID.Length == 0)
            {

            }
            return groupID;
        }
    }
}
