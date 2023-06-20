Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET

    Partial Public Class StoredXSS
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            lblMessage.Visible = False
            txtEmail.Enabled = True
            If Not Page.IsPostBack Then LoadComments()
        End Sub

        Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs)
            Try
                Dim error_message As String = du.AddComment("user_cmt", txtEmail.Text, txtComment.Text)
                txtComment.Text = error_message
                lblMessage.Visible = True
                LoadComments()
            Catch ex As Exception
                lblMessage.Text = ex.Message
                lblMessage.Visible = True
            End Try
        End Sub

        Sub LoadComments()
            Dim ds As DataSet = du.GetComments("user_cmt")
            Dim comments As String = String.Empty
            For Each row As DataRow In ds.Tables(0).Rows
                comments += "<strong>Email:</strong>" & row("email") & "<span style='font-size: x-small;color: #E47911;'> (Email Address Verified!) </span><br/>"
                comments += "<strong>Comment:</strong><br/>" & row("comment") & "<br/><hr/>"
            Next
            lblComments.Text = comments
        End Sub

        Sub FixedLoadComments()
            Dim ds As DataSet = du.GetComments("user_cmt")
            Dim comments As String = String.Empty
            For Each row As DataRow In ds.Tables(0).Rows
                comments += "<strong>Email:</strong>" & Server.HtmlEncode(row("email").ToString()) & "<span style='font-size: x-small;color: #E47911;'> (Email Address Verified!) </span><br/>"
                comments += "<strong>Comment:</strong><br/>" & Server.HtmlEncode(row("comment").ToString()) & "<br/><hr/>"
            Next
            lblComments.Text = comments
        End Sub
    End Class
End Namespace