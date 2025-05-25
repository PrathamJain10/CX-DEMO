namespace Book_Store
{
	
//
//    Filename: Login.cs
//    Generated with CodeCharge 2.0.5
//    ASP.NET C#.ccp build 03/07/2002
//
//-------------------------------
//

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.OleDb;
    using System.Drawing;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;
    using System.Text.RegularExpressions;

    /// <summary>
    ///    Summary description for Login.
    /// </summary>

	public partial class Login : System.Web.UI.Page
	
    { 



//Login CustomIncludes begin
		protected CCUtility Utility;
		
		//Login form Login variables and controls declarations
		protected System.Web.UI.HtmlControls.HtmlInputHidden Login_querystring;
		protected System.Web.UI.HtmlControls.HtmlInputHidden Login_ret_page; 
	
		// For each Login form hiddens for PK's,List of Values and Actions
		protected string Login_FormAction="ShoppingCart.aspx?";
		

	
	public Login()
	{
	this.Init += new System.EventHandler(Page_Init);
	}
	
// Login CustomIncludes end
//-------------------------------


	public void ValidateNumeric(object source, ServerValidateEventArgs args) {
			try{
				Decimal temp=Decimal.Parse(args.Value);
				args.IsValid=true;
		        }catch{
				args.IsValid=false;	}
		}
//===============================
// Login Show begin
        protected void Page_Load(object sender, EventArgs e)
        {	
		Utility=new CCUtility(this);
		//===============================
// Login Open Event begin
// Login Open Event end
		//===============================
		
		//===============================
// Login OpenAnyPage Event begin
// Login OpenAnyPage Event end
		//===============================
		//
		//===============================
		// Login PageSecurity begin
		// Login PageSecurity end
		//===============================
		if (Session["UserID"] != null && Int16.Parse(Session["UserID"].ToString()) > 0)
		Login_logged = true;

		if (!IsPostBack){
			Page_Show(sender, e);
		}
	}

	protected void Page_Unload(object sender, EventArgs e)
	{
		//
		// CODEGEN: This call is required by the ASP+ Windows Form Designer.
		//
		if(Utility!=null) Utility.DBClose();
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		//
		// CODEGEN: This call is required by the ASP+ Windows Form Designer.
		//
		InitializeComponent();
		Login_login.Click += new System.EventHandler (this.Login_login_Click);
		
		
	}

        /// <summary>
        ///    Required method for Designer support - do not modify
        ///    the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }


        protected void Page_Show(object sender, EventArgs e)
        {
		Login_Show();
		
        }



// Login Show end

/*===============================
  Display Login Form
  -------------------------------*/
protected bool Login_logged = false;

// FIX 1: Secure method to get member login with parameterized query
private string GetMemberLogin(int memberId)
{
    string sSQL = "SELECT member_login FROM members WHERE member_id = ?";
    
    OleDbCommand command = new OleDbCommand(sSQL, Utility.Connection);
    command.Parameters.AddWithValue("@member_id", memberId);
    
    OleDbDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow);
    string result = "";
    
    if (reader.Read()) {
        result = reader[0].ToString();
        if (result == null) result = "";
    }
    
    reader.Close();
    return HttpUtility.HtmlEncode(result); // Prevent XSS
}

void Login_Show() {
	
	// Login Show begin
	
// Login Open Event begin
// Login Open Event end

// Login BeforeShow Event begin
// Login BeforeShow Event end

	if (Login_logged) {
		// User logged in		
		Login_login.Text = "Logout";
		Login_trpassword.Visible = false;
		Login_trname.Visible = false;
		Login_labelname.Visible = true;
		
		// FIX 1: Use secure parameterized query instead of Dlookup
		if (Session["UserID"] != null) {
		    int userId = Convert.ToInt32(Session["UserID"]);
		    Login_labelname.Text = GetMemberLogin(userId) + "&nbsp;&nbsp;&nbsp;";
		}
	} else {
		// User is not logged in
		Login_login.Text = "Login";
		Login_trpassword.Visible = true;
		Login_trname.Visible = true;
		Login_labelname.Visible = false;
	}
	
// Login Close Event begin
// Login Close Event end

	// Login Show end
	
}

// FIX 2: Secure authentication method with parameterized queries
private bool AuthenticateUser(string username, string password, out int userId, out int userLevel)
{
    userId = 0;
    userLevel = 0;
    
    // Input validation
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return false;
        
    // Validate username format (prevent injection attempts)
    if (!IsValidUsername(username))
        return false;
        
    // Hash the password (in production, use proper password hashing like bcrypt)
    // For now, we'll use the existing system but with parameterized queries
    
    string sSQL = "SELECT member_id, member_level FROM members WHERE member_login = ? AND member_password = ?";
    
    OleDbCommand command = new OleDbCommand(sSQL, Utility.Connection);
    command.Parameters.AddWithValue("@username", username);
    command.Parameters.AddWithValue("@password", CCUtility.Quote(password));
    
    OleDbDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow);
    bool authenticated = false;
    
    if (reader.Read()) {
        userId = reader.GetInt32("member_id");
        userLevel = reader.GetInt32("member_level");
        authenticated = true;
    }
    
    reader.Close();
    return authenticated;
}

// FIX 2: Username validation to prevent injection
private bool IsValidUsername(string username)
{
    // Allow only alphanumeric characters, underscores, and common email characters
    // Adjust pattern based on your username requirements
    return Regex.IsMatch(username, @"^[a-zA-Z0-9@._-]+$") && username.Length <= 50;
}

// FIX 3: Input sanitization for login fields
private string SanitizeInput(string input)
{
    if (string.IsNullOrEmpty(input))
        return "";
        
    // Remove dangerous characters and limit length
    string sanitized = input.Trim();
    
    // Remove potential script injection attempts
    sanitized = Regex.Replace(sanitized, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);
    sanitized = sanitized.Replace("'", "''"); // Basic SQL escaping (though we use parameterized queries)
    sanitized = sanitized.Replace("--", ""); // Remove SQL comment markers
    sanitized = sanitized.Replace("/*", "").Replace("*/", ""); // Remove SQL block comments
    
    // Limit length to prevent buffer overflow attempts
    if (sanitized.Length > 100)
        sanitized = sanitized.Substring(0, 100);
        
    return sanitized;
}

void Login_login_Click(Object Src, EventArgs E) {
	if (Login_logged) {
		
		// Login Logout begin
		
// Login OnLogout Event begin
// Login OnLogout Event end
        Login_logged = false;
		Session["UserID"] = 0;
		Session["UserRights"] = 0;
		
		// FIX 4: Clear all session data on logout for security
		Session.Clear();
		Session.Abandon();
		
		Login_Show();
		// Login Logout end
		
	} else {
		
		// Login Login begin
		
		// FIX 2 & 3: Sanitize inputs and use secure authentication
		string username = SanitizeInput(Login_name.Text);
		string password = SanitizeInput(Login_password.Text);
		
		// Rate limiting check (basic implementation)
		if (IsLoginAttemptExceeded())
		{
		    Login_message.Text = "Too many login attempts. Please try again later.";
		    Login_message.Visible = true;
		    return;
		}
		
		int userId, userLevel;
		bool isAuthenticated = AuthenticateUser(username, password, out userId, out userLevel);
		
		if (isAuthenticated) {
			
// Login OnLogin Event begin
// Login OnLogin Event end
            Login_message.Visible = false;
			Session["UserID"] = userId;
			Login_logged = true;
			Session["UserRights"] = userLevel;
			
			// FIX 5: Secure session management
			Session.Timeout = 30; // 30 minute timeout
			
			// FIX 6: Validate redirect parameters to prevent open redirect
			string sQueryString = Utility.GetParam("querystring");
			string sPage = Utility.GetParam("ret_page");
			
			if (!string.IsNullOrEmpty(sPage) && IsValidRedirectUrl(sPage) && 
			    !sPage.Equals(Request.ServerVariables["SCRIPT_NAME"])) {
				Response.Redirect(sPage + "?" + HttpUtility.UrlEncode(sQueryString));
			} else {
				Response.Redirect(Login_FormAction);
			}
			
			// Reset failed login attempts on successful login
			ResetLoginAttempts();
			
		} else {
		    // FIX 7: Generic error message to prevent user enumeration
		    Login_message.Text = "Invalid username or password.";
			Login_message.Visible = true;
			
			// Track failed login attempts
			IncrementLoginAttempts();
		}
		// Login Login end
	
	}
}

// FIX 5: Rate limiting for login attempts
private bool IsLoginAttemptExceeded()
{
    const int maxAttempts = 5;
    const int lockoutMinutes = 15;
    
    if (Session["LoginAttempts"] == null)
        Session["LoginAttempts"] = 0;
        
    if (Session["LastFailedLogin"] == null)
        return false;
        
    DateTime lastFailed = (DateTime)Session["LastFailedLogin"];
    int attempts = (int)Session["LoginAttempts"];
    
    // Reset attempts after lockout period
    if (DateTime.Now.Subtract(lastFailed).TotalMinutes > lockoutMinutes)
    {
        Session["LoginAttempts"] = 0;
        return false;
    }
    
    return attempts >= maxAttempts;
}

private void IncrementLoginAttempts()
{
    if (Session["LoginAttempts"] == null)
        Session["LoginAttempts"] = 0;
        
    Session["LoginAttempts"] = (int)Session["LoginAttempts"] + 1;
    Session["LastFailedLogin"] = DateTime.Now;
}

private void ResetLoginAttempts()
{
    Session["LoginAttempts"] = 0;
    Session.Remove("LastFailedLogin");
}

// FIX 6: Validate redirect URLs to prevent open redirect attacks
private bool IsValidRedirectUrl(string url)
{
    if (string.IsNullOrEmpty(url))
        return false;
        
    // Only allow relative URLs or URLs from the same domain
    if (url.StartsWith("http://") || url.StartsWith("https://"))
    {
        try
        {
            Uri uri = new Uri(url);
            Uri currentUri = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));
            return uri.Host.Equals(currentUri.Host, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    
    // Allow relative URLs that don't start with //
    return url.StartsWith("/") && !url.StartsWith("//");
}

// End of Login form 
    }
}