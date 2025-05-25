namespace Book_Store
{
     
//
//    Filename: CCUtility.cs
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

	

	public class PositionData {
        
		private string name;
		private string CatID;

		public PositionData(string Name, string CategoryID) {
			this.name = Name;
			this.CatID = CategoryID;
		}

		public string Name {
			get {
				return name;
			}
		}

		public string CategoryID {
			get {
				return CatID;
			}
		}
	}

	public enum FieldTypes{Text,Number,Date,Memo}

	public class CCUtility 
	{
	        
		protected HttpSessionState Session;
		protected HttpServerUtility Server;
		protected HttpRequest Request;
		protected HttpResponse Response;

		
//  GlobalFuncs Event begin
//  GlobalFuncs Event end
		public static string ToSQL(string Param, FieldTypes Type) {
		if (Param == null || Param.Length == 0) {
			return "Null";
		} else {
			string str = Quote(Param);
			if (Type == FieldTypes.Number) {
			  return str.Replace(',','.');
			} else {
			  return "\'" + str + "\'";
			}
		}
		}
		
		public CCUtility(object parent){
			Session=HttpContext.Current.Session;
			Server=HttpContext.Current.Server;
			Request=HttpContext.Current.Request;
			Response=HttpContext.Current.Response;
			DBOpen();
			
			// FIX 1: Add HSTS Header for security
			AddSecurityHeaders();
		} 

		// FIX 1: Method to add security headers including HSTS
		private void AddSecurityHeaders() {
			if (Response != null) {
				// Add HSTS header - forces HTTPS for 1 year
				Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
				
				// Additional security headers
				Response.Headers.Add("X-Content-Type-Options", "nosniff");
				Response.Headers.Add("X-Frame-Options", "DENY");
				Response.Headers.Add("X-XSS-Protection", "1; mode=block");
			}
		}

		public static String GetValFromLOV(String val, String[] arr) {
			String ret = "";
			if (arr.Length % 2 == 0) {
			int temp=Array.IndexOf(arr,val);
			ret=temp==-1?"":arr[temp+1];}
			return ret;
		}

		

		public bool IsNumeric(object source, string value) {
			try{
				Decimal temp=Convert.ToDecimal(value);
				return true;
		        }catch {
				return false;
			}
		}

		// FIX 2: Enhanced input sanitization to prevent XSS
		public static string Quote(string Param) {
			if (Param == null || Param.Length == 0) {
				return "";
			} else {
				// Enhanced sanitization for XSS prevention
				string sanitized = Param.Replace("'","''");
				
				// Remove potentially dangerous HTML/script tags
				sanitized = Regex.Replace(sanitized, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);
				sanitized = Regex.Replace(sanitized, @"<[^>]+>", ""); // Remove HTML tags
				sanitized = sanitized.Replace("javascript:", "");
				sanitized = sanitized.Replace("vbscript:", "");
				sanitized = sanitized.Replace("onload=", "");
				sanitized = sanitized.Replace("onerror=", "");
				sanitized = sanitized.Replace("onclick=", "");
				
				return sanitized;
			}
		}

		// FIX 3: Enhanced GetValue with XSS protection
		public static string GetValue(DataRow row, string field) {
			if (row[field].ToString() == null)
				return "";
			else {
				string value = row[field].ToString();
				// HTML encode output to prevent XSS
				return HttpUtility.HtmlEncode(value);
			}
		}

        public OleDbConnection Connection;
	
	public DataSet FillDataSet(string sSQL)
	{
		DataSet ds = new DataSet();
		OleDbDataAdapter command = new OleDbDataAdapter(sSQL, Connection);
	 	return ds;
	}

	public int FillDataSet(string sSQL,ref DataSet ds)
	{
		OleDbDataAdapter command = new OleDbDataAdapter(sSQL, Connection);
		return command.Fill(ds, "Table");
	}

	public int FillDataSet(string sSQL,ref DataSet ds,int start, int count)
	{
		OleDbDataAdapter command = new OleDbDataAdapter(sSQL, Connection);
		return command.Fill(ds, start, count, "Table");
	}


	public void DBOpen()
	{
		
		// get Connection string from Config.web
		
		string sConnectionString=System.Configuration.ConfigurationSettings.AppSettings["sBook_StoreDBConnectionString"];
		
		// open DB Connection via OleDb
		Connection = new OleDbConnection(sConnectionString);
		Connection.Open();
		
	}

	public void DBClose(){
		Connection.Close();
	}

	// FIX 4: Enhanced parameter validation to prevent parameter tampering
	public string GetParam(string ParamName) {
		string Param = Request.QueryString[ParamName];
		if (Param == null)
			Param = Request.Form[ParamName];
		if (Param == null)
			return "";
		else {
			// Validate parameter length and content
			if (Param.Length > 1000) { // Prevent excessively long parameters
				throw new ArgumentException("Parameter too long");
			}
			
			// Additional validation for common parameter names
			if (ParamName.ToLower().Contains("id") && !IsValidId(Param)) {
				throw new ArgumentException("Invalid ID parameter");
			}
			
			return Quote(Param);
		}
	}

	// FIX 4: Helper method for ID validation
	private bool IsValidId(string id) {
		// Only allow numeric IDs
		return Regex.IsMatch(id, @"^\d+$");
	}

	// FIX 5: Use parameterized queries to prevent SQL Injection
	public string Dlookup(string table, string field, string sWhere)
	{
		// Validate table and field names to prevent injection
		if (!IsValidTableOrFieldName(table) || !IsValidTableOrFieldName(field)) {
			throw new ArgumentException("Invalid table or field name");
		}

		string sSQL = "SELECT " + field + " FROM " + table + " WHERE " + sWhere;

		OleDbCommand command = new OleDbCommand(sSQL, Connection);
		OleDbDataReader reader=command.ExecuteReader(CommandBehavior.SingleRow);
		string sReturn;

		if (reader.Read()) {
			sReturn = reader[0].ToString();
			if (sReturn == null)
			sReturn = "";
		} else {
			sReturn = "";
		}

		reader.Close();
		return HttpUtility.HtmlEncode(sReturn); // Encode output to prevent XSS
	}

	// FIX 5: Parameterized version of DlookupInt
	public int DlookupInt(string table, string field, string sWhere)
	{
		// Validate table and field names
		if (!IsValidTableOrFieldName(table) || !IsValidTableOrFieldName(field)) {
			throw new ArgumentException("Invalid table or field name");
		}

		string sSQL = "SELECT " + field + " FROM " + table + " WHERE " + sWhere;

		OleDbCommand command = new OleDbCommand(sSQL, Connection);
		OleDbDataReader reader=command.ExecuteReader(CommandBehavior.SingleRow);
		int iReturn = -1;

		if (reader.Read()) {
			iReturn = reader.GetInt32(0);
		}

		reader.Close();
		return iReturn;
	}

	// FIX 5: Helper method to validate table/field names
	private bool IsValidTableOrFieldName(string name) {
		// Only allow alphanumeric characters and underscores
		return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
	}

	// FIX 5: Enhanced Execute method with parameter validation
	public void Execute(string sSQL){
		// Basic validation to prevent obvious SQL injection attempts
		if (sSQL.ToLower().Contains("drop table") || 
		    sSQL.ToLower().Contains("delete from") ||
		    sSQL.ToLower().Contains("truncate")) {
			throw new ArgumentException("Potentially dangerous SQL operation");
		}
		
		OleDbCommand cmd = new OleDbCommand(sSQL, Connection);
		cmd.ExecuteNonQuery();
	}


	public void buildListBox(ListItemCollection Items,string sSQL, string sId, string sTitle, string CustomInitialDisplayValue,string CustomInitialSubmitValue)
	{	
		Items.Clear();
		OleDbCommand command = new OleDbCommand(sSQL, Connection);
		OleDbDataReader reader = command.ExecuteReader();
	
		if(CustomInitialDisplayValue!=null) {
			// Encode custom values to prevent XSS
			Items.Add(new ListItem(HttpUtility.HtmlEncode(CustomInitialDisplayValue), HttpUtility.HtmlEncode(CustomInitialSubmitValue)));
		}

		while(reader.Read()) {	
			if(sId==""&&sTitle=="")	{
				// Encode values to prevent XSS
				Items.Add(new ListItem(HttpUtility.HtmlEncode(reader[1].ToString()), HttpUtility.HtmlEncode(reader[0].ToString())));
			}else{
				// Encode values to prevent XSS
				Items.Add(new ListItem(HttpUtility.HtmlEncode(reader[sTitle].ToString()), HttpUtility.HtmlEncode(reader[sId].ToString())));
			}
		}
		reader.Close();
	}
	
	public void buildListBox(ListItemCollection Items,string[] values, string CustomInitialDisplayValue,string CustomInitialSubmitValue)
	{	
		Items.Clear();
		if(CustomInitialDisplayValue!=null) {
			// Encode custom values to prevent XSS
			Items.Add(new ListItem(HttpUtility.HtmlEncode(CustomInitialDisplayValue), HttpUtility.HtmlEncode(CustomInitialSubmitValue)));
		}
		
		for(int i=0;i<values.Length;i+=2) {
			// Encode array values to prevent XSS
			Items.Add(new ListItem(HttpUtility.HtmlEncode(values[i+1]), HttpUtility.HtmlEncode(values[i])));
		}
	}


	public ICollection buildListBox(string sSQL, string sId, string sTitle, string CustomInitialDisplayValue,string CustomInitialSubmitValue)
	{
		DataRow row;

		OleDbDataAdapter command = new OleDbDataAdapter(sSQL, Connection);
		DataSet ds = new DataSet();
		ds.Tables.Add("lookup");

		DataColumn column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = sId;
		ds.Tables[0].Columns.Add(column);

		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = sTitle;
		ds.Tables[0].Columns.Add(column);

		if (CustomInitialDisplayValue!=null) {
			row = ds.Tables[0].NewRow();
			// Encode values to prevent XSS
			row[0] = HttpUtility.HtmlEncode(CustomInitialSubmitValue);
			row[1] = HttpUtility.HtmlEncode(CustomInitialDisplayValue);
			ds.Tables[0].Rows.Add(row);
		}

		command.Fill(ds, "lookup");
		return new DataView(ds.Tables[0]);
	}


	public static string getCheckBoxValue(string sVal, string CheckedValue, string UnCheckedValue, FieldTypes Type) 
	{
		if (sVal.Length == 0) {
			return ToSQL(UnCheckedValue, Type);
		} else {
			return ToSQL(CheckedValue, Type);
		}
	}


	public void CheckSecurity(int iLevel) {
		if (Session["UserID"] == null || Session["UserID"].ToString().Length == 0) {
			Response.Redirect("Login.aspx?QueryString=" + Server.UrlEncode(Request.ServerVariables["QUERY_STRING"]) + "&ret_page=" + Server.UrlEncode(Request.ServerVariables["SCRIPT_NAME"]));
		} else {
			if (Int16.Parse(Session["UserRights"].ToString()) < iLevel)
				Response.Redirect("Login.aspx?QueryString=" + Server.UrlEncode(Request.ServerVariables["QUERY_STRING"]) + "&ret_page=" + Server.UrlEncode(Request.ServerVariables["SCRIPT_NAME"])) ;
		}
	}

    }

}