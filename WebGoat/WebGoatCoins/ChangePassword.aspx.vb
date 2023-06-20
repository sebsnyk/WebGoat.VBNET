Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class ChangePassword
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(sender As Object, e As EventArgs)
        End Sub

        Protected Sub ButtonChangePassword_Click(sender As Object, e As EventArgs)
            If txtPassword1.Text IsNot Nothing AndAlso txtPassword2.Text IsNot Nothing AndAlso txtPassword1.Text = txtPassword2.Text Then
                'get customer ID
                Dim customerNumber As String = ""
                If Request.Cookies("customerNumber") IsNot Nothing Then
                    customerNumber = Request.Cookies("customerNumber").Value
                End If

                Dim output As String = du.UpdateCustomerPassword(Integer.Parse(customerNumber), txtPassword1.Text)
                labelMessage.Text = output
            Else
                labelMessage.Text = "Passwords do not match!  Please try again!"
            End If
        End Sub
    End Class
End Namespace