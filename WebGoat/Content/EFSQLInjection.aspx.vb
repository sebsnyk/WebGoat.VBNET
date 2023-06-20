Imports System
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports OWASP.WebGoat.NET.Entities

Namespace OWASP.WebGoat.NET.Content
    Partial Public Class EFSQLInjection
        Inherits Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

        End Sub

        Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs)
            Using db As New CoinsDB()
                Dim code = txtOfficeCode.Text
                Dim output = db.Database _
                    .SqlQuery(Of String)("SELECT email FROM Employees " & "WHERE officeCode = {0}", code) _
                    .ToArray()

                lblOutput.Text = If(output.Length = 0, "Not found email", String.Join("<br/>", output))
            End Using
        End Sub
    End Class
End Namespace