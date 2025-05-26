namespace Book_Store
{

//    Filename: OrdersGrid.cs
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
    ///    Summary description for OrdersGrid.
    /// </summary>
    public partial class OrdersGrid : System.Web.UI.Page
    { 
        // SECURITY FIX: Add input validation and sanitization methods
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove potentially dangerous characters
            return Regex.Replace(input, @"[<>""'%;()&+]", "");
        }

        private bool IsValidNumericId(string input)
        {
            return !string.IsNullOrEmpty(input) && 
                   Regex.IsMatch(input, @"^\d+$") && 
                   int.TryParse(input, out int result) && 
                   result > 0 && result <= int.MaxValue;
        }

        // SECURITY FIX: Add HSTS Header in Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            // SECURITY FIX: Add HSTS Header
            Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // SECURITY FIX: Add additional security headers
            Response.AddHeader("X-Content-Type-Options", "nosniff");
            Response.AddHeader("X-Frame-Options", "DENY");
            Response.AddHeader("X-XSS-Protection", "1; mode=block");
            
            Utility = new CCUtility(this);
            
            // OrdersGrid PageSecurity begin
            Utility.CheckSecurity(2);
            // OrdersGrid PageSecurity end

            if (!IsPostBack)
            {
                Page_Show(sender, e);
            }
        }

        // SECURITY FIX: Secure search click handler with input validation
        void Search_search_Click(Object Src, EventArgs E) 
        {
            // SECURITY FIX: Validate and sanitize inputs
            string itemId = SanitizeInput(Search_item_id.SelectedItem?.Value ?? "");
            string memberId = SanitizeInput(Search_member_id.SelectedItem?.Value ?? "");
            
            // SECURITY FIX: Additional validation for numeric IDs
            if (!string.IsNullOrEmpty(itemId) && !IsValidNumericId(itemId))
            {
                itemId = "";
            }
            
            if (!string.IsNullOrEmpty(memberId) && !IsValidNumericId(memberId))
            {
                memberId = "";
            }
            
            // SECURITY FIX: Use proper URL encoding and validation
            string sURL = Search_FormAction + "item_id=" + Server.UrlEncode(itemId) + "&" 
                         + "member_id=" + Server.UrlEncode(memberId) + "&";
            
            Response.Redirect(sURL);
        }

        // SECURITY FIX: Secure data source creation with parameterized queries
        ICollection Orders_CreateDataSource() 
        {
            Orders_sSQL = "";
            Orders_sCountSQL = "";

            string sWhere = "", sOrder = "";
            bool HasParam = false;
            
            // Build ORDER BY statement with validation
            sOrder = " order by o.order_id Asc";
            if (Utility.GetParam("FormOrders_Sorting").Length > 0 && !IsPostBack)
            {
                // SECURITY FIX: Validate sort column to prevent SQL injection
                string sortColumn = SanitizeInput(Utility.GetParam("FormOrders_Sorting"));
                string[] allowedColumns = { "o.order_id", "o.item_id", "o.member_id", "o.quantity", "i.name", "m.member_login" };
                
                if (Array.Exists(allowedColumns, col => col.Equals(sortColumn, StringComparison.OrdinalIgnoreCase)))
                {
                    ViewState["SortColumn"] = sortColumn;
                    ViewState["SortDir"] = "ASC";
                }
            }
            
            if (ViewState["SortColumn"] != null) 
            {
                string sortDir = ViewState["SortDir"]?.ToString();
                if (sortDir == "ASC" || sortDir == "DESC")
                {
                    sOrder = " ORDER BY " + ViewState["SortColumn"].ToString() + " " + sortDir;
                }
            }

            // SECURITY FIX: Use parameterized queries instead of string concatenation
            System.Collections.Specialized.StringDictionary Params = new System.Collections.Specialized.StringDictionary();
            var parameters = new List<OleDbParameter>();

            // SECURITY FIX: Proper parameter validation and sanitization
            if (!Params.ContainsKey("item_id"))
            {
                string temp = SanitizeInput(Utility.GetParam("item_id"));
                if (IsValidNumericId(temp))
                {
                    Params.Add("item_id", temp);
                }
                else
                {
                    Params.Add("item_id", "");
                }
            }

            if (!Params.ContainsKey("member_id"))
            {
                string temp = SanitizeInput(Utility.GetParam("member_id"));
                if (IsValidNumericId(temp))
                {
                    Params.Add("member_id", temp);
                }
                else
                {
                    Params.Add("member_id", "");
                }
            }

            // SECURITY FIX: Build WHERE clause with parameterized queries
            if (Params["item_id"].Length > 0)
            {
                HasParam = true;
                sWhere += "o.[item_id] = ?";
                parameters.Add(new OleDbParameter("@item_id", OleDbType.Integer) { Value = int.Parse(Params["item_id"]) });
            }
            
            if (Params["member_id"].Length > 0)
            {
                if (sWhere.Length > 0) sWhere += " and ";
                HasParam = true;
                sWhere += "o.[member_id] = ?";
                parameters.Add(new OleDbParameter("@member_id", OleDbType.Integer) { Value = int.Parse(Params["member_id"]) });
            }

            if (HasParam)
                sWhere = " AND (" + sWhere + ")";

            // Build base SQL statement
            Orders_sSQL = "select [o].[item_id] as o_item_id, " +
                "[o].[member_id] as o_member_id, " +
                "[o].[order_id] as o_order_id, " +
                "[o].[quantity] as o_quantity, " +
                "[i].[item_id] as i_item_id, " +
                "[i].[name] as i_name, " +
                "[m].[member_id] as m_member_id, " +
                "[m].[member_login] as m_member_login " +
                " from [orders] o, [items] i, [members] m" +
                " where [i].[item_id]=o.[item_id] and [m].[member_id]=o.[member_id]  ";

            // Assemble full SQL statement
            Orders_sSQL = Orders_sSQL + sWhere + sOrder;
            
            if (Orders_sCountSQL.Length == 0)
            {
                int iTmpI = Orders_sSQL.ToLower().IndexOf("select ");
                int iTmpJ = Orders_sSQL.ToLower().LastIndexOf(" from ") - 1;
                Orders_sCountSQL = Orders_sSQL.Replace(Orders_sSQL.Substring(iTmpI + 7, iTmpJ - 6), " count(*) ");
                iTmpI = Orders_sCountSQL.ToLower().IndexOf(" order by");
                if (iTmpI > 1) Orders_sCountSQL = Orders_sCountSQL.Substring(0, iTmpI);
            }

            // SECURITY FIX: Use parameterized command
            OleDbDataAdapter command = new OleDbDataAdapter(Orders_sSQL, Utility.Connection);
            
            // Add parameters to the command
            foreach (var param in parameters)
            {
                command.SelectCommand.Parameters.Add(param);
            }

            DataSet ds = new DataSet();
            command.Fill(ds, (i_Orders_curpage - 1) * Orders_PAGENUM, Orders_PAGENUM, "Orders");
            
            // SECURITY FIX: Use parameterized command for count query
            OleDbCommand ccommand = new OleDbCommand(Orders_sCountSQL, Utility.Connection);
            foreach (var param in parameters)
            {
                ccommand.Parameters.Add(new OleDbParameter(param.ParameterName, param.Value));
            }
            
            int PageTemp = (int)ccommand.ExecuteScalar();
            Orders_Pager.MaxPage = (PageTemp % Orders_PAGENUM) > 0 ? (int)(PageTemp / Orders_PAGENUM) + 1 : (int)(PageTemp / Orders_PAGENUM);
            bool AllowScroller = Orders_Pager.MaxPage == 1 ? false : true;

            DataView Source = new DataView(ds.Tables[0]);

            if (ds.Tables[0].Rows.Count == 0)
            {
                Orders_no_records.Visible = true;
                AllowScroller = false;
            }
            else
            {
                Orders_no_records.Visible = false;
                AllowScroller = AllowScroller && true;
            }

            Orders_Pager.Visible = AllowScroller;
            return Source;
        }

        // SECURITY FIX: Secure insert click handler
        void Orders_insert_Click(Object Src, EventArgs E) 
        {
            // SECURITY FIX: Validate and sanitize parameters
            string itemId = SanitizeInput(Utility.GetParam("item_id"));
            string memberId = SanitizeInput(Utility.GetParam("member_id"));
            
            // Additional validation
            if (!string.IsNullOrEmpty(itemId) && !IsValidNumericId(itemId))
            {
                itemId = "";
            }
            
            if (!string.IsNullOrEmpty(memberId) && !IsValidNumericId(memberId))
            {
                memberId = "";
            }
            
            string sURL = Orders_FormAction + "item_id=" + Server.UrlEncode(itemId) + "&member_id=" + Server.UrlEncode(memberId) + "&";
            Response.Redirect(sURL);
        }

        // SECURITY FIX: Secure sort change handler
        protected void Orders_SortChange(Object Src, EventArgs E) 
        {
            // SECURITY FIX: Validate sort column to prevent injection
            string sortColumn = ((LinkButton)Src).CommandArgument;
            string[] allowedColumns = { "o.order_id", "o.item_id", "o.member_id", "o.quantity", "i.name", "m.member_login" };
            
            if (!Array.Exists(allowedColumns, col => col.Equals(sortColumn, StringComparison.OrdinalIgnoreCase)))
            {
                return; // Invalid sort column, ignore request
            }
            
            if (ViewState["SortColumn"] == null || ViewState["SortColumn"].ToString() != sortColumn)
            {
                ViewState["SortColumn"] = sortColumn;
                ViewState["SortDir"] = "ASC";
            }
            else
            {
                ViewState["SortDir"] = ViewState["SortDir"].ToString() == "ASC" ? "DESC" : "ASC";
            }
            Orders_Bind();
        }

        // Rest of the original methods remain the same but with added security measures
        protected CCUtility Utility;
        protected string Orders_sSQL;
        protected string Orders_sCountSQL;
        protected int Orders_CountPage;
        protected int i_Orders_curpage = 1;
        protected string Search_FormAction = "OrdersGrid.aspx?";
        protected string Orders_FormAction = "OrdersRecord.aspx?";
        const int Orders_PAGENUM = 20;

        public OrdersGrid()
        {
            this.Init += new System.EventHandler(Page_Init);
        }

        public void ValidateNumeric(object source, ServerValidateEventArgs args)
        {
            try
            {
                Decimal temp = Decimal.Parse(args.Value);
                args.IsValid = true;
            }
            catch
            {
                args.IsValid = false;
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (Utility != null) Utility.DBClose();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            InitializeComponent();
            Search_search_button.Click += new System.EventHandler(this.Search_search_Click);
            Orders_insert.Click += new System.EventHandler(this.Orders_insert_Click);
            Orders_Pager.NavigateCompleted += new NavigateCompletedHandler(this.Orders_pager_navigate_completed);
        }

        private void InitializeComponent()
        {
        }

        protected void Page_Show(object sender, EventArgs e)
        {
            Search_Show();
            Orders_Bind();
        }

        void Search_Show()
        {
            // SECURITY FIX: These buildListBox calls should also be reviewed for SQL injection
            // but keeping original for now as it's likely a utility method
            Utility.buildListBox(Search_item_id.Items, "select item_id,name from items order by 2", "item_id", "name", "All", "");
            Utility.buildListBox(Search_member_id.Items, "select member_id,member_login from members order by 2", "member_id", "member_login", "All", "");

            string s;
            s = SanitizeInput(Utility.GetParam("item_id"));
            try
            {
                Search_item_id.SelectedIndex = Search_item_id.Items.IndexOf(Search_item_id.Items.FindByValue(s));
            }
            catch { }

            s = SanitizeInput(Utility.GetParam("member_id"));
            try
            {
                Search_member_id.SelectedIndex = Search_member_id.Items.IndexOf(Search_member_id.Items.FindByValue(s));
            }
            catch { }
        }

        public void Orders_Repeater_ItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            // Orders Show Event
        }

        protected void Orders_pager_navigate_completed(Object Src, int CurrPage)
        {
            i_Orders_curpage = CurrPage;
            Orders_Bind();
        }

        void Orders_Bind()
        {
            Orders_Repeater.DataSource = Orders_CreateDataSource();
            Orders_Repeater.DataBind();
        }
    }
}namespace Book_Store
{
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
    ///    Summary description for OrdersGrid.
    /// </summary>
    public partial class OrdersGrid : System.Web.UI.Page
    { 
        // SECURITY FIX: Add input validation and sanitization methods
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove potentially dangerous characters
            return Regex.Replace(input, @"[<>""'%;()&+]", "");
        }

        private bool IsValidNumericId(string input)
        {
            return !string.IsNullOrEmpty(input) && 
                   Regex.IsMatch(input, @"^\d+$") && 
                   int.TryParse(input, out int result) && 
                   result > 0 && result <= int.MaxValue;
        }

        // SECURITY FIX: Add HSTS Header in Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            // SECURITY FIX: Add HSTS Header
            Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // SECURITY FIX: Add additional security headers
            Response.AddHeader("X-Content-Type-Options", "nosniff");
            Response.AddHeader("X-Frame-Options", "DENY");
            Response.AddHeader("X-XSS-Protection", "1; mode=block");
            
            Utility = new CCUtility(this);
            
            // OrdersGrid PageSecurity begin
            Utility.CheckSecurity(2);
            // OrdersGrid PageSecurity end

            if (!IsPostBack)
            {
                Page_Show(sender, e);
            }
        }

        // SECURITY FIX: Secure search click handler with input validation
        void Search_search_Click(Object Src, EventArgs E) 
        {
            // SECURITY FIX: Validate and sanitize inputs
            string itemId = SanitizeInput(Search_item_id.SelectedItem?.Value ?? "");
            string memberId = SanitizeInput(Search_member_id.SelectedItem?.Value ?? "");
            
            // SECURITY FIX: Additional validation for numeric IDs
            if (!string.IsNullOrEmpty(itemId) && !IsValidNumericId(itemId))
            {
                itemId = "";
            }
            
            if (!string.IsNullOrEmpty(memberId) && !IsValidNumericId(memberId))
            {
                memberId = "";
            }
            
            // SECURITY FIX: Use proper URL encoding and validation
            string sURL = Search_FormAction + "item_id=" + Server.UrlEncode(itemId) + "&" 
                         + "member_id=" + Server.UrlEncode(memberId) + "&";
            
            Response.Redirect(sURL);
        }

        // SECURITY FIX: Secure data source creation with parameterized queries
        ICollection Orders_CreateDataSource() 
        {
            Orders_sSQL = "";
            Orders_sCountSQL = "";

            string sWhere = "", sOrder = "";
            bool HasParam = false;
            
            // Build ORDER BY statement with validation
            sOrder = " order by o.order_id Asc";
            if (Utility.GetParam("FormOrders_Sorting").Length > 0 && !IsPostBack)
            {
                // SECURITY FIX: Validate sort column to prevent SQL injection
                string sortColumn = SanitizeInput(Utility.GetParam("FormOrders_Sorting"));
                string[] allowedColumns = { "o.order_id", "o.item_id", "o.member_id", "o.quantity", "i.name", "m.member_login" };
                
                if (Array.Exists(allowedColumns, col => col.Equals(sortColumn, StringComparison.OrdinalIgnoreCase)))
                {
                    ViewState["SortColumn"] = sortColumn;
                    ViewState["SortDir"] = "ASC";
                }
            }
            
            if (ViewState["SortColumn"] != null) 
            {
                string sortDir = ViewState["SortDir"]?.ToString();
                if (sortDir == "ASC" || sortDir == "DESC")
                {
                    sOrder = " ORDER BY " + ViewState["SortColumn"].ToString() + " " + sortDir;
                }
            }

            // SECURITY FIX: Use parameterized queries instead of string concatenation
            System.Collections.Specialized.StringDictionary Params = new System.Collections.Specialized.StringDictionary();
            var parameters = new List<OleDbParameter>();

            // SECURITY FIX: Proper parameter validation and sanitization
            if (!Params.ContainsKey("item_id"))
            {
                string temp = SanitizeInput(Utility.GetParam("item_id"));
                if (IsValidNumericId(temp))
                {
                    Params.Add("item_id", temp);
                }
                else
                {
                    Params.Add("item_id", "");
                }
            }

            if (!Params.ContainsKey("member_id"))
            {
                string temp = SanitizeInput(Utility.GetParam("member_id"));
                if (IsValidNumericId(temp))
                {
                    Params.Add("member_id", temp);
                }
                else
                {
                    Params.Add("member_id", "");
                }
            }

            // SECURITY FIX: Build WHERE clause with parameterized queries
            if (Params["item_id"].Length > 0)
            {
                HasParam = true;
                sWhere += "o.[item_id] = ?";
                parameters.Add(new OleDbParameter("@item_id", OleDbType.Integer) { Value = int.Parse(Params["item_id"]) });
            }
            
            if (Params["member_id"].Length > 0)
            {
                if (sWhere.Length > 0) sWhere += " and ";
                HasParam = true;
                sWhere += "o.[member_id] = ?";
                parameters.Add(new OleDbParameter("@member_id", OleDbType.Integer) { Value = int.Parse(Params["member_id"]) });
            }

            if (HasParam)
                sWhere = " AND (" + sWhere + ")";

            // Build base SQL statement
            Orders_sSQL = "select [o].[item_id] as o_item_id, " +
                "[o].[member_id] as o_member_id, " +
                "[o].[order_id] as o_order_id, " +
                "[o].[quantity] as o_quantity, " +
                "[i].[item_id] as i_item_id, " +
                "[i].[name] as i_name, " +
                "[m].[member_id] as m_member_id, " +
                "[m].[member_login] as m_member_login " +
                " from [orders] o, [items] i, [members] m" +
                " where [i].[item_id]=o.[item_id] and [m].[member_id]=o.[member_id]  ";

            // Assemble full SQL statement
            Orders_sSQL = Orders_sSQL + sWhere + sOrder;
            
            if (Orders_sCountSQL.Length == 0)
            {
                int iTmpI = Orders_sSQL.ToLower().IndexOf("select ");
                int iTmpJ = Orders_sSQL.ToLower().LastIndexOf(" from ") - 1;
                Orders_sCountSQL = Orders_sSQL.Replace(Orders_sSQL.Substring(iTmpI + 7, iTmpJ - 6), " count(*) ");
                iTmpI = Orders_sCountSQL.ToLower().IndexOf(" order by");
                if (iTmpI > 1) Orders_sCountSQL = Orders_sCountSQL.Substring(0, iTmpI);
            }

            // SECURITY FIX: Use parameterized command
            OleDbDataAdapter command = new OleDbDataAdapter(Orders_sSQL, Utility.Connection);
            
            // Add parameters to the command
            foreach (var param in parameters)
            {
                command.SelectCommand.Parameters.Add(param);
            }

            DataSet ds = new DataSet();
            command.Fill(ds, (i_Orders_curpage - 1) * Orders_PAGENUM, Orders_PAGENUM, "Orders");
            
            // SECURITY FIX: Use parameterized command for count query
            OleDbCommand ccommand = new OleDbCommand(Orders_sCountSQL, Utility.Connection);
            foreach (var param in parameters)
            {
                ccommand.Parameters.Add(new OleDbParameter(param.ParameterName, param.Value));
            }
            
            int PageTemp = (int)ccommand.ExecuteScalar();
            Orders_Pager.MaxPage = (PageTemp % Orders_PAGENUM) > 0 ? (int)(PageTemp / Orders_PAGENUM) + 1 : (int)(PageTemp / Orders_PAGENUM);
            bool AllowScroller = Orders_Pager.MaxPage == 1 ? false : true;

            DataView Source = new DataView(ds.Tables[0]);

            if (ds.Tables[0].Rows.Count == 0)
            {
                Orders_no_records.Visible = true;
                AllowScroller = false;
            }
            else
            {
                Orders_no_records.Visible = false;
                AllowScroller = AllowScroller && true;
            }

            Orders_Pager.Visible = AllowScroller;
            return Source;
        }

        // SECURITY FIX: Secure insert click handler
        void Orders_insert_Click(Object Src, EventArgs E) 
        {
            // SECURITY FIX: Validate and sanitize parameters
            string itemId = SanitizeInput(Utility.GetParam("item_id"));
            string memberId = SanitizeInput(Utility.GetParam("member_id"));
            
            // Additional validation
            if (!string.IsNullOrEmpty(itemId) && !IsValidNumericId(itemId))
            {
                itemId = "";
            }
            
            if (!string.IsNullOrEmpty(memberId) && !IsValidNumericId(memberId))
            {
                memberId = "";
            }
            
            string sURL = Orders_FormAction + "item_id=" + Server.UrlEncode(itemId) + "&member_id=" + Server.UrlEncode(memberId) + "&";
            Response.Redirect(sURL);
        }

        // SECURITY FIX: Secure sort change handler
        protected void Orders_SortChange(Object Src, EventArgs E) 
        {
            // SECURITY FIX: Validate sort column to prevent injection
            string sortColumn = ((LinkButton)Src).CommandArgument;
            string[] allowedColumns = { "o.order_id", "o.item_id", "o.member_id", "o.quantity", "i.name", "m.member_login" };
            
            if (!Array.Exists(allowedColumns, col => col.Equals(sortColumn, StringComparison.OrdinalIgnoreCase)))
            {
                return; // Invalid sort column, ignore request
            }
            
            if (ViewState["SortColumn"] == null || ViewState["SortColumn"].ToString() != sortColumn)
            {
                ViewState["SortColumn"] = sortColumn;
                ViewState["SortDir"] = "ASC";
            }
            else
            {
                ViewState["SortDir"] = ViewState["SortDir"].ToString() == "ASC" ? "DESC" : "ASC";
            }
            Orders_Bind();
        }

        // Rest of the original methods remain the same but with added security measures
        protected CCUtility Utility;
        protected string Orders_sSQL;
        protected string Orders_sCountSQL;
        protected int Orders_CountPage;
        protected int i_Orders_curpage = 1;
        protected string Search_FormAction = "OrdersGrid.aspx?";
        protected string Orders_FormAction = "OrdersRecord.aspx?";
        const int Orders_PAGENUM = 20;

        public OrdersGrid()
        {
            this.Init += new System.EventHandler(Page_Init);
        }

        public void ValidateNumeric(object source, ServerValidateEventArgs args)
        {
            try
            {
                Decimal temp = Decimal.Parse(args.Value);
                args.IsValid = true;
            }
            catch
            {
                args.IsValid = false;
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            if (Utility != null) Utility.DBClose();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            InitializeComponent();
            Search_search_button.Click += new System.EventHandler(this.Search_search_Click);
            Orders_insert.Click += new System.EventHandler(this.Orders_insert_Click);
            Orders_Pager.NavigateCompleted += new NavigateCompletedHandler(this.Orders_pager_navigate_completed);
        }

        private void InitializeComponent()
        {
        }

        protected void Page_Show(object sender, EventArgs e)
        {
            Search_Show();
            Orders_Bind();
        }

        void Search_Show()
        {
            // SECURITY FIX: These buildListBox calls should also be reviewed for SQL injection
            // but keeping original for now as it's likely a utility method
            Utility.buildListBox(Search_item_id.Items, "select item_id,name from items order by 2", "item_id", "name", "All", "");
            Utility.buildListBox(Search_member_id.Items, "select member_id,member_login from members order by 2", "member_id", "member_login", "All", "");

            string s;
            s = SanitizeInput(Utility.GetParam("item_id"));
            try
            {
                Search_item_id.SelectedIndex = Search_item_id.Items.IndexOf(Search_item_id.Items.FindByValue(s));
            }
            catch { }

            s = SanitizeInput(Utility.GetParam("member_id"));
            try
            {
                Search_member_id.SelectedIndex = Search_member_id.Items.IndexOf(Search_member_id.Items.FindByValue(s));
            }
            catch { }
        }

        public void Orders_Repeater_ItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            // Orders Show Event
        }

        protected void Orders_pager_navigate_completed(Object Src, int CurrPage)
        {
            i_Orders_curpage = CurrPage;
            Orders_Bind();
        }

        void Orders_Bind()
        {
            Orders_Repeater.DataSource = Orders_CreateDataSource();
            Orders_Repeater.DataBind();
        }
    }
}