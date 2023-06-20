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
	Public Partial Class SQLInjectionDiscovery
		Inherits System.Web.UI.Page

		Private du As IDbProvider = Settings.CurrentDbProvider

		Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

		End Sub

		Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs)
			Try
				Dim name As String = txtID.Text.Substring(0, 3)
				Dim output As String = du.GetEmailByCustomerNumber(name)

				lblOutput.Text = If(String.IsNullOrEmpty(output), "Customer Number does not exist", output)
			Catch ex As Exception
				lblOutput.Text = ex.Message
			End Try
		End Sub
	End Class
End Namespace