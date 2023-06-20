Imports System
Imports System.Web
Imports System.Web.UI
Imports System.Text.RegularExpressions

Namespace OWASP.WebGoat.NET
    Public Partial Class RegexDoS
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        End Sub

        ''' <summary>
        ''' Code from https://www.owasp.org/index.php/Regular_expression_Denial_of_Service_-_ReDoS
        ''' </summary>
        Protected Sub btnCreate_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim userName As String = txtUsername.Text
            Dim password As String = txtPassword.Text

            Dim testPassword As New Regex(userName)
            Dim match As Match = testPassword.Match(password)
            If match.Success Then
                lblError.Text = "Do not include name in password."
            Else
                lblError.Text = "Good password."
            End If
        End Sub
    End Class
End Namespace