#define DEV //gitからの場合は削除

using CoreTweet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace twitter_rename
{
	
	partial class Setting
	{

		protected int  reuse;
		protected long id;
		protected Tokens tokens;
		protected OAuth.OAuthSession session;
		protected string screen_name;
		protected string ApiKey = "";
		protected string ApiSecret = "";
		protected string AccessToken, AccessTokenSecret;
		protected string footer;
		ContToken ContToken = new ContToken();

		public Setting()
		{
			//Reset();　リセット用
			Xmlcreate();
			XmlRead();
			id = Properties.Settings.Default.My_id;
			reuse = Properties.Settings.Default.Reuse;
			screen_name = Properties.Settings.Default.Screen_name;
			footer = Properties.Settings.Default.Set_footer;

			
			
		}
		
		public void Xmlcreate()
		{
			if (!System.IO.File.Exists("settings.xml"))
			{
				ContToken.ApiKey = "null";
				ContToken.ApiSecret = "null";

				XmlSerializer serializer = new XmlSerializer(typeof(ContToken));
				FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Create);
				serializer.Serialize(fs, ContToken);
				fs.Close();
			}
			#if DEV
				DEVXmlcreate();
			#endif	

		}
		public void XmlRead() {
			XmlSerializer serializer = new XmlSerializer(typeof(ContToken));
			FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + "settings.xml", FileMode.Open);
			ContToken = (ContToken)serializer.Deserialize(fs);
			ApiKey = ContToken.ApiKey;
			ApiSecret = ContToken.ApiSecret;
		}
		public void Get_token() {
			if (!string.IsNullOrEmpty(Properties.Settings.Default.Access_Token)
				&& !string.IsNullOrEmpty(Properties.Settings.Default.Access_Token_Secret))//アクセストークンがある場合は読み込む
			{
				tokens = Tokens.Create(
					ApiKey
					, ApiSecret
					, Properties.Settings.Default.Access_Token
					, Properties.Settings.Default.Access_Token_Secret);
			}
			else { //認証
				//session = OAuth.Authorize(ApiKey, ApiSecret); //PINコードを利用する
				session = OAuth.Authorize(ApiKey, ApiSecret,"http://127.0.0.1:2001/");
				Console.Write("please access = \n{0}",session.AuthorizeUri.ToString());
				
				ProcessStartInfo oauth = new ProcessStartInfo();
				oauth.FileName = session.AuthorizeUri.ToString();
				oauth.UseShellExecute = true;
				Process.Start(oauth);
				
				Console.Write("\nplease enter PIN\n");
				int pincode = int.Parse(Console.ReadLine());
				tokens = session.GetTokens(pincode.ToString());
				AccessToken = tokens.AccessToken;
				AccessTokenSecret = tokens.AccessTokenSecret;
				Properties.Settings.Default.Access_Token = AccessToken;
				Properties.Settings.Default.Access_Token_Secret = AccessTokenSecret;

				tokens = Tokens.Create(
					ApiKey
					, ApiSecret
					, Properties.Settings.Default.Access_Token
					, Properties.Settings.Default.Access_Token_Secret);

			}
		}
		public long Id {

			get{
				return id;
			}
			set
			{
				Properties.Settings.Default.My_id = value;
			}
		}
		public string Screen_name
		{
			get
			{
				return screen_name;
			}
			set
			{
				Properties.Settings.Default.Screen_name = value;
				return;
			}
		}
		public int Usec
		{
			get
			{
				return reuse;
			}
			set
			{
				Properties.Settings.Default.Reuse = value;
				return;
			}
		}
		public string Footer
		{
			get
			{
				return footer;
			}
			set
			{
				Properties.Settings.Default.Set_footer = value;
				return;
			}
		}
		public Tokens Token(){
			return tokens;
		}
		public long Checkid() 
		{
			 if (reuse == 0 || id == 0  )
			{
				//IDが存在するか実行
				try{
					var userResponse = tokens.Account.VerifyCredentials();
					id = (long)(userResponse.Id);
					screen_name = userResponse.ScreenName;
					this.Id = id;
					this.Screen_name = screen_name;
				}catch{
				
				}
			}
			else if (reuse == 5) 
			{
				reuse = -1;
			}
			reuse++;
			this.Usec=reuse;
			return this.Id;
		}
		public string Getname(long id){
			string name;
			var parm = new Dictionary<string, object>();
			parm["id"] = id;
			parm["screen_name"] = screen_name;
			User showedUser = tokens.Users.Show(parm);
			name = showedUser.Name;
			return name;
		}
		public void Setname(string name) {
			var parm = new Dictionary<string, object>();
			parm["name"] = name;
			User showedUser = tokens.Account.UpdateProfile(parm);
		
		}
        public void Post(string tweet)
        {
            tokens.Statuses.Update(status => tweet);
        }
		public void Save()
		{
			Properties.Settings.Default.Save();
			Console.WriteLine("save your setting");

		}
		public void Reset() {
			Console.WriteLine("your setting is reset\n");
			Properties.Settings.Default.Reset();
		}
		public void View() {
			Boolean emp_token;
			emp_token = (!string.IsNullOrEmpty(Properties.Settings.Default.Access_Token)
				&& !string.IsNullOrEmpty(Properties.Settings.Default.Access_Token_Secret));
			Console.WriteLine("Save  .... " );
			Console.WriteLine("Reuse time(max 4) = " + this.Usec );
			Console.WriteLine("My ID  = " + this.Id);
			Console.WriteLine("Screen_name = " + this.Screen_name);
			Console.WriteLine("Footer = " + this.Footer);
			Console.WriteLine("Token entity = " + emp_token);
		
		}
	}
	public class ContToken
	{
		public ContToken()
		{
			
		}
		public string ApiKey { get; set; }
		public string ApiSecret { get; set; }

	}
	class Program
	{
		public Program()
		{
		}
		static void Main(string[] args)
		{

			string oldname, newname, temptext,footer;

			Setting userset = new Setting();
			if (!(args.Length == 0))//特殊処理
			{
				Console.WriteLine("コマンドライン引数\"{0}\"を受け取りました",args[0]);

				if (args[0].Equals("r", StringComparison.OrdinalIgnoreCase) || args[0].Equals("Reset", StringComparison.OrdinalIgnoreCase))
				{//設定をリセットする引数
					userset.Reset();
				}
				else if ((args[0].Equals("v", StringComparison.OrdinalIgnoreCase) || args[0].Equals("View", StringComparison.OrdinalIgnoreCase)))
				{//設定を閲覧する引数
					userset.View();
				}
				else if ((args[0].Equals("e", StringComparison.OrdinalIgnoreCase) || args[0].Equals("Edit", StringComparison.OrdinalIgnoreCase)))
				{//フッターを設定する引数
					Console.WriteLine("Edit your footer->\n");
					string text = Console.ReadLine();
					userset.Footer = text;
					//userset.Get_token();
				}
			}
			else
			{
				userset.Get_token();
				long id = userset.Checkid();
				oldname = userset.Getname(id);
				footer = userset.Footer;
				newname = Changename(oldname, footer);
				userset.Setname(newname);

				temptext = "read your name " + oldname + " changed " + newname + ".   " + "Refle:" + userset.Usec;
				Console.WriteLine(temptext);
			} 

            //userset.Post(temptext);
			userset.Save();
			Console.Read();
		}

		static string Changename(string name)
		{
			string now;

			now = Changename(name, "LB");

			return now;
		}
		static string Changename(string name, string fotter)
		{//書き換え判定 未完成
			string now;
			//正規表現パターンを指定してRegexオブジェクトを作成
			System.Text.RegularExpressions.Regex r =
				new System.Text.RegularExpressions.Regex(
					@"(@.*)");

			//文字が含まれているか調べる
			if (r.IsMatch(name))
			{
				//Console.WriteLine("include text {0}",name);
				now = r.Replace(name, "");
			}
			else
			{
				now = name + "@" + fotter;
			}
			return now;
		}
	}
	
}
