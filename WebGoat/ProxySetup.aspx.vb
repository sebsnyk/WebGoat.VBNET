Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace OWASP.WebGoat.NET
    Partial Public Class ProxySetup
        Inherits System.Web.UI.Page

        Protected Sub btnReverse_Click(sender As Object, e As EventArgs)

            Dim name As String = txtName.Text
            txtName.Text = ""
            lblOutput.Text = "Thank you for using WebGoat.NET " & reverse(name)

        End Sub

        Private Function reverse(s As String) As String

            Dim charArray As Char() = s.ToCharArray()
            Array.Reverse(charArray)
            Return New String(charArray)

        End Function

    End Class
End Namespace