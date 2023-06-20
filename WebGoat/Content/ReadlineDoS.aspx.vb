Imports System
Imports System.Web
Imports System.Web.UI
Imports System.IO

Namespace OWASP.WebGoat.NET
    Partial Public Class ReadlineDoS 
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load

        End Sub

        Protected Sub btnUpload_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUpload.Click
            lblFileContent.Text = String.Empty
            Dim fileContents As Stream = file1.PostedFile.InputStream

            Using reader As New StreamReader(fileContents)
                While Not reader.EndOfStream
                    lblFileContent.Text += reader.ReadLine() & "<br />"
                End While
            End Using
        End Sub
    End Class
End Namespace